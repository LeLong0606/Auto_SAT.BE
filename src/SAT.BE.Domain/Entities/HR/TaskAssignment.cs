using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public class TaskAssignment
    {
        [Key]
        public int TaskAssignmentId { get; set; }

        [Required, MaxLength(255)]
        public string TaskName { get; set; } = default!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
