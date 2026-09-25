# H.I.H. API (achihapi) — Design Document

> **Status:** Current, derived from the codebase on **2026-09-20** (app version `1.8.397`, DB schema version `24`).
> This is the only design document of the project. Review-era records live in [`archived/`](./archived/).

---

## 1. Purpose & System Context

`achihapi` is the OData v4 Web API backend of **H.I.H. (Home Information Hub)**, a personal/family
information platform (finances, library, events, blog). It is one of three cooperating services:

```
acidserver (OIDC identity, HTTPS :7228 dev)
     ▲ issues JWTs                        achihui (Angular UI, HTTPS :29521 dev)
     │                                    ─ presents the JWT on every call ─▶ achihapi
     └──────── validates tokens against its own authority ────────────────────┘
```

| | Development | Production |
|---|---|---|
| API base URL | `https://localhost:44360` (HTTP 25688) | `https://www.alvachien.com/hihapi` |
| Identity authority | `https://localhost:7228` | `https://www.alvachien.com/idserver` |
| JWT audience | `api.hih` | `api.hih` |

The API is **stateless** with respect to the UI: every request carries a Bearer token; tenancy and
authorization decisions are made per request from the token's identity claim plus the `HomeID` scoping
convention (§7).

## 2. Technology Stack

| Concern | Choice | Version |
|---|---|---|
| Framework | ASP.NET Core, target `net10.0`, minimal-hosting `Program.cs` (top-level statements, 182 lines) | .NET 10 |
| API protocol | OData v4 (`Microsoft.AspNetCore.OData`) | 9.4.1 |
| Persistence | EF Core + SQLite (single file `Data Source=hih.db`), `Microsoft.EntityFrameworkCore.Sqlite` / `Microsoft.Data.Sqlite` | 10.0.10 |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` (OIDC JWT validation against acidserver) | 10.0.10 |
| Logging | Serilog (`Serilog.AspNetCore`, `Serilog.Sinks.File`) | 10.0.0 / 7.0.0 |
| Analysis | Roslyn + IDE analyzers + `Meziantou.Analyzer` | 2.0.214 |
| Tests | xUnit v3, Moq, `Microsoft.AspNetCore.Mvc.Testing`, coverlet | see §13 |

Release builds define `USE_ALIYUN` — **currently vestigial**: no source file references it (verified by grep 2026-09-19).

## 3. Solution Layout

```
achihapi.sln
├── src/hihapi/
│   ├── Program.cs                  # host composition: services, pipeline, seeding
│   ├── Controllers/                # Finance/ Home/ Library/ Common/ (Blog/ Event/ — disabled, kept)
│   │                               # + PhotoFileController (plain MVC)
│   ├── Models/                     # POCOs mirroring the controller domains + EdmModelBuilder.cs
│   ├── DataContext/hihDataContext.cs  # single DbContext (607 lines)
│   ├── Utilities/                  # DatabaseSeeder, CommonUtility, ErrorHandlingMiddleware,
│   │                               # LibraryNameGuard/NameGuardLock (per-home name uniqueness),
│   │                               # BlogDeployUtility
│   ├── Exceptions/                 # BadRequest / NotFound / DBOperation / Unauthorized
│   ├── Extensions/                 # ODataEndpointController ($odata debug route)
│   └── Sqls/                       # reference-only schema SQL; Delta/v1-v21.sql embedded, never executed
└── test/
    ├── hihapi.test/                # unit tests (xUnit + Moq, shared in-memory SQLite)
    ├── hihapi.integrationtest/     # OData-level tests via WebApplicationFactory
    └── hihapi.test.common/         # shared DDL + in-memory datasets (DataSetupUtility)
```

## 4. Startup & Runtime Architecture

### 4.1 Composition order (`Program.cs`)

1. **Serilog** configured per environment (dev: console/Information; prod: daily-rolling file
   `../Logs/hihapi/log-.txt`, Warning, 14 files retained; other envs: silent).
2. **Upload folders** created: `<ContentRoot>/data/uploads`, `<ContentRoot>/data/blogs` (path-traversal-guarded).
3. **EF Core** `AddDbContext<hihDataContext>` with `ConnectionStrings:DefaultDB` (registered only when non-empty).
4. `AddHttpContextAccessor`, then `AddControllers().AddJsonOptions(JsonStringEnumConverter).AddOData(...)`
   with query allow-list `Count/Filter/Expand/Select/OrderBy`, `SetMaxTop(100)`, and **one EDM model bound
   to two prefixes** — `""` and `"v1"`.
5. **JWT Bearer** — registered *only* inside Development/Production branches (differing only in
   `RequireHttpsMetadata`); authority/audience read from config with code fallbacks. Any other
   environment (e.g. test) registers **no authentication** — test hosts supply their own.
6. **CORS** policy `"_myAllowSpecificOrigins"`: configured origins, any header, `GET/POST/PUT/PATCH/DELETE`, credentials.
7. **Fallback authorization**: `RequireAuthenticatedUser()` — *everything* is authenticated-by-default;
   exposure requires explicit `.AllowAnonymous()`.
8. Response caching, memory cache (registered; see §10.2), health checks.
9. **Before the pipeline is built**, `app.Services.CreateScope()` → `DatabaseSeeder.SeedAsync(db)`:
   seeding failure aborts startup (fail-fast) and the port never opens (§8).

### 4.2 Middleware pipeline (exact code order, verified 2026-09-19)

```
UseDeveloperExceptionPage (dev) / UseHsts (else)
→ UseSerilogRequestLogging
→ inline security headers: X-Content-Type-Options=nosniff, X-Frame-Options=DENY,
  Referrer-Policy=strict-origin-when-cross-origin, X-XSS-Protection=1; mode=block
→ UseMiddleware<ErrorHandlingMiddleware>
→ UseHttpsRedirection            (unconditional)
→ UseODataBatching
→ UseResponseCaching
→ UseRouting
→ UseCors(_myAllowSpecificOrigins)
→ UseAuthentication → UseAuthorization
→ MapControllers; MapHealthChecks("/health").AllowAnonymous()
```

`ErrorHandlingMiddleware` sits before the MVC/OData handlers so every exception path funnels into one
error contract (§10.1). Do not reorder — both CLAUDE.md files document this same order.

## 5. API Surface Design

### 5.1 Conventions

- **EDM-first, convention-bound.** `Models/EdmModelBuilder.cs` uses `ODataConventionModelBuilder`
  (namespace `hihapi.Models`), `EntitySet<T>("Name")` per entity; controllers carry **no** `[Route]`
  attributes and bind to entity sets by name — so every endpoint exists at both `/` and `/v1`.
- **Standard CRUD template** per set: `GET /{set}`, `GET /{set}(key)`, `POST`, `PUT(key)`, `PATCH(key)`, `DELETE(key)`.
- Enums are serialized as **strings** (`JsonStringEnumConverter` wire format, enum member names) and
  declared with `EnumType<>` in the EDM: `HomeMemberRelationType`, `FinanceAccountStatus`, `RepeatFrequency`,
  `LoanRepaymentMethod`, `FinancePlanTypeEnum`, `LibraryBookReadingStatus`.
- Date-only properties are `.AsDate()` (`Edm.Date`): `FinanceDocument.TranDate`, `FinanceAssetDepreciationResult.TranDate`,
  `LibraryBookReadingRecord.FromDate/ToDate`.
- Report/key-figure types are `EntityType` with composite keys (e.g. `FinanceReportByAccount {HomeID, AccountID}`)
  and **no entity set** — they are returned via `ReturnsFromEntitySet`/action payloads, not queried directly.

### 5.2 Domains and operations

**Common** — `Currencies`, `Languages` (CRUD reference data; `HomeID = NULL` rows are system-shared),
and `DBVersions`: CRUD over `T_DBVERSION`, plus `POST /DBVersions` (**AllowAnonymous**) returning
`{ StorageVersion, APIVersion }` — the actually-stored schema version and the assembly version.
`DBVersionsController` additionally hosts the four **unbound** repeat-date functions
(`GetRepeatedDates`, `GetRepeatedDates2`, `GetRepeatedDatesWithAmount`, `GetRepeatedDatesWithAmountAndInterest`)
that serve Finance plans/loan scheduling to the UI.

**Home** — `HomeDefines` (CRUD; the tenant aggregate), `HomeMembers` (read-only `GET`, scoped to the
caller's memberships). Delete safety is model-side: `HomeDefine.IsDeleteAllowed` counts referencing rows
across ~11 DbSets (§6.2).

**Finance** (16 controllers) — accounts and categories, documents/items/types, orders and S-rules,
control centers, plans, asset categories, three temporary document sets, and read views.
Domain logic lives in **bound actions/functions**:

| Set | Operations |
|---|---|
| `FinanceAccounts` | `CloseAccount`, `SettleAccount` (actions). *(Also defines `CreateLegacyLoanAccount` in the controller but not registered in the EDM — currently unreachable.)* |
| `FinanceDocuments` | function `IsChangable`; actions `PostDPDocument`, `PostLoanDocument`, `PostAssetBuyDocument`, `PostAssetSellDocument`, `PostAssetValueChangeDocument`, `PostAssetDepreciationDocument`, `GetAssetDepreciationResult` — the document-posting pipeline that mutates account balances |
| `FinanceTmpDPDocuments` / `FinanceTmpLoanDocuments` | staging: `PostDocument` / `PostRepayDocument`, `PostPrepaymentDocument` |
| `FinanceReports` | 15 actions, POST-only with `HomeID` in the body: balance/MOM/segment reports by transaction type, account, control center, order; cash flow (`GetCashReport*`, `GetDailyCashReport`), income & expense (`Get*StatementOfIncomeAndExpense*`), `GetFinanceOverviewKeyFigure` |

**Library** (9 controllers) — books plus category/location/person/organization registries; many-to-many
links (authors, translators, presses, categories, locations) materialized as explicit linkage entities.
Actions: `LibraryBooks` → `GetLibraryOverviewKeyFigure`; `LibraryBookReadingRecords` →
`CompleteReading` / `AbortReading` (shared `FinalizeReadingAsync`; the newest entity, schema v22).

**Non-OData** — `PhotoFileController` (`[Route("api/PhotoFile")]`): photo listing/serving with
`[AllowAnonymous]` GET serving, `[Authorize]` upload/delete. `ODataEndpointController` adds
`GET /$odata` — an HTML route inventory, **Development-only** (404 elsewhere) and authenticated (§11).

**Disabled (temporarily, 2026-08-02)** — Blog (`BlogFormats`, `BlogUserSettings`, `BlogCollections`,
`BlogPosts`, `BlogPostCollections`, `BlogPostTags` + `Deploy`/`ClearDeploy`) and Event
(`NormalEvents`, `RecurEvents` + `MarkAsCompleted`/`GenerateNormalEvents`) entity sets are commented out
in `EdmModelBuilder.cs` with dated re-enable headers; their controllers/models/DbSets/tables are untouched,
so those endpoints **404** at the EDM level.

## 6. Data Layer Design

### 6.1 Single DbContext, annotation-first mapping

`hihDataContext` is the sole context (SQLite). Mapping is **attribute-driven**: models carry
`[Table("T_FIN_ACCOUNT")]`, `[Key]`, `[Column("HID", TypeName="INT")]`, `[MaxLength]` — legacy uppercase
`T_*` / abbreviated-column schema. Fluent `OnModelCreating` adds only what attributes cannot express:

- `HasDefaultValueSql("CURRENT_DATE")` on audit timestamps (repeated per entity — deliberately no global convention);
- `HasColumnType("INTEGER").ValueGeneratedOnAdd()` on config-table keys so system rows can carry **explicit IDs < 1000** (§9.2);
- composite keys (`HomeMember{HomeID,User}`, `FinanceDocumentItem{DocID,ItemID}`, linkage tables), named FK
  constraints (`FK_t_*`) incl. 1:1 split cascades for `FinanceAccountExtra{DP,AS,Loan}`, and the Library
  many-to-many via five `UsingEntity<TLinkage>` join entities;
- ten **read-only report views** (`V_FIN_*`: document-item flattening, account/control-center/order
  groupings, balance rollups) mapped `HasNoKey().ToView(...)`. The views are **not** migrations — they are
  (re)created every startup by `DatabaseSeeder.SeedViews` (`DROP VIEW IF EXISTS` + `CREATE VIEW`).

There are **no EF Core migrations** by design; the schema lifecycle is entirely the seeder's (§8–9).

### 6.2 Model conventions

All domain entities derive from `Models/Common/BaseModel` (abstract): audit fields
`Createdby/CreatedAt/Updatedby/UpdatedAt` plus **virtual business-validation hooks**
`IsValid(hihDataContext)` and `IsDeleteAllowed(hihDataContext)` — so validity and delete-safety are
entity-local rules executed by controllers, not scattered validation attributes.

## 7. Tenancy & Authorization Model

- **Authentication** is OIDC JWT Bearer: signature/issuer/audience/expiry against the acidserver authority.
  The user identity is the immutable `sub` claim (`HIHAPIUtility.GetUserID` deliberately falls back to
  `NameIdentifier`, never the mutable `name` claim — documented fix, 2026-06-24).
- **Authorization has no roles/policies**: the fallback policy proves identity; *tenancy* is enforced in
  controller code by the **canonical HomeID ownership pattern**:
  1. `ModelState` gate → `BadRequestException` (400)
  2. resolve `usrName` from claims; empty → 401
  3. for updates, **load the existing row first**; missing → 404
  4. membership check **against the existing row's `HomeID`** (`HomeMembers WHERE HomeID = row.HomeID AND User = me`) → 401 if absent — never against the body's `HomeID` (anti mass-assignment)
  5. reject `HomeID` changes on PUT → 400
  6. `entity.IsValid(_context)`; stamp audit fields; save; return `Created`/`Ok`
- Reference-data sets allow `HomeID = NULL` (system-shared). `HomeDefines`' key **is** the home ID.
- Known fragility: isolation is per-controller, not a global EF query filter; action endpoints taking
  `(HomeID, entityID)` pairs were the historical IDOR source (fixed; see archived 2026-08-12 review).
  Defense-in-depth via `HasQueryFilter` remains a proposed option.
- Anonymous exceptions: `POST /DBVersions` (version probe), `GET /Languages` (login page),
  `GET api/PhotoFile/{file}` (image serving), `/health`.

## 8. Database Version Design

The DB **schema version** is an integer owned solely by `DatabaseSeeder.CurrentVersion`
(`src/hihapi/Utilities/DatabaseSeeder.cs`), recorded per application in table `T_DBVERSION`
(`VersionID` + `ReleasedDate` + `AppliedDate`). It is an independent counter from the app/assembly
version and the UI version (§15).

### 8.1 Baseline — frozen history at v21

**`LegacyBaselineVersion = 21` is the baseline.** Everything at or below it is frozen history and its SQL
is **no longer supported** as a going-forward path:

- `Sqls/Delta/v1.sql … v21.sql` belong to the old SQL Server delta era (frozen 2022-09-30); they are
  embedded resources that are **never executed** against SQLite. A pre-upgrade database with an empty
  `T_DBVERSION` is baselined straight to `LegacyBaselineVersion = 21` and only steps *above* 21 run.
- The registered steps are **v22** (`EnsureReadingRecordSchema` — book reading-records table + `STATUS`
  column), **v23** (`EnsureEducationBookCategories`) and **v24** (`EnsureBookCopyCountSchema`), and
  `CurrentVersion` is **24**. Each is maintained only as a catch-up path for straggler databases below
  it; **no new work may depend on modifying an existing step's logic** — the next schema change is a new
  step (§8.2), never an edit to one already released.
- Retirement is per-step and only ever for the **oldest** registered step: once every deployed database
  (dev and `www.alvachien.com`) is verified past it, raise `LegacyBaselineVersion` to that step's version
  and drop the step — the drift-guard test (§13) then only demands steps for versions **above** the new
  baseline. **v22 is the current retirement candidate** (raise the baseline to 22 and drop the v22 step).

Fresh databases never run any historical step: `EnsureCreatedAsync()` builds the *current* schema from
the model and `CurrentVersion` is stamped directly.

### 8.2 One-by-one upgrade execution

With each new version, upgrades execute **step by step, in order** (`ApplySchemaVersions` at startup):
the stored `MAX(VersionID)` is compared against `CurrentVersion`; every registered
`SchemaUpgrade(Version, ReleasedDate, Description, Action<hihDataContext> Apply)` with a higher version
runs exactly once, immediately followed by its `T_DBVERSION` stamp row — so a crash between step and
stamp **re-runs the same step** (never skips it), which is why every step is written **idempotently**
(`CREATE TABLE IF NOT EXISTS`, `ColumnExists`/`TableExists` pragma guards, per-row `Any()` checks before
inserts — the v22 step shows the pragma guards, the v23 step the per-row insert check). A throwing step
**aborts startup** (fail-fast): the API never
serves against a half-migrated database.

The contract for adding a schema change is exactly two edits in `DatabaseSeeder.cs`: **register one step +
bump `CurrentVersion`** — never a delta `.sql` file. The contract is enforced **twice**: a *startup guard*
in `ApplySchemaVersions` aborts (fail-fast) when the highest registered step ≠ `CurrentVersion`, or when
the stored maximum still doesn't equal `CurrentVersion` after the catch-up loop; and the *CI drift-guard
test* (§13) catches the same mismatch before commit. Two project skills encode the contract:
`.claude/skills/bump-db-version` (schema) and `bump-project-version` (app version).

### 8.3 Two-part upgrade design — API startup (implemented) & UI-required (planned)

The design places upgrade execution in **two parts**; today only the first is built:

1. **API startup — implemented, and currently the *only* execution point.** Every
   `DatabaseSeeder.SeedAsync` run (before the web pipeline is built, §4.1.9) performs the
   compare-and-catch-up of §8.2. All schema changes happen here, transparently, with fail-fast.
2. **UI-required upgrade — design intent, NOT YET IMPLEMENTED.** The target design adds a second part:
   upgrades driven by what the UI requires (version handling initiated from the client side), instead of
   relying solely on the API process having restarted. Its mechanism is deliberately undecided here —
   until it is built and documented in this section, **no UI code path may alter schema**, and nothing in
   the UI triggers migration.
3. **What the UI does today (read-only).** After login, `AppComponent` calls `POST /DBVersions`
   (AllowAnonymous), which reports the actually stored `Max(VersionID)` plus the API assembly version as
   `{ StorageVersion, APIVersion }`; the About page displays both. This loop lets a human see "the
   deployed DB has not caught up with the code" — which a successful boot now *hard-guarantees* against
   (the `ApplySchemaVersions` startup guard, §8.2): the probe is what proves the guarantee holds on a
   given box.

## 9. Reference Seeding & ID-Range Reservation

### 9.1 Idempotent seed passes

After `EnsureCreated`, `Seed*` passes insert system reference rows (currencies, languages, account/asset
categories, document & transaction types, person roles, organization types, book categories) — they
populate fresh databases and are no-ops on existing ones. Most are guarded by a table-wide `Any()` check,
which is why those passes are **not** a delivery path for later system rows (they skip existing
databases). The exception is `SeedLibraryBookCategories`, which checks ids **per row**: the v23 step
inserts system rows into the same table, so a table-wide guard reads those as "already seeded" and skips —
which left a database whose category table was empty (a first start that aborted before the pass was
saved) holding the step's seven subjects and none of the base categories, all parented to a root that was
never delivered. Per-row checking keeps the set complete wherever the pass runs, in either order relative
to the steps, and leaves an id already claimed to its owner (`DatabaseSeederTest`:
`TestCase_EducationStepDoesNotStrandTheBaseCategorySeed`).

### 9.2 Config ID ranges

System-delivered config rows use explicit IDs **below 1000**; home-created rows get IDs **≥ 1000**.
`ReserveCustomerIdRanges` runs at the end of `SeedAsync` (after `SaveChangesAsync`, because the seed
inserts are what create `sqlite_sequence`) and for the seven home-bounded config tables executes a
monotone, idempotent `UPDATE sqlite_sequence SET seq = CASE WHEN seq < 1000 THEN 1000 ELSE seq END`
(with an INSERT when the sequence row is absent), so IDs generated from then on never collide with
system rows. `HomeID IS NULL` remains the authoritative system/home marker — pre-bump home rows on old
databases keep their low IDs, which is why **a new system row must be delivered by an `Any()`-checked
`SchemaUpgrade` step** (§8.2), never appended at a "free" low ID.

## 10. Cross-Cutting Design

### 10.1 Error contract

`ErrorHandlingMiddleware` is the single funnel. Mapping (`Utilities/ErrorHandlingMiddleware.cs:34-38`):

| Thrown | Status | Body |
|---|---|---|
| `NotFoundException` | 404 | `{"error": "<message>"}` (echoed) |
| `UnauthorizedException` / BCL `UnauthorizedAccessException` | 401 | echoed |
| `BadRequestException` | 400 | echoed |
| `DBOperationException` | 400 | echoed (callers must not pass raw `ex.Message` — flagged in the 2026-08 review) |
| anything else | 500 | fixed `"An internal server error occurred."` — exception detail is logged, never leaked |

Controllers signal failures by throwing these (defined in `Exceptions/`, sealed, `message+inner` ctors);
the generic-BCL `UnauthorizedAccessException` is the dominant 401 vehicle.

### 10.2 Caching

Only **HTTP response caching**, deliberately narrow: `[ResponseCache]` on the read-only reference GETs —
`Currencies` (3600 s), `DBVersions` GETs (3600 s), `Languages` (86400 s), `PhotoFile` serving (864000 s).
`AddMemoryCache()` is registered but **unused** in current code — no in-memory cache key families exist
anywhere. Report endpoints are uncached hot paths — an acknowledged trade-off, not an oversight to "fix"
silently.

### 10.3 Logging

Serilog request logging for every call (pipeline step 2); prod writes daily-rolling Warning-level files
(14 days) under `../Logs/hihapi/`; dev logs to colored console at Information. Handled 4xx log Warning,
unhandled 5xx log Error.

## 11. Operational & Debug Surfaces

- `GET /health` — anonymous health-check endpoint (`MapHealthChecks`).
- `GET /$odata` — Development-only authenticated HTML dump of every registered endpoint
  (`Extensions/ODataEndpointController.cs`), including OData metadata (return types, HTTP methods, route templates).
- `POST /DBVersions` — anonymous live probe of stored schema version + API version (drives the UI About page, §8.3).

## 12. Configuration Design

Three files, base + overlays (`src/hihapi/`): the base carries only `Logging` — **connection string,
`Auth:*` and `Cors:AllowedOrigins` live exclusively in the Development/Production overlays**, and
`Program.cs` holds hardcoded fallbacks (note: the code fallback authority `https://localhost:44353` is
stale; the dev overlay's `https://localhost:7228` wins). Serilog levels are code-configured, not config-driven.
Production additionally pins `AllowedHosts: www.alvachien.com`. No IISEnvironment overlay exists for the
API (that is an acidserver convention).

## 13. Test Architecture

| Project | Role |
|---|---|
| `test/hihapi.test` (`hihapi.unittest`) | Unit tests, xUnit v3 + Moq. `SqliteDatabaseFixture` keeps one open `DataSource=:memory:` connection, builds the schema via `DataSetupUtility.CreateDatabaseTables/Views`, exposes `GetCurrentDataContext()` (`UseRelationalNulls`, TrackAll). Layout mirrors domains (`UnitTests/{Common,Finance,Home,Library,Blog,Models,Utility}`), method naming `TestCase_*`. |
| `test/hihapi.integrationtest` | End-to-end OData via `CustomWebApplicationFactory<TProgram> : WebApplicationFactory<Program>` — same in-memory SQLite bootstrapping plus its own auth substitution (necessary because non-dev/prod envs register no JWT, §4.1.5). Exercises real EDM/query behavior (e.g. `LibraryBookReadingRecordsODataTest` — `in` queries, `Edm.Date`). |
| `test/hihapi.test.common` | Shared library (no test SDK): `DataSetupUtility` = hand-written DDL (tables **and** views — a second schema site that must stay in sync, per the new-entity checklist) + fixed datasets (users A–D, Home1–5 membership matrix). |

Notable guard tests: `DatabaseSeederTest` — fresh DB stamps `CurrentVersion`; unversioned DB baselines at 21 and catches up; **`TestCase_EveryVersionAboveBaselineHasAStep`** drift guard
(`test/hihapi.test/UnitTests/Utility/DatabaseSeederTest.cs:164-173`): asserts `CurrentVersion` equals the
highest registered step **and** every version in `(LegacyBaselineVersion, CurrentVersion]` has a step —
the CI half of §8.2's double enforcement (the startup guard covers the runtime half). The main csproj
grants `InternalsVisibleTo` to both test assemblies so internals (`SchemaUpgrades`, `HIHAPIUtility`) are
testable.

## 14. Code Quality & CI

`EnableNETAnalyzers` + `AnalysisLevel=latest` + `EnforceCodeStyleInBuild` + `GenerateDocumentationFile`
in all three shipping csprojs; Meziantou.Analyzer everywhere; XML-doc warnings (CS1591/CS157x/CS158x)
suppressed via `.editorconfig`. CI (`.github/workflows/build-test.yml`) builds and tests on .NET 10.0.x
per push with coverage. Publish is FDD `win-x64` decided in the sibling repo's `publish-hih-all.ps1`,
never in the csproj (keeps CI host-agnostic).

## 15. Versioning — Three Independent Counters

| Counter | Lives in | Bumped by |
|---|---|---|
| App version `1.8.397` | `src/hihapi/hihapi.csproj` `<Version>` | `bump-project-version` skill / `publish-hih-all.ps1` (running number, not strict semver) |
| **DB schema version `24`** | `DatabaseSeeder.CurrentVersion` (§8) | `bump-db-version` skill — always paired with a `SchemaUpgrades` step (test-enforced) |
| UI version (e.g. `1.8.550`) | achihui `package.json` + environment files | achihui's `bump-version` skill — **not** synced with either API counter; never assume they match |

## 16. Verified Document-vs-Code Discrepancies

Cross-checked 2026-09-19; `hih/CLAUDE.md` (root) agrees with the code on pipeline order and stack. Stale claims elsewhere:

| Claim | Where | Reality |
|---|---|---|
| Middleware order "Authentication → Routing → HTTPS redirect → Authorization → CORS" | `achihapi/CLAUDE.md` | Code order is §4.2 (root CLAUDE.md is correct) |
| "`hihDataContext.cs` (~26K)" | both CLAUDE.md files | 607 lines |
| "OData validators" in Utilities | both CLAUDE.md files | None exist; only `LibraryNameGuard`/`NameGuardLock` (name-uniqueness) |
| "USE_ALIYUN gates deployment behavior" | root CLAUDE.md, csproj comment | Constant defined on Release but referenced nowhere in `src/` |
| `CreateLegacyLoanAccount` | `FinanceAccountsController` | Present but not registered in the EDM → unreachable |

The CLAUDE.md files were corrected against this table on 2026-09-19. The two *code* anomalies
(`CreateLegacyLoanAccount` unregistered in the EDM; the stale `44353` code-fallback authority) remain in
code, deliberately untouched.
