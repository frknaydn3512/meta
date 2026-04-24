using System.ComponentModel.DataAnnotations;

namespace AdReport.Application.DTOs.AgencyClient;

public class CreateAgencyClientDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Industry { get; set; } = string.Empty;
}