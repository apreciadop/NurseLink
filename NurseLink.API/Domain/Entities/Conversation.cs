using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Conversations")]
    public class Conversation
    {
        [Key]
        [Column("conversation_id")]
        public int ConversationId { get; set; }

        [Required]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        [Required]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(NurseId))]
        public Nurse Nurse { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}