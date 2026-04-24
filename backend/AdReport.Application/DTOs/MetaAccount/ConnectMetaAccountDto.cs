using System.ComponentModel.DataAnnotations;

namespace AdReport.Application.DTOs.MetaAccount;

public class ConnectMetaAccountDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public string AccountId { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;
}
