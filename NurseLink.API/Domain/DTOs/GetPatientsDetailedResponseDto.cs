using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class GetPatientsDetailedResponseDto
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FullName => $"{Name} {Surname}".Trim();

        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? Birthdate { get; set; }
        public int? Age { get; set; }

        public string Surgery { get; set; } = string.Empty;
        public DateTime SurgeryDate { get; set; }

        public int AlertCount { get; set; }
        public ReportStatus Status { get; set; }

        public bool Active { get; set; }
        public string? Photo { get; set; }

        public int? AssignedNurseId { get; set; }
        public string? AssignedNurseName { get; set; }
    }
}