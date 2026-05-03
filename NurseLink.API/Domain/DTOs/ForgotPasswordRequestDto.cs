using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}