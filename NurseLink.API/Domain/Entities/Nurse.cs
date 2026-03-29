using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Nurses")]
    public class Nurse
    {
        [Key]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}