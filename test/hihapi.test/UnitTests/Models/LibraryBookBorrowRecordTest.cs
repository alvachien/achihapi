using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using hihapi.Models.Library;

namespace hihapi.unittest.UnitTests.Models
{
    public class LibraryBookBorrowRecordTest
    {
        [Fact]
        public void Invalid_HomeIDIsMust()
        {
            var vm = new LibraryBookBorrowRecord
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
            var vm = new LibraryBookBorrowRecord
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
            var vm = new LibraryBookBorrowRecord
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
            var vm = new LibraryBookBorrowRecord
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
        public void Valid_FromDateIsEmpty()
        {
            var vm = new LibraryBookBorrowRecord
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
            var vm = new LibraryBookBorrowRecord
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
    }
}
