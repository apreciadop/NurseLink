using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;
using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(NurseLinkDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateAdminResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid data. Please check required fields.");

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Surname) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Name, surname, email and password are required.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.UserEmail == request.Email);

            if (emailExists)
                return BadRequest("Email already exists.");

            try
            {
                var user = new User
                {
                    UserRole = (int)UserRole.Admin,
                    UserName = request.Name,
                    UserSurname = request.Surname,
                    UserEmail = request.Email,
                    UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserActive = true,
                    UserBirthdate = null,
                    UserPhone = null,
                    UserPhoto = null,
                    CreatedAt = DateTime.UtcNow
                };

                var admin = new Administrator
                {
                    User = user,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Administrators.Add(admin);

                await _context.SaveChangesAsync();

                return Ok(new CreateAdminResponseDto
                {
                    Message = "Administrator created successfully.",
                    AdminId = admin.AdminId,
                    UserId = admin.User.UserId,
                    Name = admin.User.UserName,
                    Surname = admin.User.UserSurname,
                    Email = admin.User.UserEmail,
                    Role = admin.User.UserRole,
                    RoleName = ((UserRole)admin.User.UserRole).ToString(),
                    Active = admin.User.UserActive,
                    CreatedAt = admin.User.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating administrator with email {Email}", request.Email);                
                return StatusCode(500, "Error creating administrator.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var admins = await _context.Administrators
                    .Include(a => a.User)
                    .Select(a => new
                    {
                        adminId = a.AdminId,
                        userId = a.User.UserId,
                        name = a.User.UserName,
                        surname = a.User.UserSurname,
                        email = a.User.UserEmail,
                        role = a.User.UserRole,
                        roleName = ((UserRole)a.User.UserRole).ToString(),
                        active = a.User.UserActive,
                        createdAt = a.User.CreatedAt
                    })
                    .ToListAsync();

                return Ok(admins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting administrators");
                return StatusCode(500, "Error consulting administrators.");
            }
        }
    }
}