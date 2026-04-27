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
    public class SurgeryTypeController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<SurgeryTypeController> _logger;

        public SurgeryTypeController(NurseLinkDbContext context, ILogger<SurgeryTypeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateSurgeryTypeResponseDto>> Create([FromBody] CreateSurgeryTypeRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Surgery type name is required.");

            var normalizedName = request.Name.Trim();

            var exists = await _context.SurgeryTypes
                .AnyAsync(s => s.SurgeryTypeName == normalizedName);

            if (exists)
                return BadRequest("Surgery type already exists.");

            try
            {
                var surgeryType = new SurgeryType
                {
                    SurgeryTypeName = normalizedName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SurgeryTypes.Add(surgeryType);
                await _context.SaveChangesAsync();

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
                return StatusCode(500, "Error creating surgery type.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSurgeryTypeResponseDto>>> GetAll()
        {
            try
            {
                var surgeryTypes = await _context.SurgeryTypes
                    .Select(s => new GetSurgeryTypeResponseDto
                    {
                        SurgeryTypeId = s.SurgeryTypeId,
                        Name = s.SurgeryTypeName
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync();

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