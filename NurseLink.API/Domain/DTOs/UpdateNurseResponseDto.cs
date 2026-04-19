namespace NurseLink.API.Domain.DTOs
{
    public class UpdateNurseResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int NurseId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
    }
}