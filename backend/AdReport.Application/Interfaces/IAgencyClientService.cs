using AdReport.Application.DTOs.AgencyClient;
using AdReport.Application.Common;

namespace AdReport.Application.Interfaces;

public interface IAgencyClientService
{
    Task<ApiResponse<List<AgencyClientDto>>> GetClientsAsync(int agencyId);
    Task<ApiResponse<AgencyClientDto>> GetClientByIdAsync(int agencyId, int clientId);
    Task<ApiResponse<AgencyClientDto>> CreateClientAsync(int agencyId, CreateAgencyClientDto request);
    Task<ApiResponse<AgencyClientDto>> UpdateClientAsync(int agencyId, int clientId, UpdateAgencyClientDto request);
    Task<ApiResponse<object>> DeleteClientAsync(int agencyId, int clientId);
}