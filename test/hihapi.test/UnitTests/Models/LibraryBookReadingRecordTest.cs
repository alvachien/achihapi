using System;
using hihapi.Models.Library;
using Xunit;

namespace hihapi.unittest.UnitTests.Models
{
    public class LibraryBookReadingRecordTest
    {
        [Fact]
        public void Invalid_HomeIDIsMust()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                BookId = 1,
                User = "abc",
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_BookIDIsMust()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                User = "abc",
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_UserIsMust()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_DateRangeIsWrong()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2022, 1, 1),
                ToDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Valid_SameDayReading()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2022, 1, 1),
                ToDate = new DateTime(2022, 1, 1),
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        // FromDate is required for every status; ToDate rules are per status
        // (see the lifecycle tests below).
        [Fact]
        public void Invalid_FromDateIsMust()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                ToDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_FromDateIsMust_Aborted()
        {
            // Even the most forgiving status still demands a start date.
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                Status = LibraryBookReadingStatus.Aborted
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_BothDatesAreMust()
        {
            // Status defaults to Reading, but a missing FromDate invalidates it.
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc"
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_CompletedToDateIsMust()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Completed
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Valid_ReadingOpenEnded()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Reading
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Invalid_ReadingWithToDate()
        {
            // Reading is open-ended by definition: the end date only arrives
            // through CompleteReading / AbortReading.
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                ToDate = new DateTime(2021, 1, 20),
                Status = LibraryBookReadingStatus.Reading
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Valid_AbortedWithoutEndDate()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Aborted
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Valid_AbortedWithEndDate()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                ToDate = new DateTime(2021, 1, 20),
                Status = LibraryBookReadingStatus.Aborted
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Invalid_AbortedReversedDates()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 20),
                ToDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Aborted
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        [Fact]
        public void Invalid_UnknownStatus()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = (LibraryBookReadingStatus)9
            };

            bool isvalid = vm.IsValid(null);

            Assert.False(isvalid);
        }

        // IsFinalizeAllowed: only open (Reading) records may be finalized.
        [Fact]
        public void FinalizeAllowed_Reading()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Reading
            };

            Assert.True(vm.IsFinalizeAllowed(null));
        }

        [Fact]
        public void FinalizeDenied_Completed()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                ToDate = new DateTime(2021, 1, 20),
                Status = LibraryBookReadingStatus.Completed
            };

            Assert.False(vm.IsFinalizeAllowed(null));
        }

        [Fact]
        public void FinalizeDenied_Aborted()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1),
                Status = LibraryBookReadingStatus.Aborted
            };

            Assert.False(vm.IsFinalizeAllowed(null));
        }

        [Fact]
        public void FinalizeDenied_ReadingWithoutFromDate()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                Status = LibraryBookReadingStatus.Reading
            };

            Assert.False(vm.IsFinalizeAllowed(null));
        }
    }
}
