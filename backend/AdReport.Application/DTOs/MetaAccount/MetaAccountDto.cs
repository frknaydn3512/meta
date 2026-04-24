namespace AdReport.Application.DTOs.MetaAccount;

public class MetaAccountDto
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public int ClientId { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
