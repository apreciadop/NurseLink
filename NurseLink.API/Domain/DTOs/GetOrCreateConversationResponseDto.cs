namespace NurseLink.API.Domain.DTOs
{
    public class GetOrCreateConversationResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public int NurseId { get; set; }
        public int PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}