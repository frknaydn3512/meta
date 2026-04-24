using Hangfire.Dashboard;

namespace AdReport.API.Middleware;

/// <summary>
/// Restricts Hangfire dashboard to local requests in production.
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var isLocal = httpContext.Connection.RemoteIpAddress != null &&
                      (httpContext.Connection.RemoteIpAddress.Equals(httpContext.Connection.LocalIpAddress)
                       || httpContext.Connection.RemoteIpAddress.ToString() == "127.0.0.1"
                       || httpContext.Connection.RemoteIpAddress.ToString() == "::1");
        return isLocal || httpContext.User.Identity?.IsAuthenticated == true;
    }
}
