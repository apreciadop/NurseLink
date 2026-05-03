namespace NurseLink.API.Domain.DTOs
{
    public class GetPatientWithAlertResponseDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientSurname { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReportStatus { get; set; } = string.Empty;
        public int NurseId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public string NurseSurname { get; set; } = string.Empty;
        public int ReportPain { get; set; }
        public bool ReportFever { get; set; }
        public bool ReportBleeding { get; set; }
        public bool ReportSwelling { get; set; }
        public int AlertCount { get; set; }
    }
}