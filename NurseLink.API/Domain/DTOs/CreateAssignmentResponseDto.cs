namespace NurseLink.API.Domain.DTOs
{
    public class CreateAssignmentResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int AssignmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public int NurseId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public string NurseSurname { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}