using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class SendMessageRequestDto
    {
        [Required]
        public bool MessageSenderIsPatient { get; set; }

        public int? NurseId { get; set; }
        public int? PatientId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; } = string.Empty;
    }
}