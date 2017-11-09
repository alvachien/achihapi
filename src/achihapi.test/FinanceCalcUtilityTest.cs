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


        [TestMethod]
        public void LoanCalcTest_EqualCAndI()
        {
            LoanCalcViewModel vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                TotalMonths = 12,
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipalAndInterset
            };
            List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

            //1   8530.99 8168.50 362.50  91831.51
            Assert.AreEqual(8530.99M, results[0].TranAmount);
            Assert.AreEqual(362.50M, results[0].InterestAmount);

            //2   8530.99 8198.11 332.89  83633.41
            Assert.AreEqual(8530.99M, results[1].TranAmount);
            Assert.AreEqual(332.89M, results[1].InterestAmount);

            //3   8530.99 8227.82 303.17  75405.59
            Assert.AreEqual(8530.99M, results[2].TranAmount);
            Assert.AreEqual(303.17M, results[2].InterestAmount);

            //4   8530.99 8257.65 273.35  67147.95
            Assert.AreEqual(8530.99M, results[3].TranAmount);
            Assert.AreEqual(273.35M, results[3].InterestAmount);

            //5   8530.99 8287.58 243.41  58860.37
            //6   8530.99 8317.63 213.37  50542.75
            //7   8530.99 8347.78 183.22  42194.97
            //8   8530.99 8378.04 152.96  33816.94
            //9   8530.99 8408.41 122.59  25408.54
            //10  8530.99 8438.89 92.11   16969.65
            //11  8530.99 8469.48 61.51   8500.18
            //12  8530.99 8500.18 30.81   00.00
        }
    }
}
