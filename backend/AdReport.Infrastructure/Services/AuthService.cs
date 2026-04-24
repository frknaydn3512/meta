using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Auth;
using AdReport.Application.Common;
using AdReport.Domain.Entities;
using AdReport.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace AdReport.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IJwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingAgency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (existingAgency != null)
        {
            return ApiResponse<TokenResponseDto>.ErrorResult("Email already exists");
        }

        var agency = new Agency
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Plan = request.Plan,
            CreatedAt = DateTime.UtcNow
        };

        var tokens = _jwtService.GenerateTokens(agency);

        agency.RefreshToken = tokens.RefreshToken;
        agency.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["RefreshToken:ExpiryDays"] ?? "30"));

        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        return ApiResponse<TokenResponseDto>.SuccessResult(tokens, "Registration successful");
    }

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (agency == null || !BCrypt.Net.BCrypt.Verify(request.Password, agency.PasswordHash))
        {
            return ApiResponse<TokenResponseDto>.ErrorResult("Invalid email or password");
        }

        var tokens = _jwtService.GenerateTokens(agency);

        agency.RefreshToken = tokens.RefreshToken;
        agency.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["RefreshToken:ExpiryDays"] ?? "30"));

        await _context.SaveChangesAsync();

        return ApiResponse<TokenResponseDto>.SuccessResult(tokens, "Login successful");
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.RefreshToken == request.RefreshToken);

        if (agency == null || agency.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<TokenResponseDto>.ErrorResult("Invalid or expired refresh token");
        }

        var tokens = _jwtService.GenerateTokens(agency);

        agency.RefreshToken = tokens.RefreshToken;
        agency.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["RefreshToken:ExpiryDays"] ?? "30"));

        await _context.SaveChangesAsync();

        return ApiResponse<TokenResponseDto>.SuccessResult(tokens, "Token refreshed successfully");
    }
}