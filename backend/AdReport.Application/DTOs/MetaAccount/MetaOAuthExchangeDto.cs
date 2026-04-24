using System.ComponentModel.DataAnnotations;

namespace AdReport.Application.DTOs.MetaAccount;

public class MetaOAuthExchangeDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string RedirectUri { get; set; } = string.Empty;
}
