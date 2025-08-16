using SAT.BE.src.SAT.BE.Application.DTOs.Request.Auth;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Auth;
using SAT.BE.src.SAT.BE.Application.Common;

namespace SAT.BE.src.SAT.BE.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResult<bool>> LogoutAsync(string userId);
        Task<ServiceResult<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordRequestDto request);
        Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}