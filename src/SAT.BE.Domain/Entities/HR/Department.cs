using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAT.BE.src.SAT.BE.Domain.Entities.HR
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, MaxLength(20)]
        public string DepartmentCode { get; set; } = default!;

        [Required, MaxLength(255)]
        public string DepartmentName { get; set; } = default!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Foreign key for Leader
        public int? LeaderId { get; set; }

        // Navigation properties
        [ForeignKey("LeaderId")]
        public virtual Employee? Leader { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}