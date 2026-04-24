namespace AdReport.Application.DTOs.MetaAccount;

public class MetaInsightsDto
{
    public decimal Spend { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public decimal Ctr { get; set; }
    public decimal Cpc { get; set; }
    public decimal Roas { get; set; }
    public long Conversions { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DateStart { get; set; } = string.Empty;
    public string DateStop { get; set; } = string.Empty;
}

public class MetaCampaignDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public decimal Spend { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public decimal Roas { get; set; }
}
