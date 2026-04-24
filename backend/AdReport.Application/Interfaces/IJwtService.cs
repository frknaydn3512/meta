using AdReport.Domain.Entities;
using AdReport.Application.DTOs.Auth;

namespace AdReport.Application.Interfaces;

public interface IJwtService
{
    TokenResponseDto GenerateTokens(Agency agency);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    int? GetAgencyIdFromToken(string token);
}