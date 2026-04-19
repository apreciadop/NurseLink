using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class GetPatientByIdResponseDto
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DateTime? Birthdate { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
        public bool Active { get; set; }

        public string? PatientObservations { get; set; }

        public int SurgeryTypeId { get; set; }
        public string? SurgeryName { get; set; }
        public DateTime SurgeryDate { get; set; }

        public int? AssignedNurseId { get; set; }
        public string? AssignedNurseName { get; set; }
        public string? AssignedNursePhoto { get; set; }

        public int AlertCount { get; set; }
        public ReportStatus Status { get; set; }
    }
}