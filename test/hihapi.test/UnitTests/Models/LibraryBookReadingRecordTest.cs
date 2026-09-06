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
                User = "abc"
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
                User = "abc"
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
                ToDate = new DateTime(2021, 1, 1)
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
                ToDate = new DateTime(2022, 1, 1)
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Valid_FromDateIsEmpty()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                ToDate = new DateTime(2021, 1, 1)
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Valid_ToDateIsEmpty()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc",
                FromDate = new DateTime(2021, 1, 1)
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }

        [Fact]
        public void Valid_BothDatesEmpty()
        {
            var vm = new LibraryBookReadingRecord
            {
                Id = 1,
                HomeID = 1,
                BookId = 1,
                User = "Abc"
            };

            bool isvalid = vm.IsValid(null);

            Assert.True(isvalid);
        }
    }
}
