namespace NurseLink.API.Domain.DTOs
{
    public class GetConversationMessagesResponseDto
    {
        public int ConversationId { get; set; }

        public int NurseId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public string NurseSurname { get; set; } = string.Empty;

        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;

        public List<GetMessageResponseDto> Messages { get; set; } = new();
    }
}