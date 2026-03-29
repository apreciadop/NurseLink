using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateSurgeryRequestDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int SurgeryTypeId { get; set; }

        [Required]
        public DateTime SurgeryDate { get; set; }
    }
}