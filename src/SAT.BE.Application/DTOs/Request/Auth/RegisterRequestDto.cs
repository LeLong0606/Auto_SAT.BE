using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, ErrorMessage = "Full name must not exceed 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public int? EmployeeId { get; set; }
    }
}