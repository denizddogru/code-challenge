namespace AdessoWorldLeague.API.Entities;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
}
