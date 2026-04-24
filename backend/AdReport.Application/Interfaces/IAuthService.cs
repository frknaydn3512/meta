using AdReport.Application.DTOs.Auth;
using AdReport.Application.Common;

namespace AdReport.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
}