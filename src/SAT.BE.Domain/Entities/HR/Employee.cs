using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required, MaxLength(20)]
        public string EmployeeCode { get; set; } = default!;

        [Required, MaxLength(255)]
        public string FullName { get; set; } = default!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Foreign keys
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int WorkPositionId { get; set; }

        // Navigation properties
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = default!;

        [ForeignKey("WorkPositionId")]
        public virtual WorkPosition WorkPosition { get; set; } = default!;

        // One-to-One relationship with ApplicationUser
        public virtual ApplicationUser? User { get; set; }

        // One-to-Many relationships
        public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
        public virtual ICollection<EmployeeTaskAssignment> EmployeeTaskAssignments { get; set; } = new List<EmployeeTaskAssignment>();

        // Self-referencing relationship for Department Leader
        public virtual ICollection<Department> LeadingDepartments { get; set; } = new List<Department>();
    }
}