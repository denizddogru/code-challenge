namespace AdessoWorldLeague.API.DTOs;

public class DrawRequest
{
    public int GroupCount { get; set; }
    public string DrawerFirstName { get; set; } = string.Empty;
    public string DrawerLastName { get; set; } = string.Empty;
}
