namespace NurseLink.API.Domain.DTOs
{
    public class GetConversationDetailResponseDto
    {
        public int ConversationId { get; set; }
        public int NurseId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public string NurseSurname { get; set; } = string.Empty;
        public string? NursePhoto { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public string? PatientPhoto { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}