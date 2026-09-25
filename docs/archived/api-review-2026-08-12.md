# achihapi Code Review - Findings

**Date:** 2026-08-12
**Scope:** `achihapi/` OData v4 Web API (ASP.NET Core 10 + EF Core + SQLite)
**Reviewer:** Claude Code

## Summary

The codebase is generally well-secured: a global authenticated-by-default fallback
policy, secure 500 handling (no `ex.Message` leak for unhandled exceptions),
parameterized SQL everywhere (no injection), path-traversal guards on file/upload
operations, the debug `$odata` endpoint gated to Development + `[Authorize]`, and a
correct multi-tenant authorization pattern on most CRUD endpoints
(find existing → check `existing.HomeID` membership → reject HomeID changes).

However, one genuine cross-tenant vulnerability and several smaller issues were found.
The most significant is an IDOR in `CloseAccount` that survived the 2026-08-02
remediation pass which *did* fix its sibling actions.

| # | Severity | Area | Finding |
|---|----------|------|---------|
| 1 | HIGH | FinanceAccounts | `CloseAccount` IDOR - closes another home's account ✅ Fixed |
| 2 | MEDIUM | FinanceAccounts | `SettleAccount` missing account-to-home binding ✅ Fixed |
| 3 | MEDIUM | Library borrow records | Cross-tenant visibility via attacker-controlled `User` |
| 4 | MEDIUM | Finance tmp/loan docs | `ex.Message` leaked to client in transaction catch blocks |
| 5 | LOW | Reference data (8 ctlrs) | Nullable `HomeID.Value` throws 500 on shared rows |
| 6 | LOW | Reference data (8 ctlrs) | `GET(key)` LEFT JOIN leaks orphaned-home rows |
| 7 | LOW | Library borrow records | GET/DELETE scope mismatch |
| 8 | LOW | Finance (10 ctlrs) | `DBOperationException(exp.Message)` in concurrency handlers |
| 9 | LOW | PhotoFile | `[AllowAnonymous]` image fetch (documented, residual) |

---

## HIGH

### 1. `CloseAccount` - cross-tenant IDOR (closes another home's account) ✅ Fixed

**Status:** Fixed 2026-08-12. Added `acntDB.HomeID == hid` guard so a non-member
account is treated as not-closeable (returns `Ok(false)`, no state change). Build
clean, 301 unit tests pass.

**Location:** `src/hihapi/Controllers/Finance/FinanceAccountsController.cs:455-509`
(the gap is at line 495)

The action checks membership against `hid` (from the request body), then loads the
account by `accountID` **without verifying `acntDB.HomeID == hid`**, and closes it:

```csharp
var hms = await _context.HomeMembers
    .Where(p => p.HomeID == hid && p.User == usrName).CountAsync(); // passes: caller's own home
if (hms <= 0) throw new UnauthorizedAccessException();
var acntDB = await _context.FinanceAccount.FindAsync(accountID); // accountID from body - NOT bound to hid
...
if (ret) { acntDB.Status = FinanceAccountStatus.Closed; await _context.SaveChangesAsync(); }
```

`FinanceAccount.IsCloseAllowed` (model) only checks `CategoryID == Asset &&
Status == Normal` - it never checks HomeID.

**Failure scenario:** An authenticated user who is a member of *any* home supplies
their own valid `hid` plus a victim's `accountID`. The membership check passes
(it validates the caller's home), `FindAsync(accountID)` returns the victim's
account, and it gets closed. Account IDs are sequential integers, so enumeration
is feasible. An attacker can close **any other home's Normal Asset account**.

**Fix:** after `FindAsync(accountID)`, bind the account to the checked home:

```csharp
var acntDB = await _context.FinanceAccount.FindAsync(accountID);
if (acntDB == null || acntDB.HomeID != hid) return NotFound();
```

**Why this was missed:** The 2026-08-02 remediation explicitly fixed the sibling
actions `FinanceTmpLoanDocumentsController.PostRepayDocument` /
`PostPrepaymentDocument` (comments: *"must belong to the verified home (prevents
operating on another home's loan account via a guessed LoanAccountID)"*,
*"client-supplied DocumentInfo.HomeID must not be trusted"*). `CloseAccount` and
`SettleAccount` were skipped.

---

## MEDIUM

### 2. `SettleAccount` - missing account-to-home binding ✅ Fixed

**Status:** Fixed 2026-08-12. Added `acntDB.HomeID == hid` guard; a non-member
account now falls through to the existing `NotFound()` branch. Build clean,
301 unit tests pass.

**Location:** `src/hihapi/Controllers/Finance/FinanceAccountsController.cs:512-646`
(gap at line 556)

Same shape as #1: membership checked on `hid`, then `FindAccount(accountID)` with no
`HomeID == hid` check. It then creates a `FinanceDocument` with `HomeID = hid` and a
`FinanceDocumentItem` whose `AccountID` is the unverified account.

**Impact** is lower than #1: the victim's account status is not changed, and report
queries filter by `docheader.HomeID`, so victim balances are not directly corrupted.
But it breaks the tenant-isolation invariant, creates a dangling cross-home document
reference in the caller's home, and offers an account-existence oracle
(`NotFound` vs `Ok(true/false)`).

**Fix:** same one-line binding check as #1.

### 3. `LibraryBookBorrowRecords` - cross-tenant visibility via attacker-controlled `User`

**Location:** `src/hihapi/Controllers/Library/LibraryBookBorrowRecordsController.cs`

- `Post` (68-108): checks membership against `tbc.HomeID` but stores `tbc.User`
  straight from the request body (only `Createdby` is set to the caller).
- `Get` (28-45) and `Get(key)` (49-66): filter **only** by `record.User == usrName`,
  with **no HomeID membership join**.

**Failure scenario:** A member of Home 1 POSTs
`{ HomeID: 1, BookId: 42, User: "<victim sub>" }`. The victim - who is *not* a
member of Home 1 - then sees that borrow record (book id, home id, organization,
dates, comments) via their own `GET /LibraryBookBorrowRecords`.

**Fix:**

1. In `Post`, set `tbc.User = usrName` (the borrower is the caller; do not trust the
   body for the access key).
2. Add a HomeID-membership join to `Get` / `Get(key)`, matching the pattern used by
   the other Library controllers (e.g. `LibraryBooksController`).

### 4. `ex.Message` leaked to client in transaction catch blocks

`ErrorHandlingMiddleware` returns `ex.Message` for all 4xx responses, and these
locations throw the inner DB exception's message back to the caller:

| File | Line |
|------|------|
| `FinanceAccountsController.CreateLegacyLoanAccount` | 442 (`BadRequestException(errorString)`) |
| `FinanceTmpDPDocumentsController.PostDocument` | 184 (`DBOperationException(errorString)`) |
| `FinanceTmpLoanDocumentsController.PostRepayDocument` | 231 (`DBOperationException(errorString)`) |
| `FinanceTmpLoanDocumentsController.PostPrepaymentDocument` | 399 (`DBOperationException(errorString)`) |

(where `errorString = exp.Message` captured in the transaction `catch`.)

These can expose SQLite internals (table/column/constraint names, SQL fragments).

**Fix:** The sibling `Post*Document` actions in `FinanceDocumentsController` already
use a fixed string - `"Transaction failed. Please check your input data."` - make
these four consistent.

---

## LOW

### 5. Nullable `HomeID.Value` throws 500 on shared reference data (8 controllers)

Where `HomeID` is `int?` (shared/system rows carry `HomeID == null`), the membership
check does `p.HomeID == tbd.HomeID.Value`, which throws `InvalidOperationException`
→ HTTP 500 instead of a clean 401/400.

| Controller | Line(s) |
|------------|---------|
| `FinanceAccountCategoriesController` | 114 |
| `FinanceAssetCategoriesController` | 113 |
| `FinanceDocumentTypesController` | 113 |
| `FinanceTransactionTypesController` | 109 |
| `LibraryBookCategoriesController` | 105, 145 |
| `LibraryOrganizationTypesController` | 101, 140 |
| `LibraryPersonRolesController` | 105, 145 |
| `LibraryBookLocationsController` | 142 |

**Fix:** `p.HomeID == tbd.HomeID` (EF translates nullable equality correctly) or
null-guard before the check.

### 6. `GET(key)` LEFT JOIN leaks orphaned-home rows to non-members (same 8 controllers)

The reference-data `GET(key)` uses:

```csharp
where ctgy.Id == key && (nhmem == null || nhmem.User == usrName)
```

The `nhmem == null` branch is intended to allow shared (`HomeID == null`) rows, but
it also returns rows whose `HomeID` is non-null but whose home has **zero members**
(orphaned home data) - the LEFT JOIN yields `nhmem == null` and the filter passes.

**Fix:**

```csharp
where ctgy.Id == key && (ctgy.HomeID == null || (nhmem != null && nhmem.User == usrName))
```

Note: the `Get()` *list* endpoints already handle this correctly via `Union` of
`HomeID == null` rows with a member-home join - only `GET(key)` has the bug.

### 7. Borrow-record GET/DELETE scope mismatch

**Location:** `src/hihapi/Controllers/Library/LibraryBookBorrowRecordsController.cs`

`Get` is personal (`User == usrName`) but `Delete` (126-143) is home-wide (HomeID
membership). Any home member can delete another member's borrow records that they
cannot even see via GET.

**Fix:** Either scope `Delete` to `tbd.User == usrName`, or make `Get` home-wide.
Likely the personal scope is intended, so add `tbd.User == usrName` to DELETE.

### 8. `DBOperationException(exp.Message)` in concurrency handlers (~10 controllers)

| File | Line |
|------|------|
| `HomeDefinesController` | 250 |
| `FinanceControlCentersController` | 202 |
| `FinanceAccountsController` | 198 |
| `FinanceDocumentsController` | 246 |
| `FinanceOrdersController` | 222 |
| `FinancePlansController` | 180 |
| `FinanceAccountCategoriesController` | 200 |
| `FinanceDocumentTypesController` | 196 |
| `FinanceAssetCategoriesController` | 199 |
| `FinanceTransactionTypesController` | 188 |

These are `DbUpdateConcurrencyException` handlers. The concurrency message is
low-value, but returning it is inconsistent with the no-leak policy. Return a generic
message instead.

### 9. `PhotoFileController.Get` is `[AllowAnonymous]` (documented, residual)

**Location:** `src/hihapi/Controllers/PhotoFileController.cs:83-110`

Image fetch is anonymous (markdown `<img src>` cannot carry a Bearer token); access
control is the unguessable GUID filename. This is documented inline and accepted, but
filenames can leak via logs / `Referer` headers. The proper fix (authenticated image
fetch via the auth interceptor + blob URLs) is tracked to UI auth-flow work
(UI-01/UI-02). Noted as residual risk, not a regression.

---

## Scope & Notes

**Audited directly:**

- Infrastructure: `Program.cs`, `Utilities/ErrorHandlingMiddleware.cs`,
  `Utilities/CommonUtility.cs`, `Models/EdmModelBuilder.cs`, `appsettings*.json`
- Controllers: `Home/*`, `Finance/FinanceDocuments`,
  `FinanceAccounts`, `FinanceReports`, `FinanceControlCenters`,
  `FinanceDocumentItems`, `FinanceTmpDPDocuments`, `FinanceTmpLoanDocuments`,
  `FinanceOrders` (Put), `FinancePlans` (Put), `Library/*`, `PhotoFileController`,
  `Extensions/ODataEndpointController`
- Utilities: `Utilities/BlogDeployUtility.cs`
- Cross-cutting greps: all `ExecuteSqlRaw`/`FromSqlRaw` (parameterized - clean),
  all `ODataActionParameters` action endpoints (enumerated - only `CloseAccount`/
  `SettleAccount` lack binding), all `.HomeID.Value` / `DefaultIfEmpty` occurrences.

**Not deeply audited (no issues surfaced in targeted greps):**

- `DataContext/hihDataContext.cs` - no `HasQueryFilter` global tenant filter; tenant
  isolation is enforced per-controller. This works today but is fragile: a new
  controller that forgets the membership check (as `CloseAccount` did) has no
  backstop. Consider a global query filter on `HomeID` as defense-in-depth.
- `Utilities/DatabaseSeeder.cs` - static `CREATE VIEW` DDL, parameter-free (safe).
  Runs `DROP VIEW IF EXISTS` + `CREATE VIEW` on every startup (safe, no data loss).
- `Controllers/Common/Currencies|Languages|DBVersions` - no nullable-HomeID pattern
  surfaced; likely clean global read-only reference data.

**Positive findings (things done right):**

- Global `FallbackPolicy = RequireAuthenticatedUser()`; `/health` is the only
  anonymous endpoint.
- `ErrorHandlingMiddleware` returns a generic message for 500s (no `ex.Message`
  leak for unhandled exceptions).
- All raw SQL uses `SqliteParameter` (parameterized) - no injection.
- `PhotoFileController` and `BlogDeployUtility` validate resolved paths stay within
  their root folders (path-traversal guarded).
- `ODataEndpointController.$odata` is `[Authorize]` and returns 404 outside
  Development; route values are `HtmlEncode`d.
- Standard CRUD across Finance (Accounts/ControlCenters/Orders/Plans/Documents)
  correctly checks membership against the **existing** entity's `HomeID` and rejects
  HomeID changes via PUT.
- No secrets in `appsettings*.json` (JWT validation uses OIDC discovery against the
  authority; no symmetric keys in config; SQLite file connection string).
