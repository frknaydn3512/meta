using AdReport.Domain.Enums;

namespace AdReport.Domain.Entities;

public class Report
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public int ClientId { get; set; }
    public int MetaAccountId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? PdfPath { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? InsightsJson { get; set; }
    public string? CampaignsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Agency Agency { get; set; } = null!;
    public AgencyClient Client { get; set; } = null!;
    public MetaAccount MetaAccount { get; set; } = null!;
}
