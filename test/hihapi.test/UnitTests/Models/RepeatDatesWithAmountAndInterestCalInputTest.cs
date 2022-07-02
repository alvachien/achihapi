using hihapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace hihapi.unittest.UnitTests.Models
{
    public class RepeatDatesWithAmountAndInterestCalInputTest
    {
        [Fact]
        public void Invalid_TotalAmountIsMust()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 0,
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Total amount", exception.Message);
        }

        [Fact]
        public void Invalid_InterestFreeMustNoRate()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                InterestFreeLoan = true,
                InterestRate = 1,
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Interest-Free", exception.Message);
        }

        [Fact]
        public void Invalid_NegativeInterestRate()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                InterestFreeLoan = false,
                InterestRate = -1,
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Interest rate", exception.Message);
        }

        [Fact]
        public void Invalid_TotalMonthIsMustForEqualPrincipal()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                InterestFreeLoan = false,
                InterestRate = 1,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                TotalMonths = 0
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Total months", exception.Message);
        }

        [Fact]
        public void Invalid_TotalMonthIsMustForEqualPrincipalAndInterset()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                InterestFreeLoan = false,
                InterestRate = 1,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipalAndInterset,
                TotalMonths = 0
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Total months", exception.Message);
        }

        [Fact]
        public void Invalid_InterestRateIsMustForEqualPrincipalAndInterset()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipalAndInterset,
                InterestFreeLoan = true,
                TotalMonths = 10,
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("Payment method need interest", exception.Message);
        }

        [Fact]
        public void Invalid_EndDateIsMustForDuePayment()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                RepaymentMethod = LoanRepaymentMethod.DueRepayment,
                InterestFreeLoan = true,
                TotalMonths = 10,
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("End date is mandatory", exception.Message);
        }

        [Fact]
        public void Invalid_EndDateIsUselessForInformalPayment()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                TotalAmount = 10000,
                RepaymentMethod = LoanRepaymentMethod.InformalPayment,
                InterestFreeLoan = true,
                TotalMonths = 10,
                EndDate = new DateTime(2022, 7, 2),
            };
            Action act = () => vm.doVerify();
            // assert
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Contains("End date", exception.Message);
        }
    }
}
