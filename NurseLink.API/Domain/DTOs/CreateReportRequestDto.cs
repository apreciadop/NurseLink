using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateReportRequestDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        [Range(0, 10)]
        public int PainLevel { get; set; }

        [Required]
        public bool HasFever { get; set; }

        [Required]
        public bool HasBleeding { get; set; }

        [Required]
        public bool HasSwelling { get; set; }

        public string? Observations { get; set; }
    }
}