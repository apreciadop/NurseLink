using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;
using NurseLink.API.Domain.Enums;
using System.Text.RegularExpressions;

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
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Surname) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                !request.Birthdate.HasValue ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Name, surname, email, birthday and password are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await _context.Users
                .AnyAsync(u => u.UserEmail.ToLower() == normalizedEmail);

            if (emailExists)
            {
                return BadRequest("Email already exists.");
            }

            try
            {
                var user = new User
                {
                    UserRole = UserRole.Nurse,
                    UserName = request.Name.Trim(),
                    UserSurname = request.Surname.Trim(),
                    UserEmail = normalizedEmail,
                    UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserActive = true,
                    UserBirthdate = request.Birthdate,
                    UserPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
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
                    Birthdate = nurse.User.UserBirthdate,
                    Photo = nurse.User.UserPhoto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating nurse with email {Email}", request.Email);
                return StatusCode(500, "Error creating nurse with email '" + request.Email + "'");
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
                        birthdate = n.User.UserBirthdate,
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

        [HttpGet("nursesDetailed")]
        public async Task<ActionResult<IEnumerable<GetNursesDetailedResponseDto>>> GetNursesDetailed()
        {
            try
            {
                var nurses = await _context.Nurses
                    .Include(n => n.User)
                    .Select(n => new GetNursesDetailedResponseDto
                    {
                        NurseId = n.NurseId,
                        UserId = n.User.UserId,
                        Name = n.User.UserName,
                        Surname = n.User.UserSurname,
                        PhoneNumber = n.User.UserPhone ?? string.Empty,
                        Active = n.User.UserActive,
                        Photo = n.User.UserPhoto,

                        PatientCount = _context.Assignments
                            .Count(a => a.NurseId == n.NurseId),

                        AlertCount = _context.Assignments
                            .Where(a => a.NurseId == n.NurseId)
                            .Count(a =>
                                _context.Reports
                                    .Where(r => r.PatientId == a.PatientId)
                                    .OrderByDescending(r => r.ReportDate)
                                    .ThenByDescending(r => r.CreatedAt)
                                    .Select(r => r.ReportStatus)
                                    .FirstOrDefault() == ReportStatus.Alert)
                    })
                    .ToListAsync();

                return Ok(nurses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting detailed nurses list");
                return StatusCode(500, "Error consulting detailed nurses list.");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<UpdateNurseResponseDto>> UpdateNurse(int id, [FromBody] UpdateNurseRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Surname) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Name, surname and email are required.");
            }

            try
            {
                var nurse = await _context.Nurses
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.NurseId == id);

                if (nurse == null)
                {
                    return NotFound("Nurse not found.");
                }

                var normalizedEmail = request.Email.Trim().ToLower();

                var emailExists = await _context.Users
                    .AnyAsync(u => u.UserEmail.ToLower() == normalizedEmail && u.UserId != nurse.User.UserId);

                if (emailExists)
                {
                    return BadRequest("Email already exists.");
                }

                if (!request.Active)
                {
                    var hasAssignedPatients = await _context.Assignments
                        .AnyAsync(a => a.NurseId == id);

                    if (hasAssignedPatients)
                    {
                        return BadRequest("This nurse cannot be deactivated because she still has assigned patients.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    if (request.Password.Length < 6 || request.Password.Length > 255)
                    {
                        return BadRequest("Password must be between 6 and 255 characters.");
                    }

                    var passwordRegex = new Regex(
                        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$"
                    );

                    if (!passwordRegex.IsMatch(request.Password))
                    {
                        return BadRequest("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.");
                    }

                    nurse.User.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                nurse.User.UserName = request.Name.Trim();
                nurse.User.UserSurname = request.Surname.Trim();
                nurse.User.UserEmail = normalizedEmail;
                nurse.User.UserBirthdate = request.Birthdate;
                nurse.User.UserPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                nurse.User.UserPhoto = request.Photo;
                nurse.User.UserActive = request.Active;

                await _context.SaveChangesAsync();

                return Ok(new UpdateNurseResponseDto
                {
                    Message = "Nurse updated successfully.",
                    NurseId = nurse.NurseId,
                    UserId = nurse.User.UserId,
                    Name = nurse.User.UserName,
                    Surname = nurse.User.UserSurname,
                    Email = nurse.User.UserEmail,
                    Active = nurse.User.UserActive,
                    Birthdate = nurse.User.UserBirthdate,
                    Phone = nurse.User.UserPhone,
                    Photo = nurse.User.UserPhoto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating nurse with id {NurseId}", id);
                return StatusCode(500, "Error updating nurse with id " + id);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetNurseByIdResponseDto>> GetById(int id)
        {
            try
            {
                var nurse = await _context.Nurses
                    .Include(n => n.User)
                    .Where(n => n.NurseId == id)
                    .Select(n => new GetNurseByIdResponseDto
                    {
                        NurseId = n.NurseId,
                        UserId = n.User.UserId,
                        Name = n.User.UserName,
                        Surname = n.User.UserSurname,
                        Email = n.User.UserEmail,
                        Birthdate = n.User.UserBirthdate,
                        Phone = n.User.UserPhone,
                        Photo = n.User.UserPhoto,
                        Active = n.User.UserActive
                    })
                    .FirstOrDefaultAsync();

                if (nurse == null)
                {
                    return NotFound("Nurse not found.");
                }

                return Ok(nurse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting nurse with id {NurseId}", id);
                return StatusCode(500, "Error consulting nurse with id " + id);
            }
        }

        [HttpGet("{id}/assignedPatients")]
        public async Task<ActionResult<IEnumerable<GetAssignedPatientsByNurseResponseDto>>> GetAssignedPatientsByNurse(int id)
        {
            try
            {
                var nurseExists = await _context.Nurses
                    .AnyAsync(n => n.NurseId == id);

                if (!nurseExists)
                {
                    return NotFound("Nurse not found.");
                }

                var assignments = await _context.Assignments
                    .Where(a => a.NurseId == id)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Surgery)
                            .ThenInclude(s => s.SurgeryType)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Reports)
                    .ToListAsync();

                var assignedPatients = assignments
                    .Select(a =>
                    {
                        var latestReport = a.Patient.Reports
                            .OrderByDescending(r => r.ReportDate)
                            .ThenByDescending(r => r.CreatedAt)
                            .FirstOrDefault();

                        var alertCount = latestReport?.ReportAlerts ?? 0;
                        var status = GetPatientStatus(alertCount);

                        return new GetAssignedPatientsByNurseResponseDto
                        {
                            PatientId = a.Patient.PatientId,
                            Name = a.Patient.User.UserName,
                            Surname = a.Patient.User.UserSurname,
                            Photo = a.Patient.User.UserPhoto,

                            Birthdate = a.Patient.User.UserBirthdate,
                            Age = CalculateAge(a.Patient.User.UserBirthdate),

                            Surgery = a.Patient.Surgery.SurgeryType.SurgeryTypeName,
                            SurgeryDate = a.Patient.Surgery.SurgeryDate,

                            Phone = a.Patient.User.UserPhone,
                            Active = a.Patient.User.UserActive,

                            AlertCount = alertCount,
                            Status = status
                        };
                    })
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .ToList();

                return Ok(assignedPatients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting assigned patients for nurse with id {NurseId}", id);
                return StatusCode(500, "Error consulting assigned patients for nurse with id " + id);
            }
        }

        private static ReportStatus GetPatientStatus(int alertCount)
        {
            if (alertCount == 0)
            {
                return ReportStatus.Stable;
            }

            if (alertCount >= 1 && alertCount <= 2)
            {
                return ReportStatus.Warning;
            }

            return ReportStatus.Alert;
        }

        private static int? CalculateAge(DateTime? birthdate)
        {
            if (!birthdate.HasValue)
            {
                return null;
            }

            var today = DateTime.Today;
            var age = today.Year - birthdate.Value.Year;

            if (birthdate.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}