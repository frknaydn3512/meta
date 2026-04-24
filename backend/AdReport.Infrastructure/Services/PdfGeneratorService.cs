using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Configuration;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.Report;

namespace AdReport.Infrastructure.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly string _outputDir;
    private readonly string _logoDir;

    public PdfGeneratorService(IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _outputDir = configuration["Pdf:OutputDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
        _logoDir = configuration["Template:LogoDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
        Directory.CreateDirectory(_outputDir);
    }

    private static readonly string[] TurkishMonths =
        ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
         "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];

    /// <inheritdoc/>
    public Task<string> GenerateAsync(ReportDataDto data)
    {
        var fileName = $"report_{data.ReportId}_{data.Year}_{data.Month:00}.pdf";
        var filePath = Path.Combine(_outputDir, fileName);

        var primary = data.Template.PrimaryColor;
        var monthLabel = $"{TurkishMonths[data.Month - 1]} {data.Year}";
        var logoBytes = ResolveLogoBytes(data.Template.LogoUrl);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(header => BuildHeader(header, data, monthLabel, primary, logoBytes));
                page.Content().Element(content => BuildContent(content, data, primary));
                page.Footer().Element(footer => BuildFooter(footer, data.Template));
            });
        });

        document.GeneratePdf(filePath);
        return Task.FromResult(filePath);
    }

    private byte[]? ResolveLogoBytes(string? logoUrl)
    {
        if (string.IsNullOrEmpty(logoUrl)) return null;
        var fileName = Path.GetFileName(logoUrl);
        var filePath = Path.Combine(_logoDir, fileName);
        if (!File.Exists(filePath)) return null;
        return File.ReadAllBytes(filePath);
    }

    private static void BuildHeader(IContainer container, ReportDataDto data, string monthLabel, string primaryColor, byte[]? logoBytes)
    {
        container.PaddingBottom(16).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                if (logoBytes != null)
                    col.Item().Height(40).Image(logoBytes).FitHeight();
                else
                    col.Item().Text(data.Template.AgencyDisplayName)
                        .FontSize(18).Bold().FontColor(primaryColor);

                col.Item().PaddingTop(4).Text($"Meta Reklam Raporu — {monthLabel}")
                    .FontSize(12).FontColor(Colors.Grey.Darken2);

                col.Item().PaddingTop(2).Text($"Müşteri: {data.ClientName} | Hesap: {data.AccountName}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(4).Background(primaryColor);
        });
    }

    private static void BuildContent(IContainer container, ReportDataDto data, string primaryColor)
    {
        container.Column(col =>
        {
            col.Spacing(16);

            // Metric cards
            col.Item().Text("Performans Özeti").FontSize(13).Bold().FontColor(primaryColor);
            col.Item().Row(row =>
            {
                row.Spacing(8);
                AddMetricCard(row.RelativeItem(), "Toplam Harcama", $"{data.Currency} {data.Insights.Spend:N2}");
                AddMetricCard(row.RelativeItem(), "Gösterim", data.Insights.Impressions.ToString("N0"));
                AddMetricCard(row.RelativeItem(), "Tıklama", data.Insights.Clicks.ToString("N0"));
                AddMetricCard(row.RelativeItem(), "ROAS", $"{data.Insights.Roas:F2}x");
            });

            col.Item().Row(row =>
            {
                row.Spacing(8);
                AddMetricCard(row.RelativeItem(), "TO (CTR)", $"{data.Insights.Ctr:F2}%");
                AddMetricCard(row.RelativeItem(), "TBM (CPC)", $"{data.Currency} {data.Insights.Cpc:N2}");
                AddMetricCard(row.RelativeItem(), "Dönüşüm", data.Insights.Conversions.ToString("N0"));
                row.RelativeItem();
            });

            // Campaigns table
            if (data.Campaigns.Count > 0)
            {
                col.Item().PaddingTop(8).Text("Kampanyalar").FontSize(13).Bold().FontColor(primaryColor);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(primaryColor).Padding(6)
                            .Text("Kampanya").Bold().FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(primaryColor).Padding(6)
                            .Text("Hedef").Bold().FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(primaryColor).Padding(6)
                            .Text("Durum").Bold().FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(primaryColor).Padding(6)
                            .Text("Gösterim").Bold().FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(primaryColor).Padding(6)
                            .Text("Tıklama").Bold().FontColor(Colors.White).FontSize(9);
                    });

                    // Rows
                    var even = false;
                    foreach (var camp in data.Campaigns)
                    {
                        var bg = even ? Colors.Grey.Lighten4 : Colors.White;
                        even = !even;

                        table.Cell().Background(bg).Padding(5).Text(camp.Name).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(camp.Objective).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(camp.Status).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(camp.Impressions.ToString("N0")).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(camp.Clicks.ToString("N0")).FontSize(9);
                    }
                });
            }
        });
    }

    private static void AddMetricCard(IContainer container, string label, string value)
    {
        container
            .Border(1).BorderColor(Colors.Grey.Lighten2)
            .Padding(12)
            .Column(col =>
            {
                col.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Medium);
                col.Item().Text(value).FontSize(16).Bold();
            });
    }

    private static void BuildFooter(IContainer container, ReportTemplateDto template)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(template.AgencyEmail)) parts.Add(template.AgencyEmail);
                if (!string.IsNullOrEmpty(template.AgencyPhone)) parts.Add(template.AgencyPhone);
                if (!string.IsNullOrEmpty(template.AgencyWebsite)) parts.Add(template.AgencyWebsite);
                text.Span(string.Join("  |  ", parts)).FontSize(8).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(80).AlignRight().Text(text =>
            {
                text.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
