using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public enum TaskPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    public enum TaskStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        OnHold = 4
    }

    public class TaskAssignment
    {
        [Key]
        public int TaskAssignmentId { get; set; }

        [Required, MaxLength(255)]
        public string TaskName { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public int? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<EmployeeTaskAssignment> EmployeeTaskAssignments { get; set; } = new List<EmployeeTaskAssignment>();
    }
}