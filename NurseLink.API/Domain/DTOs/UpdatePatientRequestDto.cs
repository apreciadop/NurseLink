using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class UpdatePatientRequestDto
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

        public bool Active { get; set; } = true;

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        public string? Photo { get; set; }

        [MaxLength(1000)]
        public string? PatientObservations { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SurgeryTypeId must be greater than 0.")]
        public int SurgeryTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? SurgeryDate { get; set; }
    }
}