using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Auth;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new agency
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>JWT tokens</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT tokens</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New JWT tokens</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken(RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}