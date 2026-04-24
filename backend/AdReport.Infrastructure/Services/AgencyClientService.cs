using Microsoft.EntityFrameworkCore;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.AgencyClient;
using AdReport.Application.Common;
using AdReport.Domain.Entities;
using AdReport.Domain.Extensions;
using AdReport.Infrastructure.Data;

namespace AdReport.Infrastructure.Services;

public class AgencyClientService : IAgencyClientService
{
    private readonly AppDbContext _context;

    public AgencyClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<AgencyClientDto>>> GetClientsAsync(int agencyId)
    {
        var clients = await _context.AgencyClients
            .Where(c => c.AgencyId == agencyId)
            .Select(c => new AgencyClientDto
            {
                Id = c.Id,
                AgencyId = c.AgencyId,
                Name = c.Name,
                Email = c.Email,
                Industry = c.Industry,
                CreatedAt = c.CreatedAt
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return ApiResponse<List<AgencyClientDto>>.SuccessResult(clients);
    }

    public async Task<ApiResponse<AgencyClientDto>> GetClientByIdAsync(int agencyId, int clientId)
    {
        var client = await _context.AgencyClients
            .Where(c => c.AgencyId == agencyId && c.Id == clientId)
            .Select(c => new AgencyClientDto
            {
                Id = c.Id,
                AgencyId = c.AgencyId,
                Name = c.Name,
                Email = c.Email,
                Industry = c.Industry,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (client == null)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult("Client not found");
        }

        return ApiResponse<AgencyClientDto>.SuccessResult(client);
    }

    public async Task<ApiResponse<AgencyClientDto>> CreateClientAsync(int agencyId, CreateAgencyClientDto request)
    {
        // Get agency with plan info
        var agency = await _context.Agencies.FindAsync(agencyId);
        if (agency == null)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult("Agency not found");
        }

        // Check plan limit
        var currentClientCount = await _context.AgencyClients
            .CountAsync(c => c.AgencyId == agencyId);

        var maxClients = agency.Plan.GetMaxClientsLimit();
        if (currentClientCount >= maxClients)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult(
                $"Client limit reached. Your {agency.Plan} plan allows maximum {maxClients} clients.");
        }

        // Check if email already exists for this agency
        var existingClient = await _context.AgencyClients
            .AnyAsync(c => c.AgencyId == agencyId && c.Email == request.Email);

        if (existingClient)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult("A client with this email already exists");
        }

        var client = new AgencyClient
        {
            AgencyId = agencyId,
            Name = request.Name,
            Email = request.Email,
            Industry = request.Industry,
            CreatedAt = DateTime.UtcNow
        };

        _context.AgencyClients.Add(client);
        await _context.SaveChangesAsync();

        var clientDto = new AgencyClientDto
        {
            Id = client.Id,
            AgencyId = client.AgencyId,
            Name = client.Name,
            Email = client.Email,
            Industry = client.Industry,
            CreatedAt = client.CreatedAt
        };

        return ApiResponse<AgencyClientDto>.SuccessResult(clientDto, "Client created successfully");
    }

    public async Task<ApiResponse<AgencyClientDto>> UpdateClientAsync(int agencyId, int clientId, UpdateAgencyClientDto request)
    {
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == clientId);

        if (client == null)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult("Client not found");
        }

        // Check if email already exists for another client in this agency
        var emailExists = await _context.AgencyClients
            .AnyAsync(c => c.AgencyId == agencyId && c.Email == request.Email && c.Id != clientId);

        if (emailExists)
        {
            return ApiResponse<AgencyClientDto>.ErrorResult("A client with this email already exists");
        }

        client.Name = request.Name;
        client.Email = request.Email;
        client.Industry = request.Industry;

        await _context.SaveChangesAsync();

        var clientDto = new AgencyClientDto
        {
            Id = client.Id,
            AgencyId = client.AgencyId,
            Name = client.Name,
            Email = client.Email,
            Industry = client.Industry,
            CreatedAt = client.CreatedAt
        };

        return ApiResponse<AgencyClientDto>.SuccessResult(clientDto, "Client updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteClientAsync(int agencyId, int clientId)
    {
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == clientId);

        if (client == null)
        {
            return ApiResponse<object>.ErrorResult("Client not found");
        }

        _context.AgencyClients.Remove(client);
        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResult(new { }, "Client deleted successfully");
    }
}