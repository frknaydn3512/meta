using AdReport.Application.DTOs.Template;
using AdReport.Application.Common;

namespace AdReport.Application.Interfaces;

public interface IReportTemplateService
{
    /// <summary>
    /// Returns the agency's report template, creating a default one if none exists.
    /// </summary>
    Task<ApiResponse<ReportTemplateDto>> GetTemplateAsync(int agencyId);

    /// <summary>
    /// Updates colors and contact info for the agency's template.
    /// </summary>
    Task<ApiResponse<ReportTemplateDto>> UpdateTemplateAsync(int agencyId, ReportTemplateUpdateDto request);

    /// <summary>
    /// Saves an uploaded logo file and updates the template's LogoUrl.
    /// Returns the public URL of the saved logo.
    /// </summary>
    Task<ApiResponse<string>> UploadLogoAsync(int agencyId, Stream fileStream, string fileName);
}
