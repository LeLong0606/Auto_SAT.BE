using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class RefreshToken
    {
        [Key]
        public int RefreshTokenId { get; set; }

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? RevokedReason { get; set; }

        // Foreign key
        [Required]
        public int UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = default!;
    }
}