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
    public class ConversationsController(NurseLinkDbContext context, ILogger<ConversationsController> logger) : ControllerBase
    {
        private readonly NurseLinkDbContext _context = context;
        private readonly ILogger<ConversationsController> _logger = logger;

        [HttpGet("nurse/{nurseId}")]
        public async Task<ActionResult<GetNurseConversationsResponseDto[]>> GetNurseConversations(
            int nurseId,
            CancellationToken cancellation)
        {
            try
            {
                var nurseExists = await _context.Nurses
                    .AsNoTracking()
                    .AnyAsync(n => n.NurseId == nurseId, cancellation);

                if (!nurseExists)
                    return NotFound("Nurse not found.");

                var conversations = await _context.Conversations
                    .AsNoTracking()
                    .Where(c => c.NurseId == nurseId)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Messages)
                    .ToArrayAsync(cancellation);

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
                    .ToArray();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversations for nurse with id {NurseId}", nurseId);
                return StatusCode(500, "Error loading conversations for nurse with id " + nurseId);
            }
        }

        [HttpPost("getOrCreate")]
        public async Task<ActionResult<GetOrCreateConversationResponseDto>> GetOrCreateConversation([FromBody] GetOrCreateConversationRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            try
            {
                var assignmentExists = await _context.Assignments
                    .AsNoTracking()
                    .AnyAsync(a =>
                        a.NurseId == request.NurseId &&
                        a.PatientId == request.PatientId,
                        cancellation);

                if (!assignmentExists)
                    return BadRequest("This nurse and patient are not currently assigned.");

                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.PatientId == request.PatientId, cancellation);

                if (conversation != null && conversation.NurseId != request.NurseId)
                    return BadRequest("This patient already has a conversation with another nurse.");

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        NurseId = request.NurseId,
                        PatientId = request.PatientId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync(cancellation);
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
        public async Task<ActionResult<GetOrCreateConversationResponseDto>> GetOrCreateConversationForPatient(int patientId, CancellationToken cancellation)
        {
            try
            {
                var patientExists = await _context.Patients
                    .AsNoTracking()
                    .AnyAsync(p => p.PatientId == patientId, cancellation);

                if (!patientExists)
                    return NotFound("Patient not found.");

                var assignment = await _context.Assignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.PatientId == patientId, cancellation);

                if (assignment == null)
                    return BadRequest("This patient does not have an assigned nurse.");

                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.PatientId == patientId, cancellation);

                if (conversation != null && conversation.NurseId != assignment.NurseId)
                    return BadRequest("The existing conversation does not match the current assignment.");

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        PatientId = patientId,
                        NurseId = assignment.NurseId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync(cancellation);
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
        public async Task<ActionResult<GetConversationDetailResponseDto>> GetConversationDetail(int conversationId, CancellationToken cancellation)
        {
            try
            {
                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .Include(c => c.Nurse)
                        .ThenInclude(n => n.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellation);

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
        public async Task<ActionResult<GetConversationMessagesResponseDto>> GetConversationMessages(int conversationId, [FromQuery] int? nurseId, [FromQuery] int? patientId, CancellationToken cancellation)
        {
            try
            {
                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .Include(c => c.Nurse)
                        .ThenInclude(n => n.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellation);

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
        public async Task<ActionResult<SendMessageResponseDto>> SendMessage(int conversationId, [FromBody] SendMessageRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(request.MessageText))
                return BadRequest("Message text is required.");

            try
            {
                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellation);

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
                await _context.SaveChangesAsync(cancellation);

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
        public async Task<ActionResult<MarkConversationAsReadResponseDto>> MarkConversationAsRead(int conversationId, [FromBody] MarkConversationAsReadRequestDto request, CancellationToken cancellation)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            try
            {
                var conversation = await _context.Conversations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellation);

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
                    .ToArrayAsync(cancellation);

                foreach (var message in unreadMessages)
                    message.MessageRead = true;

                await _context.SaveChangesAsync(cancellation);

                return Ok(new MarkConversationAsReadResponseDto
                {
                    Message = "Conversation messages marked as read.",
                    ConversationId = conversationId,
                    UpdatedMessagesCount = unreadMessages.Length
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