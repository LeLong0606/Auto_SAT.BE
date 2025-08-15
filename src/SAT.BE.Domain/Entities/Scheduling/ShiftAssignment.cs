using System.ComponentModel.DataAnnotations;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Scheduling
{
    public class ShiftAssignment
    {
        [Key]
        public int ShiftAssignmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required, MaxLength(10)]
        public string StatusCode { get; set; } = default!; // "X", "RO", "LE",...

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        [Required]
        public int ShiftId { get; set; }
        public Shift Shift { get; set; } = default!;
    }
}
