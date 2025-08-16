using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required, MaxLength(255)]
        public string FullName { get; set; } = default!;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // One-to-One relationship with Employee
        public int? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        // Navigation properties
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}