# Atlas Portfolio Engine

A full-stack robo-advisor demo built for a fintech interview. Demonstrates Clean Architecture, portfolio management domain logic, compliance enforcement, and a modern Angular frontend.

---

## Tech Stack

| Layer    | Technology                                        |
| -------- | ------------------------------------------------- |
| Backend  | .NET 10, C#, Clean Architecture                   |
| Database | SQL Server, EF Core 9                             |
| Frontend | Angular 19, Tailwind CSS 3, Standalone Components |
| Auth     | JWT Bearer Tokens                                 |
| Testing  | xUnit, FluentAssertions, Moq (26 tests)           |

---

## Project Structure

```
AtlasPortfolioEngine/
├── AtlasPortfolioEngine/               # .NET Solution
│   ├── AtlasPortfolioEngine.Domain/    # Entities, enums, value objects
│   ├── AtlasPortfolioEngine.Application/   # 5 business engines + interfaces
│   ├── AtlasPortfolioEngine.Infrastructure/  # EF Core, DbContext, seed data
│   ├── AtlasPortfolioEngine.API/       # Controllers, JWT auth, CORS
│   └── AtlasPortfolioEngine.Tests/     # 26 unit tests
└── AtlasPortfolioEngine.Web/           # Angular 19 frontend
    └── src/app/
        ├── core/                       # Auth service, API service, guard, interceptor
        ├── features/                   # 7 views (auth, clients, portfolio, rebalance, suitability)
        └── shared/                     # Shared components
```

---

## The 5 Fintech Engines

### 1. Risk Profiling Engine

Accepts a questionnaire (5 questions, scored 1–5), normalizes answers to a 0–100 score, and categorizes clients:

- **Conservative** (score ≤ 33)
- **Balanced** (score ≤ 66)
- **Growth** (score > 66)

### 2. Model Portfolio Engine

Returns target asset allocation based on risk category:

| Asset Class          | Conservative | Balanced | Growth |
| -------------------- | ------------ | -------- | ------ |
| Canadian Equity      | 5%           | 20%      | 40%    |
| US Equity            | 10%          | 25%      | 30%    |
| International Equity | 10%          | 15%      | 15%    |
| Canadian Bonds       | 50%          | 25%      | 10%    |
| Global Bonds         | 20%          | 10%      | 5%     |
| Cash                 | 5%           | 5%       | 0%     |

### 3. Drift Detection Engine

Compares actual portfolio weights vs model targets. Flags any asset class with drift > **5%** as requiring rebalancing.

### 4. Rebalancing Engine

Generates buy/sell orders to restore target allocations. Sells are always executed before buys to free up cash. Orders are recorded as transactions and logged to the audit trail.

### 5. Suitability Engine (Compliance)

Validates every trade against the client's risk profile before execution:

- Enforces maximum equity concentration per risk category (Conservative: 30%, Balanced: 65%, Growth: 90%)
- Every check — approved or rejected — is logged immutably to the audit trail
- Returns structured reason codes: `APPROVED`, `EQUITY_CONCENTRATION_BREACH`, `UNSUITABLE_FOR_RISK_PROFILE`

---

## API Endpoints

All endpoints (except `/api/auth/token`) require `Authorization: Bearer <token>`.

| Method | Endpoint                            | Description                      |
| ------ | ----------------------------------- | -------------------------------- |
| POST   | `/api/auth/token`                   | Get JWT token                    |
| GET    | `/api/clients`                      | List all clients                 |
| GET    | `/api/clients/{id}`                 | Client detail + risk profile     |
| POST   | `/api/clients/{id}/risk-assessment` | Submit risk questionnaire        |
| GET    | `/api/portfolio/{clientId}`         | Holdings, market value, return % |
| GET    | `/api/portfolio/{clientId}/drift`   | Drift vs target allocation       |
| GET    | `/api/rebalance/{clientId}/preview` | Preview rebalance orders         |
| POST   | `/api/rebalance/{clientId}/execute` | Execute rebalance                |
| POST   | `/api/suitability/check`            | Validate trade suitability       |

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (local or Express)
- Node.js 20+
- Angular CLI 20+

### 1. Database Setup

Create the database and user in SQL Server:

```sql
CREATE DATABASE AtlasPortfolioEngine;
CREATE LOGIN AtlasPortfolioUser WITH PASSWORD = 'AtlasPortfolioUser';
USE AtlasPortfolioEngine;
CREATE USER AtlasPortfolioUser FOR LOGIN AtlasPortfolioUser;
ALTER ROLE db_owner ADD MEMBER AtlasPortfolioUser;
```

### 2. Run the API

```bash
cd AtlasPortfolioEngine/AtlasPortfolioEngine.API
dotnet run
```

The API starts at `http://localhost:5146`. Swagger UI is available at `http://localhost:5146/swagger`.

Seed data is applied automatically on first run:

- **Client:** Sarah Mitchell (sarah.mitchell@email.com)
- **Risk Profile:** Score 72 → Growth
- **Portfolio:** 5 Canadian ETFs (XIC, XUS, XEF, XBB, XGBO) — $32,595 total value

### 3. Run the Frontend

```bash
cd AtlasPortfolioEngine.Web
npm install
ng serve
```

Open `http://localhost:4200`.

**Demo credentials:** `advisor` / `atlas2026`

### 4. Run Unit Tests

```bash
cd AtlasPortfolioEngine/AtlasPortfolioEngine.Tests
dotnet test
```

Expected: **26 tests passing**.

---

## Architecture Decisions

**Clean Architecture** — Domain has zero external dependencies. Application depends only on Domain. Infrastructure implements Application interfaces. API depends only on Application interfaces, never on Infrastructure directly.

**Dependency inversion** — All 5 engines are injected via interfaces (`IRiskProfileService`, `IModelPortfolioService`, etc.), making them fully testable in isolation with Moq.

**Domain-driven design** — Entities enforce invariants via private setters and factory methods. Computed properties (`MarketValue`, `UnrealizedGainLoss`) live on the entity, not the service layer.

**Financial-grade precision** — Money values use `decimal` with precision `(18, 2)`. Quantities use `(18, 6)`. No `float` or `double` anywhere near financial calculations.

**Immutable audit trail** — Every risk assessment, rebalance execution, and suitability check is appended to `AuditLogs` with a check constraint preventing updates or deletes.

**Functional JWT guard** — Angular auth guard uses the modern `CanActivateFn` pattern (no class-based guards). HTTP interceptor automatically attaches Bearer tokens to all API requests.

---

## Production Considerations

This is a demo project. In production:

| Concern           | Demo Approach                       | Production Approach                              |
| ----------------- | ----------------------------------- | ------------------------------------------------ |
| Auth              | Hardcoded JWT credentials in config | Azure AD B2C / IdentityServer                    |
| JWT Secret        | `appsettings.json`                  | Azure Key Vault                                  |
| Connection String | `appsettings.json`                  | Azure Key Vault / Managed Identity               |
| CORS              | `localhost:4200` whitelist          | Environment-specific origins                     |
| Prices            | Static seed data                    | Market data feed (e.g. Alpha Vantage, Refinitiv) |
| Multi-tenancy     | Single advisor                      | Role-based access per firm                       |

---

## Screenshots

| View          | Description                                          |
| ------------- | ---------------------------------------------------- |
| Login         | JWT authentication with advisor credentials          |
| Client List   | All clients with risk category badges                |
| Client Detail | Risk profile + navigation to all 4 engines           |
| Portfolio     | Holdings table with market value, weights, gain/loss |
| Drift Report  | Target vs actual allocation with rebalancing flag    |
| Rebalance     | Preview buy/sell orders, execute with one click      |
| Suitability   | Compliance check with reason codes                   |
