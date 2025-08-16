using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Scheduling
{
    public enum ShiftType
    {
        Morning = 1,
        Afternoon = 2,
        Night = 3,
        Overtime = 4
    }

    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }

        [Required, MaxLength(50)]
        public string ShiftName { get; set; } = default!;

        [Required]
        public ShiftType Type { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Calculated property - at least 8 hours is valid
        public bool IsValidShift => (EndTime - StartTime).TotalHours >= 8 ||
                                   (EndTime < StartTime && (TimeSpan.FromHours(24) - StartTime + EndTime).TotalHours >= 8);

        // Navigation properties
        public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    }
}