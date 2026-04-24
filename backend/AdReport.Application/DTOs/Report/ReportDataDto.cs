using AdReport.Application.DTOs.MetaAccount;

namespace AdReport.Application.DTOs.Report;

public class ReportDataDto
{
    public int ReportId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public MetaInsightsDto Insights { get; set; } = new();
    public List<MetaCampaignDto> Campaigns { get; set; } = new();

    // White-label template
    public ReportTemplateDto Template { get; set; } = new();
}

public class ReportTemplateDto
{
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1a56db";
    public string SecondaryColor { get; set; } = "#f3f4f6";
    public string AgencyDisplayName { get; set; } = string.Empty;
    public string? AgencyEmail { get; set; }
    public string? AgencyPhone { get; set; }
    public string? AgencyWebsite { get; set; }
}
