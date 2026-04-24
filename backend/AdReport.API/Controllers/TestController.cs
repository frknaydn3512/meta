using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test endpoint that requires authentication
    /// </summary>
    /// <returns>Agency information from token</returns>
    [HttpGet("protected")]
    [Authorize]
    public ActionResult<ApiResponse<object>> GetProtected()
    {
        var agencyId = User.FindFirst("agencyId")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var plan = User.FindFirst("plan")?.Value;

        var result = new
        {
            AgencyId = agencyId,
            Email = email,
            Name = name,
            Plan = plan,
            Message = "Authentication successful!"
        };

        return Ok(ApiResponse<object>.SuccessResult(result));
    }
}