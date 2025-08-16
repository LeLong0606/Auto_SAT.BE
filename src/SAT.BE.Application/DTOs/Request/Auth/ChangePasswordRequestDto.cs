using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth
{
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [StringLength(100, ErrorMessage = "Current password must be between 6 and 100 characters", MinimumLength = 6)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "New password must be between 6 and 100 characters", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}