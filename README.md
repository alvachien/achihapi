[![build and test](https://github.com/alvachien/achihapi/actions/workflows/build-test.yml/badge.svg)](https://github.com/alvachien/achihapi/actions/workflows/build-test.yml)
[![MIT Licence](https://badges.frapsoft.com/os/mit/mit.svg?v=103)](https://opensource.org/licenses/mit-license.php)



# achihapi
H.I.H. (Home Info. Hub) — an OData v4 Web API for [HIH](https://github.com/alvachien/achihui.git), built on ASP.NET Core 10.0 with EF Core + SQLite.

Reference projects:
- [HIH UI](https://github.com/alvachien/achihui.git)   
- [AC ID Server](https://github.com/alvachien/acidserver.git)


## Installation    
To install this Web API to your own server, please follow the steps below.


### Step 1. Clone or Download
You can clone this [repository](https://github.com/alvachien/achihapi.git) or download it.


### Step 2. Database
The API uses SQLite with EF Core. On first run, the database is created automatically and seeded with reference data via `DatabaseSeeder.Seed()`. No manual SQL script execution is required.

For custom schema changes, SQL scripts are available under `src/hihapi/Sqls/`:
1. `DBSchema_Table.sql`
2. `DBSchema_View.sql`
3. `Predeliver_Content.sql`


### Step 3. Configuration

Configure the API via `appsettings.json`:

```javascript
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=hih.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "JwtSettings": {
    "Authority": "https://localhost:44353"
  }
}
```

**Authentication**: JWT Bearer tokens. The default authority is `https://localhost:44353` in development environment coming from another project named [**acidserver**](https://github.com/alvachien/acidserver.git).


### Step 4. CORS

CORS is configured per environment:
- **Development**: `localhost` ports 29521, 29528, 29525

### Step 5. Deployment

Deploy this Web API to IIS, Kestrel, or other HTTP servers. To run locally:

```bash
dotnet run --project src/hihapi/hihapi.csproj
```

The API starts at `http://localhost:25688`.

## Development Tools

This project targets .NET 10.0 and can be developed with Visual Studio 2022, Visual Studio Code, JetBrains Rider, or any IDE that supports ASP.NET Core and .NET 10.0.

## Code Quality & Linting

The project is configured with layered code analysis:

| Layer | Tool | Coverage |
|---|---|---|
| Built-in | Roslyn analyzers (CA rules) | Correctness, design, performance |
| Built-in | IDE analyzers (IDE rules) | Unnecessary usings, unused members, formatting |
| Third-party | [Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer) | Async misuse, nullability, EF Core anti-patterns, naming |
| Formatting | `dotnet format` | Whitespace, style consistency |

### Running the linter

```bash
# Auto-fix style issues
dotnet format style

# Auto-fix analyzer issues
dotnet format analyzers

# Verify no formatting changes are needed (for CI)
dotnet format --verify-no-changes
```

### Configuration files

- `.editorconfig` — defines diagnostic severity rules
- `*.csproj` — `<EnableNETAnalyzers>`, `<EnforceCodeStyleInBuild>`, `<GenerateDocumentationFile>`, and `Meziantou.Analyzer` package reference

## CI/CD

GitHub Actions is used for continuous integration (`.github/workflows/build-test.yml`). The workflow builds the solution, runs all unit and integration tests, and reports code coverage on every push.

### Unit Tests

```bash
dotnet test --filter DisplayName~hihapi.test.UnitTests
```

### Integration Tests

```bash
dotnet test --filter DisplayName~hihapi.test.integrationtests
```

### Tests with Code Coverage

```bash
dotnet test test/hihapi.test/hihapi.unittest.csproj /p:CollectCoverage=true
```

## Solution Structure

```
achihapi.sln
├── src/hihapi/                    # Main ASP.NET Core Web API
│   ├── Program.cs                 # Entry point (minimal hosting model)
│   ├── Controllers/               # Domain-organized OData controllers
│   │   ├── Finance/               # Accounts, documents, orders, plans, reports...
│   │   ├── Home/                  # HomeDefines, HomeMembers
│   │   ├── Library/               # Books, categories, locations, persons, organizations
│   │   ├── Blog/                  # Posts, collections, formats, tags, settings
│   │   ├── Event/                 # NormalEvents, RecurEvents
│   │   └── Common/                # Currencies, Languages, DBVersions
│   ├── Models/                    # Domain models (mirrors Controllers structure)
│   ├── DataContext/               # EF Core DbContext (hihDataContext)
│   ├── Utilities/                 # DatabaseSeeder, CommonUtility, middleware, OData validators
│   └── Exceptions/                # Custom exception types
├── test/
│   ├── hihapi.test/               # Unit tests (xUnit + Moq, in-memory SQLite)
│   ├── hihapi.integrationtest/    # Integration tests (xUnit + WebApplicationFactory)
│   └── hihapi.test.common/        # Shared test data setup
```

## API Endpoints

The API exposes OData v4 endpoints accessible via two route prefixes: the default (`/`) and versioned (`/v1`).
All controllers follow the OData convention: `GET /{Controller}`, `GET /{Controller}({key})`, `POST /{Controller}`, `PUT /{Controller}({key})`, `PATCH /{Controller}({key})`, `DELETE /{Controller}({key})`.

> **Authentication**: Endpoints marked with 🔒 require a valid JWT Bearer token in the `Authorization` header.

### Blog

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `BlogCollections` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `PATCH({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `BlogFormats` | `GET`, `GET({key})` | — | — |
| `BlogPostCollections` | `GET` | — | — |
| `BlogPosts` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})`, `GET Deploy({key})`, `GET ClearDeploy({key})` | 🔒 | Query / Body |
| `BlogPostTags` | `GET`, `GET({key})` | 🔒 | Query / Body |
| `BlogUserSettings` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `GET Deploy({key})` | 🔒 | Body |

### Common

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `Currencies` | `GET`, `GET({key})` | 🔒 | — |
| `DBVersions` | `GET`, `GET({key})`, `POST`, `GET GetRepeatedDates2(...)`, `POST GetRepeatedDates`, `POST GetRepeatedDatesWithAmount`, `POST GetRepeatedDatesWithAmountAndInterest` | 🔒 | — |
| `Languages` | `GET`, `GET({key})` | — | — |

### Event

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `NormalEvents` | `GET`, `GET({key})`, `POST`, `DELETE({key})`, `POST Recalculate` | 🔒 | Query / Body |
| `RecurEvents` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |

### Finance

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `FinanceAccounts` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `PATCH({key})`, `DELETE({key})`, `POST CreateLegacyLoanAccount`, `POST CloseAccount`, `POST SettleAccount` | 🔒 | Query / Body |
| `FinanceAccountCategories` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceAssetCategories` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceControlCenters` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `PATCH({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceDocumentItems` | `GET` | 🔒 | Query |
| `FinanceDocumentItemViews` | `GET` | 🔒 | Query |
| `FinanceDocuments` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `PATCH({key})`, `DELETE({key})`, `POST PostDPDocument`, `POST PostLoanDocument`, `POST PostAssetBuyDocument`, `POST PostAssetValueChangeDocument` | 🔒 | Query / Body |
| `FinanceDocumentTypes` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceOrders` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `PATCH({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceOrderSRules` | `GET` | 🔒 | Query |
| `FinancePlans` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Query / Body |
| `FinanceReports` | `POST GetReportByTranType`, `POST GetAccountBalance`, `POST GetAccountBalanceEx`, `POST GetReportByAccount`, `POST GetReportByControlCenter`, `POST GetReportByOrder`, `POST GetFinanceOverviewKeyFigure`, `POST GetReportByTranTypeMOM`, `POST GetReportByAccountMOM`, `POST GetReportByControlCenterMOM` | 🔒 | Body (required) |
| `FinanceTmpDPDocuments` | `GET`, `POST PostDocument` | 🔒 | Body |
| `FinanceTmpLoanDocuments` | `GET`, `POST PostRepayDocument`, `POST PostPrepaymentDocument` | 🔒 | Body |
| `FinanceTransactionTypes` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Query / Body |

### Home

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `HomeDefines` | `GET`, `GET({key})`, `POST`, `PUT({key})`, `DELETE({key})` | 🔒 | Key |
| `HomeMembers` | `GET`, `GET({key})` | 🔒 | Implicit (user's homes) |

### Library

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `LibraryBooks` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryBookBorrowRecords` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryBookCategories` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryBookLocations` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryOrganizations` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryOrganizationTypes` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryPersons` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |
| `LibraryPersonRoles` | `GET`, `GET({key})`, `POST`, `DELETE({key})` | 🔒 | Query / Body |

### Other

| Controller | Methods | Auth | Home ID |
|---|---|---|---|
| `PhotoFile` | `GET`, `POST Upload`, `DELETE({key})` | 🔒 | — |
| `WeatherForecast` | `GET` | — | — |

# Author
**Alva Chien (Hongjun Qian) | 钱红俊**

A programmer, and a certificated Advanced Photographer.  
 
Contact me:

1. Via mail: alvachien@163.com. Or,
2. [Check my website](http://www.alvachien.com). 


# Licence
MIT
