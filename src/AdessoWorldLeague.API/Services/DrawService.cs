using AdessoWorldLeague.API.Abstractions;
using AdessoWorldLeague.API.Data;
using AdessoWorldLeague.API.DTOs;
using AdessoWorldLeague.API.Entities;
using Microsoft.Extensions.Localization;

namespace AdessoWorldLeague.API.Services;

public class DrawService : IDrawService
{
    private static readonly string[] GroupNames = ["A", "B", "C", "D", "E", "F", "G", "H"];

    private readonly AppDbContext _db;
    private readonly ITeamDataProvider _teamDataProvider;
    private readonly IStringLocalizer<DrawMessages> _localizer;

    public DrawService(
        AppDbContext db,
        ITeamDataProvider teamDataProvider,
        IStringLocalizer<DrawMessages> localizer)
    {
        _db = db;
        _teamDataProvider = teamDataProvider;
        _localizer = localizer;
    }

    public async Task<DrawResponse> PerformDrawAsync(DrawRequest request)
    {
        if (request.GroupCount != 4 && request.GroupCount != 8)
            throw new ArgumentException(_localizer["GroupCountInvalid"]);

        if (string.IsNullOrWhiteSpace(request.DrawerFirstName) || string.IsNullOrWhiteSpace(request.DrawerLastName))
            throw new ArgumentException(_localizer["DrawerNameRequired"]);

        var groupSlots = PerformDraw(_teamDataProvider.GetAll(), request.GroupCount);
        var drawnAt = DateTime.UtcNow;

        await PersistAsync(request, groupSlots, drawnAt);

        return MapToResponse(request, groupSlots, drawnAt);
    }

    // Retries with a new shuffle whenever a deadlock is encountered.
    // Deadlocks are rare but possible with greedy round-robin on certain shuffle orderings.
    private static List<List<TeamRecord>> PerformDraw(IReadOnlyList<TeamRecord> allTeams, int groupCount)
    {
        while (true)
        {
            var shuffled = allTeams.OrderBy(_ => Random.Shared.Next()).ToList();
            var result = TryAssign(shuffled, groupCount);
            if (result is not null)
                return result;
        }
    }

    // Core draw: round-robin slot filling with country-uniqueness constraint per group.
    // Returns null if no valid candidate exists for a slot (deadlock) — caller will retry.
    private static List<List<TeamRecord>>? TryAssign(List<TeamRecord> pool, int groupCount)
    {
        int teamsPerGroup = pool.Count / groupCount;

        var groups = Enumerable.Range(0, groupCount)
            .Select(_ => new List<TeamRecord>())
            .ToList();

        for (int slot = 0; slot < teamsPerGroup; slot++)
        {
            for (int g = 0; g < groupCount; g++)
            {
                var usedCountries = groups[g]
                    .Select(t => t.Country)
                    .ToHashSet(StringComparer.Ordinal);

                var candidates = pool
                    .Where(t => !usedCountries.Contains(t.Country))
                    .ToList();

                if (candidates.Count == 0)
                    return null; // Deadlock — signal retry with different shuffle

                var pick = candidates[Random.Shared.Next(candidates.Count)];
                groups[g].Add(pick);
                pool.Remove(pick);
            }
        }

        return groups;
    }

    private async Task PersistAsync(
        DrawRequest request,
        List<List<TeamRecord>> groupSlots,
        DateTime drawnAt)
    {
        var session = new DrawSession
        {
            Id = Guid.NewGuid(),
            DrawerFirstName = request.DrawerFirstName,
            DrawerLastName = request.DrawerLastName,
            DrawnAt = drawnAt,
            GroupCount = request.GroupCount
        };

        for (int i = 0; i < groupSlots.Count; i++)
        {
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = GroupNames[i],
                DrawSessionId = session.Id
            };

            foreach (var teamRecord in groupSlots[i])
            {
                group.Teams.Add(new Team
                {
                    Id = Guid.NewGuid(),
                    Name = teamRecord.Name,
                    Country = teamRecord.Country,
                    GroupId = group.Id
                });
            }

            session.Groups.Add(group);
        }

        _db.DrawSessions.Add(session);
        await _db.SaveChangesAsync();
    }

    private static DrawResponse MapToResponse(
        DrawRequest request,
        List<List<TeamRecord>> groupSlots,
        DateTime drawnAt) => new()
    {
        DrawerName = $"{request.DrawerFirstName} {request.DrawerLastName}",
        DrawnAt = drawnAt,
        Groups = groupSlots
            .Select((teams, i) => new GroupDto
            {
                GroupName = GroupNames[i],
                Teams = teams.Select(t => new TeamDto { Name = t.Name }).ToList()
            })
            .ToList()
    };
}
