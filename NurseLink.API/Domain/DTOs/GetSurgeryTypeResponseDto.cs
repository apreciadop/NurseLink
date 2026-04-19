namespace NurseLink.API.Domain.DTOs
{
    public class GetSurgeryTypeResponseDto
    {
        public int SurgeryTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}