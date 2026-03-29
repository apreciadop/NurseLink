using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NurseLink.API.Domain.Entities
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Required]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [Column("report_date")]
        public DateTime ReportDate { get; set; }

        [Required]
        [Column("report_pain")]
        [Range(0, 10)]
        public int ReportPain { get; set; }

        [Required]
        [Column("report_fever")]
        public bool ReportFever { get; set; } = false;

        [Required]
        [Column("report_bleeding")]
        public bool ReportBleeding { get; set; } = false;

        [Required]
        [Column("report_swelling")]
        public bool ReportSwelling { get; set; } = false;

        [MaxLength(1000)]
        [Column("report_observations")]
        public string? ReportObservations { get; set; }

        [Required]
        [Column("report_alerts")]
        public int ReportAlerts { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        [Column("report_status")]
        public string ReportStatus { get; set; } = string.Empty;

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [MaxLength(1000)]
        [Column("nurse_observations")]
        public string? NurseObservations { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(NurseId))]
        public Nurse? Nurse { get; set; }
    }
}