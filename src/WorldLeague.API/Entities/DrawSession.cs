namespace WorldLeague.API.Entities;

public class DrawSession
{
    public Guid Id { get; set; }
    public string DrawerFirstName { get; set; } = string.Empty;
    public string DrawerLastName { get; set; } = string.Empty;
    public DateTime DrawnAt { get; set; }
    public int GroupCount { get; set; }

    public List<Group> Groups { get; set; } = [];
}
