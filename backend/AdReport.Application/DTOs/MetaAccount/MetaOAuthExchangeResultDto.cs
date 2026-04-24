namespace AdReport.Application.DTOs.MetaAccount;

public class MetaOAuthExchangeResultDto
{
    public string EncryptedToken { get; set; } = string.Empty;
    public List<MetaAdAccountListDto> AdAccounts { get; set; } = new();
}
