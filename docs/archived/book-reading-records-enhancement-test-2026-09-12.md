# Enhanced Book Reading Records — End-User Test Guide

> Date: 2026-09-12
> Scope: the **uncommitted** changes on
> `achihapi` (branch `chroe/keepimprv-2`, v1.8.375 → 1.8.378) and
> `achihui` (branch `feat/library`, v1.8.520).
> Companion docs: [new-entity schema checklist], `achihui/docs/filter-dialog-generic-design.md`

## 1. What the change contains

### API (`achihapi`)

| File | Change |
|---|---|
| `src/hihapi/Models/Library/LibraryBookReadingRecord.cs` | New lifecycle enum `LibraryBookReadingStatus`: **Reading (0) / Completed (1) / Aborted (2)**; `Status` property; `IsValid` rewritten — `FromDate` now mandatory; `ToDate` rules follow the status (Reading = must be null, Completed = required, Aborted = optional); `ToDate >= FromDate`; new `IsFinalizeAllowed`. |
| `src/hihapi/Models/EdmModelBuilder.cs` | Registers `EnumType<LibraryBookReadingStatus>` and two bound collection actions: `CompleteReading(HomeID, RecordID, ToDate)` / `AbortReading(HomeID, RecordID, ToDate?)`, both returning the updated entity. |
| `src/hihapi/Controllers/Library/LibraryBookReadingRecordsController.cs` | POST: server owns `Status` (client value ignored) — only `FromDate` → Reading; both dates → Completed; Aborted cannot be created via POST. New `CountOverlappingRecordsAsync` overlap refusal (same Home+Book+Reader, day granularity, non-aborted only; open record = unbounded end). Actions `CompleteReading`/`AbortReading` share `FinalizeReadingAsync`: Reading-only gate (else 400), ToDate mandatory for Completed, date parse/order checks (400s), overlap re-check on the finalized range (self excluded), cross-home/unknown RecordID → **404**, non-member claimed HomeID → **401**; stamps `UpdatedAt/Updatedby`. |
| `src/hihapi/Utilities/DatabaseSeeder.cs` | Idempotent DB upgrade: adds `STATUS INTEGER NOT NULL DEFAULT 0` to existing `T_LIB_BOOK_READING_RECORD` (via `pragma_table_info` column check) and backfills legacy rows `TODATE IS NOT NULL → STATUS = 1 (Completed)`. **Already applied** on the 2026-09-12 dev startup (ALTER TABLE + UPDATE visible in the log). |
| `src/hihapi/Controllers/Library/LibraryPersonsController.cs` | Same-wave side fix: POST/PUT now filter blank linkage rows (`RoleId 0`) and validate role IDs against existing/visible rows before raw-SQL insert (fixes EF-graph 500 on person save with blank role rows). |
| Tests | ~22 new unit tests (`LibraryBookReadingRecordsControllerTest.cs`, `LibraryBookReadingRecordTest.cs`, `LibraryPersonsControllerTest.cs`) + integration test `ReadingLifecycle_Actions_RouteAndTransition`. |

### UI (`achihui`)

| File | Change |
|---|---|
| `src/app/pages/library/reading-record-create-dlg/*` | Date range fully **required** (both ends; `completeRangeValidator` catches half-picked ranges), red `FieldIsMandatory` error tip, `nzRequired` label; green **"Created successfully" toast** on success; **red error modal surfacing the server verdict** (e.g. overlap refusal) instead of silent failure. |
| `src/app/model/librarymodel.ts` | `BookReadingRecord.onVerify` now requires both `FromDate` and `ToDate` (mirrors the API rule). |
| `src/app/shared/filter-dialog/*` (+ ~50 list pages incl. `reading-record-list`, `book-list`) | Shared filter dialog redesign on `actslib` `FilterRoot`: single-condition filters propagate correctly, an empty tree is not submittable, Cancel/Esc keep the previous filter, `hasActiveFilterDefinition` drives the highlight/Clear state. |
| `src/app/pages/library/person/person-detail/*` | Blank role-assignment rows (`ID 0`) are dropped before submit (pairs with the API fix). |

### ⚠️ Known gap

The UI has **no Status column and no Complete/Abort buttons** — `Reading`/`Aborted`
states and the two lifecycle actions are currently **API-only**. Every UI creation
sends both dates, so the server marks every UI-created record `Completed`.
Part 2 below covers testing the lifecycle half via direct API calls.

Entry points: menu **Library Trace → Reading Records** (`/library/readingrecord`),
and per-book **“Create reading record”** on the Books list.

---

## 2. Part 1 — UI tests (pure end-user)

**Setup:** start the stack (`start-hih-all.ps1`), sign in at <https://localhost:29521/>,
pick a home that has books in its catalog, go to **Library Trace → Reading Records**.

| # | Scenario | Steps | Expected |
|---|---|---|---|
| 1 | Legacy data survived the upgrade | Open the Reading Records list before creating anything | All pre-existing records show unchanged (dates, comments). Rows that had an end date are now internally `Completed` — they must look exactly the same. |
| 2 | Dialog gating | Click **Create**. Try Submit empty → book only → then set only the start date and clear the end date | Submit stays **disabled** until book + complete range are present; after a touched attempt the Date Range field shows a red **“Field is mandatory”** tooltip. |
| 3 | Happy path | Book = any, Date Range = e.g. `2026-09-01 → 2026-09-05`, Comment = `great`, Submit | Green toast **“Created successfully”**, dialog closes, list refreshes: new row with the book, **Reader = your login name**, StartDate/EndDate as picked. |
| 4 | Same-day reading | Create with start = end date | Accepted (equal dates are valid). |
| 5 | **Overlap refused (key new behavior)** | For the *same book*, create again with an overlapping range, e.g. `2026-09-03 → 2026-09-10` | **Red error modal** containing the server message: *“Reading period overlaps an existing record of the same reader for this book”*. Dialog stays open, nothing is saved. |
| 6 | Adjacent periods allowed | Same book, range starting the day **after** record #3 ends (sharing the end date day still overlaps) | Accepted — overlap is inclusive per day. |
| 7 | Different book / different reader | Same range as #3 but a *different* book | Accepted (overlap is per book + reader). Different reader in the same home also accepted (needs a second account). |
| 8 | Books-page shortcut | Library Trace → Books → row menu → **Create reading record**, pick a range | Dialog opens with the book **pre-filled**; same validation/feedback applies. |
| 9 | Free-text search | Type part of a book title, reader name, or comment into the filter bar | Debounced live narrowing; `N \| M` count updates; case-insensitive. |
| 10 | Structured filter | Filter ▾ → **Edit filter…** → add **one** condition (e.g. Comment contains `great`) → Submit; reopen, add a second (StartDate between …) → Submit | **Single-condition filters work** (fixed defect), multi-condition AND works; filter button highlighted, Clear filter enabled. **Cancel / Esc keeps the previous filter**; an all-cleared dialog cannot be submitted. |
| 11 | Clear filter | Filter ▾ → **Clear filter** | Full list returns; the Clear item is disabled again when no filter is active. |
| 12 | Delete | Row → **Delete** → confirm | Row removed; `N \| M` counts adjust. |
| 13 | Shared-component regression | Apply a single-condition filter on **Books**, **Borrow Records**, and one finance list (e.g. Currencies) | The redesign touches ~50 lists — confirm filtering + clearing behave the same there. |
| 14 | Persons blank-role fix (same wave) | Library Trace → Persons → edit a person → add a role row but leave it blank → Save | **No 500 error**; person saves; the blank row is not persisted (client drops it, API would also refuse it). |

---

## 3. Part 2 — Lifecycle tests (API-only for now)

The `Reading → Completed/Aborted` flow has **no UI buttons yet**, so test it with a
bearer token: browser DevTools → Network → click any OData request in the app →
copy its `Authorization: Bearer …` header, then call
`https://localhost:44360/LibraryBookReadingRecords/…` with curl/Postman
(Content-Type: application/json).

> **URL note (corrected 2026-09-12):** there is **no `/odata` prefix** — the API's route
> prefixes are `/` and `/v1` — and the actions are invoked by their **bare name**
> (`…/LibraryBookReadingRecords/CompleteReading`), *not* `Default.CompleteReading`:
> the EDM namespace is `hihapi.Models`, and ASP.NET Core OData routes bound actions
> without a namespace prefix (same convention as the UI's `CloseAccount`/`SettleAccount`
> calls). Proven by the integration test `ReadingLifecycle_Actions_RouteAndTransition`.

| # | Step | Call | Expected |
|---|---|---|---|
| 1 | Start an open reading | `POST /LibraryBookReadingRecords` body `{"HomeID":<h>,"BookId":<b>,"FromDate":"2026-09-01"}` (no `ToDate`) | 201; `Status: "Reading"` (the enum travels as its **member name string**, not a number — pinned by the integration test). In the UI list the row appears with a **blank End Date** (reload). |
| 2 | Client can't forge status | Same POST plus `"Status": "Completed"` | 201 with `Status: "Reading"` — client value ignored (both dates → `"Completed"`; no ToDate → `"Reading"`; never `"Aborted"`). |
| 3 | Open record blocks overlaps | POST same book `FromDate 2026-09-20 → ToDate 2026-09-25` | **400 overlap** — an open record has an unbounded end, so anything from its start onward collides. |
| 4 | Missing start date | POST without `FromDate` | 400. |
| 5 | Complete it | `POST /LibraryBookReadingRecords/CompleteReading` body `{"HomeID":<h>,"RecordID":<id>,"ToDate":"2026-09-18"}` | 200, `Status: "Completed"`, `ToDate` set; UI row now shows the end date. |
| 6 | Terminal states | Repeat step 5, or try `AbortReading` on the same record | 400 *“Only a record in Reading status can be completed/aborted”*. |
| 7 | Complete-reading validation | `CompleteReading` with `ToDate` missing / `"abc"` / earlier than `FromDate` | 400 each: `ToDate is required to complete a reading` / `ToDate is not a valid date` / `ToDate must not be earlier than FromDate`. |
| 8 | Complete into an overlap | Complete an open record with a `ToDate` that collides with another record of the same reader/book (self excluded) | 400 overlap. |
| 9 | Abort without end date | Open a new record (step 1), then `AbortReading` body `{"HomeID":<h>,"RecordID":<id>}` (no ToDate) | 200, `Status: "Aborted"`, ToDate stays null. |
| 10 | Aborted never blocks | After step 9, POST the same book with a range covering the aborted period | **Accepted** — abandoned readings don't prevent re-reads. |
| 11 | Security | `CompleteReading` with another home's RecordID → **404** (no existence leak); `HomeID` you're not a member of → **401**; no token → 401. | As stated. |

---

## 4. Part 3 — Automated gates (before/after the manual pass)

```bash
# API (achihapi/): new unit tests + lifecycle integration test
dotnet test test/hihapi.test/hihapi.unittest.csproj --filter "FullyQualifiedName~LibraryBookReadingRecords"
dotnet test test/hihapi.integrationtest/ --filter "FullyQualifiedName~ReadingLifecycle"

# UI (achihui/): create-dlg + filter-dialog + librarymodel specs
npm run test-headless
```

## 5. Bottom line

Part 1 covers everything an end user can click: required dates, overlap refusal,
lifecycle-proof feedback UX, and the filter-dialog redesign regression surface.
Part 2 is required because the lifecycle half of the feature
(`Reading` / `CompleteReading` / `AbortReading`) is **server-side only** at this
point — wiring it into the UI (Status column + Complete/Abort row actions) is the
main remaining gap before this change ships.
