using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class MarkConversationAsReadRequestDto
    {
        [Required]
        public bool ReaderIsPatient { get; set; }
        public int? NurseId { get; set; }
        public int? PatientId { get; set; }
    }
}