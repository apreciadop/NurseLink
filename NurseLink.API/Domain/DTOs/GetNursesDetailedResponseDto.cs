namespace NurseLink.API.Domain.DTOs
{
    public class GetNursesDetailedResponseDto
    {
        public int NurseId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool Active { get; set; }
        public int PatientCount { get; set; }
        public int AlertCount { get; set; }

        public string? Photo { get; set; }
    }
}