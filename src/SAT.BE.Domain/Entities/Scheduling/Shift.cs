using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Scheduling
{
    public enum ShiftType
    {
        Morning,
        Afternoon,
        Night
    }

    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }

        [Required]
        public ShiftType Type { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Đủ 8h hợp lệ
        public bool IsValid => (EndTime - StartTime).TotalHours >= 8;

        public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    }
}
