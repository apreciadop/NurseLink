using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateReportRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0.")]
        public int PatientId { get; set; }

        [Range(0, 10, ErrorMessage = "Pain level must be between 0 and 10.")]
        public int PainLevel { get; set; }

        public bool HasFever { get; set; }

        public bool HasBleeding { get; set; }

        public bool HasSwelling { get; set; }

        [MaxLength(2000)]
        public string? Observations { get; set; }
    }
}