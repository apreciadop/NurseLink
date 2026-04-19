namespace NurseLink.API.Domain.DTOs
{
    public class GetUnassignedPatientResponseDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public string SurgeryTypeName { get; set; } = string.Empty;
        public DateTime SurgeryDate { get; set; }
    }
}