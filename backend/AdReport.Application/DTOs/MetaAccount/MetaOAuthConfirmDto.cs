using System.ComponentModel.DataAnnotations;

namespace AdReport.Application.DTOs.MetaAccount;

public class MetaOAuthConfirmDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public string AccountId { get; set; } = string.Empty;

    [Required]
    public string EncryptedToken { get; set; } = string.Empty;
}
