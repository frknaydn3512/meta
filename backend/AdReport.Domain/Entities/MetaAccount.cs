namespace AdReport.Domain.Entities;

public class MetaAccount
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public int ClientId { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Agency Agency { get; set; } = null!;
    public AgencyClient Client { get; set; } = null!;
}
