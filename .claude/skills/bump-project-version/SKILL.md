---
name: bump-project-version
description: Bump the hihapi app version in the csproj and rebuild the whole solution, but only after the user explicitly confirms the build. Use whenever the user asks to bump, release, or set the version of the API (achihapi / hihapi), even if they don't name the csproj or this skill.
---

# Bump Project Version

The API version lives in exactly **one** place:

| File | Field |
|---|---|
| `src/hihapi/hihapi.csproj` | `<Version>` |

Because it is a single field, edit it directly with the Edit tool — no helper script needed (unlike the achihui `bump-version` skill, which scripts three fields across three files precisely to prevent drift; there is nothing here to drift).

Do **not** touch, in the same breath, these other version numbers — they are independent counters:

- `DatabaseSeeder.CurrentVersion` in `src/hihapi/Utilities/DatabaseSeeder.cs` — the integer **DB schema** version (upgrades, not releases).
- The achihui version (`package.json` / environment files) — bump it with that project's `bump-version` skill.
- `<Version>` entries belonging to test csprojs (they don't carry one; if one ever appears, leave it).

## Version scheme

Semver `MAJOR.MINOR.PATCH` (e.g. `1.8.395`). PATCH is a running release number, not strictly +1 — always confirm the target version with the user before applying it.

## Steps

1. **Determine the target version.** If the user gave an explicit version (e.g. `1.8.400`), use it. Otherwise read the current `<Version>` from the csproj, propose the next patch, and confirm with the user before writing.

2. **Apply the bump.** Read `src/hihapi/hihapi.csproj`, replace the `<Version>X.Y.Z</Version>` value with the target.

3. **Verify** the file now reports the new version (the Edit result is sufficient; a quick grep of `<Version>` confirms it if there is any doubt).

4. **Ask before building (human-in-loop — required).** A solution build costs real minutes, and often the user only wants the file bumped. Use the AskUserQuestion tool with options like:
   - "Build `achihapi.sln` now (Recommended)" — verifies the bump in a full compile
   - "Skip the build" — stop after the version edit

   Never run the build without an explicit yes, and never substitute a different verification (tests, publish) unless the user asks for it.

5. **If approved, build** from the `achihapi/` directory:
   ```bash
   dotnet build achihapi.sln
   ```
   Report the outcome faithfully — if it fails, show the error output; do not retry silently or "fix" unrelated things.

6. **Commit only if the user explicitly asks.** Repo convention:
   ```
   chore: bump version to <X.Y.Z>
   ```
   Stage `src/hihapi/hihapi.csproj` only.
