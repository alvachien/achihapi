# achihapi — Code Review, uncommitted changes (2026-09-20)

Scope: working tree of `achihapi` on branch `chroe/keepimprv-4` — 16 modified tracked files plus
untracked `docs/api-design.md`, `docs/archived/*`, `test/hihapi.integrationtest/LibraryBooksCopyCountODataTest.cs`
and `.claude/skills/*`. Includes the documentation restructure.

Change under review: the Library books **copy-count** feature (`LibraryBook.CopyCount`,
`LibraryBooksController`, `LibraryOverviewKeyFigure.TotalCopies`, `DatabaseSeeder` v23/v24 steps,
schema SQL) plus a docs reorganization (`Design.md` and three prior review docs removed, `docs/api-design.md`
introduced).

> This review was re-run after an earlier attempt (session `712f6f9c`, 2026-09-20 19:20 local) was
> terminated by a `429 token-plan quota exhausted` error before producing any findings. The working
> tree was unchanged between the two runs (all source edits ≤ 15:34; failed attempt 18:59).

## Fix status

| # | Status | Verification |
|---|---|---|
| 1 | FIXED | `SeedLibraryBookCategories` now per-row checks ids instead of the table-wide `Any()` guard; `TestCase_EducationStepDoesNotStrandTheBaseCategorySeed` reproduces the stranded state (fails on the old guard, passes now); `DatabaseSeederTest` 10/10. Doc follow-through: `api-design.md` §9.1 now describes the per-row exception, §8.2 points at the v22/v23 steps for their respective patterns |
| 2 | FIXED | `LibraryBook.IsValid` rejects a negative `CopyCount`; POST and PUT now call it (`Not a valid object`); `TestCase_Post_RejectsNegativeCopyCount` + `TestCase_Put_RejectsNegativeCopyCount` (the PUT case also asserts the stored count survives the rejected write); `LibraryBooksControllerTest` 18/18 |
| 3 | FIXED | PUT keeps the recorded `CopyCount` when the payload omits it (any value sent, 0 included, still writes through); `TestCase_Put_WithoutCopyCountKeepsARecordedZero` added, and `TestCase_GetLibraryOverviewKeyFigure` now sets the counts *before* its linkage PUTs so it proves preservation; both fail with the preserve line disabled; `LibraryBooksControllerTest` 18/18 |
| 4 | FIXED | Status line now `1.8.397` / schema `24` / 2026-09-20; §8.1 rewritten — the baseline is `LegacyBaselineVersion = 21`, the registered steps are v22/v23/v24, and retirement applies only to the oldest step (v22 is the current candidate); §15 table updated |
| 5 | FIXED | §2: `Program.cs` 183 → 182 lines; §3 tree: `LibraryNameGuard`/`NameGuardLock` moved out of `Controllers/` (they live in `Utilities/`, which now lists all six files) |
| 6 | FIXED | `TotalBooks`/`TotalCopies` are derived from the materialized `linkedBooks` graph instead of their own `COUNT`/`SUM` round trips; the now-stale SQL-`SUM`-nullability note is gone; unit 408/408 + integration 5/5 |
| 7 | FIXED | Step 23 inserts through the EF add path like every other seed (the hand-built concatenated `INSERT` is gone, and with it the EF1003 build warning); `TestCase_EducationStepDeliversSubjectsAndReparentsOnAPopulatedDatabase` exercises both halves — the seven subjects and the two re-parent UPDATEs — on a populated pre-v23 database |

## Findings

| # | Severity | File | Line | Issue |
|---|---|---|---|---|
| 1 | HIGH | `src/hihapi/Utilities/DatabaseSeeder.cs` | 192 | v23 step's insert pre-empts the `Any()`-guarded `SeedLibraryBookCategories` pass → 7-row category tree |
| 2 | MEDIUM | `src/hihapi/Models/Library/LibraryBook.cs` | 141 | `CopyCount` invariant documented but unvalidated — accepts any `Int32` |
| 3 | MEDIUM | `src/hihapi/Controllers/Library/LibraryBooksController.cs` | 157 | PUT `SetValues` + no PATCH → omitted `CopyCount` writes NULL over a recorded 0 |
| 4 | MEDIUM | `docs/api-design.md` | 215 | Doc pins app 1.8.395 / schema v22; same change sets 1.8.397 / v24 |
| 5 | LOW | `docs/api-design.md` | 54 | §3 solution tree contradicts the doc's own §16 table; `Program.cs` line count wrong |
| 6 | LOW | `src/hihapi/Controllers/Library/LibraryBooksController.cs` | 358 | `TotalCopies` issues a dedicated `SUM` round trip although the rows are materialized below |
| 7 | LOW | `src/hihapi/Utilities/DatabaseSeeder.cs` | 196 | `EnsureEducationBookCategories` is the only hand-built-SQL seed path, and no test executes it |

### 1. HIGH — v23 insert makes the category seed pass skip permanently

`src/hihapi/Utilities/DatabaseSeeder.cs:192`

The v23 step inserts rows into `T_LIB_BOOKCTGY_DEF` *before* the table-wide `Any()`-guarded
`SeedLibraryBookCategories` pass (called at line 55, guard at line 665). On any database whose category
table is empty, the step's own 7 rows make that seed pass skip — permanently leaving only the 7 subjects
and none of the base system categories.

**Failure scenario:** startup on a pre-existing DB with an empty `T_DBVERSION` (baselined to 21), or any DB
whose `T_LIB_BOOKCTGY_DEF` is empty at startup (e.g. one whose first start aborted before the seed passes
were saved): `ApplySchemaVersions` runs step 23 first and inserts 42..48 (children of 41, re-parenting
61/62), then `SeedLibraryBookCategories()` sees `BookCategories.Any() == true` and returns immediately, so
ids 1-9/21/41/51/61/71/81/91 are never inserted. The run stamps 24, so every later start skips step 23
while the `Any()` guard still blocks the seed: the library category tree is permanently 7 rows whose
`PARID` 41 does not exist (no FK on `PARID` catches it) and the UI loses every base category. The existing
`TestCase_LegacyDatabaseBaselinesThenUpgrades` does not catch it because its category table is already
populated.

### 2. MEDIUM — `CopyCount` invariant documented, never enforced

`src/hihapi/Models/Library/LibraryBook.cs:141`

`CopyCount` carries a documented invariant (0 = retired, NULL = unrecorded, >0 = owned) but no validation
at all — `LibraryBook` never overrides `IsValid`, there is no `[Range]`, and `LibraryBooksController` never
calls `IsValid(_context)` (only `ModelState.IsValid`), so any `Int32` value is accepted.

**Failure scenario:** `POST/PUT {"CopyCount": -3}` (or a client-side off-by-one when a home gives away the
last copy) is stored as-is; `LibraryOverviewKeyFigure.TotalCopies`, documented and displayed as "physical
books on the shelf", then drops below `TotalBooks` or goes negative, and the retired-book query
`$filter=CopyCount eq 0` shows nothing for that row. The new integration/unit tests only cover 3, 0 and
NULL, so nothing detects it.

### 3. MEDIUM — PUT overwrites a recorded 0 with NULL

`src/hihapi/Controllers/Library/LibraryBooksController.cs:157`

PUT applies `CurrentValues.SetValues(update)` and there is no PATCH endpoint, so a payload that omits
`CopyCount` writes NULL over a recorded 0 — silently converting "retired" into "unrecorded, still owned",
the exact distinction the column was introduced for.

**Failure scenario:** a book retired with `CopyCount = 0` is edited by any client whose payload lacks the
field (the previously deployed achihui bundle, a script, or a PUT built from a `$select` that omitted
`CopyCount`): the 0 is overwritten with NULL, `$filter=CopyCount eq 0` stops listing the book, `TotalCopies`
gains a phantom copy, and the original value is unrecoverable. The unit test added in this change documents
the reset behaviour instead of preventing it, and the model comment ("or by a client that omits it") only
justifies it for newly created rows.

### 4. MEDIUM — design doc version claims contradict the code in the same change

`docs/api-design.md:215`

Line 3 and §15 (lines 361-362) pin "app version 1.8.395, DB schema version 22", and lines 215/222 assert
"the baseline is version 22" and that "the v22 step ... is the last registered step", while this same commit
sets `CurrentVersion = 24` (adding the v23/v24 steps) and `hihapi.csproj` `Version = 1.8.397`.

**Failure scenario:** a maintainer acting on §8.1's retirement path ("once every deployed database is
verified at 22, raise `LegacyBaselineVersion` to 22 and drop the v22 step") works from a false baseline,
and the §8.3/§15 description of what `POST /DBVersions` reports is off by two schema versions and two app
versions on every box (the About page now shows 24 / 1.8.397).

### 5. LOW — solution-tree map contradicts the doc's own table

`docs/api-design.md:54`

§3's solution tree lists `Utilities/` as "DatabaseSeeder, CommonUtility, ErrorHandlingMiddleware" and puts
`LibraryNameGuard`/`NameGuardLock` under `Controllers/`, contradicting the same doc's §16 table, which
correctly says those two classes are what `Utilities` contains; §2 (line 34) also claims `Program.cs` is
"183 lines" where it is 182.

**Failure scenario:** a maintainer chasing the per-home name-uniqueness rule that the `LibraryBooks` PUT
path depends on is sent to `Controllers/` and finds nothing, while the doc's own discrepancy table sends
them to `Utilities/` — the layout map this change introduces cannot be trusted for the two classes it calls
out by name, and the file-length figure is already wrong on arrival.

### 6. LOW — redundant `SUM` round trip per overview request

`src/hihapi/Controllers/Library/LibraryBooksController.cs:358`

`TotalCopies` adds a dedicated `SUM` round trip per overview request even though every book row of the same
home (`CopyCount` included) is materialized twenty lines below for the rankings, so the figure could be
derived from that materialized list.

**Failure scenario:** each Library overview load now issues `SELECT SUM(COALESCE(COPY_COUNT,1)) ...` on top
of the existing `COUNT` for `TotalBooks`, while `linkedBooks = Books.Where(HomeID == hid).Include(...).ToListAsync()`
already fetches the same rows with the same column; on a large home that is two extra full scans of
`T_LIB_BOOK_DEF` per page view for numbers that are already in memory.

### 7. LOW — only hand-built-SQL seed path, untested

`src/hihapi/Utilities/DatabaseSeeder.cs:196`

`EnsureEducationBookCategories` is the only seed path that writes rows with hand-built SQL string
concatenation instead of the EF add path every other seed uses, and no test ever executes it —
`DatabaseSeederTest` rolls the stamp back only to `CurrentVersion-1` so step 24 runs, and the drift guard
merely asserts a step exists.

**Failure scenario:** a later change to `LibraryBookCategory`'s mapping (renamed column, an added NOT NULL
column without a DB default, a changed audit default) leaves this INSERT compiling but failing at runtime;
because step 23 only ever runs on a pre-v23 database, the failure surfaces as an aborted startup (fail-fast)
on the one deployment that needs the upgrade, and CI cannot catch it because no test constructs a pre-v23
database.

## Verified OK (deliberately not flagged)

- The root and `achihapi/CLAUDE.md` claims fixed in this change (607-line `hihDataContext.cs`, stale `44353`
  fallback, vestigial `USE_ALIYUN`, corrected middleware order vs `Program.cs`) all match the code.
- The new `finalMax` startup guard and the CI drift guard agree.
- The v24 backfill / guarded-`ALTER` pattern and the `Int32?` SUM-nullability handling are correct.

## Operational notes (not defects)

- The new `docs/archived/*` and `docs/api-design.md` are **untracked**, so a `git add -u` commit would
  record only the deletions and break the doc's `archived/` links — they must be added explicitly.
- `.claude/settings.local.json` is excluded by the global gitignore, so it will not be committed with the
  skills it sits beside.

## Status of this file

Findings are as reported at review time; the fix state for each is in the Fix status table above. The
findings text below is left as written (line numbers are the pre-fix ones) so the fixes can be checked
against the defects they answer. Nothing here has been committed — all of it is still working-tree
changes.
