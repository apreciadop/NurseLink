namespace NurseLink.API.Domain.DTOs
{
    public class CreateNurseResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public int NurseId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string? Photo { get; set; }
    }
}