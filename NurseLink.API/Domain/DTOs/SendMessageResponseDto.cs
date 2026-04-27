namespace NurseLink.API.Domain.DTOs
{
    public class SendMessageResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public GetMessageResponseDto MessageData { get; set; } = new();
    }
}