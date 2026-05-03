using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.Common;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;
using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NursesController(NurseLinkDbContext context, ILogger<NursesController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<NursesController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreateNurseResponseDto>> CreateNurse([FromBody] CreateNurseRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            var passwordError = PasswordValidator.Validate(request.Password);

            if (passwordError != null)
                return BadRequest(passwordError);

            var email = request.Email.Trim();

            var emailExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserEmail == email, cancellation);

            if (emailExists)
                return BadRequest("Email already exists.");

            try
            {
                var user = new User
                {
                    UserRole = UserRole.Nurse,
                    UserName = request.Name.Trim(),
                    UserSurname = request.Surname.Trim(),
                    UserEmail = email,
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
                await _context.SaveChangesAsync(cancellation);

                return Ok(new CreateNurseResponseDto
                {
                    Message = "Nurse created successfully.",
                    NurseId = nurse.NurseId,
                    UserId = nurse.User.UserId,
                    Name = nurse.User.UserName,
                    Surname = nurse.User.UserSurname,
                    Email = nurse.User.UserEmail,
                    Role = nurse.User.UserRole,
                    RoleName = nurse.User.UserRole.ToString(),
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
        public async Task<IActionResult> GetAll(CancellationToken cancellation)
        {
            try
            {
                var nurses = await _context.Nurses
                    .AsNoTracking()
                    .Select(n => new
                    {
                        nurseId = n.NurseId,
                        userId = n.User.UserId,
                        name = n.User.UserName,
                        surname = n.User.UserSurname,
                        email = n.User.UserEmail,
                        role = n.User.UserRole,
                        roleName = n.User.UserRole.ToString(),
                        active = n.User.UserActive,
                        createdAt = n.User.CreatedAt,
                        birthdate = n.User.UserBirthdate,
                        photo = n.User.UserPhoto
                    })
                    .OrderBy(n => n.name)
                    .ThenBy(n => n.surname)
                    .ToArrayAsync(cancellation);

                return Ok(nurses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting nurses");
                return StatusCode(500, "Error consulting nurses.");
            }
        }

        [HttpGet("nursesDetailed")]
        public async Task<ActionResult<GetNursesDetailedResponseDto[]>> GetNursesDetailed(CancellationToken cancellation)
        {
            try
            {
                var nurses = await _context.Nurses
                    .AsNoTracking()
                    .Select(n => new GetNursesDetailedResponseDto
                    {
                        NurseId = n.NurseId,
                        UserId = n.User.UserId,
                        Name = n.User.UserName,
                        Surname = n.User.UserSurname,
                        PhoneNumber = n.User.UserPhone ?? string.Empty,
                        Active = n.User.UserActive,
                        Photo = n.User.UserPhoto,

                        PatientCount = n.Assignments.Count(),

                        AlertCount = n.Reports
                            .GroupBy(r => r.PatientId)
                            .Select(g => g
                                .OrderByDescending(r => r.ReportDate)
                                .ThenByDescending(r => r.CreatedAt)
                                .Select(r => r.ReportAlerts)
                                .FirstOrDefault())
                            .Sum()
                    })
                    .OrderBy(n => n.Name)
                    .ThenBy(n => n.Surname)
                    .ToArrayAsync(cancellation);

                return Ok(nurses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting detailed nurses list");
                return StatusCode(500, "Error consulting detailed nurses list.");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<UpdateNurseResponseDto>> UpdateNurse(int id, [FromBody] UpdateNurseRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            try
            {
                var nurse = await _context.Nurses
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.NurseId == id, cancellation);

                if (nurse == null)
                    return NotFound("Nurse not found.");

                var email = request.Email.Trim();

                var emailExists = await _context.Users
                    .AnyAsync(u => u.UserEmail == email && u.UserId != nurse.User.UserId, cancellation);

                if (emailExists)
                    return BadRequest("Email already exists.");

                if (!request.Active)
                {
                    var hasAssignedPatients = await _context.Assignments
                        .AnyAsync(a => a.NurseId == id, cancellation);

                    if (hasAssignedPatients)
                        return BadRequest("This nurse cannot be deactivated because she still has assigned patients.");
                }

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    var passwordError = PasswordValidator.Validate(request.Password);

                    if (passwordError != null)
                        return BadRequest(passwordError);

                    nurse.User.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                nurse.User.UserName = request.Name.Trim();
                nurse.User.UserSurname = request.Surname.Trim();
                nurse.User.UserEmail = email;
                nurse.User.UserBirthdate = request.Birthdate;
                nurse.User.UserPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                nurse.User.UserPhoto = request.Photo;
                nurse.User.UserActive = request.Active;

                await _context.SaveChangesAsync(cancellation);

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
        public async Task<ActionResult<GetNurseByIdResponseDto>> GetById(int id, CancellationToken cancellation)
        {
            try
            {
                var nurse = await _context.Nurses
                    .AsNoTracking()
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
                    .FirstOrDefaultAsync(cancellation);

                if (nurse == null)
                    return NotFound("Nurse not found.");

                return Ok(nurse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting nurse with id {NurseId}", id);
                return StatusCode(500, "Error consulting nurse with id " + id);
            }
        }

        [HttpGet("{id}/assignedPatients")]
        public async Task<ActionResult<GetAssignedPatientsByNurseResponseDto[]>> GetAssignedPatientsByNurse(int id, CancellationToken cancellation)
        {
            try
            {
                var nurseExists = await _context.Nurses
                    .AsNoTracking()
                    .AnyAsync(n => n.NurseId == id, cancellation);

                if (!nurseExists)
                    return NotFound("Nurse not found.");

                var assignedPatients = await _context.Assignments
                    .AsNoTracking()
                    .Where(a => a.NurseId == id)
                    .Select(a => new
                    {
                        a.Patient.PatientId,
                        Name = a.Patient.User.UserName,
                        Surname = a.Patient.User.UserSurname,
                        Photo = a.Patient.User.UserPhoto,
                        Birthdate = a.Patient.User.UserBirthdate,
                        Surgery = a.Patient.Surgery.SurgeryType.SurgeryTypeName,
                        SurgeryDate = a.Patient.Surgery.SurgeryDate,
                        Phone = a.Patient.User.UserPhone,
                        Active = a.Patient.User.UserActive,
                        AlertCount = a.Patient.Reports
                            .OrderByDescending(r => r.ReportDate)
                            .ThenByDescending(r => r.CreatedAt)
                            .Select(r => r.ReportAlerts)
                            .FirstOrDefault()
                    })
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Surname)
                    .ToArrayAsync(cancellation);

                var response = assignedPatients
                    .Select(p => new GetAssignedPatientsByNurseResponseDto
                    {
                        PatientId = p.PatientId,
                        Name = p.Name,
                        Surname = p.Surname,
                        Photo = p.Photo,
                        Birthdate = p.Birthdate,
                        Age = CalculateAge(p.Birthdate),
                        Surgery = p.Surgery,
                        SurgeryDate = p.SurgeryDate,
                        Phone = p.Phone,
                        Active = p.Active,
                        AlertCount = p.AlertCount,
                        Status = GetPatientStatus(p.AlertCount)
                    })
                    .ToArray();

                return Ok(response);
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
                return ReportStatus.Stable;

            if (alertCount >= 1 && alertCount <= 2)
                return ReportStatus.Warning;

            return ReportStatus.Alert;
        }

        private static int? CalculateAge(DateTime? birthdate)
        {
            if (!birthdate.HasValue)
                return null;

            var today = DateTime.Today;
            var age = today.Year - birthdate.Value.Year;

            if (birthdate.Value.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
}