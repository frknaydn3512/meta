using Microsoft.AspNetCore.Mvc;

namespace AdReport.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int? TryGetAgencyId()
    {
        var claim = User.FindFirst("agencyId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    protected ActionResult<T> AgencyNotFound<T>() =>
        Unauthorized(new { success = false, errors = new[] { "Agency claim missing from token" } });
}
