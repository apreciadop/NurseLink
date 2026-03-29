using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("SurgeryTypes")]
    public class SurgeryType
    {
        [Key]
        [Column("surgeryType_id")]
        public int SurgeryTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("surgeryType_name")]
        public string SurgeryTypeName { get; set; } = string.Empty;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public ICollection<Surgery> Surgeries { get; set; } = new List<Surgery>();
    }
}