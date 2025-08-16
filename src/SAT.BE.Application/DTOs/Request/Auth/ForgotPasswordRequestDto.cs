using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;
    }
}