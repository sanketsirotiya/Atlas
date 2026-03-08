# CLAUDE.md — Atlas Portfolio Engine

This file gives Claude full context about this project so any conversation can resume immediately without re-explaining anything.

---

## What This Project Is

A full-stack robo-advisor demo built for fun.

- **Owner:** Sanket Sirotiya (sanketsirotiya)
- **GitHub:** https://github.com/sanketsirotiya/Atlas
- **Purpose:** Showcase fintech domain knowledge — risk profiling, portfolio management, drift detection, rebalancing, compliance

---

## Live Deployment (Railway)

| Service  | URL |
| -------- | --- |
| Frontend | https://atlas-production-5b80.up.railway.app |
| API      | https://atlas-production-ad63.up.railway.app |
| Database | PostgreSQL on Railway (internal) |

---

## Folder Structure

```
C:\Users\DELL\source\Atlas\
├── README.md
├── CLAUDE.md                              ← This file (read by Claude Code automatically)
├── AtlasPortfolioEngine\                  ← .NET 10 solution
│   ├── AtlasPortfolioEngine.Domain\
│   ├── AtlasPortfolioEngine.Application\
│   ├── AtlasPortfolioEngine.Infrastructure\
│   ├── AtlasPortfolioEngine.API\
│   ├── AtlasPortfolioEngine.Tests\
│   ├── Dockerfile                         ← Docker build for Railway
│   └── railway.toml                       ← Railway config (dockerfile builder)
└── AtlasPortfolioEngine.Web\             ← Angular 19 frontend
    ├── Dockerfile                         ← Docker build for Railway (node:22.12-alpine)
    ├── railway.toml                       ← Railway config (dockerfile builder)
    └── src\app\
        ├── core\
        │   ├── services\auth.service.ts
        │   ├── services\api.service.ts
        │   ├── guards\auth-guard.ts
        │   └── interceptors\auth.interceptor.ts
        ├── environments\
        │   ├── environment.ts             ← Dev: localhost:5146
        │   └── environment.prod.ts        ← Prod: atlas-production-ad63.up.railway.app
        ├── features\
        │   ├── auth\login\
        │   ├── clients\client-list\
        │   ├── clients\client-detail\
        │   ├── portfolio\portfolio-dashboard\
        │   ├── portfolio\drift-report\
        │   ├── rebalance\rebalance-preview\
        │   └── suitability\suitability-check\
        └── shared\components\sidebar\
```

---

## Tech Stack

| Layer    | Tech                                              |
| -------- | ------------------------------------------------- |
| Backend  | .NET 10, C#, Clean Architecture                   |
| ORM      | EF Core 9, PostgreSQL (Npgsql)                    |
| Frontend | Angular 19, Tailwind CSS 3, Standalone Components |
| Auth     | JWT Bearer Tokens                                 |
| Tests    | xUnit, FluentAssertions, Moq — 26 tests passing   |
| Hosting  | Railway (Docker for both API and Angular)         |

---

## Running the Project

### API (local)

```bash
cd C:\Users\DELL\source\Atlas\AtlasPortfolioEngine
dotnet run --project AtlasPortfolioEngine.API
# Runs on http://localhost:5146
# Swagger: http://localhost:5146/swagger
```

### Angular (local)

```bash
cd C:\Users\DELL\source\Atlas\AtlasPortfolioEngine.Web
ng serve
# Runs on http://localhost:4200
```

### Tests

```bash
cd C:\Users\DELL\source\Atlas\AtlasPortfolioEngine
dotnet test
# Expected: 26 passing
```

### Demo Credentials

- Username: `advisor`
- Password: `atlas2026`

---

## Database

### Local (PostgreSQL)

- **Connection string:** `Host=localhost;Port=5432;Database=atlas_portfolio;Username=postgres;Password=superuser;`

### Production (Railway)

- Railway PostgreSQL service — connection injected via `DATABASE_URL` env var
- Public proxy: `switchback.proxy.rlwy.net:53681`
- Schema auto-created via `EnsureCreatedAsync()` on startup (no migrations needed)

### Seed Data

- **Client:** Sarah Mitchell (sarah.mitchell@email.com, DOB 1985-06-15)
- **Risk Profile:** Score 72 → Growth category
- **Account:** $5,000 cash
- **Holdings:** XIC, XUS, XEF, XBB, XGBO (~$32,595 total market value)

---

## API Endpoints

| Method | Endpoint                          | Auth |
| ------ | --------------------------------- | ---- |
| POST   | /api/v1/auth/token                | No   |
| GET    | /api/v1/clients                   | Yes  |
| GET    | /api/v1/clients/{id}              | Yes  |
| POST   | /api/v1/clients/{id}/risk-assessment | Yes |
| GET    | /api/v1/portfolio/{clientId}      | Yes  |
| GET    | /api/v1/portfolio/{clientId}/drift | Yes |
| GET    | /api/v1/rebalance/{clientId}/preview | Yes |
| POST   | /api/v1/rebalance/{clientId}/execute | Yes |
| POST   | /api/v1/suitability/check         | Yes  |

---

## Angular Routes

| Route                    | Component          |
| ------------------------ | ------------------ |
| /login                   | Login              |
| /clients                 | ClientList         |
| /clients/:id             | ClientDetail       |
| /clients/:id/portfolio   | PortfolioDashboard |
| /clients/:id/drift       | DriftReport        |
| /clients/:id/rebalance   | RebalancePreview   |
| /clients/:id/suitability | SuitabilityCheck   |

---

## Angular Key Facts

- **Angular version:** 19 (CLI 20)
- **Styling:** Tailwind CSS 3 (NOT v4 — v4 has Angular compatibility issues)
- **Components:** Standalone (no NgModules)
- **File naming:** Angular 19 drops `.component` suffix — files are `login.ts`, `client-list.ts` etc.
- **Class naming:** Angular 19 drops `Component` suffix — classes are `Login`, `ClientList` etc.
- **Change detection:** All components use `ChangeDetectorRef.detectChanges()` after HTTP calls (Angular 19 zoneless behavior)
- **Auth guard file:** `auth-guard.ts` (not `auth.guard.ts`)
- **API base URL:** Controlled via `environment.ts` / `environment.prod.ts`
- **Sidebar:** Shared `app-sidebar` component in `shared/components/sidebar/` — mobile responsive with hamburger menu

---

## Known Issues / Decisions

- Tailwind 4 was tried but has Angular CLI compatibility issues — using Tailwind 3
- Swashbuckle 10.x has breaking OpenApi 2.x changes — JWT Swagger UI skipped, auth works via Postman/interceptor
- `UseAuthentication()` must come before `UseAuthorization()` in middleware pipeline
- Railway Nixpacks doesn't support .NET 10 or Node 22.12+ — both services use Dockerfile instead
- Npgsql's `NpgsqlConnectionStringBuilder` does NOT accept `postgresql://` URL format — Program.cs converts it to key=value format at startup
- Railway `DATABASE_URL` uses internal hostname — must use `DATABASE_PUBLIC_URL` injected as `DATABASE_URL` in API Variables

---

## JWT Configuration

```json
"Jwt": {
  "Key": "AtlasPortfolioEngine-SuperSecret-Key-2026!",
  "Issuer": "AtlasPortfolioEngine",
  "Audience": "AtlasPortfolioEngine.Client",
  "DemoUsername": "advisor",
  "DemoPassword": "atlas2026"
}
```

---

## Stage 2 — Planned Improvements

- [ ] Risk assessment questionnaire UI (POST endpoint already built)
- [ ] Portfolio allocation pie chart
- [ ] Drift bar chart visualization
- [ ] More seed clients with different risk profiles
- [ ] Nav bar shared component (currently duplicated across views)
- [ ] Error handling improvements
- [ ] Loading skeletons instead of plain text

---

## Owner Background

- 16 years software development, Team Lead
- Skills: .NET, Angular, SQL Server, Azure
- Learning: React, Next.js, Python, AI Engineering
- Goal: Transition to AI Engineer role
