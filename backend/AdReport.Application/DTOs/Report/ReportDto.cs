using AdReport.Domain.Enums;

namespace AdReport.Application.DTOs.Report;

public class ReportDto
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int MetaAccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool HasPdf { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateReportDto
{
    public int ClientId { get; set; }
    public int MetaAccountId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
