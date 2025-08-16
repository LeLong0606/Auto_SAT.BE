using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}