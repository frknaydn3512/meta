using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.AgencyClient;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgencyClientsController : ApiControllerBase
{
    private readonly IAgencyClientService _agencyClientService;

    public AgencyClientsController(IAgencyClientService agencyClientService)
    {
        _agencyClientService = agencyClientService;
    }

    /// <summary>
    /// Get all clients for the current agency
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgencyClientDto>>>> GetClients()
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<List<AgencyClientDto>>>();
        var result = await _agencyClientService.GetClientsAsync(agencyId);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific client by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> GetClient(int id)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<AgencyClientDto>>();
        var result = await _agencyClientService.GetClientByIdAsync(agencyId, id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> CreateClient(CreateAgencyClientDto request)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<AgencyClientDto>>();
        var result = await _agencyClientService.CreateClientAsync(agencyId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetClient), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> UpdateClient(int id, UpdateAgencyClientDto request)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<AgencyClientDto>>();
        var result = await _agencyClientService.UpdateClientAsync(agencyId, id, request);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteClient(int id)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<object>>();
        var result = await _agencyClientService.DeleteClientAsync(agencyId, id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}