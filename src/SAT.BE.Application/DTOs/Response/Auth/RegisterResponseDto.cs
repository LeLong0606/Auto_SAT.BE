namespace SAT.BE.src.SAT.BE.Application.DTOs.Response.Auth
{
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; } = "Registration successful";
    }
}