using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;

namespace NurseLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurgeriesController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<SurgeriesController> _logger;

        public SurgeriesController(NurseLinkDbContext context, ILogger<SurgeriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateSurgeryResponseDto>> Create([FromBody] CreateSurgeryRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid data. Please check required fields.");

            if (request.PatientId <= 0 || request.SurgeryTypeId <= 0 || request.SurgeryDate == default)
                return BadRequest("Patient, SurgeryType and SurgeryDate are required.");

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);

            if (patient == null)
                return BadRequest("Patient does not exist.");

            var surgeryType = await _context.SurgeryTypes
                .FirstOrDefaultAsync(s => s.SurgeryTypeId == request.SurgeryTypeId);

            if (surgeryType == null)
                return BadRequest("Surgery type does not exist.");

            var patientAlreadyHasSurgery = await _context.Surgeries
                .AnyAsync(s => s.PatientId == request.PatientId);

            if (patientAlreadyHasSurgery)
                return BadRequest("This patient already has a surgery assigned.");

            try
            {
                var surgery = new Surgery
                {
                    PatientId = request.PatientId,
                    SurgeryTypeId = request.SurgeryTypeId,
                    SurgeryDate = request.SurgeryDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync();

                return Ok(new CreateSurgeryResponseDto
                {
                    Message = "Surgery created successfully.",
                    SurgeryId = surgery.SurgeryId,
                    PatientId = patient.PatientId,
                    PatientName = patient.User.UserName,
                    PatientSurname = patient.User.UserSurname,
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
                    request.PatientId,
                    request.SurgeryTypeId);

                return StatusCode(
                    500,
                    "Error creating surgery for PatientId " + request.PatientId + " and SurgeryTypeId " + request.SurgeryTypeId + "."
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var surgeries = await _context.Surgeries
                    .Include(s => s.Patient)
                        .ThenInclude(p => p.User)
                    .Include(s => s.SurgeryType)
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
                    .ToListAsync();

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