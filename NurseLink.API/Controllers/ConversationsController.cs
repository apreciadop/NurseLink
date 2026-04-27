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
    public class ConversationsController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(NurseLinkDbContext context, ILogger<ConversationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("nurse/{nurseId}")]
        public async Task<ActionResult<List<GetNurseConversationsResponseDto>>> GetNurseConversations(int nurseId)
        {
            try
            {
                var nurseExists = await _context.Nurses
                    .AnyAsync(n => n.NurseId == nurseId);

                if (!nurseExists)
                    return NotFound("Nurse not found.");

                var conversations = await _context.Conversations
                    .Where(c => c.NurseId == nurseId)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Messages)
                    .ToListAsync();

                var result = conversations
                    .Select(c =>
                    {
                        var lastMessage = c.Messages
                            .OrderByDescending(m => m.MessageDate)
                            .ThenByDescending(m => m.CreatedAt)
                            .FirstOrDefault();

                        var unreadCount = c.Messages
                            .Count(m => !m.MessageRead && m.MessageSender == 1);

                        return new GetNurseConversationsResponseDto
                        {
                            ConversationId = c.ConversationId,
                            NurseId = c.NurseId,
                            PatientId = c.PatientId,
                            PatientName = c.Patient.User.UserName,
                            PatientSurname = c.Patient.User.UserSurname,
                            PatientPhoto = c.Patient.User.UserPhoto,
                            LastMessage = lastMessage?.MessageText,
                            LastMessageDate = lastMessage?.MessageDate,
                            LastMessageSenderIsPatient = lastMessage != null ? lastMessage.MessageSender == 1 : null,
                            UnreadCount = unreadCount,
                            CreatedAt = c.CreatedAt
                        };
                    })
                    .OrderByDescending(c => c.LastMessageDate ?? c.CreatedAt)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversations for nurse with id {NurseId}", nurseId);
                return StatusCode(500, "Error loading conversations for nurse with id " + nurseId);
            }
        }

        [HttpPost("getOrCreate")]
        public async Task<ActionResult<GetOrCreateConversationResponseDto>> GetOrCreateConversation(
            [FromBody] GetOrCreateConversationRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.NurseId <= 0 || request.PatientId <= 0)
                return BadRequest("NurseId and PatientId must be greater than 0.");

            try
            {
                var assignmentExists = await _context.Assignments
                    .AnyAsync(a => a.NurseId == request.NurseId && a.PatientId == request.PatientId);

                if (!assignmentExists)
                    return BadRequest("This nurse and patient are not currently assigned.");

                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c =>
                        c.NurseId == request.NurseId &&
                        c.PatientId == request.PatientId);

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        NurseId = request.NurseId,
                        PatientId = request.PatientId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();
                }

                return Ok(new GetOrCreateConversationResponseDto
                {
                    Message = "Conversation ready.",
                    ConversationId = conversation.ConversationId,
                    NurseId = conversation.NurseId,
                    PatientId = conversation.PatientId,
                    CreatedAt = conversation.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating conversation for nurse {NurseId} and patient {PatientId}", request.NurseId, request.PatientId);

                return StatusCode(500, "Error getting or creating conversation for nurse " + request.NurseId + " and patient " + request.PatientId + ".");
            }
        }

        [HttpGet("patient/{patientId}/getOrCreate")]
        public async Task<ActionResult<GetOrCreateConversationResponseDto>> GetOrCreateConversationForPatient(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                    return NotFound("Patient not found.");

                var assignment = await _context.Assignments
                    .FirstOrDefaultAsync(a => a.PatientId == patientId);

                if (assignment == null)
                    return BadRequest("This patient does not have an assigned nurse.");

                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c =>
                        c.PatientId == patientId &&
                        c.NurseId == assignment.NurseId);

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        PatientId = patientId,
                        NurseId = assignment.NurseId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();
                }

                return Ok(new GetOrCreateConversationResponseDto
                {
                    Message = "Conversation ready.",
                    ConversationId = conversation.ConversationId,
                    NurseId = conversation.NurseId,
                    PatientId = conversation.PatientId,
                    CreatedAt = conversation.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating conversation for patient with id {PatientId}", patientId);
                return StatusCode(500, "Error getting or creating conversation for patient with id " + patientId);
            }
        }

        [HttpGet("{conversationId}")]
        public async Task<ActionResult<GetConversationDetailResponseDto>> GetConversationDetail(int conversationId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Nurse)
                        .ThenInclude(n => n.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation == null)
                    return NotFound("Conversation not found.");

                return Ok(new GetConversationDetailResponseDto
                {
                    ConversationId = conversation.ConversationId,
                    NurseId = conversation.NurseId,
                    NurseName = conversation.Nurse.User.UserName,
                    NurseSurname = conversation.Nurse.User.UserSurname,
                    NursePhoto = conversation.Nurse.User.UserPhoto,
                    PatientId = conversation.PatientId,
                    PatientName = conversation.Patient.User.UserName,
                    PatientSurname = conversation.Patient.User.UserSurname,
                    PatientPhoto = conversation.Patient.User.UserPhoto,
                    CreatedAt = conversation.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversation detail for id {ConversationId}", conversationId);
                return StatusCode(500, "Error loading conversation detail for id " + conversationId);
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<GetConversationMessagesResponseDto>> GetConversationMessages(
            int conversationId,
            [FromQuery] int? nurseId,
            [FromQuery] int? patientId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Nurse)
                        .ThenInclude(n => n.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation == null)
                    return NotFound("Conversation not found.");

                var hasAccess =
                    (nurseId.HasValue && conversation.NurseId == nurseId.Value) ||
                    (patientId.HasValue && conversation.PatientId == patientId.Value);

                if (!hasAccess)
                    return Forbid();

                var messages = conversation.Messages
                    .OrderBy(m => m.MessageDate)
                    .ThenBy(m => m.CreatedAt)
                    .Select(m => new GetMessageResponseDto
                    {
                        MessageId = m.MessageId,
                        ConversationId = m.ConversationId,
                        MessageDate = m.MessageDate,
                        MessageSenderIsPatient = m.MessageSender == 1,
                        MessageText = m.MessageText,
                        MessageRead = m.MessageRead,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList();

                return Ok(new GetConversationMessagesResponseDto
                {
                    ConversationId = conversation.ConversationId,
                    NurseId = conversation.NurseId,
                    NurseName = conversation.Nurse.User.UserName,
                    NurseSurname = conversation.Nurse.User.UserSurname,
                    PatientId = conversation.PatientId,
                    PatientName = conversation.Patient.User.UserName,
                    PatientSurname = conversation.Patient.User.UserSurname,
                    Messages = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages for conversation with id {ConversationId}", conversationId);
                return StatusCode(500, "Error loading messages for conversation with id " + conversationId);
            }
        }

        [HttpPost("{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponseDto>> SendMessage(
            int conversationId,
            [FromBody] SendMessageRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.MessageText))
                return BadRequest("Message text is required.");

            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation == null)
                    return NotFound("Conversation not found.");

                var hasAccess =
                    (request.NurseId.HasValue && conversation.NurseId == request.NurseId.Value) ||
                    (request.PatientId.HasValue && conversation.PatientId == request.PatientId.Value);

                if (!hasAccess)
                    return Forbid();

                if (request.NurseId.HasValue && request.MessageSenderIsPatient)
                    return BadRequest("Invalid sender for nurse.");

                if (request.PatientId.HasValue && !request.MessageSenderIsPatient)
                    return BadRequest("Invalid sender for patient.");

                var message = new Message
                {
                    ConversationId = conversationId,
                    MessageDate = DateTime.UtcNow,
                    MessageSender = request.MessageSenderIsPatient ? 1 : 0,
                    MessageText = request.MessageText.Trim(),
                    MessageRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new SendMessageResponseDto
                {
                    Message = "Message sent successfully.",
                    MessageData = new GetMessageResponseDto
                    {
                        MessageId = message.MessageId,
                        ConversationId = message.ConversationId,
                        MessageDate = message.MessageDate,
                        MessageSenderIsPatient = message.MessageSender == 1,
                        MessageText = message.MessageText,
                        MessageRead = message.MessageRead,
                        CreatedAt = message.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message for conversation with id {ConversationId}", conversationId);
                return StatusCode(500, "Error sending message for conversation with id " + conversationId);
            }
        }

        [HttpPut("{conversationId}/read")]
        public async Task<ActionResult<MarkConversationAsReadResponseDto>> MarkConversationAsRead(
            int conversationId,
            [FromBody] MarkConversationAsReadRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation == null)
                    return NotFound("Conversation not found.");

                var hasAccess =
                    (request.NurseId.HasValue && conversation.NurseId == request.NurseId.Value) ||
                    (request.PatientId.HasValue && conversation.PatientId == request.PatientId.Value);

                if (!hasAccess)
                    return Forbid();

                var senderToMarkAsRead = request.ReaderIsPatient ? 0 : 1;

                var unreadMessages = await _context.Messages
                    .Where(m =>
                        m.ConversationId == conversationId &&
                        m.MessageSender == senderToMarkAsRead &&
                        !m.MessageRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                    message.MessageRead = true;

                await _context.SaveChangesAsync();

                return Ok(new MarkConversationAsReadResponseDto
                {
                    Message = "Conversation messages marked as read.",
                    ConversationId = conversationId,
                    UpdatedMessagesCount = unreadMessages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for conversation with id {ConversationId}", conversationId);
                return StatusCode(500, "Error marking messages as read for conversation with id " + conversationId);
            }
        }
    }
}