using WorldLeague.API.Abstractions;

namespace WorldLeague.API.Infrastructure;

/// <summary>
/// Provides the fixed set of 32 teams (8 countries × 4 teams) from static in-memory data.
/// Swap this implementation for a DB-backed provider without touching DrawService.
/// </summary>
public class StaticTeamDataProvider : ITeamDataProvider
{
    private static readonly IReadOnlyList<TeamRecord> Teams =
    [
        new("İstanbul",  "Türkiye"),
        new("Ankara",    "Türkiye"),
        new("İzmir",     "Türkiye"),
        new("Antalya",   "Türkiye"),

        new("Berlin",    "Almanya"),
        new("Frankfurt", "Almanya"),
        new("Münih",     "Almanya"),
        new("Dortmund",  "Almanya"),

        new("Paris",     "Fransa"),
        new("Marsilya",  "Fransa"),
        new("Nice",      "Fransa"),
        new("Lyon",      "Fransa"),

        new("Amsterdam", "Hollanda"),
        new("Rotterdam", "Hollanda"),
        new("Lahey",     "Hollanda"),
        new("Eindhoven", "Hollanda"),

        new("Lisbon",    "Portekiz"),
        new("Porto",     "Portekiz"),
        new("Braga",     "Portekiz"),
        new("Coimbra",   "Portekiz"),

        new("Roma",      "İtalya"),
        new("Milano",    "İtalya"),
        new("Venedik",   "İtalya"),
        new("Napoli",    "İtalya"),

        new("Sevilla",   "İspanya"),
        new("Madrid",    "İspanya"),
        new("Barselona", "İspanya"),
        new("Granada",   "İspanya"),

        new("Brüksel",   "Belçika"),
        new("Brugge",    "Belçika"),
        new("Gent",      "Belçika"),
        new("Anvers",    "Belçika"),
    ];

    public IReadOnlyList<TeamRecord> GetAll() => Teams;
}
