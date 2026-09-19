# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

H.I.H. (Home Info. Hub) — an OData v4 Web API built on ASP.NET Core (`net10.0`) with EF Core + SQLite. The API serves as the backend for [achihui](https://github.com/alvachien/achihui), providing domains for Finance, Home management, Library, Blog, and Events.

> **Temporarily disabled (2026-08-02):** The **Blog** and **Event** OData endpoints are shut down: their entity-set registrations are commented out in `Models/EdmModelBuilder.cs`, so they return **404**. The controllers, models, `hihDataContext` DbSets, and DB tables are preserved. UI entry points are also hidden (see `achihui`). Reversible.

## Build & Test Commands

```bash
# Build entire solution
dotnet build achihapi.sln

# Build only the main project
dotnet build src/hihapi/hihapi.csproj

# Run the API locally (http://localhost:25688)
dotnet run --project src/hihapi/hihapi.csproj

# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter DisplayName~hihapi.test.UnitTests

# Run a single unit test class (example)
dotnet test --filter DisplayName~hihapi.test.UnitTests.Finance.Account

# Run integration tests only
dotnet test --filter DisplayName~hihapi.test.integrationtests

# Run tests with code coverage
dotnet test test/hihapi.test/hihapi.unittest.csproj /p:CollectCoverage=true

# Run linting
dotnet format style
dotnet format analyzers
```

## Code Quality & Linting

The project uses a layered analysis setup:

- **Roslyn analyzers** (`<EnableNETAnalyzers>`) — CA rules for correctness, design, performance
- **IDE analyzers** (`<EnforceCodeStyleInBuild>` + `<GenerateDocumentationFile>`) — IDE0005 (unnecessary usings), unused members, formatting
- **Meziantou.Analyzer** — async misuse, nullability, EF Core anti-patterns, naming, empty statements
- **`.editorconfig`** — defines diagnostic severity levels per rule
- **XML doc warnings suppressed** — CS1591, CS1570, CS1572, CS1573, CS1574, CS1587 set to `none`

## Solution Structure

```
achihapi.sln
├── src/hihapi/                    # Main ASP.NET Core Web API
│   ├── Program.cs                 # Entry point (minimal hosting model)
│   ├── Controllers/               # Domain-organized OData controllers
│   │   ├── Finance/               # 15 controllers (accounts, documents, orders, plans, reports...)
│   │   ├── Home/                  # HomeDefines, HomeMembers
│   │   ├── Library/               # Books, categories, locations, persons, organizations
│   │   ├── Blog/                  # Posts, collections, formats, tags, settings
│   │   ├── Event/                 # NormalEvents, RecurEvents
│   │   └── Common/                # Currencies, Languages, DBVersions
│   ├── Models/                    # Domain models (mirrors Controllers structure)
│   ├── DataContext/hihDataContext.cs  # Single EF Core DbContext (~26K)
│   ├── Utilities/                 # DatabaseSeeder, CommonUtility, ErrorHandlingMiddleware, OData validators
│   ├── Extensions/                # ODataEndpointController (debug endpoint)
│   ├── Exceptions/                # Custom exception types (BadRequest, NotFound, DBOperation, Unauthorized)
│   └── Sqls/                      # SQL schema scripts (DBSchema_Table.sql, DBSchema_View.sql, Predeliver_Content.sql)
├── test/
│   ├── hihapi.test/               # Unit tests (xUnit + Moq, uses in-memory SQLite via SqliteDatabaseFixture)
│   ├── hihapi.integrationtest/    # Integration tests (xUnit + WebApplicationFactory)
│   └── hihapi.test.common/        # Shared test data setup (DataSetupUtility.cs)
```

## Architecture Notes

- **OData-centric**: All controllers expose OData endpoints. The EDM model is built in `EdmModelBuilder`. Two route prefixes exist: default and `/v1`.
- **Home ID convention**: Most domain entities have a `HomeID` property for multi-tenant isolation. `HomeDefines` uses `{key}` as the Home ID. `HomeMembers` is scoped to the authenticated user's home memberships. Reference controllers (Currencies, Languages, DBVersions) and some category controllers allow `HomeID = null` for shared reference data. `FinanceReports` requires HomeID in the request body.
- **Config ID ranges**: System-delivered configuration rows use explicit IDs below 1000; home-created rows get IDs ≥ 1000. `DatabaseSeeder` raises the SQLite AUTOINCREMENT sequence of the seven home-bounded config tables to the boundary at every startup (monotone + idempotent — no delta script needed), so ids **generated from the bump onward** never collide. Pre-bump home rows on existing databases keep their low IDs, so a NEW system row must be delivered via a `SchemaUpgrade` step that per-row checks the id is unclaimed — never appended at a "free" low id directly, and never via the `Any()`-guarded seed passes (they skip existing databases). `HomeID IS NULL` stays the authoritative system/home marker.
- **Single DbContext**: `hihDataContext` is the sole EF Core context, using SQLite (`Data Source=hih.db`).
- **Authentication**: JWT Bearer tokens. Authority is `https://localhost:44353` in development, `https://www.alvachien.com/idserver` in production.
- **CORS**: Different allowed origins per environment (dev: localhost ports 29521/29528/29525; prod: alvachien.com paths).
- **Middleware pipeline** (order matters): Serilog request logging → ErrorHandlingMiddleware → OData batching → Response caching → Authentication → Routing → HTTPS redirect → Authorization → CORS → Endpoints.
- **Startup seeding**: `DatabaseSeeder.SeedAsync(db)` runs on app startup to populate reference data and to raise the config-table ID sequences to the customer range boundary.
- **Version-based auto-upgrade**: `DatabaseSeeder.CurrentVersion` + the `SchemaUpgrades` step registry replace the old delta scripts. At startup the stored max `T_DBVERSION.VersionID` is compared to the code; every missing step runs (idempotently) in order and stamps its version row. Fresh DBs are stamped at `CurrentVersion`; unversioned pre-existing DBs are baselined at v21 first. A new schema change = one registered step + one `CurrentVersion` bump (a unit test guards that every version above the baseline has a step). `Sqls/Delta/*.sql` are never executed.
- **InternalsVisibleTo**: The main project exposes internals to both test projects.
- **Release build**: Defines `USE_ALIYUN` constant (conditional compilation for Aliyun deployment).

## CI Status

The GitHub Actions workflow (`.github/workflows/build-test.yml`) targets .NET 10.0.x and matches the project's target framework.
