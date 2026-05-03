using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateAdminRequestDto
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

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}