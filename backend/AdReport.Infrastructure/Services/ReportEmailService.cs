using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Hangfire;
using AdReport.Application.Interfaces;
using AdReport.Application.Common;
using AdReport.Application.DTOs.Report;
using AdReport.Domain.Enums;
using AdReport.Infrastructure.Data;

namespace AdReport.Infrastructure.Services;

public class ReportEmailService : IReportEmailService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IReportService _reportService;
    private readonly IBackgroundJobClient _jobClient;
    private readonly string _appBaseUrl;

    public ReportEmailService(
        AppDbContext context,
        IEmailService emailService,
        IReportService reportService,
        IBackgroundJobClient jobClient,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _reportService = reportService;
        _jobClient = jobClient;
        _appBaseUrl = configuration["App:BaseUrl"] ?? "http://localhost:5000";
    }

    /// <inheritdoc/>
    public async Task SendReportEmailAsync(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.Client)
            .Include(r => r.Agency).ThenInclude(a => a.Template)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null || report.Status != ReportStatus.Completed)
            return;

        var template = report.Agency.Template;
        var reportUrl = $"{_appBaseUrl}/r/{report.Slug}";
        var period = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");

        var html = EmailTemplates.ReportReady(
            clientName: report.Client.Name,
            agencyName: template?.AgencyDisplayName ?? report.Agency.Name,
            agencyLogoUrl: template?.LogoUrl,
            primaryColor: template?.PrimaryColor ?? "#1a56db",
            reportPeriod: period,
            reportUrl: reportUrl);

        byte[]? pdfBytes = null;
        string? pdfFileName = null;
        if (!string.IsNullOrEmpty(report.PdfPath) && File.Exists(report.PdfPath))
        {
            pdfBytes = await File.ReadAllBytesAsync(report.PdfPath);
            pdfFileName = $"report_{new DateTime(report.Year, report.Month, 1):yyyy-MM}.pdf";
        }

        await _emailService.SendReportEmailAsync(
            toEmail: report.Client.Email,
            toName: report.Client.Name,
            subject: $"{period} Meta Reklam Raporunuz Hazır — {template?.AgencyDisplayName ?? report.Agency.Name}",
            htmlBody: html,
            pdfAttachment: pdfBytes,
            pdfFileName: pdfFileName);
    }

    /// <inheritdoc/>
    public async Task RunMonthlyReportsAsync()
    {
        // Target: previous month
        var now = DateTime.UtcNow;
        var targetDate = now.AddMonths(-1);
        var month = targetDate.Month;
        var year = targetDate.Year;

        // Collect all active Meta accounts
        var metaAccounts = await _context.MetaAccounts
            .Include(a => a.Client)
            .ToListAsync();

        foreach (var account in metaAccounts)
        {
            // Skip if report already exists for this period
            var exists = await _context.Reports.AnyAsync(r =>
                r.MetaAccountId == account.Id &&
                r.Month == month &&
                r.Year == year);

            if (exists) continue;

            // Create report record + enqueue PDF job
            var createResult = await _reportService.CreateReportAsync(account.AgencyId, new CreateReportDto
            {
                ClientId = account.ClientId,
                MetaAccountId = account.Id,
                Month = month,
                Year = year
            });

            if (!createResult.Success || createResult.Data == null) continue;

            var reportId = createResult.Data.Id;

            // Enqueue email after PDF job completes (chained via continuation)
            // We schedule with a delay to give the PDF job time to finish
            _jobClient.Schedule<IReportEmailService>(
                svc => svc.SendReportEmailAsync(reportId),
                TimeSpan.FromMinutes(10));
        }
    }
}
