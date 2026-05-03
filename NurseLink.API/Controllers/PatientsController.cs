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
    public class PatientsController(NurseLinkDbContext context, ILogger<PatientsController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<PatientsController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreatePatientResponseDto>> CreatePatient([FromBody] CreatePatientRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!request.SurgeryDate.HasValue)
                return BadRequest("Surgery date is required.");

            var surgeryDate = request.SurgeryDate.Value;

            var passwordError = PasswordValidator.Validate(request.Password);

            if (passwordError != null)
                return BadRequest(passwordError);

            var email = request.Email.Trim();

            var emailExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserEmail == email, cancellation);

            if (emailExists)
                return BadRequest("Email already exists.");

            var surgeryTypeExists = await _context.SurgeryTypes
                .AsNoTracking()
                .AnyAsync(st => st.SurgeryTypeId == request.SurgeryTypeId, cancellation);

            if (!surgeryTypeExists)
                return BadRequest("Selected surgery type does not exist.");

            try
            {
                var user = new User
                {
                    UserRole = UserRole.Patient,
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

                var patient = new Patient
                {
                    User = user,
                    PatientObservations = string.IsNullOrWhiteSpace(request.PatientObservations)
                        ? null
                        : request.PatientObservations.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync(cancellation);

                var surgery = new Surgery
                {
                    PatientId = patient.PatientId,
                    SurgeryTypeId = request.SurgeryTypeId,
                    SurgeryDate = surgeryDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync(cancellation);

                return Ok(new CreatePatientResponseDto
                {
                    Message = "Patient created successfully.",
                    PatientId = patient.PatientId,
                    UserId = patient.User.UserId,
                    Name = patient.User.UserName,
                    Surname = patient.User.UserSurname,
                    Email = patient.User.UserEmail,
                    Role = patient.User.UserRole,
                    RoleName = patient.User.UserRole.ToString(),
                    Active = patient.User.UserActive,
                    CreatedAt = patient.User.CreatedAt,
                    Birthdate = patient.User.UserBirthdate,
                    Photo = patient.User.UserPhoto,
                    PatientObservations = patient.PatientObservations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient with email {Email}", request.Email);
                return StatusCode(500, "Error creating patient with email '" + request.Email + "'");
            }
        }

        [HttpPut("update/{patientId}")]
        public async Task<ActionResult<UpdatePatientResponseDto>> UpdatePatient(int patientId, [FromBody] UpdatePatientRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!request.SurgeryDate.HasValue)
                return BadRequest("Surgery date is required.");

            var surgeryDate = request.SurgeryDate.Value;

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                    .Include(p => p.Assignment)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId, cancellation);

                if (patient == null)
                    return NotFound("Patient not found.");

                var email = request.Email.Trim();

                var emailExists = await _context.Users
                    .AnyAsync(u => u.UserId != patient.UserId && u.UserEmail == email, cancellation);

                if (emailExists)
                    return BadRequest("Email already exists.");

                var surgeryTypeExists = await _context.SurgeryTypes
                    .AsNoTracking()
                    .AnyAsync(st => st.SurgeryTypeId == request.SurgeryTypeId, cancellation);

                if (!surgeryTypeExists)
                    return BadRequest("Selected surgery type does not exist.");

                if (!request.Active && patient.Assignment != null)
                    return BadRequest("This patient cannot be deactivated because there is still a nurse assigned.");

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    var passwordError = PasswordValidator.Validate(request.Password);

                    if (passwordError != null)
                        return BadRequest(passwordError);

                    patient.User.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                patient.User.UserName = request.Name.Trim();
                patient.User.UserSurname = request.Surname.Trim();
                patient.User.UserEmail = email;
                patient.User.UserBirthdate = request.Birthdate;
                patient.User.UserPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                patient.User.UserPhoto = request.Photo;
                patient.User.UserActive = request.Active;

                patient.PatientObservations = string.IsNullOrWhiteSpace(request.PatientObservations)
                    ? null
                    : request.PatientObservations.Trim();

                if (patient.Surgery == null)
                {
                    var surgery = new Surgery
                    {
                        PatientId = patient.PatientId,
                        SurgeryTypeId = request.SurgeryTypeId,
                        SurgeryDate = surgeryDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Surgeries.Add(surgery);
                    patient.Surgery = surgery;
                }
                else
                {
                    patient.Surgery.SurgeryTypeId = request.SurgeryTypeId;
                    patient.Surgery.SurgeryDate = surgeryDate;
                }

                await _context.SaveChangesAsync(cancellation);

                return Ok(new UpdatePatientResponseDto
                {
                    Message = "Patient updated successfully.",
                    PatientId = patient.PatientId,
                    UserId = patient.User.UserId,
                    Name = patient.User.UserName,
                    Surname = patient.User.UserSurname,
                    Email = patient.User.UserEmail,
                    Active = patient.User.UserActive,
                    Birthdate = patient.User.UserBirthdate,
                    Phone = patient.User.UserPhone,
                    Photo = patient.User.UserPhoto,
                    PatientObservations = patient.PatientObservations,
                    SurgeryTypeId = patient.Surgery.SurgeryTypeId,
                    SurgeryDate = patient.Surgery.SurgeryDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with id {PatientId}", patientId);
                return StatusCode(500, "Error updating patient with id '" + patientId + "'");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellation)
        {
            try
            {
                var patients = await _context.Patients
                    .AsNoTracking()
                    .Select(p => new
                    {
                        patientId = p.PatientId,
                        userId = p.User.UserId,
                        name = p.User.UserName,
                        surname = p.User.UserSurname,
                        email = p.User.UserEmail,
                        role = p.User.UserRole,
                        roleName = p.User.UserRole.ToString(),
                        active = p.User.UserActive,
                        createdAt = p.User.CreatedAt,
                        birthdate = p.User.UserBirthdate,
                        photo = p.User.UserPhoto,
                        patientObservations = p.PatientObservations
                    })
                    .OrderBy(p => p.name)
                    .ThenBy(p => p.surname)
                    .ToArrayAsync(cancellation);

                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting patients");
                return StatusCode(500, "Error consulting patients.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetPatientByIdResponseDto>> GetById(int id, CancellationToken cancellation)
        {
            try
            {
                var patient = await _context.Patients
                    .AsNoTracking()
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                        .ThenInclude(s => s.SurgeryType)
                    .Include(p => p.Assignment!.Nurse)
                        .ThenInclude(n => n.User)
                    .Include(p => p.Reports)
                    .FirstOrDefaultAsync(p => p.PatientId == id, cancellation);

                if (patient == null)
                    return NotFound("Patient not found.");

                var latestReport = patient.Reports
                    .OrderByDescending(r => r.ReportDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                var alertCount = latestReport?.ReportAlerts ?? 0;
                var status = GetPatientStatus(alertCount);

                var assignedNurse = patient.Assignment?.Nurse;

                return Ok(new GetPatientByIdResponseDto
                {
                    PatientId = patient.PatientId,
                    UserId = patient.UserId,
                    Name = patient.User.UserName,
                    Surname = patient.User.UserSurname,
                    Email = patient.User.UserEmail,
                    Birthdate = patient.User.UserBirthdate,
                    Phone = patient.User.UserPhone,
                    Photo = patient.User.UserPhoto,
                    Active = patient.User.UserActive,
                    PatientObservations = patient.PatientObservations,
                    SurgeryTypeId = patient.Surgery.SurgeryTypeId,
                    SurgeryName = patient.Surgery.SurgeryType.SurgeryTypeName,
                    SurgeryDate = patient.Surgery.SurgeryDate,

                    AssignedNurseId = assignedNurse?.NurseId,

                    AssignedNurseName = assignedNurse != null
                        ? (assignedNurse.User.UserName + " " + assignedNurse.User.UserSurname).Trim()
                        : null,

                    AssignedNursePhoto = assignedNurse?.User.UserPhoto,

                    AlertCount = alertCount,
                    Status = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting patient with id {PatientId}", id);
                return StatusCode(500, "Error consulting patient with id " + id);
            }
        }

        [HttpGet("patientsDetailed")]
        public async Task<ActionResult<GetPatientsDetailedResponseDto[]>> GetDetailed(CancellationToken cancellation)
        {
            try
            {
                var patients = await _context.Patients
                    .AsNoTracking()
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                        .ThenInclude(s => s.SurgeryType)
                    .Include(p => p.Reports)
                    .Include(p => p.Assignment!.Nurse)
                        .ThenInclude(n => n.User)
                    .ToArrayAsync(cancellation);

                var result = patients
                    .Select(p =>
                    {
                        var latestReport = p.Reports
                            .OrderByDescending(r => r.ReportDate)
                            .ThenByDescending(r => r.CreatedAt)
                            .FirstOrDefault();

                        var alertCount = latestReport?.ReportAlerts ?? 0;
                        var assignedNurse = p.Assignment?.Nurse;

                        return new GetPatientsDetailedResponseDto
                        {
                            PatientId = p.PatientId,
                            UserId = p.UserId,
                            Name = p.User.UserName,
                            Surname = p.User.UserSurname,
                            Email = p.User.UserEmail,
                            Phone = p.User.UserPhone,
                            Birthdate = p.User.UserBirthdate,
                            Age = CalculateAge(p.User.UserBirthdate),
                            Surgery = p.Surgery.SurgeryType.SurgeryTypeName,
                            SurgeryDate = p.Surgery.SurgeryDate,
                            AlertCount = alertCount,
                            Status = GetPatientStatus(alertCount),
                            Active = p.User.UserActive,
                            Photo = p.User.UserPhoto,
                            AssignedNurseId = assignedNurse?.NurseId,
                            AssignedNurseName = assignedNurse != null
                                ? $"{assignedNurse.User.UserName} {assignedNurse.User.UserSurname}".Trim()
                                : null
                        };
                    })
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Surname)
                    .ToArray();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting detailed patients.");
                return StatusCode(500, "Error consulting detailed patients.");
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