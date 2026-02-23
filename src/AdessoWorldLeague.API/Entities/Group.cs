namespace AdessoWorldLeague.API.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid DrawSessionId { get; set; }
    public DrawSession DrawSession { get; set; } = null!;

    public List<Team> Teams { get; set; } = [];
}
