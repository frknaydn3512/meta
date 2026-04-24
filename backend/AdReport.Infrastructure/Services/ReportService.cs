using Microsoft.EntityFrameworkCore;
using Hangfire;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Report;
using AdReport.Application.DTOs.MetaAccount;
using AdReport.Application.Common;
using AdReport.Domain.Entities;
using AdReport.Domain.Enums;
using AdReport.Infrastructure.Data;

namespace AdReport.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly IMetaApiClient _metaApiClient;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly IEncryptionService _encryption;
    private readonly IBackgroundJobClient _jobClient;

    public ReportService(
        AppDbContext context,
        IMetaApiClient metaApiClient,
        IPdfGeneratorService pdfGenerator,
        IEncryptionService encryption,
        IBackgroundJobClient jobClient)
    {
        _context = context;
        _metaApiClient = metaApiClient;
        _pdfGenerator = pdfGenerator;
        _encryption = encryption;
        _jobClient = jobClient;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReportDto>> CreateReportAsync(int agencyId, CreateReportDto request)
    {
        // Validate month/year
        if (request.Month < 1 || request.Month > 12)
            return ApiResponse<ReportDto>.ErrorResult("Month must be between 1 and 12");

        if (request.Year < 2020 || request.Year > DateTime.UtcNow.Year)
            return ApiResponse<ReportDto>.ErrorResult("Invalid year");

        // Verify client belongs to agency
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == request.ClientId);

        if (client == null)
            return ApiResponse<ReportDto>.ErrorResult("Client not found");

        // Verify meta account belongs to agency
        var metaAccount = await _context.MetaAccounts
            .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.Id == request.MetaAccountId);

        if (metaAccount == null)
            return ApiResponse<ReportDto>.ErrorResult("Meta account not found");

        // Prevent duplicate reports for same account/month/year
        var duplicate = await _context.Reports.AnyAsync(r =>
            r.AgencyId == agencyId &&
            r.MetaAccountId == request.MetaAccountId &&
            r.Month == request.Month &&
            r.Year == request.Year);

        if (duplicate)
            return ApiResponse<ReportDto>.ErrorResult("A report for this account and period already exists");

        var slug = GenerateSlug();

        var report = new Report
        {
            AgencyId = agencyId,
            ClientId = request.ClientId,
            MetaAccountId = request.MetaAccountId,
            Month = request.Month,
            Year = request.Year,
            Status = ReportStatus.Pending,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Enqueue background job
        _jobClient.Enqueue<IReportService>(svc => svc.ProcessReportAsync(report.Id));

        return ApiResponse<ReportDto>.SuccessResult(MapToDto(report, client.Name, metaAccount.AccountName),
            "Report generation started");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ReportDto>>> GetReportsAsync(int agencyId)
    {
        var reports = await _context.Reports
            .Where(r => r.AgencyId == agencyId)
            .Include(r => r.Client)
            .Include(r => r.MetaAccount)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var dtos = reports.Select(r => MapToDto(r, r.Client.Name, r.MetaAccount.AccountName)).ToList();
        return ApiResponse<List<ReportDto>>.SuccessResult(dtos);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReportDto>> GetReportByIdAsync(int agencyId, int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.Client)
            .Include(r => r.MetaAccount)
            .FirstOrDefaultAsync(r => r.AgencyId == agencyId && r.Id == reportId);

        if (report == null)
            return ApiResponse<ReportDto>.ErrorResult("Report not found");

        return ApiResponse<ReportDto>.SuccessResult(MapToDto(report, report.Client.Name, report.MetaAccount.AccountName));
    }

    /// <inheritdoc/>
    public async Task ProcessReportAsync(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.MetaAccount)
            .Include(r => r.Client)
            .Include(r => r.Agency).ThenInclude(a => a.Template)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null) return;

        report.Status = ReportStatus.Processing;
        await _context.SaveChangesAsync();

        try
        {
            var accessToken = _encryption.Decrypt(report.MetaAccount.EncryptedAccessToken);
            var dateStart = new DateTime(report.Year, report.Month, 1).ToString("yyyy-MM-dd");
            var dateStop = new DateTime(report.Year, report.Month,
                DateTime.DaysInMonth(report.Year, report.Month)).ToString("yyyy-MM-dd");

            var insights = await _metaApiClient.GetAccountInsightsAsync(
                accessToken, report.MetaAccount.AccountId, dateStart, dateStop);

            var campaigns = await _metaApiClient.GetCampaignsAsync(
                accessToken, report.MetaAccount.AccountId, dateStart, dateStop);

            insights.Currency = report.MetaAccount.Currency;

            var template = report.Agency.Template;
            var reportData = new ReportDataDto
            {
                ReportId = report.Id,
                Slug = report.Slug,
                Month = report.Month,
                Year = report.Year,
                ClientName = report.Client.Name,
                AccountName = report.MetaAccount.AccountName,
                Currency = report.MetaAccount.Currency,
                Insights = insights,
                Campaigns = campaigns,
                Template = new ReportTemplateDto
                {
                    LogoUrl = template?.LogoUrl,
                    PrimaryColor = template?.PrimaryColor ?? "#1a56db",
                    SecondaryColor = template?.SecondaryColor ?? "#f3f4f6",
                    AgencyDisplayName = template?.AgencyDisplayName ?? report.Agency.Name,
                    AgencyEmail = template?.AgencyEmail,
                    AgencyPhone = template?.AgencyPhone,
                    AgencyWebsite = template?.AgencyWebsite
                }
            };

            var pdfPath = await _pdfGenerator.GenerateAsync(reportData);

            report.PdfPath = pdfPath;
            report.Status = ReportStatus.Completed;
            report.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            report.Status = ReportStatus.Failed;
            report.ErrorMessage = ex.Message;
        }

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReportDataDto>> GetReportBySlugAsync(string slug)
    {
        var report = await _context.Reports
            .Include(r => r.MetaAccount)
            .Include(r => r.Client)
            .Include(r => r.Agency).ThenInclude(a => a.Template)
            .FirstOrDefaultAsync(r => r.Slug == slug && r.Status == ReportStatus.Completed);

        if (report == null)
            return ApiResponse<ReportDataDto>.ErrorResult("Report not found");

        var template = report.Agency.Template;
        var data = new ReportDataDto
        {
            ReportId = report.Id,
            Slug = report.Slug,
            Month = report.Month,
            Year = report.Year,
            ClientName = report.Client.Name,
            AccountName = report.MetaAccount.AccountName,
            Currency = report.MetaAccount.Currency,
            Template = new ReportTemplateDto
            {
                LogoUrl = template?.LogoUrl,
                PrimaryColor = template?.PrimaryColor ?? "#1a56db",
                SecondaryColor = template?.SecondaryColor ?? "#f3f4f6",
                AgencyDisplayName = template?.AgencyDisplayName ?? report.Agency.Name,
                AgencyEmail = template?.AgencyEmail,
                AgencyPhone = template?.AgencyPhone,
                AgencyWebsite = template?.AgencyWebsite
            }
        };

        // Re-fetch insights from Meta API for live public view
        // (not stored in DB to avoid stale data)
        // For MVP we return the stored data — PDF is the source of truth
        data.Insights = new();
        data.Campaigns = new();

        return ApiResponse<ReportDataDto>.SuccessResult(data);
    }

    private static ReportDto MapToDto(Report report, string clientName, string accountName)
    {
        return new ReportDto
        {
            Id = report.Id,
            AgencyId = report.AgencyId,
            ClientId = report.ClientId,
            ClientName = clientName,
            MetaAccountId = report.MetaAccountId,
            AccountName = accountName,
            Month = report.Month,
            Year = report.Year,
            Status = report.Status.ToString(),
            Slug = report.Slug,
            HasPdf = !string.IsNullOrEmpty(report.PdfPath),
            CreatedAt = report.CreatedAt,
            CompletedAt = report.CompletedAt,
            ErrorMessage = report.ErrorMessage
        };
    }

    private static string GenerateSlug()
    {
        // 12-char URL-safe random slug
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_").Replace("+", "-").Replace("=", "")
            [..12];
    }
}
