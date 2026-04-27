using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class GetOrCreateConversationRequestDto
    {
        [Required]
        public int NurseId { get; set; }

        [Required]
        public int PatientId { get; set; }
    }
}