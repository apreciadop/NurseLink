namespace NurseLink.API.Domain.DTOs
{
    public class MarkConversationAsReadResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public int UpdatedMessagesCount { get; set; }
    }
}