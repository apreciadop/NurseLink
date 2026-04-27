using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Required]
        [Column("conversation_id")]
        public int ConversationId { get; set; }

        [Required]
        [Column("message_date")]
        public DateTime MessageDate { get; set; }

        [Required]
        [Column("message_sender")]
        public int MessageSender { get; set; } // 0 = Nurse, 1 = Patient

        [Required]
        [Column("message_text")]
        public string MessageText { get; set; } = string.Empty;

        [Required]
        [Column("message_read")]
        public bool MessageRead { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; } = null!;
    }
}