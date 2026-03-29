using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Surgeries")]
    public class Surgery
    {
        [Key]
        [Column("surgery_id")]
        public int SurgeryId { get; set; }

        [Required]
        [Column("surgery_date")]
        public DateTime SurgeryDate { get; set; }

        [Required]
        [Column("surgeryType_id")]
        public int SurgeryTypeId { get; set; }

        [Required]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(SurgeryTypeId))]
        public SurgeryType SurgeryType { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;
    }
}