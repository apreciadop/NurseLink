using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;

namespace NurseLink.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SurgeriesController(NurseLinkDbContext context, ILogger<SurgeriesController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<SurgeriesController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreateSurgeryResponseDto>> Create([FromBody] CreateSurgeryRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!request.SurgeryDate.HasValue)
                return BadRequest("Surgery date is required.");

            var patientId = request.PatientId;
            var surgeryTypeId = request.SurgeryTypeId;
            var surgeryDate = request.SurgeryDate.Value;

            try
            {
                var patient = await _context.Patients
                    .AsNoTracking()
                    .Where(p => p.PatientId == patientId)
                    .Select(p => new
                    {
                        p.PatientId,
                        p.User.UserName,
                        p.User.UserSurname
                    })
                    .FirstOrDefaultAsync(cancellation);

                if (patient == null)
                    return BadRequest("Patient does not exist.");

                var surgeryType = await _context.SurgeryTypes
                    .AsNoTracking()
                    .Where(s => s.SurgeryTypeId == surgeryTypeId)
                    .Select(s => new
                    {
                        s.SurgeryTypeId,
                        s.SurgeryTypeName
                    })
                    .FirstOrDefaultAsync(cancellation);

                if (surgeryType == null)
                    return BadRequest("Surgery type does not exist.");

                var patientAlreadyHasSurgery = await _context.Surgeries
                    .AsNoTracking()
                    .AnyAsync(s => s.PatientId == patientId, cancellation);

                if (patientAlreadyHasSurgery)
                    return BadRequest("This patient already has a surgery assigned.");

                var surgery = new Surgery
                {
                    PatientId = patientId,
                    SurgeryTypeId = surgeryTypeId,
                    SurgeryDate = surgeryDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync(cancellation);

                return Ok(new CreateSurgeryResponseDto
                {
                    Message = "Surgery created successfully.",
                    SurgeryId = surgery.SurgeryId,
                    PatientId = patient.PatientId,
                    PatientName = patient.UserName,
                    PatientSurname = patient.UserSurname,
                    SurgeryTypeId = surgeryType.SurgeryTypeId,
                    SurgeryTypeName = surgeryType.SurgeryTypeName,
                    SurgeryDate = surgery.SurgeryDate,
                    CreatedAt = surgery.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating surgery for PatientId {PatientId} and SurgeryTypeId {SurgeryTypeId}",
                    patientId,
                    surgeryTypeId);

                return StatusCode(500, "Error creating surgery for PatientId " + patientId + " and SurgeryTypeId " + surgeryTypeId + ".");
            }
        }

        [HttpGet]
        public async Task<ActionResult<object[]>> GetAll(CancellationToken cancellation)
        {
            try
            {
                var surgeries = await _context.Surgeries
                    .AsNoTracking()
                    .Select(s => new
                    {
                        surgeryId = s.SurgeryId,
                        patientId = s.PatientId,
                        patientName = s.Patient.User.UserName,
                        patientSurname = s.Patient.User.UserSurname,
                        surgeryTypeId = s.SurgeryTypeId,
                        surgeryTypeName = s.SurgeryType.SurgeryTypeName,
                        surgeryDate = s.SurgeryDate,
                        createdAt = s.CreatedAt
                    })
                    .OrderBy(s => s.patientName)
                    .ThenBy(s => s.patientSurname)
                    .ThenByDescending(s => s.surgeryDate)
                    .ToArrayAsync(cancellation);

                return Ok(surgeries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting surgeries");
                return StatusCode(500, "Error consulting surgeries.");
            }
        }
    }
}