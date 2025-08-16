using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Scheduling
{
    public enum ShiftStatus
    {
        Scheduled = 0,     // X - Scheduled
        RestDay = 1,       // RO - Rest Day/Off
        Leave = 2,         // LE - Leave
        Sick = 3,          // SL - Sick Leave
        Absent = 4,        // AB - Absent
        Present = 5,       // P - Present
        Late = 6,          // L - Late
        EarlyLeave = 7     // EL - Early Leave
    }

    public class ShiftAssignment
    {
        [Key]
        public int ShiftAssignmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public ShiftStatus Status { get; set; } = ShiftStatus.Scheduled;

        [MaxLength(10)]
        public string StatusCode { get; set; } = "X"; // "X", "RO", "LE", "SL", "AB", "P", "L", "EL"

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Foreign keys
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = default!;

        [ForeignKey("ShiftId")]
        public virtual Shift Shift { get; set; } = default!;
    }
}