using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.AgencyClient;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgencyClientsController : ControllerBase
{
    private readonly IAgencyClientService _agencyClientService;

    public AgencyClientsController(IAgencyClientService agencyClientService)
    {
        _agencyClientService = agencyClientService;
    }

    private int GetCurrentAgencyId()
    {
        var agencyIdClaim = User.FindFirst("agencyId")?.Value;
        return int.Parse(agencyIdClaim!);
    }

    /// <summary>
    /// Get all clients for the current agency
    /// </summary>
    /// <returns>List of agency clients</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgencyClientDto>>>> GetClients()
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _agencyClientService.GetClientsAsync(agencyId);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific client by ID
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Agency client details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> GetClient(int id)
    {
        var agencyId = GetCurrentAgencyId();
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
    /// <param name="request">Client creation details</param>
    /// <returns>Created client details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> CreateClient(CreateAgencyClientDto request)
    {
        var agencyId = GetCurrentAgencyId();
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
    /// <param name="id">Client ID</param>
    /// <param name="request">Updated client details</param>
    /// <returns>Updated client details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AgencyClientDto>>> UpdateClient(int id, UpdateAgencyClientDto request)
    {
        var agencyId = GetCurrentAgencyId();
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
    /// <param name="id">Client ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteClient(int id)
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _agencyClientService.DeleteClientAsync(agencyId, id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}