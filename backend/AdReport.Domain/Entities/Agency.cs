using AdReport.Domain.Enums;

namespace AdReport.Domain.Entities;

public class Agency
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PlanType Plan { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public ICollection<AgencyClient> Clients { get; set; } = new List<AgencyClient>();
    public ICollection<MetaAccount> MetaAccounts { get; set; } = new List<MetaAccount>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ReportTemplate? Template { get; set; }
}