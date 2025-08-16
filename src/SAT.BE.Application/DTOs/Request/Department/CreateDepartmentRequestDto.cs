using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Request.Department
{
    public class CreateDepartmentRequestDto
    {
        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20, ErrorMessage = "Department Code must not exceed 20 characters")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(255, ErrorMessage = "Department Name must not exceed 255 characters")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        public int? LeaderId { get; set; }
    }
}