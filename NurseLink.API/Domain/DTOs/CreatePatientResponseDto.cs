using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class CreatePatientResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public int PatientId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? Birthdate { get; set; }
        public string? Photo { get; set; }

        public string? PatientObservations { get; set; }
    }
}