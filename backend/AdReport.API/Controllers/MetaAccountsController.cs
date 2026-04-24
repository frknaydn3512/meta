using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.MetaAccount;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/meta-accounts")]
[Authorize]
public class MetaAccountsController : ControllerBase
{
    private readonly IMetaAccountService _metaAccountService;
    private readonly IConfiguration _configuration;

    public MetaAccountsController(IMetaAccountService metaAccountService, IConfiguration configuration)
    {
        _metaAccountService = metaAccountService;
        _configuration = configuration;
    }

    private int GetCurrentAgencyId()
    {
        var agencyIdClaim = User.FindFirst("agencyId")?.Value;
        return int.Parse(agencyIdClaim!);
    }

    /// <summary>
    /// Returns all Meta ad accounts connected to the current agency.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MetaAccountDto>>>> GetAccounts()
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _metaAccountService.GetAccountsAsync(agencyId);
        return Ok(result);
    }

    /// <summary>
    /// Validates and connects a new Meta ad account for a client.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MetaAccountDto>>> ConnectAccount(ConnectMetaAccountDto request)
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _metaAccountService.ConnectAccountAsync(agencyId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAccounts), result);
    }

    /// <summary>
    /// Disconnects (removes) a connected Meta ad account.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DisconnectAccount(int id)
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _metaAccountService.DisconnectAccountAsync(agencyId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Returns the Facebook OAuth dialog URL the frontend should redirect the user to.
    /// </summary>
    [HttpGet("oauth/url")]
    public IActionResult GetOAuthUrl([FromQuery] int clientId, [FromQuery] string redirectUri)
    {
        var appId = _configuration["MetaApi:AppId"];
        if (string.IsNullOrEmpty(appId))
            return BadRequest(ApiResponse<object>.ErrorResult("Meta App ID is not configured"));

        const string scope = "ads_read,ads_management,business_management";
        var url = $"https://www.facebook.com/dialog/oauth"
            + $"?client_id={appId}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&scope={scope}"
            + $"&state={clientId}"
            + $"&response_type=code";

        return Ok(ApiResponse<object>.SuccessResult(new { url }));
    }

    /// <summary>
    /// Exchanges an OAuth authorization code for a long-lived token and returns accessible ad accounts.
    /// </summary>
    [HttpPost("oauth/exchange")]
    public async Task<ActionResult<ApiResponse<MetaOAuthExchangeResultDto>>> ExchangeOAuth(MetaOAuthExchangeDto request)
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _metaAccountService.ExchangeOAuthCodeAsync(agencyId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Confirms connecting a specific ad account selected from the OAuth exchange result.
    /// </summary>
    [HttpPost("oauth/confirm")]
    public async Task<ActionResult<ApiResponse<MetaAccountDto>>> ConfirmOAuth(MetaOAuthConfirmDto request)
    {
        var agencyId = GetCurrentAgencyId();
        var result = await _metaAccountService.ConfirmOAuthConnectionAsync(agencyId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAccounts), result);
    }
}
