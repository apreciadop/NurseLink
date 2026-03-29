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
    public class NursesController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<NursesController> _logger;

        public NursesController(NurseLinkDbContext context, ILogger<NursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateNurseResponseDto>> CreateNurse([FromBody] CreateNurseRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body required.");
            }

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
                    UserRole = (int)UserRole.Nurse,
                    UserName = request.Name,
                    UserSurname = request.Surname,
                    UserEmail = request.Email,
                    UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserActive = true,
                    UserBirthdate = request.Birthdate,
                    UserPhone = request.Phone,
                    UserPhoto = request.Photo,
                    CreatedAt = DateTime.UtcNow
                };

                var nurse = new Nurse
                {
                    User = user,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Nurses.Add(nurse);
                await _context.SaveChangesAsync();

                return Ok(new CreateNurseResponseDto
                {
                    Message = "Nurse created successfully.",
                    NurseId = nurse.NurseId,
                    UserId = nurse.User.UserId,
                    Name = nurse.User.UserName,
                    Surname = nurse.User.UserSurname,
                    Email = nurse.User.UserEmail,
                    Role = nurse.User.UserRole,
                    RoleName = ((UserRole)nurse.User.UserRole).ToString(),
                    Active = nurse.User.UserActive,
                    CreatedAt = nurse.User.CreatedAt,
                    Birthdate = nurse.User.UserBirthdate.HasValue ? DateOnly.FromDateTime(nurse.User.UserBirthdate.Value) : null,
                    Photo = nurse.User.UserPhoto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating nurse with email {Email}", request.Email);
                return StatusCode(500, "Error creating nurse.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var nurses = await _context.Nurses
                    .Include(n => n.User)
                    .Select(n => new
                    {
                        nurseId = n.NurseId,
                        userId = n.User.UserId,
                        name = n.User.UserName,
                        surname = n.User.UserSurname,
                        email = n.User.UserEmail,
                        role = n.User.UserRole,
                        roleName = ((UserRole)n.User.UserRole).ToString(),
                        active = n.User.UserActive,
                        createdAt = n.User.CreatedAt,
                        birthdate = n.User.UserBirthdate.HasValue ? (DateOnly?)DateOnly.FromDateTime(n.User.UserBirthdate.Value) : null,
                        photo = n.User.UserPhoto
                    })
                    .ToListAsync();

                return Ok(nurses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting nurses");
                return StatusCode(500, "Error consulting nurses.");
            }
        }
    }
}


