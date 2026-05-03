using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class UpdateNurseRequestDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(150)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        public string? Photo { get; set; }

        public bool Active { get; set; }
    }
}