using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Utilities
{
    // Shared duplicate-name guard for the home-scoped Library catalogs
    // (books / persons / organizations): a row's NativeName or non-empty
    // ChineseName must not equal any OTHER row's NativeName or non-empty
    // ChineseName within the same home, folded trimmed and case-insensitively
    // ("Cross" == " cross ").
    //
    // The candidate rows' name pairs are materialized and folded in C# (Trim +
    // ToLowerInvariant) rather than compared with SQL lower()/trim(): SQLite's
    // lower() is ASCII-only and its trim() strips only U+0020, which silently
    // disabled the rule for cased non-ASCII names (e.g. Cyrillic) and exotic
    // whitespace, while C# folds both sides Unicode-completely. The per-home
    // row set is small by design (personal/family library scale), so loading
    // two columns of it is cheap.
    //
    // The check is check-then-act: pair it with NameGuardLock (acquire before
    // calling, release after the save) to close the race in-process.
    internal static class LibraryNameGuard
    {
        // Projected name pair of one candidate row (translatable in a Select).
        internal readonly record struct NamePair(string NativeName, string ChineseName);

        // A name field the request actually changes: the message label naming
        // the field, the raw incoming value for the message, and the folded
        // value used for the comparison.
        internal readonly record struct NameGuardItem(string Label, string Value, string Folded);

        // Builds one entry per name field whose incoming value differs (folded)
        // from the stored value; the create paths pass null stored values so
        // every non-blank incoming field yields an entry. Blank/whitespace-only
        // names match nothing and are skipped. Keeping the UNCHANGED fields out
        // is what lets a row that already collides with a legacy sibling stay
        // savable when a non-name field is edited.
        internal static List<NameGuardItem> BuildNameGuards(
            string nativeName, string chineseName, string oldNativeName, string oldChineseName)
        {
            var guards = new List<NameGuardItem>();

            var native = Fold(nativeName);
            if (native.Length > 0 && !string.Equals(native, Fold(oldNativeName), StringComparison.Ordinal))
            {
                guards.Add(new NameGuardItem("named", nativeName, native));
            }

            var chinese = Fold(chineseName);
            if (chinese.Length > 0 && !string.Equals(chinese, Fold(oldChineseName), StringComparison.Ordinal))
            {
                guards.Add(new NameGuardItem("with Chinese name", chineseName, chinese));
            }

            return guards;
        }

        // Throws BadRequestException as soon as any field the request changes
        // collides with the folded names of another row of the same home.
        // nounForMessage carries the article-aware noun ("A book", "An
        // organization"); the message names the field that actually collided.
        internal static async Task EnsureNoDuplicateNameAsync(
            IQueryable<NamePair> otherRowsInHome,
            string nativeName, string chineseName,
            string oldNativeName, string oldChineseName,
            string nounForMessage)
        {
            var guards = BuildNameGuards(nativeName, chineseName, oldNativeName, oldChineseName);
            if (guards.Count == 0)
            {
                return;
            }

            var folded = new HashSet<string>(StringComparer.Ordinal);
            var rows = await otherRowsInHome.ToListAsync();
            foreach (var row in rows)
            {
                var n = Fold(row.NativeName);
                if (n.Length > 0)
                {
                    folded.Add(n);
                }

                var c = Fold(row.ChineseName);
                if (c.Length > 0)
                {
                    folded.Add(c);
                }
            }

            foreach (var item in guards)
            {
                if (folded.Contains(item.Folded))
                {
                    throw new BadRequestException(
                        $"{nounForMessage} {item.Label} '{item.Value}' already exists in this home");
                }
            }
        }

        private static string Fold(string value)
        {
            return value?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
