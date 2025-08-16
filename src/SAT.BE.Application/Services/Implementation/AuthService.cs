using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SAT.BE.src.SAT.BE.Application.Services.Interfaces;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Auth;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.Authentication;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using AutoMapper;
using System.Security.Claims;

namespace SAT.BE.src.SAT.BE.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
                    return ServiceResult<RegisterResponseDto>.Failure("A user with this email already exists.", 400);
                }

                Domain.Entities.HR.Employee? employee = null;
                string roleToAssign = "User"; // Default role

                // Validate and load employee if EmployeeId is provided
                if (request.EmployeeId.HasValue)
                {
                    employee = await _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.WorkPosition)
                        .FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId.Value);

                    if (employee == null)
                    {
                        _logger.LogWarning("Registration failed: Employee ID {EmployeeId} not found", request.EmployeeId.Value);
                        return ServiceResult<RegisterResponseDto>.Failure("Invalid employee ID. Employee not found.", 400);
                    }

                    // Check if employee is already linked to another user
                    var existingUserWithEmployee = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId.Value);
                    if (existingUserWithEmployee != null)
                    {
                        _logger.LogWarning("Registration failed: Employee {EmployeeId} already linked to user {UserId}", request.EmployeeId.Value, existingUserWithEmployee.Id);
                        return ServiceResult<RegisterResponseDto>.Failure("This employee is already linked to another user.", 400);
                    }

                    // Determine role based on work position level and department leadership
                    roleToAssign = DetermineRoleFromEmployee(employee);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    EmployeeId = request.EmployeeId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true // Auto-confirm for development
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Registration failed for email {Email}: {Errors}", request.Email, errors);
                    return ServiceResult<RegisterResponseDto>.Failure($"Registration failed: {errors}", 400);
                }

                // Assign role based on employee position
                var roleAssignResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!roleAssignResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign role {Role} to user {Email}: {Errors}", 
                        roleToAssign, request.Email, string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
                    // Still continue, user was created successfully
                }

                // Always assign basic User role
                if (roleToAssign != "User")
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Add permissions based on role and employee position
                await AssignPermissionsToUser(user, roleToAssign, employee);

                _logger.LogInformation("User registered successfully: {Email} with role {Role}", request.Email, roleToAssign);

                var response = new RegisterResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    Message = $"Registration successful with role: {roleToAssign}"
                };

                return ServiceResult<RegisterResponseDto>.Success(response, "User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for email: {Email}", request.Email);
                return ServiceResult<RegisterResponseDto>.Failure("An internal server error occurred during registration.", 500);
            }
        }

        private string DetermineRoleFromEmployee(Domain.Entities.HR.Employee employee)
        {
            // Check if employee is a department leader
            var isLeader = _context.Departments.Any(d => d.LeaderId == employee.EmployeeId);
            
            if (isLeader)
            {
                return "Manager"; // Department leaders get Manager role
            }

            // Determine role based on work position level
            return employee.WorkPosition.Level switch
            {
                1 => "Employee", // Level 1: Basic employee
                2 => "Employee", // Level 2: Senior employee
                3 => "TeamLeader", // Level 3: Team leader
                4 => "Director", // Level 4: Director
                5 => "Manager", // Level 5: Manager
                _ => "Employee" // Default to employee
            };
        }

        private async Task AssignPermissionsToUser(ApplicationUser user, string role, Domain.Entities.HR.Employee? employee)
        {
            var claims = new List<Claim>();

            // Add employee-specific claims if available
            if (employee != null)
            {
                claims.Add(new Claim("EmployeeId", employee.EmployeeId.ToString()));
                claims.Add(new Claim("DepartmentId", employee.DepartmentId.ToString()));
                claims.Add(new Claim("WorkPositionId", employee.WorkPositionId.ToString()));
                claims.Add(new Claim("PositionLevel", employee.WorkPosition.Level.ToString()));
            }

            // Add role-specific permissions as claims
            switch (role)
            {
                case "SuperAdmin":
                case "Admin":
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_CREATE),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_UPDATE),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_DELETE),
                        new Claim("Permission", PermissionConstants.DEPARTMENT_VIEW),
                        new Claim("Permission", PermissionConstants.DEPARTMENT_MANAGE_ALL),
                        new Claim("Permission", PermissionConstants.SCHEDULE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_CREATE),
                        new Claim("Permission", PermissionConstants.USER_MANAGEMENT),
                        new Claim("Permission", PermissionConstants.ROLE_MANAGEMENT)
                    });
                    break;

                case "Director":
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_CREATE),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_UPDATE),
                        new Claim("Permission", PermissionConstants.DEPARTMENT_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_CREATE)
                    });
                    break;

                case "Manager":
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_UPDATE),
                        new Claim("Permission", PermissionConstants.DEPARTMENT_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_CREATE)
                    });
                    break;

                case "TeamLeader":
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_CREATE)
                    });
                    break;

                case "HR":
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_CREATE),
                        new Claim("Permission", PermissionConstants.EMPLOYEE_UPDATE),
                        new Claim("Permission", PermissionConstants.DEPARTMENT_VIEW),
                        new Claim("Permission", PermissionConstants.USER_MANAGEMENT)
                    });
                    break;

                case "Employee":
                case "User":
                default:
                    claims.AddRange(new[]
                    {
                        new Claim("Permission", PermissionConstants.EMPLOYEE_VIEW),
                        new Claim("Permission", PermissionConstants.SCHEDULE_VIEW)
                    });
                    break;
            }

            // Add all claims to user
            foreach (var claim in claims)
            {
                var result = await _userManager.AddClaimAsync(user, claim);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to add claim {ClaimType}:{ClaimValue} to user {UserId}", 
                        claim.Type, claim.Value, user.Id);
                }
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", request.Username);

                // Find user by username or email
                var user = await _userManager.FindByNameAsync(request.Username) ?? 
                          await _userManager.FindByEmailAsync(request.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Username} not found", request.Username);
                    return ServiceResult<LoginResponseDto>.Failure("Invalid username or password.", 401);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User {Username} is not active", request.Username);
                    return ServiceResult<LoginResponseDto>.Failure("Account is disabled. Please contact administrator.", 401);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
                    return ServiceResult<LoginResponseDto>.Failure("Invalid username or password.", 401);
                }

                // Get user roles and claims (permissions)
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                // Generate tokens
                var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    CreatedDate = DateTime.UtcNow
                };

                // Remove old refresh tokens for this user
                var oldTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var oldToken in oldTokens)
                {
                    oldToken.IsRevoked = true;
                    oldToken.RevokedReason = "Replaced by new token";
                }

                _context.RefreshTokens.Add(refreshTokenEntity);

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await _context.SaveChangesAsync();

                // Load employee information if available
                if (user.EmployeeId.HasValue)
                {
                    await _context.Entry(user).Reference(u => u.Employee).LoadAsync();
                    if (user.Employee != null)
                    {
                        await _context.Entry(user.Employee).Reference(e => e.Department).LoadAsync();
                        await _context.Entry(user.Employee).Reference(e => e.WorkPosition).LoadAsync();
                    }
                }

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                        LastName = string.Join(" ", user.FullName.Split(' ').Skip(1)),
                        Roles = roles.ToList(),
                        Permissions = permissions
                    }
                };

                _logger.LogInformation("Login successful for user: {Username} with roles: {Roles}", request.Username, string.Join(", ", roles));
                return ServiceResult<LoginResponseDto>.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for username: {Username}", request.Username);
                return ServiceResult<LoginResponseDto>.Failure("An internal server error occurred during login.", 500);
            }
        }

        public async Task<ServiceResult<bool>> LogoutAsync(string userId)
        {
            try
            {
                if (!int.TryParse(userId, out int userIdInt))
                {
                    return ServiceResult<bool>.Failure("Invalid user ID.", 400);
                }

                _logger.LogInformation("Logout attempt for user: {UserId}", userId);

                // Revoke all refresh tokens for this user
                var refreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userIdInt && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in refreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedReason = "User logout";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Logout successful for user: {UserId}", userId);
                return ServiceResult<bool>.Success(true, "Logout successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout for user: {UserId}", userId);
                return ServiceResult<bool>.Failure("An internal server error occurred during logout.", 500);
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                var tokenEntity = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

                if (tokenEntity == null || tokenEntity.ExpiryDate <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
                    return ServiceResult<LoginResponseDto>.Failure("Invalid or expired refresh token.", 401);
                }

                var user = tokenEntity.User;
                if (!user.IsActive)
                {
                    _logger.LogWarning("Token refresh failed: User {UserId} is not active", user.Id);
                    return ServiceResult<LoginResponseDto>.Failure("Account is disabled.", 401);
                }

                // Get user roles and permissions
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                // Generate new tokens
                var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                // Revoke old refresh token
                tokenEntity.IsRevoked = true;
                tokenEntity.RevokedReason = "Replaced by new token";

                // Create new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    CreatedDate = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                var response = new LoginResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                        LastName = string.Join(" ", user.FullName.Split(' ').Skip(1)),
                        Roles = roles.ToList(),
                        Permissions = permissions
                    }
                };

                _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);
                return ServiceResult<LoginResponseDto>.Success(response, "Token refreshed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during token refresh");
                return ServiceResult<LoginResponseDto>.Failure("An internal server error occurred during token refresh.", 500);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Change password attempt for user: {UserId}", request.UserId);

                var user = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    return ServiceResult<bool>.Failure("User not found.", 404);
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Change password failed for user {UserId}: {Errors}", request.UserId, errors);
                    return ServiceResult<bool>.Failure($"Password change failed: {errors}", 400);
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
                return ServiceResult<bool>.Success(true, "Password changed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password change for user: {UserId}", request.UserId);
                return ServiceResult<bool>.Failure("An internal server error occurred during password change.", 500);
            }
        }

        public async Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Forgot password request for email: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return ServiceResult<bool>.Success(true, "If the email exists in our system, you will receive a password reset link.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // In a real application, you would send an email with the reset link
                // For now, we'll just log the token for development purposes
                _logger.LogInformation("Password reset token generated for user {Email}: {Token}", request.Email, token);

                return ServiceResult<bool>.Success(true, "If the email exists in our system, you will receive a password reset link.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during forgot password for email: {Email}", request.Email);
                return ServiceResult<bool>.Failure("An internal server error occurred during password reset request.", 500);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Reset password attempt for email: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ServiceResult<bool>.Failure("Invalid reset token.", 400);
                }

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Reset password failed for email {Email}: {Errors}", request.Email, errors);
                    return ServiceResult<bool>.Failure($"Password reset failed: {errors}", 400);
                }

                _logger.LogInformation("Password reset successful for email: {Email}", request.Email);
                return ServiceResult<bool>.Success(true, "Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password reset for email: {Email}", request.Email);
                return ServiceResult<bool>.Failure("An internal server error occurred during password reset.", 500);
            }
        }
    }
}