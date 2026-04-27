using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class UpdateNurseRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public DateTime? Birthdate { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }
        public string? Photo { get; set; }
        public bool Active { get; set; }
    }
}