using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;
using NurseLink.API.Domain.Enums;
using System.Text.RegularExpressions;

namespace NurseLink.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(NurseLinkDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreatePatientResponseDto>> CreatePatient([FromBody] CreatePatientRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Surname) || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) || !request.Birthdate.HasValue || !request.SurgeryTypeId.HasValue || !request.SurgeryDate.HasValue)
                return BadRequest("Name, surname, birthdate, email, password, surgery type and surgery date are required.");

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await _context.Users
                .AnyAsync(u => u.UserEmail.ToLower() == normalizedEmail);

            if (emailExists)
                return BadRequest("Email already exists.");

            var surgeryTypeExists = await _context.SurgeryTypes
                .AnyAsync(st => st.SurgeryTypeId == request.SurgeryTypeId.Value);

            if (!surgeryTypeExists)
                return BadRequest("Selected surgery type does not exist.");

            try
            {
                var user = new User
                {
                    UserRole = UserRole.Patient,
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

                var patient = new Patient
                {
                    User = user,
                    PatientObservations = string.IsNullOrWhiteSpace(request.PatientObservations)
                        ? null
                        : request.PatientObservations.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var surgery = new Surgery
                {
                    PatientId = patient.PatientId,
                    SurgeryTypeId = request.SurgeryTypeId.Value,
                    SurgeryDate = request.SurgeryDate.Value,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync();

                return Ok(new CreatePatientResponseDto
                {
                    Message = "Patient created successfully.",
                    PatientId = patient.PatientId,
                    UserId = patient.User.UserId,
                    Name = patient.User.UserName,
                    Surname = patient.User.UserSurname,
                    Email = patient.User.UserEmail,
                    Role = patient.User.UserRole,
                    RoleName = ((UserRole)patient.User.UserRole).ToString(),
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
        public async Task<ActionResult<UpdatePatientResponseDto>> UpdatePatient(int patientId, [FromBody] UpdatePatientRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Surname) || string.IsNullOrWhiteSpace(request.Email) ||
                !request.Birthdate.HasValue || !request.SurgeryTypeId.HasValue || !request.SurgeryDate.HasValue)
                return BadRequest("Name, surname, birthdate, email, surgery type and surgery date are required.");

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password.Length < 6 || request.Password.Length > 255)
                    return BadRequest("Password must be between 6 and 255 characters.");

                var passwordRegex = new Regex(
                    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$"
                );

                if (!passwordRegex.IsMatch(request.Password))
                    return BadRequest("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.");
            }

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                    .Include(p => p.Assignment)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                    return NotFound("Patient not found.");

                var normalizedEmail = request.Email.Trim().ToLower();

                var emailExists = await _context.Users
                    .AnyAsync(u => u.UserId != patient.UserId && u.UserEmail.ToLower() == normalizedEmail);

                if (emailExists)
                    return BadRequest("Email already exists.");

                var surgeryTypeExists = await _context.SurgeryTypes
                    .AnyAsync(st => st.SurgeryTypeId == request.SurgeryTypeId.Value);

                if (!surgeryTypeExists)
                    return BadRequest("Selected surgery type does not exist.");

                if (!request.Active && patient.Assignment != null)
                    return BadRequest("This patient cannot be deactivated because there is still a nurse assigned.");

                patient.User.UserName = request.Name.Trim();
                patient.User.UserSurname = request.Surname.Trim();
                patient.User.UserEmail = normalizedEmail;
                patient.User.UserBirthdate = request.Birthdate;
                patient.User.UserPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                patient.User.UserPhoto = request.Photo;
                patient.User.UserActive = request.Active;

                if (!string.IsNullOrWhiteSpace(request.Password))
                    patient.User.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                patient.PatientObservations = string.IsNullOrWhiteSpace(request.PatientObservations)
                    ? null
                    : request.PatientObservations.Trim();

                if (patient.Surgery == null)
                {
                    patient.Surgery = new Surgery
                    {
                        PatientId = patient.PatientId,
                        SurgeryTypeId = request.SurgeryTypeId.Value,
                        SurgeryDate = request.SurgeryDate.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    patient.Surgery.SurgeryTypeId = request.SurgeryTypeId.Value;
                    patient.Surgery.SurgeryDate = request.SurgeryDate.Value;
                }

                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new
                    {
                        patientId = p.PatientId,
                        userId = p.User.UserId,
                        name = p.User.UserName,
                        surname = p.User.UserSurname,
                        email = p.User.UserEmail,
                        role = p.User.UserRole,
                        roleName = ((UserRole)p.User.UserRole).ToString(),
                        active = p.User.UserActive,
                        createdAt = p.User.CreatedAt,
                        birthdate = p.User.UserBirthdate,
                        photo = p.User.UserPhoto,
                        patientObservations = p.PatientObservations
                    })
                    .ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting patients");
                return StatusCode(500, "Error consulting patients.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetPatientByIdResponseDto>> GetById(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                        .ThenInclude(s => s.SurgeryType)
                    .Include(p => p.Assignment)
                        .ThenInclude(a => a.Nurse)
                            .ThenInclude(n => n.User)
                    .Include(p => p.Reports)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null)
                    return NotFound("Patient not found.");

                var latestReport = patient.Reports
                    .OrderByDescending(r => r.ReportDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                var alertCount = latestReport?.ReportAlerts ?? 0;
                var status = GetPatientStatus(alertCount);

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
                    AssignedNurseId = patient.Assignment != null
                        ? patient.Assignment.NurseId
                        : null,

                    AssignedNurseName = patient.Assignment != null
                        ? (patient.Assignment.Nurse.User.UserName + " " + patient.Assignment.Nurse.User.UserSurname).Trim()
                        : null,

                    AssignedNursePhoto = patient.Assignment != null
                        ? patient.Assignment.Nurse.User.UserPhoto
                        : null,

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
        public async Task<ActionResult<List<GetPatientsDetailedResponseDto>>> GetDetailed()
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Surgery)
                        .ThenInclude(s => s.SurgeryType)
                    .Include(p => p.Reports)
                    .Include(p => p.Assignment)
                        .ThenInclude(a => a.Nurse)
                            .ThenInclude(n => n.User)
                    .ToListAsync();

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
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .ToList();

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