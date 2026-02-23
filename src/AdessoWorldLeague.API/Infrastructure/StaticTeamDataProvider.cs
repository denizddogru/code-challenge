using AdessoWorldLeague.API.Abstractions;

namespace AdessoWorldLeague.API.Infrastructure;

/// <summary>
/// Provides the fixed set of 32 teams (8 countries × 4 teams) from static in-memory data.
/// Swap this implementation for a DB-backed provider without touching DrawService.
/// </summary>
public class StaticTeamDataProvider : ITeamDataProvider
{
    private static readonly IReadOnlyList<TeamRecord> Teams =
    [
        new("Adesso İstanbul",  "Türkiye"),
        new("Adesso Ankara",    "Türkiye"),
        new("Adesso İzmir",     "Türkiye"),
        new("Adesso Antalya",   "Türkiye"),

        new("Adesso Berlin",    "Almanya"),
        new("Adesso Frankfurt", "Almanya"),
        new("Adesso Münih",     "Almanya"),
        new("Adesso Dortmund",  "Almanya"),

        new("Adesso Paris",     "Fransa"),
        new("Adesso Marsilya",  "Fransa"),
        new("Adesso Nice",      "Fransa"),
        new("Adesso Lyon",      "Fransa"),

        new("Adesso Amsterdam", "Hollanda"),
        new("Adesso Rotterdam", "Hollanda"),
        new("Adesso Lahey",     "Hollanda"),
        new("Adesso Eindhoven", "Hollanda"),

        new("Adesso Lisbon",    "Portekiz"),
        new("Adesso Porto",     "Portekiz"),
        new("Adesso Braga",     "Portekiz"),
        new("Adesso Coimbra",   "Portekiz"),

        new("Adesso Roma",      "İtalya"),
        new("Adesso Milano",    "İtalya"),
        new("Adesso Venedik",   "İtalya"),
        new("Adesso Napoli",    "İtalya"),

        new("Adesso Sevilla",   "İspanya"),
        new("Adesso Madrid",    "İspanya"),
        new("Adesso Barselona", "İspanya"),
        new("Adesso Granada",   "İspanya"),

        new("Adesso Brüksel",   "Belçika"),
        new("Adesso Brugge",    "Belçika"),
        new("Adesso Gent",      "Belçika"),
        new("Adesso Anvers",    "Belçika"),
    ];

    public IReadOnlyList<TeamRecord> GetAll() => Teams;
}
