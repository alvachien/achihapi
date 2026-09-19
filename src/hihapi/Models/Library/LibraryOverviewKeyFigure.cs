using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hihapi.Models.Library
{
    // One row of a top-3 ranking (categories / authors / presses) on the Library
    // overview. Complex type (no key): travels as a collection property of
    // LibraryOverviewKeyFigure.
    public class LibraryRankingItem
    {
        // Associated entity row id as string (client track-by key).
        public String Key { get; set; }

        // Display name: category Name; person/press NativeName else ChineseName
        // (matching the client helper the old walk used).
        public String Name { get; set; }

        // Distinct books of the home linked to this entity.
        public Int32 Count { get; set; }
    }

    // Server-side aggregate for the Library overview landing page, returned by
    // the GetLibraryOverviewKeyFigure action on LibraryBooks. Replaces the old
    // client-side walk of the whole book collection ($expand included). Month
    // windows are computed against the SERVER clock - the deployment hosts the
    // API and its users on one machine (same single-clock model as the Finance
    // overview keyfigure).
    public class LibraryOverviewKeyFigure
    {
        [Key]
        [Required]
        public Int32 HomeID { get; set; }

        public Int32 TotalBooks { get; set; }

        // Books whose CreatedAt falls in the half-open month window
        // [startOfMonth, start + 1 month) of the server's current / previous month.
        public Int32 AddedThisMonth { get; set; }
        public Int32 AddedLastMonth { get; set; }

        // DISTINCT books with a Completed reading record whose ToDate falls in the
        // same windows (open/aborted readings excluded, like the client did).
        public Int32 CompletedThisMonth { get; set; }
        public Int32 CompletedLastMonth { get; set; }

        // Top 3 by book count, ties by name (ordinal); empty lists allowed.
        public IList<LibraryRankingItem> TopCategories { get; set; }
        public IList<LibraryRankingItem> TopAuthors { get; set; }
        public IList<LibraryRankingItem> TopPresses { get; set; }
    }
}
