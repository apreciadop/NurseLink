namespace NurseLink.API.Domain.DTOs
{
    public class DeleteAssignmentResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int AssignmentId { get; set; }
        public int PatientId { get; set; }
        public int NurseId { get; set; }
    }
}