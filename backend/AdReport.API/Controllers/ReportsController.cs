using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Report;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportEmailService _reportEmailService;

    public ReportsController(IReportService reportService, IReportEmailService reportEmailService)
    {
        _reportService = reportService;
        _reportEmailService = reportEmailService;
    }

    /// <summary>
    /// Creates a new report and enqueues background generation.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReportDto>>> CreateReport(CreateReportDto request)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<ReportDto>>();
        var result = await _reportService.CreateReportAsync(agencyId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetReport), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Returns all reports for the current agency.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReportDto>>>> GetReports()
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<List<ReportDto>>>();
        var result = await _reportService.GetReportsAsync(agencyId);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single report by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> GetReport(int id)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<ReportDto>>();
        var result = await _reportService.GetReportByIdAsync(agencyId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Downloads the generated PDF for a report.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadReport(int id)
    {
        if (TryGetAgencyId() is not int agencyId) return Unauthorized();
        var result = await _reportService.GetReportByIdAsync(agencyId, id);

        if (!result.Success)
            return NotFound(ApiResponse<object>.ErrorResult("Report not found"));

        var report = result.Data!;

        if (!report.HasPdf)
            return BadRequest(ApiResponse<object>.ErrorResult("PDF is not ready yet. Status: " + report.Status));

        // Re-derive file path from the same convention used in PdfGeneratorService
        var pdfDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
        var fileName = $"report_{report.Id}_{report.Year}_{report.Month:00}.pdf";
        var filePath = Path.Combine(pdfDir, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound(ApiResponse<object>.ErrorResult("PDF file not found on disk"));

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Manually sends the report-ready email to the client.
    /// </summary>
    [HttpPost("{id}/send-email")]
    public async Task<ActionResult<ApiResponse<object>>> SendEmail(int id)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<object>>();
        var reportResult = await _reportService.GetReportByIdAsync(agencyId, id);

        if (!reportResult.Success)
            return NotFound(ApiResponse<object>.ErrorResult("Report not found"));

        if (reportResult.Data!.Status != "Completed")
            return BadRequest(ApiResponse<object>.ErrorResult("Report is not completed yet. Status: " + reportResult.Data.Status));

        await _reportEmailService.SendReportEmailAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Email sent successfully"));
    }
}
