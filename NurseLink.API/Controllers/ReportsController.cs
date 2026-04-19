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
    public class ReportsController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(NurseLinkDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<GetReportResponseDto>>> GetReportsByPatient(int patientId)
        {
            try
            {
                var patientExists = await _context.Patients
                    .AnyAsync(p => p.PatientId == patientId);

                if (!patientExists)
                {
                    return NotFound("Patient not found.");
                }

                var reports = await _context.Reports
                    .Where(r => r.PatientId == patientId)
                    .OrderByDescending(r => r.ReportDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .Select(r => new GetReportResponseDto
                    {
                        ReportId = r.ReportId,
                        PatientId = r.PatientId,
                        ReportDate = r.ReportDate,
                        CreatedAt = r.CreatedAt,
                        PainLevel = r.ReportPain,
                        HasFever = r.ReportFever,
                        HasBleeding = r.ReportBleeding,
                        HasSwelling = r.ReportSwelling,
                        Observations = r.ReportObservations,
                        AlertCount = r.ReportAlerts,
                        Status = r.ReportStatus,
                        NurseId = r.NurseId,
                        NurseObservations = r.ReportNurseObservations
                    })
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports for patient {PatientId}", patientId);
                return StatusCode(500, "Error loading reports for patient with id " + patientId);
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateReportResponseDto>> CreateReport([FromBody] CreateReportRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Assignment)
                    .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);

                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                var alertCount = CalculateAlertCount(
                    request.PainLevel,
                    request.HasFever,
                    request.HasBleeding,
                    request.HasSwelling
                );

                var status = CalculateStatus(alertCount);

                var report = new Report
                {
                    PatientId = request.PatientId,
                    ReportDate = DateTime.UtcNow.Date,
                    ReportPain = request.PainLevel,
                    ReportFever = request.HasFever,
                    ReportBleeding = request.HasBleeding,
                    ReportSwelling = request.HasSwelling,
                    ReportObservations = string.IsNullOrWhiteSpace(request.Observations)
                        ? null
                        : request.Observations.Trim(),
                    ReportAlerts = alertCount,
                    ReportStatus = status,
                    NurseId = patient.Assignment?.NurseId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return Ok(new CreateReportResponseDto
                {
                    Message = "Report created successfully.",
                    ReportId = report.ReportId,
                    PatientId = report.PatientId,
                    ReportDate = report.ReportDate,
                    CreatedAt = report.CreatedAt,
                    PainLevel = report.ReportPain,
                    HasFever = report.ReportFever,
                    HasBleeding = report.ReportBleeding,
                    HasSwelling = report.ReportSwelling,
                    Observations = report.ReportObservations,
                    AlertCount = report.ReportAlerts,
                    Status = report.ReportStatus,
                    NurseId = report.NurseId
                });
            }
            catch (Exception ex)
            {
                var patientId = request?.PatientId ?? 0;

                _logger.LogError(ex, "Error creating report for patient {PatientId}", patientId);
                return StatusCode(500, "Error creating report for patient with id " + patientId);
            }
        }

        private static int CalculateAlertCount(
            int painLevel,
            bool hasFever,
            bool hasBleeding,
            bool hasSwelling)
        {
            var alerts = 0;

            /*
             * Alert calculation rule:
             *
             * Each symptom that exceeds the established threshold generates one alert.
             * The total number of alerts is later used to determine the recovery status.
             *
             * Current rules:
             * - Pain level greater than or equal to 7 generates 1 alert.
             * - Fever reported by the patient generates 1 alert.
             * - Bleeding reported by the patient generates 1 alert.
             * - Swelling reported by the patient generates 1 alert.
             *
             * Example:
             * Pain level 8 + fever + bleeding = 3 alerts.
             */

            if (painLevel >= 7)
            {
                alerts++;
            }

            if (hasFever)
            {
                alerts++;
            }

            if (hasBleeding)
            {
                alerts++;
            }

            if (hasSwelling)
            {
                alerts++;
            }

            return alerts;
        }

        private static ReportStatus CalculateStatus(int alertCount)
        {
            /*
             * Recovery status rule:
             *
             * - 0 alerts: Stable
             * - 1 or 2 alerts: Warning
             * - More than 2 alerts: Alert
             */

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
    }
}