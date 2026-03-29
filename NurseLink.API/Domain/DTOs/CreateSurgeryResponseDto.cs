namespace NurseLink.API.Domain.DTOs
{
    public class CreateSurgeryResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int SurgeryId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public int SurgeryTypeId { get; set; }
        public string SurgeryTypeName { get; set; } = string.Empty;
        public DateTime SurgeryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}