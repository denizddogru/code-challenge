# Adesso World League API — Technical Documentation (EN)

## 1. Technology Choices

| Technology | Version | Reason |
|---|---|---|
| .NET | 10 | Latest release, minimal API overhead, top-tier async support |
| ASP.NET Core Web API | 10 | Industry standard, first-class DI, robust routing |
| Entity Framework Core | 10 | Type-safe ORM, clean DbContext abstraction, Npgsql compatible |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10 | Official EF Core provider for PostgreSQL |
| PostgreSQL | — | Reliable, ACID-compliant, UUID natively supported |
| Scalar.AspNetCore | 2.x | Modern, clean API explorer replacing Swagger UI |
| Swashbuckle.AspNetCore | 10.x | Classic Swagger UI alongside Scalar |
| Microsoft.AspNetCore.OpenApi | 10 | Native .NET OpenAPI spec generation |
| Microsoft.Extensions.Localization | 10 | Built-in .NET localization via `IStringLocalizer<T>` |
| Tailwind CSS CDN | 3.x | UI styling — zero build step, loaded via CDN |

---

## 2. Architecture

The project follows a **simple layered architecture** — intentionally kept flat to avoid premature abstraction for a single-domain API.

```
HTTP Request
    │
    ▼
┌───────────────────────────┐
│  GlobalExceptionHandler   │  ← Intercepts all exceptions before they reach the client
└─────────────┬─────────────┘
              │
              ▼
┌─────────────────────────────┐
│       DrawController        │  ← Thin HTTP adapter. No try-catch, no business logic.
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│        DrawService          │  ← All business logic: validation, draw algorithm,
│       (IDrawService)        │    persistence orchestration
└──────┬──────────┬───────────┘
       │          │
       ▼          ▼
┌──────────┐  ┌───────────────────┐
│AppDbCtx  │  │ ITeamDataProvider │  ← Dependency-inverted data source
│(EF Core) │  │ (StaticTeamData   │    Swap to DB-backed without touching DrawService
└──────────┘  │  Provider)        │
       │      └───────────────────┘
       ▼
  PostgreSQL
```

### Folder Structure

```
src/AdessoWorldLeague.API/
├── Abstractions/           → ITeamDataProvider + TeamRecord
├── Controllers/            → DrawController (HTTP layer only)
├── Data/                   → AppDbContext + EF entity configuration
├── DTOs/                   → DrawRequest, DrawResponse, GroupDto, TeamDto
├── Entities/               → DrawSession, Group, Team
├── Infrastructure/         → StaticTeamDataProvider, DbSeeder
├── Middleware/             → GlobalExceptionHandler
├── Migrations/             → EF Core migrations (auto-applied on startup)
├── Resources/              → DrawMessages.resx (TR), DrawMessages.en.resx (EN)
├── Services/               → IDrawService, DrawService
├── wwwroot/                → index.html (Draw UI — served at "/")
├── DrawMessages.cs         → Localizer marker class
├── Program.cs              → Bootstrap, DI registrations
└── appsettings.json        → Configuration (connection string)
```

### URL Map

| URL | Description |
|---|---|
| `http://localhost:5116/` | Draw UI |
| `http://localhost:5116/scalar` | Scalar API explorer |
| `http://localhost:5116/swagger` | Swagger UI |
| `http://localhost:5116/api/draw` | REST endpoint (POST) |

### Database Schema

```
draw_sessions
  id                (UUID PK)
  drawer_first_name (VARCHAR 100)
  drawer_last_name  (VARCHAR 100)
  drawn_at          (TIMESTAMPTZ)
  group_count       (INT)

groups
  id               (UUID PK)
  name             (VARCHAR 1)    → 'A' through 'H'
  draw_session_id  (UUID FK → draw_sessions.id, CASCADE)

teams
  id        (UUID PK)
  name      (VARCHAR 100)
  country   (VARCHAR 50)
  group_id  (UUID FK → groups.id, CASCADE)
```

---

## 3. Software Development Practices

### Dependency Injection
All dependencies are registered in `Program.cs` via the built-in ASP.NET Core DI container:
- `ITeamDataProvider` → Singleton (data is immutable, shared safely across requests)
- `IDrawService` → Scoped (shares the `AppDbContext` lifetime per request)
- `AppDbContext` → Scoped (EF Core default — one instance per HTTP request)

### Interface-Based Programming
Every significant dependency is hidden behind an interface:
- `IDrawService` — controller doesn't know the concrete `DrawService`
- `ITeamDataProvider` — service doesn't know where teams come from

### Localization via `IStringLocalizer<T>`
All user-facing error messages are stored in `.resx` resource files. `DrawService` injects `IStringLocalizer<DrawMessages>` and uses string keys (`"GroupCountInvalid"`, `"DrawerNameRequired"`). Adding a new language means adding a new `.resx` file — zero code changes.

### Centralized Exception Handling
`GlobalExceptionHandler` (implementing `IExceptionHandler`) intercepts all unhandled exceptions. Controllers throw, the handler catches and formats the response. Controllers contain zero try-catch blocks.

### Separation of Concerns
- **DTOs** exist only at the HTTP boundary — entities never escape the service layer
- **Entities** are internal persistence models — never serialized to JSON
- **Services** own all domain logic and orchestration
- **Infrastructure** isolates external concerns (data sources) behind abstractions

### Async/Await Throughout
All database operations use EF Core async methods (`SaveChangesAsync`) to prevent thread blocking.

---

## 4. Design Patterns

### Service Pattern
Business logic is fully encapsulated in `DrawService` behind `IDrawService`. The controller is a thin HTTP adapter.

### Data Transfer Object (DTO) Pattern
`DrawRequest` and `DrawResponse` serve as the API contract. They prevent entity coupling to the HTTP layer and allow both to evolve independently.

### Repository Pattern (via EF Core DbContext)
`AppDbContext` acts as the Unit of Work. `DbSet<T>` provides the implicit repository abstraction. A separate explicit repository layer was intentionally omitted — it would add indirection with no benefit for a single-domain API.

### Dependency Inversion Principle (DIP)
`DrawService` depends on `ITeamDataProvider`, not `StaticTeamDataProvider`. If teams ever need to come from a database or external API, a new provider is registered in `Program.cs` — `DrawService` is untouched.

### Strategy Pattern (implicit)
`ITeamDataProvider` is effectively a Strategy for team data retrieval. The draw algorithm is decoupled from where its input data comes from.

---

## 5. Creative Solution Approach

Here is where things get interesting.

The core challenge was: **how do you randomly assign 32 teams into groups such that no two teams from the same country end up together?**

This is a **constrained random assignment problem**. The naive approach — shuffle all teams and assign sequentially — fails immediately. It will happily put two Turkish teams in Group A before you can stop it.

The approach used here is a **greedy round-robin draw with a live constraint check**:

1. All 32 teams are shuffled randomly (via LINQ `OrderBy(random.Next())`).
2. The draw proceeds **slot by slot**, exactly as the spec describes: one pick for Group A, one for Group B, ..., then back to Group A for the second slot, and so on.
3. Before each pick, the algorithm identifies which countries are **already in that group** and filters the pool to only valid candidates.

This mimics the real UEFA draw: a ball is drawn from the pot, but if it creates a conflict (same country already in the group), it goes back and another is drawn. The mathematical structure of this problem — 8 equal-sized country pots with perfectly divisible group sizes — guarantees the greedy approach **never deadlocks**. A valid candidate always exists at every step.

The `ITeamDataProvider` abstraction makes the algorithm itself agnostic to where teams come from. The team data is flat (`IReadOnlyList<TeamRecord>`) — no grouping by country embedded in the structure. The algorithm discovers country groupings dynamically during the draw via a live `HashSet<string>` check per group. Clean, decoupled, and easy to reason about.
