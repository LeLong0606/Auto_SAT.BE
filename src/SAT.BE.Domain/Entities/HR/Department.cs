using System.ComponentModel.DataAnnotations;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

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

        // Tổ trưởng
        public int? LeaderId { get; set; }
        public Employee? Leader { get; set; }

        // Navigation
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
