using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Domain.Entities
{

    [Table("Users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("user_role")]
        public UserRole UserRole { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("user_name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("user_surname")]
        public string UserSurname { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("user_password")]
        public string UserPassword { get; set; } = string.Empty;

        [Required]
        [Column("user_active")]
        public bool UserActive { get; set; } = true;

        [Column("user_birthdate", TypeName = "date")]
        public DateTime? UserBirthdate { get; set; }

        [MaxLength(30)]
        [Column("user_phone")]
        public string? UserPhone { get; set; }

        [Column("user_photo")]
        public string? UserPhoto { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}