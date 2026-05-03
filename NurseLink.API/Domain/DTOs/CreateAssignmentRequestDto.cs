using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateAssignmentRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0.")]
        public int PatientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "NurseId must be greater than 0.")]
        public int NurseId { get; set; }
    }
}