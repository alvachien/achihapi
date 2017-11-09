using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;
using achihapi.Controllers;

namespace achihapi.test
{
    [TestClass]
    public class FinanceCalcUtilityTest
    {
        //[TestMethod]
        //public void LoanCalcTest_EmptyInput()
        //{
        //    LoanCalcViewModel vm = new LoanCalcViewModel();
        //    List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

        //    Assert.AreEqual(0, results.Count);
        //}

        [TestMethod]
        public void LoanCalcTest_InterestFree()
        {
            LoanCalcViewModel vm = new LoanCalcViewModel
            {
                InterestFreeLoan = true,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 300000,
                TotalMonths = 12
            };
            List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(300000, results[0].TranAmount);

            var realdate = results[0].TranDate;
            Assert.AreEqual(2021, realdate.Year);
            Assert.AreEqual(1, realdate.Month);
            Assert.AreEqual(1, realdate.Day);
        }

        [TestMethod]
        public void LoanCalcTest_InputChecks()
        {
            // Scenario 0. No input
            List<LoanCalcResult> results = null;
            try
            {
                results = FinanceCalcUtility.LoanCalculate(null);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch(Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 1. Interest free VS Interest rate
            LoanCalcViewModel vm = new LoanCalcViewModel
            {
                InterestFreeLoan = true,
                StartDate = DateTime.Now,
                TotalAmount = 300000,
                InterestRate = 3
            };
            try
            {
                results = FinanceCalcUtility.LoanCalculate(vm);
                Assert.Fail("UT Failed: throw exception for Interest rate inputted in Interest-Free loan!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 2. Negative interest rate
            vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 300000,
                InterestRate = -3
            };
            try
            {
                results = FinanceCalcUtility.LoanCalculate(vm);
                Assert.Fail("UT Failed: throw exception for negative Interest rate");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 3. Missing Total amount
            vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 0,
                InterestRate = 3
            };
            try
            {
                results = FinanceCalcUtility.LoanCalculate(vm);
                Assert.Fail("UT Failed: throw exception invalid total amount");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 4. Missing total month
            vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 0,
                InterestRate = 3
            };
            try
            {
                results = FinanceCalcUtility.LoanCalculate(vm);
                Assert.Fail("UT Failed: throw exception invalid total mouth");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 5. Missing Start date
            vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                TotalAmount = 0,
                InterestRate = 3
            };
            try
            {
                results = FinanceCalcUtility.LoanCalculate(vm);
                Assert.Fail("UT Failed: throw exception invalid start date");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 6. Others?
        }
    }
}
