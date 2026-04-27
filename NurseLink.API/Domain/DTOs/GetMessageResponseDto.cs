namespace NurseLink.API.Domain.DTOs
{
    public class GetMessageResponseDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public DateTime MessageDate { get; set; }
        public bool MessageSenderIsPatient { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public bool MessageRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}