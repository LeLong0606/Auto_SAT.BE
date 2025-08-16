using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Request.Employee
{
    public class UpdateEmployeeRequestDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee Code is required")]
        [StringLength(20, ErrorMessage = "Employee Code must not exceed 20 characters")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(255, ErrorMessage = "Full Name must not exceed 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone Number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Work Position is required")]
        public int WorkPositionId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}