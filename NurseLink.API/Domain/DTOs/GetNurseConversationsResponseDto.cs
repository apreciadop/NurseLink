namespace NurseLink.API.Domain.DTOs
{
    public class GetNurseConversationsResponseDto
    {
        public int ConversationId { get; set; }
        public int NurseId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public string? PatientPhoto { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public bool? LastMessageSenderIsPatient { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}