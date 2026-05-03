using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateSurgeryRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0.")]
        public int PatientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SurgeryTypeId must be greater than 0.")]
        public int SurgeryTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? SurgeryDate { get; set; }
    }
}