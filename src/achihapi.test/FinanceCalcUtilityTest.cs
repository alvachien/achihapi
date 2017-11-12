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
            catch (Exception exp)
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
            Assert.IsTrue(Math.Abs(8168.50M - results[0].TranAmount) <= 0.01M);
            //Assert.AreEqual(8168.50M, results[0].TranAmount);
            Assert.IsTrue(Math.Abs(362.50M - results[0].InterestAmount) <= 0.01M);
            //Assert.AreEqual(362.50M, results[0].InterestAmount);
            Assert.IsTrue(Math.Abs(8530.99M - (results[0].TranAmount + results[0].InterestAmount)) <= 0.01M);

            //2   8530.99 8198.11 332.89  83633.41
            //Assert.AreEqual(8198.11M, results[1].TranAmount);
            //Assert.AreEqual(332.89M, results[1].InterestAmount);
            Assert.IsTrue(Math.Abs(8198.11M - results[1].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(332.89M - results[1].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[1].TranAmount + results[1].InterestAmount)) <= 0.01M);

            //3   8530.99 8227.82 303.17  75405.59
            //Assert.AreEqual(8227.82M, results[2].TranAmount);
            //Assert.AreEqual(303.17M, results[2].InterestAmount);
            Assert.IsTrue(Math.Abs(8227.82M - results[2].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(303.17M - results[2].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[2].TranAmount + results[2].InterestAmount)) <= 0.01M);

            //4   8530.99 8257.65 273.35  67147.95
            //Assert.AreEqual(8257.65M, results[3].TranAmount);
            //Assert.AreEqual(273.35M, results[3].InterestAmount);
            Assert.IsTrue(Math.Abs(8257.65M - results[3].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(273.35M - results[3].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[3].TranAmount + results[3].InterestAmount)) <= 0.01M);

            //5   8530.99 8287.58 243.41  58860.37
            Assert.IsTrue(Math.Abs(8287.58M - results[4].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(243.41M - results[4].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[4].TranAmount + results[4].InterestAmount)) <= 0.01M);

            //6   8530.99 8317.63 213.37  50542.75
            Assert.IsTrue(Math.Abs(8317.63M - results[5].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(213.37M - results[5].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[5].TranAmount + results[5].InterestAmount)) <= 0.01M);

            //7   8530.99 8347.78 183.22  42194.97
            Assert.IsTrue(Math.Abs(8347.78M - results[6].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(183.22M - results[6].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[6].TranAmount + results[6].InterestAmount)) <= 0.01M);

            //8   8530.99 8378.04 152.96  33816.94
            Assert.IsTrue(Math.Abs(8378.04M - results[7].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(152.96M - results[7].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[7].TranAmount + results[7].InterestAmount)) <= 0.01M);

            //9   8530.99 8408.41 122.59  25408.54
            Assert.IsTrue(Math.Abs(8408.41M - results[8].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(122.59M - results[8].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[8].TranAmount + results[8].InterestAmount)) <= 0.01M);

            //10  8530.99 8438.89 92.11   16969.65
            Assert.IsTrue(Math.Abs(8438.89M - results[9].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(92.11M - results[9].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[9].TranAmount + results[9].InterestAmount)) <= 0.01M);

            //11  8530.99 8469.48 61.51   8500.18
            Assert.IsTrue(Math.Abs(8469.48M - results[10].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(61.51M - results[10].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[10].TranAmount + results[10].InterestAmount)) <= 0.01M);

            //12  8530.99 8500.18 30.81   00.00
            Assert.IsTrue(Math.Abs(8500.18M - results[11].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(30.81M - results[11].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8530.99M - (results[11].TranAmount + results[11].InterestAmount)) <= 0.01M);
        }

        [TestMethod]
        public void LoanCalcTest_EqualC()
        {
            LoanCalcViewModel vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                TotalMonths = 12,
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal
            };
            List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

            //1   8695.83 8333.34 362.50  91666.67
            Assert.IsTrue(Math.Abs(8333.34M - results[0].TranAmount) <= 0.01M);
            //Assert.AreEqual(8168.50M, results[0].TranAmount);
            Assert.IsTrue(Math.Abs(362.50M - results[0].InterestAmount) <= 0.01M);
            //Assert.AreEqual(362.50M, results[0].InterestAmount);
            Assert.IsTrue(Math.Abs(8695.83M - (results[0].TranAmount + results[0].InterestAmount)) <= 0.01M);

            //2   8665.63 8333.34 332.29  83333.33
            Assert.IsTrue(Math.Abs(8333.34M - results[1].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(332.29M - results[1].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8665.63M - (results[1].TranAmount + results[1].InterestAmount)) <= 0.01M);

            //3   8635.42 8333.34 302.08  75000.00
            Assert.IsTrue(Math.Abs(8333.34M - results[2].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(302.08M - results[2].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8635.42M - (results[2].TranAmount + results[2].InterestAmount)) <= 0.01M);

            //4   8605.21 8333.34 271.88  66666.67
            Assert.IsTrue(Math.Abs(8333.34M - results[3].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(271.88M - results[3].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8605.21M - (results[3].TranAmount + results[3].InterestAmount)) <= 0.01M);

            //5   8575.00 8333.34 241.67  58333.33
            Assert.IsTrue(Math.Abs(8333.34M - results[4].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(241.67M - results[4].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8575.00M - (results[4].TranAmount + results[4].InterestAmount)) <= 0.01M);

            //6   8544.79 8333.34 211.46  50000.00
            Assert.IsTrue(Math.Abs(8333.34M - results[5].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(211.46M - results[5].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8544.79M - (results[5].TranAmount + results[5].InterestAmount)) <= 0.01M);

            //7   8514.58 8333.34 181.25  41666.67
            Assert.IsTrue(Math.Abs(8333.34M - results[6].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(181.25M - results[6].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8514.58M - (results[6].TranAmount + results[6].InterestAmount)) <= 0.01M);

            //8   8484.38 8333.34 151.04  33333.33
            Assert.IsTrue(Math.Abs(8333.34M - results[7].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(151.04M - results[7].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8484.38M - (results[7].TranAmount + results[7].InterestAmount)) <= 0.01M);

            //9   8454.17 8333.34 120.83  25000.00
            Assert.IsTrue(Math.Abs(8333.34M - results[8].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(120.83M - results[8].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8454.17M - (results[8].TranAmount + results[8].InterestAmount)) <= 0.01M);

            //10  8423.96 8333.34 90.63   16666.67
            Assert.IsTrue(Math.Abs(8333.34M - results[9].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(90.63M - results[9].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8423.96M - (results[9].TranAmount + results[9].InterestAmount)) <= 0.01M);

            //11  8393.75 8333.34 60.42   8333.33
            Assert.IsTrue(Math.Abs(8333.34M - results[10].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(60.42M - results[10].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8393.75M - (results[10].TranAmount + results[10].InterestAmount)) <= 0.01M);

            //12  8363.54 8333.34 30.21   00.00
            Assert.IsTrue(Math.Abs(8333.34M - results[11].TranAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(30.21M - results[11].InterestAmount) <= 0.01M);
            Assert.IsTrue(Math.Abs(8363.54M - (results[11].TranAmount + results[11].InterestAmount)) <= 0.01M);
        }

        [TestMethod]
        public void LoanCalcTest_DueReayment()
        {
            LoanCalcViewModel vm = new LoanCalcViewModel
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                TotalMonths = 12,
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.DueRepayment
            };
            List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

            Assert.AreEqual<Decimal>(100000M, results[0].TranAmount);
            Assert.AreEqual<Decimal>(4350M, results[0].InterestAmount);
        }

        [TestMethod]
        public void ADPTmpTest_InputCheck()
        {
            // Scenario 0. No input
            List<ADPGenerateResult> results = null;
            try
            {
                results = FinanceCalcUtility.GenerateAdvancePaymentTmps(null);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 1. Invalid data range
            ADPGenerateViewModel vm = new ADPGenerateViewModel
            {
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now
            };
            try
            {
                results = FinanceCalcUtility.GenerateAdvancePaymentTmps(vm);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 2. Invalid total amount
            vm = new ADPGenerateViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                TotalAmount = 0
            };
            try
            {
                results = FinanceCalcUtility.GenerateAdvancePaymentTmps(vm);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 3. Invalid desp
            vm = new ADPGenerateViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                TotalAmount = 1000,
                Desp = String.Empty
            };
            try
            {
                results = FinanceCalcUtility.GenerateAdvancePaymentTmps(vm);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);
        }

        [TestMethod]
        public void ADPTmpTest_Day()
        {
            List<ADPGenerateResult> results = null;
            ADPGenerateViewModel vm = new ADPGenerateViewModel
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddDays(10),
                TotalAmount = 10000,
                Desp = 'Test_Day'
            };

            results = FinanceCalcUtility.GenerateAdvancePaymentTmps(vm);
        }

        [TestMethod]
        public void ADPTmpTest_Week()
        {
            List<ADPGenerateResult> results = null;

        }

        [TestMethod]
        public void ADPTmpTest_Month()
        {
            List<ADPGenerateResult> results = null;

        }

        [TestMethod]
        public void ADPTmpTest_Year()
        {
            List<ADPGenerateResult> results = null;

        }
    }
}

