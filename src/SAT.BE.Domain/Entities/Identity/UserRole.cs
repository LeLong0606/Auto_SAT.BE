using System.ComponentModel.DataAnnotations;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        [Required]
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; } = default!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
