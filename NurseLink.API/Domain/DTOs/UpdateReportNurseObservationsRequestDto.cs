using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class UpdateReportNurseObservationsRequestDto
    {
        [MaxLength(2000)]
        public string? NurseObservations { get; set; }
    }
}