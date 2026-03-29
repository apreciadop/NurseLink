namespace NurseLink.API.Domain.DTOs
{
    public class CreateAdminResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int AdminId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}