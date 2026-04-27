using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class GetAssignedPatientsByNurseResponseDto
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public DateTime? Birthdate { get; set; }
        public int? Age { get; set; }
        public string Surgery { get; set; } = string.Empty;
        public DateTime? SurgeryDate { get; set; }
        public string? Phone { get; set; }
        public bool Active { get; set; }
        public int AlertCount { get; set; }
        public ReportStatus Status { get; set; }
    }
}