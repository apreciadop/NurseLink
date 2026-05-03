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
    public class SurgeryTypeController(NurseLinkDbContext context, ILogger<SurgeryTypeController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<SurgeryTypeController> _logger = logger;

        [HttpPost("create")]
        public async Task<ActionResult<CreateSurgeryTypeResponseDto>> Create([FromBody] CreateSurgeryTypeRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body required.");

            var name = request.Name.Trim();

            var exists = await _context.SurgeryTypes
                .AsNoTracking()
                .AnyAsync(s => s.SurgeryTypeName == name, cancellation);

            if (exists)
                return BadRequest("Surgery type already exists.");

            try
            {
                var surgeryType = new SurgeryType
                {
                    SurgeryTypeName = name,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SurgeryTypes.Add(surgeryType);
                await _context.SaveChangesAsync(cancellation);

                return Ok(new CreateSurgeryTypeResponseDto
                {
                    Message = "Surgery type created successfully.",
                    SurgeryTypeId = surgeryType.SurgeryTypeId,
                    Name = surgeryType.SurgeryTypeName,
                    CreatedAt = surgeryType.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating surgery type with name {Name}", request.Name);
                return StatusCode(500, "Error creating surgery type: " + request.Name + ".");
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetSurgeryTypeResponseDto[]>> GetAll(CancellationToken cancellation)
        {
            try
            {
                var surgeryTypes = await _context.SurgeryTypes
                    .AsNoTracking()
                    .Select(s => new GetSurgeryTypeResponseDto
                    {
                        SurgeryTypeId = s.SurgeryTypeId,
                        Name = s.SurgeryTypeName
                    })
                    .OrderBy(s => s.Name)
                    .ToArrayAsync(cancellation);

                return Ok(surgeryTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting surgery types");
                return StatusCode(500, "Error consulting surgery types.");
            }
        }
    }
}