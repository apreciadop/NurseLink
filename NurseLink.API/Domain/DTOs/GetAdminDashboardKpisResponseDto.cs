namespace NurseLink.API.Domain.DTOs
{
    public class GetAdminDashboardKpisResponseDto
    {
        public int TotalPatients { get; set; }
        public int TotalNurses { get; set; }
        public int TotalAlerts { get; set; }
        public int UnassignedPatients { get; set; }
    }
}