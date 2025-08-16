using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Response.Employee
{
    public class EmployeeResponseDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Department Information
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        
        // Position Information
        public int WorkPositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        
        // Age calculation
        public int Age => DateTime.Now.Year - DateOfBirth.Year - 
                         (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}