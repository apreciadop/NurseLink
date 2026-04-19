using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreatePatientRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [RegularExpression(
            @"^(?=.{6,255}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character."
        )]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        public string? Photo { get; set; }

        [MaxLength(1000)]
        public string? PatientObservations { get; set; }

        [Required]
        public int? SurgeryTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? SurgeryDate { get; set; }
    }
}