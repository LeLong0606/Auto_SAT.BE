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
        public int RoleId { get; set; }
        public Role Role { get; set; } = default!;
    }
}
