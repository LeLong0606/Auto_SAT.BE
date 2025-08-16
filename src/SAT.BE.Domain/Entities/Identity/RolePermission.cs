using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual ApplicationRole Role { get; set; } = default!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = default!;
    }
}