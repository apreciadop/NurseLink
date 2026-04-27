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
    public class AssignmentsController(NurseLinkDbContext context, ILogger<AssignmentsController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<AssignmentsController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreateAssignmentResponseDto>> Create([FromBody] CreateAssignmentRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid data. Please check required fields.");

            if (request.PatientId <= 0 || request.NurseId <= 0)
                return BadRequest("PatientId and NurseId must be greater than 0.");

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);

            if (patient == null)
                return NotFound("Patient not found.");

            var nurse = await _context.Nurses
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NurseId == request.NurseId);

            if (nurse == null)
                return NotFound("Nurse not found.");

            var assignmentExists = await _context.Assignments
                .AnyAsync(a => a.PatientId == request.PatientId);

            if (assignmentExists)
                return BadRequest("This patient is already assigned.");

            try
            {
                var assignment = new Assignment
                {
                    PatientId = request.PatientId,
                    NurseId = request.NurseId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                return Ok(new CreateAssignmentResponseDto
                {
                    Message = "Assignment created successfully.",
                    AssignmentId = assignment.AssignmentId,
                    PatientId = patient.PatientId,
                    PatientName = patient.User.UserName,
                    PatientSurname = patient.User.UserSurname,
                    NurseId = nurse.NurseId,
                    NurseName = nurse.User.UserName,
                    NurseSurname = nurse.User.UserSurname,
                    CreatedAt = assignment.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment for patient {PatientId} and nurse {NurseId}", request.PatientId, request.NurseId);
                return StatusCode(500, "Error creating assignment for patient " + request.PatientId + " and nurse " + request.NurseId + ".");
            }
        }

        [HttpDelete("patient/{patientId}")]
        public async Task<ActionResult<DeleteAssignmentResponseDto>> DeleteByPatient(int patientId)
        {
            if (patientId <= 0)
                return BadRequest("PatientId must be greater than 0.");

            try
            {
                var assignment = await _context.Assignments
                    .FirstOrDefaultAsync(a => a.PatientId == patientId);

                if (assignment == null)
                    return NotFound("Assignment not found for this patient.");

                var hasReports = await _context.Reports
                    .AnyAsync(r => r.PatientId == assignment.PatientId);

                if (hasReports)
                    return BadRequest("This patient cannot be unassigned because there are already symptom reports for this patient.");

                var hasConversation = await _context.Conversations
                    .AnyAsync(c =>
                        c.PatientId == assignment.PatientId &&
                        c.NurseId == assignment.NurseId);

                if (hasConversation)
                    return BadRequest("This patient cannot be unassigned because there is an existing conversation with the assigned nurse.");

                var response = new DeleteAssignmentResponseDto
                {
                    Message = "Patient unassigned successfully.",
                    AssignmentId = assignment.AssignmentId,
                    PatientId = assignment.PatientId,
                    NurseId = assignment.NurseId
                };

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment for patient with id {PatientId}", patientId);
                return StatusCode(500, "Error deleting assignment for patient with id " + patientId);
            }
        }
    }
}