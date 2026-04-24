using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Template;
using AdReport.Application.Common;

namespace AdReport.API.Controllers;

[ApiController]
[Route("api/template")]
[Authorize]
public class ReportTemplateController : ApiControllerBase
{
    private readonly IReportTemplateService _templateService;

    public ReportTemplateController(IReportTemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Returns the current agency's white-label template.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ReportTemplateDto>>> GetTemplate()
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<ReportTemplateDto>>();
        var result = await _templateService.GetTemplateAsync(agencyId);
        return Ok(result);
    }

    /// <summary>
    /// Updates colors and contact info for the white-label template.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ReportTemplateDto>>> UpdateTemplate(ReportTemplateUpdateDto request)
    {
        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<ReportTemplateDto>>();
        var result = await _templateService.UpdateTemplateAsync(agencyId, request);
        return Ok(result);
    }

    /// <summary>
    /// Uploads a logo image (JPG/PNG/SVG/WebP, max 5 MB) and returns its public URL.
    /// </summary>
    [HttpPost("logo")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.ErrorResult("No file provided"));

        if (TryGetAgencyId() is not int agencyId) return AgencyNotFound<ApiResponse<string>>();

        await using var stream = file.OpenReadStream();
        var result = await _templateService.UploadLogoAsync(agencyId, stream, file.FileName);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
