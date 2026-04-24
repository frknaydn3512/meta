using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Template;
using AdReport.Application.Common;
using AdReport.Domain.Entities;
using AdReport.Infrastructure.Data;

namespace AdReport.Infrastructure.Services;

public class ReportTemplateService : IReportTemplateService
{
    private readonly AppDbContext _context;
    private readonly string _logoDir;
    private readonly string _logoBaseUrl;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".svg", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ReportTemplateService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _logoDir = configuration["Template:LogoDirectory"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
        _logoBaseUrl = configuration["Template:LogoBaseUrl"] ?? "/logos";
        Directory.CreateDirectory(_logoDir);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReportTemplateDto>> GetTemplateAsync(int agencyId)
    {
        var template = await GetOrCreateTemplateAsync(agencyId);
        return ApiResponse<ReportTemplateDto>.SuccessResult(MapToDto(template));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReportTemplateDto>> UpdateTemplateAsync(int agencyId, ReportTemplateUpdateDto request)
    {
        var template = await GetOrCreateTemplateAsync(agencyId);

        if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
            template.PrimaryColor = request.PrimaryColor;

        if (!string.IsNullOrWhiteSpace(request.SecondaryColor))
            template.SecondaryColor = request.SecondaryColor;

        if (!string.IsNullOrWhiteSpace(request.AgencyDisplayName))
            template.AgencyDisplayName = request.AgencyDisplayName;

        if (request.AgencyEmail is not null)
            template.AgencyEmail = request.AgencyEmail;

        if (request.AgencyPhone is not null)
            template.AgencyPhone = request.AgencyPhone;

        if (request.AgencyWebsite is not null)
            template.AgencyWebsite = request.AgencyWebsite;

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<ReportTemplateDto>.SuccessResult(MapToDto(template), "Template updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<string>> UploadLogoAsync(int agencyId, Stream fileStream, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return ApiResponse<string>.ErrorResult($"Unsupported file type. Allowed: {string.Join(", ", AllowedExtensions)}");

        if (fileStream.Length > MaxFileSizeBytes)
            return ApiResponse<string>.ErrorResult("File size exceeds 5 MB limit");

        // Delete old logo if one exists
        var template = await GetOrCreateTemplateAsync(agencyId);
        if (!string.IsNullOrEmpty(template.LogoUrl))
        {
            var oldFileName = Path.GetFileName(template.LogoUrl.Split('?')[0]);
            var oldPath = Path.Combine(_logoDir, oldFileName);
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        var newFileName = $"logo_{agencyId}_{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(_logoDir, newFileName);

        await using var output = File.Create(savePath);
        await fileStream.CopyToAsync(output);

        var publicUrl = $"{_logoBaseUrl}/{newFileName}";
        template.LogoUrl = publicUrl;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResult(publicUrl, "Logo uploaded successfully");
    }

    private async Task<ReportTemplate> GetOrCreateTemplateAsync(int agencyId)
    {
        var template = await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.AgencyId == agencyId);

        if (template != null)
            return template;

        var agency = await _context.Agencies.FindAsync(agencyId);

        template = new ReportTemplate
        {
            AgencyId = agencyId,
            AgencyDisplayName = agency?.Name ?? string.Empty,
            PrimaryColor = "#1a56db",
            SecondaryColor = "#f3f4f6",
            UpdatedAt = DateTime.UtcNow
        };

        _context.ReportTemplates.Add(template);
        await _context.SaveChangesAsync();

        return template;
    }

    private static ReportTemplateDto MapToDto(ReportTemplate t) => new()
    {
        Id = t.Id,
        AgencyId = t.AgencyId,
        LogoUrl = t.LogoUrl,
        PrimaryColor = t.PrimaryColor,
        SecondaryColor = t.SecondaryColor,
        AgencyDisplayName = t.AgencyDisplayName,
        AgencyEmail = t.AgencyEmail,
        AgencyPhone = t.AgencyPhone,
        AgencyWebsite = t.AgencyWebsite,
        UpdatedAt = t.UpdatedAt
    };
}
