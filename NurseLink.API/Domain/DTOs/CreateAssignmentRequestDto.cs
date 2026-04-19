namespace NurseLink.API.Domain.DTOs
{
    public class CreateAssignmentRequestDto
    {
        public int PatientId { get; set; }
        public int NurseId { get; set; }
    }
}