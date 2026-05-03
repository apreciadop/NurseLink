using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NurseLink.API.Database;
using NurseLink.API.Domain.Common;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NurseLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(NurseLinkDbContext context, IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthController> _logger = logger;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellation)
        {
            try
            {
                var email = request.Email.Trim();

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserEmail == email && u.UserActive, cancellation);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.UserPassword))
                    return Unauthorized("Invalid credentials or user inactive.");

                switch ((UserRole)user.UserRole)
                {
                    case UserRole.Admin:
                        {
                            var admin = await _context.Administrators
                                .AsNoTracking()
                                .Include(a => a.User)
                                .FirstOrDefaultAsync(a => a.UserId == user.UserId, cancellation);

                            if (admin == null)
                                return Unauthorized("Administrator profile not found.");

                            var fullName = admin.User.UserName + " " + admin.User.UserSurname;

                            var token = GenerateJwtToken(
                                admin.AdminId,
                                admin.User.UserEmail,
                                UserRole.Admin,
                                fullName
                            );

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
                                .AsNoTracking()
                                .Include(n => n.User)
                                .FirstOrDefaultAsync(n => n.UserId == user.UserId, cancellation);

                            if (nurse == null)
                                return Unauthorized("Nurse profile not found.");

                            var fullName = nurse.User.UserName + " " + nurse.User.UserSurname;

                            var token = GenerateJwtToken(
                                nurse.NurseId,
                                nurse.User.UserEmail,
                                UserRole.Nurse,
                                fullName
                            );

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
                                .AsNoTracking()
                                .Include(p => p.User)
                                .FirstOrDefaultAsync(p => p.UserId == user.UserId, cancellation);

                            if (patient == null)
                                return Unauthorized("Patient profile not found.");

                            var fullName = patient.User.UserName + " " + patient.User.UserSurname;

                            var token = GenerateJwtToken(
                                patient.PatientId,
                                patient.User.UserEmail,
                                UserRole.Patient,
                                fullName
                            );

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
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellation)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var passwordError = PasswordValidator.Validate(request.NewPassword);

            if (passwordError != null)
                return BadRequest(passwordError);

            try
            {
                var email = request.Email.Trim();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserEmail == email, cancellation);

                if (user == null)
                    return NotFound("User with this email was not found.");

                if (!user.UserActive)
                    return BadRequest("This user is inactive.");

                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                await _context.SaveChangesAsync(cancellation);

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

        private string GenerateJwtToken(int userId, string email, UserRole role, string name)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role.ToString()),
                new(ClaimTypes.Name, name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration[ConstantConfiguration.jwtKey]!)
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = int.Parse(_configuration[ConstantConfiguration.jwtExpireMinutes]!);

            var token = new JwtSecurityToken(
                issuer: _configuration[ConstantConfiguration.jwtIssuer],
                audience: _configuration[ConstantConfiguration.jwtAudience],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}