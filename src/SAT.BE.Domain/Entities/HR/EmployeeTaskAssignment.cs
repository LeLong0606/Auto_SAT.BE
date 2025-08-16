using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public class EmployeeTaskAssignment
    {
        [Key]
        public int EmployeeTaskAssignmentId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int TaskAssignmentId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? StartedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? CompletionNotes { get; set; }

        public int ProgressPercentage { get; set; } = 0;

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = default!;

        [ForeignKey("TaskAssignmentId")]
        public virtual TaskAssignment TaskAssignment { get; set; } = default!;
    }
}