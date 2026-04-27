using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace NurseLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(NurseLinkDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required.");

            try
            {
                var normalizedEmail = request.Email.Trim().ToLower();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserEmail.ToLower() == normalizedEmail && u.UserActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.UserPassword))
                    return Unauthorized("Invalid credentials or user inactive.");

                switch ((UserRole)user.UserRole)
                {
                    case UserRole.Admin:
                        {
                            var admin = await _context.Administrators
                                .Include(a => a.User)
                                .FirstOrDefaultAsync(a => a.UserId == user.UserId);

                            if (admin == null)
                                return Unauthorized("Administrator profile not found.");

                            var fullName = admin.User.UserName + " " + admin.User.UserSurname;

                            var token = GenerateJwtToken(
                                admin.AdminId,
                                admin.User.UserEmail,
                                UserRole.Admin,
                                fullName);

                            return Ok(new LoginResponseDto
                            {
                                Token = token,
                                Role = UserRole.Admin.ToString(),
                                UserId = admin.AdminId,
                                Email = admin.User.UserEmail,
                                Name = fullName
                            });
                        }

                    case UserRole.Nurse:
                        {
                            var nurse = await _context.Nurses
                                .Include(n => n.User)
                                .FirstOrDefaultAsync(n => n.UserId == user.UserId);

                            if (nurse == null)
                                return Unauthorized("Nurse profile not found.");

                            var fullName = nurse.User.UserName + " " + nurse.User.UserSurname;

                            var token = GenerateJwtToken(
                                nurse.NurseId,
                                nurse.User.UserEmail,
                                UserRole.Nurse,
                                fullName);

                            return Ok(new LoginResponseDto
                            {
                                Token = token,
                                Role = UserRole.Nurse.ToString(),
                                UserId = nurse.NurseId,
                                Email = nurse.User.UserEmail,
                                Name = fullName
                            });
                        }

                    case UserRole.Patient:
                        {
                            var patient = await _context.Patients
                                .Include(p => p.User)
                                .FirstOrDefaultAsync(p => p.UserId == user.UserId);

                            if (patient == null)
                                return Unauthorized("Patient profile not found.");

                            var fullName = patient.User.UserName + " " + patient.User.UserSurname;

                            var token = GenerateJwtToken(
                                patient.PatientId,
                                patient.User.UserEmail,
                                UserRole.Patient,
                                fullName);

                            return Ok(new LoginResponseDto
                            {
                                Token = token,
                                Role = UserRole.Patient.ToString(),
                                UserId = patient.PatientId,
                                Email = patient.User.UserEmail,
                                Name = fullName
                            });
                        }

                    default:
                        return Unauthorized("Invalid user role.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", request.Email);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("forgotPassword")]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return BadRequest("Email, new password and confirm password are required.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 255)
                return BadRequest("Password must be between 6 and 255 characters.");

            var passwordRegex = new Regex(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$"
            );

            if (!passwordRegex.IsMatch(request.NewPassword))
                return BadRequest("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.");

            try
            {
                var normalizedEmail = request.Email.Trim().ToLower();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserEmail.ToLower() == normalizedEmail);

                if (user == null)
                    return NotFound("User with this email was not found.");

                if (!user.UserActive)
                    return BadRequest("This user is inactive.");

                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new ForgotPasswordResponseDto
                {
                    Message = "Password updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email {Email}", request.Email);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        private string GenerateJwtToken(int? userId, string email, UserRole role, string name)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role.ToString()),
                new(ClaimTypes.Name, name)
            };

            if (userId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}