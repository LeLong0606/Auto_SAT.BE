using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required, MaxLength(100)]
        public string PermissionName { get; set; } = default!;

        [Required, MaxLength(100)]
        public string PermissionCode { get; set; } = default!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = default!; // HR, Scheduling, System, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}