using System.ComponentModel.DataAnnotations;
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

        // Quan hệ phòng ban
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = default!;

        // Vị trí làm việc
        public int WorkPositionId { get; set; }
        public WorkPosition WorkPosition { get; set; } = default!;

        // Navigation
        public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
