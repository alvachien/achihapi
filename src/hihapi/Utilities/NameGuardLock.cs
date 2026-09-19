using System;
using System.Collections.Concurrent;
using System.Threading;

namespace hihapi.Utilities
{
    // In-process async gate keyed by (entity type, home) that serializes the
    // check-then-act of the Library duplicate-name guards (LibraryNameGuard)
    // against one table + home: acquire before the guard, release after the
    // row (and any linkage writes) have been saved.
    //
    // Why not a UNIQUE index instead: live databases already contain duplicate
    // name rows written before the guard existed (the very "Cross" vs " cross "
    // case the guard comment calls common), so index creation would fail and a
    // dedup migration would have to pick winners inside user data. The gate
    // closes the realistic race - double-clicked saves, concurrent household
    // members, $batch - for this single-process deployment; scaling the API out
    // across processes would require a DB constraint plus a reviewed dedup.
    //
    // Gates are never evicted: one tiny SemaphoreSlim per table x home actually
    // seen, bounded by the (small) number of homes.
    internal static class NameGuardLock
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.Ordinal);

        // Keyed by entity CLR type so the three Library catalogs never contend
        // with each other; the home keeps different tenants independent.
        internal static SemaphoreSlim For<TEntity>(Int32 homeID)
        {
            return Gates.GetOrAdd(typeof(TEntity).Name + ":" + homeID, _ => new SemaphoreSlim(1, 1));
        }
    }
}
