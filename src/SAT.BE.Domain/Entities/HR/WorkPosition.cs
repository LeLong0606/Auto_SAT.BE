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

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
