namespace AdReport.Domain.Entities;

public class AgencyClient
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Agency Agency { get; set; } = null!;
    public ICollection<MetaAccount> MetaAccounts { get; set; } = new List<MetaAccount>();
}