namespace NurseLink.API.Domain.DTOs
{
    public class CreateSurgeryTypeResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int SurgeryTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}