namespace AdReport.Application.DTOs.Template;

public class ReportTemplateDto
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1a56db";
    public string SecondaryColor { get; set; } = "#f3f4f6";
    public string AgencyDisplayName { get; set; } = string.Empty;
    public string? AgencyEmail { get; set; }
    public string? AgencyPhone { get; set; }
    public string? AgencyWebsite { get; set; }
    public DateTime UpdatedAt { get; set; }
}
