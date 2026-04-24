using Microsoft.AspNetCore.Mvc;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Report;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/r")]
public class PublicReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public PublicReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Public endpoint — returns report data for a given slug. No authentication required.
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ReportDataDto>>> GetPublicReport(string slug)
    {
        var result = await _reportService.GetReportBySlugAsync(slug);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
