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
    public class AdminController(NurseLinkDbContext context, ILogger<AdminController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<AdminController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreateAdminResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body is required.");

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
                    UserRole = UserRole.Admin,
                    UserName = request.Name.Trim(),
                    UserSurname = request.Surname.Trim(),
                    UserEmail = email,
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
                await _context.SaveChangesAsync(cancellation);

                return Ok(new CreateAdminResponseDto
                {
                    Message = "Administrator created successfully.",
                    AdminId = admin.AdminId,
                    UserId = admin.User.UserId,
                    Name = admin.User.UserName,
                    Surname = admin.User.UserSurname,
                    Email = admin.User.UserEmail,
                    Role = admin.User.UserRole,
                    RoleName = admin.User.UserRole.ToString(),
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
        public async Task<IActionResult> GetAll(CancellationToken cancellation)
        {
            try
            {
                var admins = await _context.Administrators
                    .AsNoTracking()
                    .Select(a => new
                    {
                        adminId = a.AdminId,
                        userId = a.User.UserId,
                        name = a.User.UserName,
                        surname = a.User.UserSurname,
                        email = a.User.UserEmail,
                        role = a.User.UserRole,
                        roleName = a.User.UserRole.ToString(),
                        active = a.User.UserActive,
                        createdAt = a.User.CreatedAt
                    })
                    .OrderBy(a => a.name)
                    .ThenBy(a => a.surname)
                    .ToArrayAsync(cancellation);

                return Ok(admins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting administrators");
                return StatusCode(500, "Error consulting administrators.");
            }
        }

        [HttpGet("dashboardKpis")]
        public async Task<ActionResult<GetAdminDashboardKpisResponseDto>> GetDashboardKpis(CancellationToken cancellation)
        {
            try
            {
                var totalPatients = await _context.Patients.CountAsync(cancellation);
                var totalNurses = await _context.Nurses.CountAsync(cancellation);

                var latestReports = _context.Reports
                    .AsNoTracking()
                    .Where(r => !_context.Reports.Any(r2 =>
                        r2.PatientId == r.PatientId &&
                        (
                            r2.ReportDate > r.ReportDate ||
                            (r2.ReportDate == r.ReportDate && r2.ReportId > r.ReportId)
                        )));

                var totalAlerts = await latestReports
                    .SumAsync(r =>
                        (r.ReportPain >= 7 ? 1 : 0) +
                        (r.ReportFever ? 1 : 0) +
                        (r.ReportBleeding ? 1 : 0) +
                        (r.ReportSwelling ? 1 : 0),
                        cancellation);

                var unassignedPatients = await _context.Patients
                    .CountAsync(p => !_context.Assignments.Any(a => a.PatientId == p.PatientId), cancellation);

                var response = new GetAdminDashboardKpisResponseDto
                {
                    TotalPatients = totalPatients,
                    TotalNurses = totalNurses,
                    TotalAlerts = totalAlerts,
                    UnassignedPatients = unassignedPatients
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard KPIs");
                return StatusCode(500, "Error retrieving dashboard KPIs.");
            }
        }

        [HttpGet("patientsWithAlerts")]
        public async Task<ActionResult<GetPatientWithAlertResponseDto[]>> GetPatientsWithAlerts(CancellationToken cancellation)
        {
            try
            {
                var latestReports = _context.Reports
                    .AsNoTracking()
                    .Where(r => !_context.Reports.Any(r2 =>
                        r2.PatientId == r.PatientId &&
                        (
                            r2.ReportDate > r.ReportDate ||
                            (r2.ReportDate == r.ReportDate && r2.ReportId > r.ReportId)
                        )));

                var patientsWithAlerts = await latestReports
                    .Select(r => new
                    {
                        Report = r,
                        AlertCount =
                            (r.ReportPain >= 7 ? 1 : 0) +
                            (r.ReportFever ? 1 : 0) +
                            (r.ReportBleeding ? 1 : 0) +
                            (r.ReportSwelling ? 1 : 0)
                    })
                    .Where(x => x.AlertCount > 0)
                    .OrderBy(x => x.Report.Patient.User.UserName)
                    .ThenBy(x => x.Report.Patient.User.UserSurname)
                    .ThenByDescending(x => x.Report.ReportDate)
                    .Select(x => new GetPatientWithAlertResponseDto
                    {
                        PatientId = x.Report.PatientId,
                        PatientName = x.Report.Patient.User.UserName,
                        PatientSurname = x.Report.Patient.User.UserSurname,
                        ReportDate = x.Report.ReportDate,
                        CreatedAt = x.Report.CreatedAt,
                        ReportStatus = x.Report.ReportStatus.ToString(),
                        NurseId = x.Report.NurseId!.Value,
                        NurseName = x.Report.Nurse!.User.UserName,
                        NurseSurname = x.Report.Nurse.User.UserSurname,
                        ReportPain = x.Report.ReportPain,
                        ReportFever = x.Report.ReportFever,
                        ReportBleeding = x.Report.ReportBleeding,
                        ReportSwelling = x.Report.ReportSwelling,
                        AlertCount = x.AlertCount
                    })
                    .ToArrayAsync(cancellation);

                return Ok(patientsWithAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients with alerts");
                return StatusCode(500, "Error retrieving patients with alerts.");
            }
        }

        [HttpGet("unassignedPatients")]
        public async Task<ActionResult<GetUnassignedPatientResponseDto[]>> GetUnassignedPatients(CancellationToken cancellation)
        {
            try
            {
                var unassignedPatients = await _context.Patients
                    .AsNoTracking()
                    .Where(p => !_context.Assignments.Any(a => a.PatientId == p.PatientId))
                    .OrderBy(p => p.User.UserName)
                    .ThenBy(p => p.User.UserSurname)
                    .Select(p => new GetUnassignedPatientResponseDto
                    {
                        PatientId = p.PatientId,
                        PatientName = p.User.UserName,
                        PatientSurname = p.User.UserSurname,
                        SurgeryTypeName = p.Surgery.SurgeryType.SurgeryTypeName,
                        SurgeryDate = p.Surgery.SurgeryDate
                    })
                    .ToArrayAsync(cancellation);

                return Ok(unassignedPatients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unassigned patients");
                return StatusCode(500, "Error retrieving unassigned patients.");
            }
        }
    }
}