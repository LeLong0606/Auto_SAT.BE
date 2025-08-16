namespace SAT.BE.src.SAT.BE.Application.DTOs.Response.Department
{
    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Leader Information
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        
        // Employee Count
        public int EmployeeCount { get; set; }
    }
}