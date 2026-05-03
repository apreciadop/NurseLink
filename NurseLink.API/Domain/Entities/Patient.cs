using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [MaxLength(1000)]
        [Column("patient_observations")]
        public string? PatientObservations { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public Assignment? Assignment { get; set; }

        public Surgery Surgery { get; set; } = null!;

        public Conversation? Conversation { get; set; }

        public ICollection<Report> Reports { get; set; } = [];
    }
}