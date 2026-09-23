# Book Reading Records — Implementation Review & Remediation Log

> Date: 2026-09-12
> Scope: the reading-records lifecycle wave on `achihapi` (branch `chroe/keepimprv-2`)
> and `achihui` (branch `feat/library`), reviewed against the end-user guide
> [`book-reading-records-enhancement-test-2026-09-12.md`](./book-reading-records-enhancement-test-2026-09-12.md).
> Precedents: [`api-review-2026-08-12.md`](./api-review-2026-08-12.md), memory note `new-entity-schema-checklist`.

**Overall assessment**: solid, well-tested change. The lifecycle state machine is correctly
gated (terminal states, Reading-only finalize), the server owns `Status`/`User`, the overlap
logic handles all boundary shapes, the no-existence-leak philosophy is applied in `Get(key)`
and the finalize actions, the seeder upgrade is idempotent, and all four schema checklist
sites (EDM/DbSet, seeder DDL, test DDL, home-delete cascade) are covered. The findings below
are the gaps.

## Status summary

| # | Severity | Area | Finding | Status |
|---|---|---|---|---|
| A1 | HIGH | API | `Delete` leaks cross-home record existence (401 vs 404) | **FIXED** |
| A2 | MEDIUM | Doc | Test guide Part 2 action URLs will 404 (`/odata/` prefix, `Default.` namespace) | **FIXED** |
| A3 | MEDIUM | API | `Post` trusts client-supplied `Id` (explicit-key insert → possible 500 / sequence skew) | **FIXED** |
| A4 | LOW | API/DB | Legacy rows with `NULL FromDate` are permanently stuck; misleading finalize error | **FIXED** (dev DB audited clean; **prod audit pending**) |
| A5 | LOW | API | No update path (no PUT/PATCH); no reopen of finalized records | OPEN (proposed ACCEPTED — awaiting confirmation) |
| A6 | LOW | API | Overlap check is TOCTOU (concurrent writes can both pass) | **ACCEPTED** (code comment added) |
| A7 | INFO | API | `User` MaxLength(50) vs NVARCHAR(40); `T_LIB_BOOK_READING_RECORD` missing from `Sqls/DBSchema_Table.sql`; abort `ToDate` EDM parameter not nullable | **FIXED** (all 3 parts; see 2026-09-13 note on the `.Nullable` compile error) |
| U1 | HIGH | UI | No lifecycle surface: no Status column, no Complete/Abort actions, cannot start an open reading | **FIXED** (filter-dialog Status entry deferred, as planned) |
| U2 | MEDIUM | UI | Reader-name search matches token `User` ID, not the displayed `DisplayAs` (test-guide #9 fails as written) | **FIXED** |
| U3 | MEDIUM | UI | Create dialog silently no-ops when `onVerify()` fails (VerifiedMsgs never surfaced) | **FIXED** |
| U4 | MEDIUM | UI | Error modal shows raw JSON envelope instead of the server's message | **FIXED** |
| U5 | LOW | UI | Search inlines every title-matched `BookId` into the URL (length ceiling); no server-side title navigation | OPEN (proposed DEFERRED — awaiting confirmation) |
| U6 | INFO | UI | Stale list after home switch (app-wide pattern); date columns not sortable | PARTIAL: sorting **FIXED** via U1; home-switch **DEFERRED** (proposed, awaiting confirmation) |

Statuses: `OPEN` → `IN PROGRESS` → `FIXED` (code + tests) / `ACCEPTED` (deliberate, documented) / `DEFERRED` (with rationale).

---

## Findings detail

### A1 — HIGH: `Delete` leaks cross-home record existence
`Get(key)` and `FinalizeReadingAsync` deliberately render "exists but not in your home" as
**404** ("no existence leak" — per their own code comments), but `Delete` distinguishes:
missing → 404, exists-in-another-home → 401. Any authenticated user can probe sequential IDs
and enumerate which record IDs exist across all homes — the same pattern the 2026-08-12 API
review flagged for Finance (`CloseAccount`/`SettleAccount`).

Unit test `Delete_ByNonMember_Rejected` pins the current behavior, so it looks intentional —
but it contradicts the rest of the controller.

**Fix**: membership-join the lookup (like `Get(key)`) and return `NotFound()` for both
"missing" and "not your home's record"; update the unit test accordingly.

### A2 — MEDIUM: test guide Part 2 URLs will 404
The guide instructs testers to call
`https://localhost:44360/odata/LibraryBookReadingRecords/Default.CompleteReading`:
1. There is no `/odata` route prefix — `Program.cs` registers route components for `/` and `/v1`.
2. The EDM namespace is `hihapi.Models` (`modelBuilder.Namespace = typeof(Currency).Namespace`),
   not `Default`; a qualified call would be `hihapi.Models.CompleteReading`.

The integration test proves the bare route works:
`POST /LibraryBookReadingRecords/CompleteReading` — matching the existing UI convention
(`${accountAPIUrl}/CloseAccount` in `finance-odata.service.ts`).

**Fix**: correct the URLs in the test guide (this file's companion doc).

### A3 — MEDIUM: `Post` trusts client-supplied `Id`
The POST handler overrides `User` and `Status` server-side but not `Id`. OData deserializes
`Id` from the payload; EF inserts with the explicit key: `"Id": <existing>` → UNIQUE
constraint → unhandled `DbUpdateException` → 500; a large `Id` skews the AUTOINCREMENT
sequence. The UI never sends it, but the API shouldn't depend on client cooperation.

**Fix**: `tbc.Id = 0;` next to the Status override, + unit test.

### A4 — LOW: legacy `NULL FromDate` rows are stuck
- The seeder backfill marks every `TODATE IS NOT NULL` row `Completed` — including rows with
  `FROMDATE IS NULL` from the pre-dates era, which now violate the Completed invariant.
- A `Reading` row with `NULL FromDate` fails `IsFinalizeAllowed` → error message says
  "Only a record in Reading status can be completed" although the status *is* Reading.
- Such rows cannot be repaired via API (no PUT) — only deleted.

**Fix**: distinct finalize error for missing start date (+ test); audit query for the dev/prod
DB: `SELECT ID, FROMDATE, TODATE, STATUS FROM T_LIB_BOOK_READING_RECORD WHERE FROMDATE IS NULL;`
—if rows exist, normalize them (e.g. `FROMDATE = TODATE`) before shipping.

### A5 — LOW: no update path
No PUT/PATCH: fixing a typo in `Comment` or a wrong end date on a terminal record requires
delete + recreate (loses `CreatedAt/Createdby` provenance), and finalized records cannot be
reopened. **Proposed disposition: ACCEPTED** as a deliberate v1 lifecycle choice — document it;
if PUT is added later it needs the same self-excluded overlap re-check as `FinalizeReadingAsync`.

### A6 — LOW: overlap check is TOCTOU
`CountOverlappingRecordsAsync` + `SaveChangesAsync` is not atomic; two concurrent POSTs for the
same book/reader could both pass. SQLite + single-family usage makes this near-impossible, and
range-overlap cannot be expressed as a DB constraint. **Proposed disposition: ACCEPTED** —
document with a code comment.

### A7 — INFO: cosmetic drift
- `User`: `[MaxLength(50)]` vs `NVARCHAR(40)` column type (model + DDLs). Align to 40.
- `Sqls/DBSchema_Table.sql` has no `T_LIB_BOOK_READING_RECORD` entry at all (drift since the
  2026-09-02 feature). Reference-only, but add the table for completeness.
- `AbortReading`'s `ToDate` EDM parameter is non-nullable yet functionally optional —
  `.Nullable()` makes `$metadata` honest for external clients.

### U1 — HIGH: no lifecycle surface in the UI
`fetchBookReadingRecords`'s `$select` omits `Status`; `BookReadingRecord` (librarymodel.ts) has
no Status field; the list has no Status column and no row actions; `onVerify` +
`completeRangeValidator` force both dates — so every UI-created record is `Completed`, and
API-created `Reading` rows appear only as a blank End Date with no way to complete/abort them.
This is the main pre-ship gap (test guide §5).

**Fix plan**:
1. Model: `BookReadingStatus` string enum (wire = member name, pinned by the integration
   test) + `Status` on `BookReadingRecord` + relaxed `onVerify` (FromDate always required;
   ToDate optional → creates Reading).
2. Service: `Status` in `$select`; `completeBookReadingRecord` / `abortBookReadingRecord`
   posting to `${bookReadingRecordAPIURL}/CompleteReading|AbortReading` (bare action name).
3. List: Status column (nz-tag), Complete/Abort row actions for Reading rows, date sorting.
4. New `reading-record-finalize-dlg` (date mandatory for Complete, optional for Abort; dates
   before FromDate disabled).
5. Create dialog: "Still reading" switch → single start-date picker, sends FromDate only.
6. i18n keys (en/zh) for all of the above.
Deferred within U1: Status entry in the structured filter dialog (enum literals need the
qualified OData spelling — verify against `$metadata` first).

### U2 — MEDIUM: reader-name search doesn't match the displayed name
Test-guide #9 expects searching by "reader name" to work, but the display name is the member's
`DisplayAs` while the server query matches `contains(tolower(User), …)` — `User` is the token's
user ID. Searching for the display name shown in the Reader column returns nothing.

**Fix**: mirror the `matchedBookIds` pattern — resolve `MembersInChosedHome` client-side to
matching `User` IDs and inline `User in ('id1',…)` into the OR clause of
`fetchBookReadingRecords`.

### U3 — MEDIUM: silent no-op on `onVerify()` failure (create dialog)
`handleOk` enables Submit based only on form validity + selected book. If `ChosedHome` is null
(`HID = 0`) or the auth subject lacks a user ID, `onVerify()` fails and the handler only writes
a console log — collected `VerifiedMsgs` never surface.

**Fix**: show the first verification message via `messageService.error(...)`.

### U4 — MEDIUM: error modal shows a raw JSON blob
`ErrorHandlingMiddleware` writes `{"error":"<message>"}`; `_buildHttpErrorMessage` produces
`400 Bad Request: {"error":"Reading period overlaps…"}; Http failure response for …`. The
promised "server verdict" is buried in noise.

**Fix**: in `_buildHttpErrorMessage`, prefer `error.error?.error` when it is a string
(one place, benefits every library call).

### U5 — LOW: `BookId in (...)` URL ceiling
Title search inlines every matched BookId into the query string (Kestrel default request-line
limit ~8 KB). Fine at family scale; long-term fix is a `Book` navigation property so the server
can filter on `Book/NativeName` (would also remove the catalog-race workaround in the list).
**Proposed disposition: DEFERRED.**

### U6 — INFO
- Stale list after switching homes while the page is open — matches every other library list
  (app-wide pattern, not a regression here). **DEFERRED.**
- Only the ID column is sortable. Addressed within U1 (FromDate/ToDate sorting added there).

---

## Remediation log

### 2026-09-12 — A2 FIXED (doc-only)
`book-reading-records-enhancement-test-2026-09-12.md` Part 2: URLs corrected to the bare
route (`https://localhost:44360/LibraryBookReadingRecords/CompleteReading`, no `/odata`
prefix, no `Default.` namespace) with an explanatory note; expected `Status` wire values
corrected from numbers (`0/1/2`) to the enum member-name strings (`"Reading"/"Completed"/
"Aborted"`) pinned by `ReadingLifecycle_Actions_RouteAndTransition`.
**Tests**: baseline confirmed before the edit — API 53 unit + 1 integration pass, UI 55 pass;
the integration test is the proof for the corrected route (no code change).

### 2026-09-12 (late night) — A1/A3/A4, A6, A7a/b, U1–U4 FIXED (session ended mid-verification)

Applied in the follow-up review session, fix + tests per item:

- **A1**: `Delete` now membership-joins the lookup (`HomeMembers.Any(...)` on the record's own
  `HomeID`) — missing and cross-home both render 404. Tests rewritten:
  `Delete_ByNonMember_NotFound`, `Delete_CoMemberAllowed`, `Delete_UnknownKey_NotFound`.
- **A3**: `tbc.Id = 0` alongside the server-side Status/User overrides. Test
  `Post_ClientSentId_Ignored`.
- **A4**: distinct finalize error `"Cannot finalize a record without a start date"` before the
  status gate. Test `Finalize_LegacyRowWithoutFromDate_Rejected`.
- **A6**: ACCEPTED as proposed — TOCTOU rationale comment at the overlap check.
- **A7a**: `User` → `[MaxLength(40)]`. **A7b**: abort `ToDate` EDM parameter made nullable
  (see 2026-09-13 — the original edit did not compile).
- **U1** (full plan landed): `BookReadingStatus` enum + `Status` on the UI model; `Status` in
  `$select` + `completeBookReadingRecord`/`abortBookReadingRecord` service actions (bare action
  URLs); list Status tag column + date sorting + Complete/Abort row actions; new
  `reading-record-finalize-dlg` (date mandatory for Complete, optional for Abort, pre-FromDate
  dates disabled); create-dialog "Still reading" switch (FromDate-only → server opens Reading);
  en/zh i18n. Filter-dialog Status entry remains deferred as planned.
- **U2**: reader-name search resolves `MembersInChosedHome` → `User in ('id1',…)` OR-clause
  (mirrors `matchedBookIds`); spec covers escaping (`'o''x'`).
- **U3**: create dialog surfaces first `VerifiedMsgs` entry via `messageService.error`.
- **U4**: `_buildHttpErrorMessage` prefers the server's `error.error` string — one place,
  benefits every library call; spec asserts the raw envelope text is gone.

### 2026-09-13 — verification + A7b compile fix + A7c + A4 dev audit

The late-night session died before step 3 ("update status in the document"), leaving the
status table stale and one **build-breaking** edit uncaught (`dotnet test` exit code was
masked by a `| tail` pipeline):

- **A7b fixed**: `ParameterConfiguration.Nullable` is a property, not a method —
  `.Nullable()` was **CS1955**; now `.Nullable = true` (`EdmModelBuilder.cs`).
- **A7c done**: `T_LIB_BOOK_READING_RECORD` added to `Sqls/DBSchema_Table.sql` (SQL Server
  reference dialect, matching sibling `t_lib_*` style, with intent-only FKs noted).
- **A4 dev audit**: `SELECT ID, FROMDATE, TODATE, STATUS FROM T_LIB_BOOK_READING_RECORD
  WHERE FROMDATE IS NULL` on local `hih.db` → **0 rows** (1 record total). **Prod DB audit
  still pending.**
- **Full suites green**: API 363 pass / 0 fail (360 unit + 3 integration);
  UI 1299 pass / 0 fail (142 files; 53 skipped incl. the deferred Event/Blog specs).

### Remaining

- A5 disposition (proposed ACCEPTED), U5/U6 home-switch disposition (proposed DEFERRED) —
  awaiting owner confirmation, then mark in the table.
- Prod DB audit for A4 legacy rows.
- U1 deferred sub-item: Status entry in the structured filter dialog (verify qualified
  enum literal spelling against `$metadata` first).
- Commit the wave: `achihapi` `chroe/keepimprv-2`, `achihui` `feat/library`.
