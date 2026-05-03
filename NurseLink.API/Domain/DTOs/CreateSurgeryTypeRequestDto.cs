using System.ComponentModel.DataAnnotations;

namespace NurseLink.API.Domain.DTOs
{
    public class CreateSurgeryTypeRequestDto
    {
        [Required]
        [MinLength(5)]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
    }
}