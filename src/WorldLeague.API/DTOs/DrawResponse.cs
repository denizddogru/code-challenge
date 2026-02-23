namespace WorldLeague.API.DTOs;

public class DrawResponse
{
    public string DrawerName { get; set; } = string.Empty;
    public DateTime DrawnAt { get; set; }
    public List<GroupDto> Groups { get; set; } = [];
}

public class GroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public List<TeamDto> Teams { get; set; } = [];
}

public class TeamDto
{
    public string Name { get; set; } = string.Empty;
}
