namespace AdessoWorldLeague.API.Abstractions;

public record TeamRecord(string Name, string Country);

public interface ITeamDataProvider
{
    IReadOnlyList<TeamRecord> GetAll();
}
