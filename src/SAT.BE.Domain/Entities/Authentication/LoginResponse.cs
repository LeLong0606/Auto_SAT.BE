namespace SAT.BE.src.SAT.BE.Domain.Entities.Authentication
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public UserInfo User { get; set; } = default!;
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }
}