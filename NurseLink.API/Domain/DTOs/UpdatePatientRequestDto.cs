using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class UpdatePatientRequestDto
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

        [MaxLength(255)]
        public string? Password { get; set; }

        public bool Active { get; set; } = true;

        [Required]
        public DateTime? Birthdate { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        public string? Photo { get; set; }

        [MaxLength(1000)]
        public string? PatientObservations { get; set; }

        [Required]
        public int? SurgeryTypeId { get; set; }

        [Required]
        public DateTime? SurgeryDate { get; set; }
    }
}