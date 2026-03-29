using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.DTOs
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }        
    }
}
