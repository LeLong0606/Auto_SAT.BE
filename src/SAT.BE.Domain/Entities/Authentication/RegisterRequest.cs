using System.ComponentModel.DataAnnotations;

namespace SAT.BE.src.SAT.BE.Domain.Entities.Authentication
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = default!;

        [Required, MaxLength(255)]
        public string FullName { get; set; } = default!;

        public int? EmployeeId { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}