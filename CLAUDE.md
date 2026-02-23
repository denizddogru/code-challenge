# Adesso World League - Code Challenge

## Project Overview

Build a .NET Core Web API that performs a group draw for the **Adesso World League**.

## Domain

- **32 teams** from **8 countries**, 4 teams per country
- **Group count (n):** either `4` or `8` (validated)
- **Groups:** A, B, C, D, E, F, G, H

### Countries and Teams

| Country     | Teams                                                                  |
|-------------|------------------------------------------------------------------------|
| Türkiye     | Adesso İstanbul, Adesso Ankara, Adesso İzmir, Adesso Antalya           |
| Almanya     | Adesso Berlin, Adesso Frankfurt, Adesso Münih, Adesso Dortmund         |
| Fransa      | Adesso Paris, Adesso Marsilya, Adesso Nice, Adesso Lyon                |
| Hollanda    | Adesso Amsterdam, Adesso Rotterdam, Adesso Lahey, Adesso Eindhoven     |
| Portekiz    | Adesso Lisbon, Adesso Porto, Adesso Braga, Adesso Coimbra              |
| İtalya      | Adesso Roma, Adesso Milano, Adesso Venedik, Adesso Napoli              |
| İspanya     | Adesso Sevilla, Adesso Madrid, Adesso Barselona, Adesso Granada        |
| Belçika     | Adesso Brüksel, Adesso Brugge, Adesso Gent, Adesso Anvers              |

## Business Rules

1. **No two teams from the same country** can be in the same group.
2. **Draw order:** Fill groups round-robin style — pick 1 team for Group A, then 1 for Group B, ..., then loop back to Group A for the 2nd slot, and so on until all slots are filled.
3. **Each team belongs to exactly one group.**
4. **n=4:** 8 teams per group (all 8 countries represented per group).
5. **n=8:** 4 teams per group (4 different countries per group).
6. **Drawer info:** First name and last name of the person performing the draw must be accepted as input parameters.
7. **Persistence:** Draw results (groups + teams) and the drawer's info must be saved to a database with an appropriate schema.

## API Requirements

- **.NET Core Web API**
- Input: `n` (group count: 4 or 8), drawer's first and last name
- Validate that `n` is exactly `4` or `8`
- Output: JSON response listing each group and its teams
- Persist results to database

## Response Format

```json
{
  "groups": [
    {
      "groupName": "A",
      "teams": [
        { "name": "Adesso İstanbul" },
        { "name": "Adesso Berlin" },
        { "name": "Adesso Marsilya" },
        { "name": "Adesso Anvers" }
      ]
    },
    {
      "groupName": "B",
      "teams": [
        { "name": "Adesso İzmir" },
        { "name": "Adesso Dortmund" },
        { "name": "Adesso Barselona" },
        { "name": "Adesso Venedik" }
      ]
    }
  ]
}
```

## Tech Stack & Constraints

- **.NET Core Web API** (controller-based)
- **PostgreSQL** — connection string provided by user
- **Entity Framework Core** (Npgsql provider)
- **Scalar** instead of Swagger for API docs
- **NO git commits/pushes** — user handles git manually
- **NO EF migrations** — user handles database manually; we provide SQL DDL script

## Documentation Rules

Two documentation files must be maintained:
- `docs/documentation-en.md` (English)
- `docs/documentation-tr.md` (Turkish)

Each doc must cover:
1. Technology choices
2. Architecture
3. Software development practices
4. Design patterns used
5. Creative solution approach — written in natural/human language (not dry technical prose)

## Other Notes

- Always provide SQL DDL in `testing.md` so user can create tables manually
- Connection string placeholder: `Host=localhost;Database=adesso_world_league;Username=postgres;Password=yourpassword`
