namespace AdReport.Application.DTOs.AgencyClient;

public class AgencyClientDto
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}