# Testing Guide — Adesso World League API

## Step 1: Create the Database Tables (PostgreSQL)

Since EF Core migrations are not used, run the following SQL script manually in your PostgreSQL instance.

```sql
-- Create the draw_sessions table
CREATE TABLE draw_sessions (
    id UUID PRIMARY KEY,
    drawer_first_name VARCHAR(100) NOT NULL,
    drawer_last_name  VARCHAR(100) NOT NULL,
    drawn_at          TIMESTAMPTZ NOT NULL,
    group_count       INT NOT NULL
);

-- Create the groups table
CREATE TABLE groups (
    id              UUID PRIMARY KEY,
    name            VARCHAR(1) NOT NULL,
    draw_session_id UUID NOT NULL REFERENCES draw_sessions(id) ON DELETE CASCADE
);

-- Create the teams table
CREATE TABLE teams (
    id       UUID PRIMARY KEY,
    name     VARCHAR(100) NOT NULL,
    country  VARCHAR(50) NOT NULL,
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE
);
```

You can run this in psql:
```bash
psql -U postgres -d adesso_world_league -f schema.sql
```

Or paste directly into pgAdmin / DBeaver query editor.

---

## Step 2: Set Your Connection String

Open `src/AdessoWorldLeague.API/appsettings.json` and update the `DefaultConnection`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=adesso_world_league;Username=postgres;Password=YOUR_ACTUAL_PASSWORD"
}
```

---

## Step 3: Run the API

```bash
cd src/AdessoWorldLeague.API
dotnet run
```

You will see output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

---

## Step 4: Open Scalar API Explorer

Navigate to:
```
http://localhost:5000/scalar
```

You will see the interactive Scalar UI with the `POST /api/draw` endpoint documented.

---

## Step 5: Test the Endpoint

### Option A — via Scalar UI

1. Open `http://localhost:5000/scalar`
2. Click on `POST /api/draw`
3. Click **"Test Request"**
4. Paste the request body below and hit Send

### Option B — via curl

**Draw with 8 groups (4 teams per group):**
```bash
curl -X POST http://localhost:5000/api/draw \
  -H "Content-Type: application/json" \
  -d '{
    "groupCount": 8,
    "drawerFirstName": "Ahmet",
    "drawerLastName": "Yılmaz"
  }'
```

**Draw with 4 groups (8 teams per group):**
```bash
curl -X POST http://localhost:5000/api/draw \
  -H "Content-Type: application/json" \
  -d '{
    "groupCount": 4,
    "drawerFirstName": "Mehmet",
    "drawerLastName": "Kaya"
  }'
```

### Option C — via Postman / Insomnia / REST Client

```
POST http://localhost:5000/api/draw
Content-Type: application/json

{
  "groupCount": 8,
  "drawerFirstName": "Ahmet",
  "drawerLastName": "Yılmaz"
}
```

---

## Step 6: Expected Response (groupCount: 8)

```json
{
  "drawerName": "Ahmet Yılmaz",
  "drawnAt": "2026-02-23T10:00:00Z",
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
    // ... groups C through H
  ]
}
```

---

## Step 6b: Test Localization (Optional)

To get English error messages, add the `Accept-Language: en` header:

```bash
curl -X POST http://localhost:5000/api/draw \
  -H "Content-Type: application/json" \
  -H "Accept-Language: en" \
  -d '{ "groupCount": 5, "drawerFirstName": "Ali", "drawerLastName": "Veli" }'
```
Expected: `{ "error": "Group count must be 4 or 8." }` (English)

Without the header: `{ "error": "Grup sayısı 4 veya 8 olmalıdır." }` (Turkish default)

---

## Step 7: Verify Constraints in the Response

After receiving the response, manually verify:

1. **Country uniqueness per group**: Each group should contain teams from different countries. No two teams in the same group should share a country.
2. **n=8**: Each group has exactly 4 teams.
3. **n=4**: Each group has exactly 8 teams (all 8 countries represented).
4. **All 32 teams placed**: No team appears more than once across all groups.

---

## Step 8: Verify Database Persistence

Connect to PostgreSQL and run:

```sql
-- See all draw sessions
SELECT * FROM draw_sessions;

-- See groups for a specific session
SELECT g.name, t.name AS team, t.country
FROM groups g
JOIN teams t ON t.group_id = g.id
WHERE g.draw_session_id = '<your-session-uuid>'
ORDER BY g.name, t.country;
```

---

## Validation Error Cases

Test these invalid inputs to confirm validation works:

**Invalid group count:**
```json
{ "groupCount": 5, "drawerFirstName": "Ali", "drawerLastName": "Veli" }
```
Expected: `400 Bad Request` with `{ "error": "Grup sayısı 4 veya 8 olmalıdır." }`

**Missing drawer name:**
```json
{ "groupCount": 8, "drawerFirstName": "", "drawerLastName": "Veli" }
```
Expected: `400 Bad Request` with `{ "error": "Kurayı çeken kişinin ad ve soyad bilgisi zorunludur." }`
