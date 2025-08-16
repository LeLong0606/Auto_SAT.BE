using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using SAT.BE.src.SAT.BE.Application.Services.Interfaces;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Auth;
using SAT.BE.src.SAT.BE.Application.Common;
using System.Security.Claims;

namespace SAT.BE.src.SAT.BE.Api.Controllers.Auth
{
    /// <summary>
    /// Authentication controller for user registration, login, logout, and token management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">Registration request containing user details</param>
        /// <returns>Registration response with user information</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResult<RegisterResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<RegisterResponseDto>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.RegisterAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Registration successful for email: {Email}", request.Email);
                    return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
                }

                _logger.LogWarning("Registration failed for email: {Email}. Reason: {Message}", request.Email, result.Message);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for email: {Email}", request.Email);
                return StatusCode(500, ServiceResult<RegisterResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// User login with email/username and password
        /// </summary>
        /// <param name="request">Login request containing credentials</param>
        /// <returns>Login response with JWT tokens and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResult<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", request.Username);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<LoginResponseDto>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.LoginAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Login successful for user: {Username}", request.Username);
                    return Ok(result);
                }

                _logger.LogWarning("Login failed for user: {Username}. Reason: {Message}", request.Username, result.Message);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user: {Username}", request.Username);
                return StatusCode(500, ServiceResult<LoginResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// User logout - revokes all refresh tokens
        /// </summary>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ServiceResult<bool>>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ServiceResult<bool>.Failure("User not authenticated.", 401));
                }

                _logger.LogInformation("Logout attempt for user: {UserId}", userId);

                var result = await _authService.LogoutAsync(userId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Logout successful for user: {UserId}", userId);
                    return Ok(result);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout");
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Refresh JWT access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT tokens</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResult<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<LoginResponseDto>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Token refresh successful");
                    return Ok(result);
                }

                _logger.LogWarning("Token refresh failed. Reason: {Message}", result.Message);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during token refresh");
                return StatusCode(500, ServiceResult<LoginResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Change password request</param>
        /// <returns>Result of password change operation</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ServiceResult<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest(ServiceResult<bool>.Failure("User not authenticated.", 401));
                }

                // Ensure the user can only change their own password
                if (request.UserId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                _logger.LogInformation("Change password attempt for user: {UserId}", currentUserId);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<bool>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.ChangePasswordAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Password changed successfully for user: {UserId}", currentUserId);
                    return Ok(result);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password change");
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Request password reset via email
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Result of password reset request</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResult<bool>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Password reset request for email: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<bool>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.ForgotPasswordAsync(request);
                return Ok(result); // Always return success for security reasons
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password reset request for email: {Email}", request.Email);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Reset password using token received via email
        /// </summary>
        /// <param name="request">Reset password request with token</param>
        /// <returns>Result of password reset operation</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResult<bool>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ServiceResult<bool>.Failure(errors, "Validation failed.", 400));
                }

                var result = await _authService.ResetPasswordAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Password reset successful for email: {Email}", request.Email);
                    return Ok(result);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password reset for email: {Email}", request.Email);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ServiceResult<UserInfoDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var fullName = User.FindFirst("FullName")?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ServiceResult<UserInfoDto>.Failure("User not authenticated.", 401));
                }

                var userInfo = new UserInfoDto
                {
                    Id = int.Parse(userId),
                    Username = name ?? email ?? "",
                    Email = email ?? "",
                    FirstName = fullName?.Split(' ').FirstOrDefault() ?? "",
                    LastName = fullName?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Roles = roles,
                    Permissions = new List<string>() // Can be populated from claims if needed
                };

                return Ok(ServiceResult<UserInfoDto>.Success(userInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting current user information");
                return StatusCode(500, ServiceResult<UserInfoDto>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// The refresh token to use for generating new access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}