using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public class WorkPosition
    {
        [Key]
        public int WorkPositionId { get; set; }

        [Required, MaxLength(15)]
        public string PositionCode { get; set; } = default!;

        [Required, MaxLength(255)]
        public string PositionName { get; set; } = default!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Level { get; set; } = 1; // 1: Staff, 2: Leader, 3: Manager, 4: Director

        public decimal? BaseSalary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}