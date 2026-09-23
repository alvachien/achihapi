---
name: bump-db-version
description: Bump the achihapi database SCHEMA version (DatabaseSeeder.CurrentVersion) and register the matching idempotent SchemaUpgrade step. Use whenever the user asks to bump the DB/database/storage/schema version, add a schema upgrade or migration step, or mentions T_DBVERSION - and note this is NOT the app version (that is bump-project-version).
---

# Bump DB Version

This bumps the **integer database schema version** owned by `DatabaseSeeder`, not the app version (`hihapi.csproj` `<Version>` - that's the `bump-project-version` skill) and not the UI version.

Both edit sites live in **one file**: `src/hihapi/Utilities/DatabaseSeeder.cs`

| Site | What |
|---|---|
| `CurrentVersion` constant | the schema version in code |
| `SchemaUpgrades` array + a step method | the delta that brings old databases to that version |

Never write a delta `.sql` file - `Sqls/Delta/v1-v21.sql` are frozen history and are never executed. The step registry **is** the migration mechanism.

## Why bump and step are inseparable

A drift-guard unit test (`test/hihapi.test/UnitTests/Utility/DatabaseSeederTest.cs`) asserts that every version above `LegacyBaselineVersion` (21) up through `CurrentVersion` has a registered step, and that the highest step equals `CurrentVersion`. So bumping the number without registering a step **fails the test** - never do a "bare" version bump.

## Steps

1. **Determine the target version.**
   - If the user supplies a version (e.g. `25`), use it. If it is not `CurrentVersion + 1`, warn them that the drift guard requires a step for **every** version in between - a skipped number needs a registered no-op step, so gaps should be avoided.
   - If not specified, bump automatically: `CurrentVersion + 1`.

2. **Ask the user for the delta logic (human-in-loop - required).** You cannot invent schema changes. Ask what this version actually delivers: new table/column, data backfill, seeded rows - the concrete DDL/DML intent. Phrase the question with AskUserQuestion or plain dialog, and wait for real input before writing the step. Only if the user explicitly wants a placeholder with no change, register a no-op step (`_ => { }`) - and say so clearly in its Description.

3. **Register the step** in `SchemaUpgrades`, following the v22 entry as the template:
   ```csharp
   new(23, new DateTime(YYYY, M, D), "Short human-readable delta description",
       EnsureXxxSchema),
   ```
   Use today's date (or the user's feature date) for `ReleasedDate`.

4. **Implement the step method** as `private static void EnsureXxxSchema(hihDataContext context)`, using `context.Database.ExecuteSqlRaw(...)`. **Idempotency is mandatory** - a crash between a step and its version row re-runs it on next start:
   - `CREATE TABLE IF NOT EXISTS ...` for new tables (mirror the `[Table]`/`[Column]` annotations in the model).
   - SQLite has no `ADD COLUMN IF NOT EXISTS` - guard with the file's existing `ColumnExists(context, table, column)` / `TableExists(context, name)` helpers.
   - Data backfills (e.g. `UPDATE ... SET`) go inside the same guard as the column they fix.
   - If the delta seeds rows into config tables, check per-row with `Any(...)` first, and never claim a low ID that pre-bump home rows may already hold (see the ID-range convention in CLAUDE.md).

5. **If the delta is a whole new entity**, the bump is only one of four schema sites - also register the entity in `Models/EdmModelBuilder.cs`, add the `DbSet` to `hihDataContext`, add the test DDL to `test/hihapi.test.common/DataSetupUtility.cs`, and wire the HomeDefines delete-cascade if it is Home-scoped. Keep the reference scripts in `src/hihapi/Sqls/` consistent if touched.

6. **Bump the `CurrentVersion` constant** last, so the array and the number are never edited half-and-half.

7. **Verify** (ask first - same human-in-loop convention as `bump-project-version`): on approval, run
   ```bash
   dotnet test --filter DisplayName~DatabaseSeederTest
   ```
   from the `achihapi/` directory; it is the direct guard for the invariant this skill maintains. Report failures with the raw output.

8. **Commit only if the user explicitly asks.** Never mix this into an app-version bump commit.
