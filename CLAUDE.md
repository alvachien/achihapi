# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

H.I.H. (Home Info. Hub) — an OData v4 Web API built on ASP.NET Core (`net10.0`) with EF Core + SQLite. The API serves as the backend for [achihui](https://github.com/alvachien/achihui), providing domains for Finance, Home management, Library, Blog, and Events.

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
```

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
└── oldsrc/                        # Archived legacy code (not in solution)
```

## Architecture Notes

- **OData-centric**: All controllers expose OData endpoints. The EDM model is built in `EdmModelBuilder`. Two route prefixes exist: default and `/v1`.
- **Single DbContext**: `hihDataContext` is the sole EF Core context, using SQLite (`Data Source=hih.db`).
- **Authentication**: JWT Bearer tokens. Authority is `https://localhost:44353` in development, `https://www.alvachien.com/idserver` in production.
- **CORS**: Different allowed origins per environment (dev: localhost ports 29521/29528/29525; prod: alvachien.com paths).
- **Middleware pipeline** (order matters): Serilog request logging → ErrorHandlingMiddleware → OData batching → Response caching → Authentication → Routing → HTTPS redirect → Authorization → CORS → Endpoints.
- **Startup seeding**: `DatabaseSeeder.Seed(db)` runs on app startup to populate reference data.
- **InternalsVisibleTo**: The main project exposes internals to both test projects.
- **Release build**: Defines `USE_ALIYUN` constant (conditional compilation for Aliyun deployment).

## CI Status

The GitHub Actions workflow (`.github/workflows/build-test.yml`) targets .NET 10.0.x and matches the project's target framework.
