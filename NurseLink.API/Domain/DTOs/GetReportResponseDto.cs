using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class GetReportResponseDto
    {
        public int ReportId { get; set; }
        public int PatientId { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PainLevel { get; set; }
        public bool HasFever { get; set; }
        public bool HasBleeding { get; set; }
        public bool HasSwelling { get; set; }
        public string? Observations { get; set; }
        public int AlertCount { get; set; }
        public ReportStatus Status { get; set; }
        public int? NurseId { get; set; }
        public string? NurseObservations { get; set; }
    }
}