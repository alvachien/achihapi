using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using hihapi.Models;
using Microsoft.OData.Edm;
using hihapi.Utilities;

namespace hihapi.unittest.Utility
{
    public class CommonUtilityTest
    {
        [Theory]
        [MemberData(nameof(RepeatedDateTestingData))]
        public void RepeatedDates_Test(RepeatDatesCalculationInput datInput)
        {
            var results = CommonUtility.WorkoutRepeatedDates(datInput);

            switch(datInput.RepeatType)
            {
                case RepeatFrequency.Month:
                    Assert.Equal(12, results.Count);
                    for (int i = 0; i < 12; i++)
                    {
                        Assert.Equal(results[i].StartDate.Year, results[i].EndDate.Year);
                        Assert.Equal(results[i].StartDate.Month, results[i].EndDate.Month);
                        Assert.Equal(i + 1, results[i].StartDate.Month);
                        Assert.Equal(1, results[i].StartDate.Day);

                        switch (i)
                        {
                            case 0:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 1:
                                Assert.Equal(29, results[i].EndDate.Day);
                                break;

                            case 2:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 3:
                                Assert.Equal(30, results[i].EndDate.Day);
                                break;

                            case 4:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 5:
                                Assert.Equal(30, results[i].EndDate.Day);
                                break;

                            case 6:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 7:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 8:
                                Assert.Equal(30, results[i].EndDate.Day);
                                break;

                            case 9:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            case 10:
                                Assert.Equal(30, results[i].EndDate.Day);
                                break;

                            case 11:
                                Assert.Equal(31, results[i].EndDate.Day);
                                break;

                            default:
                                break;
                        }
                    }

                    break;

                case RepeatFrequency.Week:
                    Assert.Equal(5, results.Count);
                    break;

                default:
                    break;
            }
        }

        [Fact]
        public void WorkoutRepeatedDates_Month()
        {
            var startyear = 2020;
            RepeatDatesCalculationInput vm = new RepeatDatesCalculationInput
            {
                StartDate = new DateTime(startyear, 1, 1),
                EndDate = new DateTime(startyear, 12, 31),
                RepeatType = RepeatFrequency.Month
            };
            List<RepeatedDates> results = CommonUtility.WorkoutRepeatedDates(vm);

            Assert.Equal(12, results.Count);

            for (int i = 0; i < 12; i++)
            {
                Assert.Equal(startyear, results[i].StartDate.Year);
                Assert.Equal(results[i].StartDate.Year, results[i].EndDate.Year);
                Assert.Equal(results[i].StartDate.Month, results[i].EndDate.Month);
                Assert.Equal(i + 1, results[i].StartDate.Month);
                Assert.Equal(1, results[i].StartDate.Day);

                switch (i)
                {
                    case 0:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 1:
                        Assert.Equal(29, results[i].EndDate.Day);
                        break;

                    case 2:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 3:
                        Assert.Equal(30, results[i].EndDate.Day);
                        break;

                    case 4:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 5:
                        Assert.Equal(30, results[i].EndDate.Day);
                        break;

                    case 6:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 7:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 8:
                        Assert.Equal(30, results[i].EndDate.Day);
                        break;

                    case 9:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    case 10:
                        Assert.Equal(30, results[i].EndDate.Day);
                        break;

                    case 11:
                        Assert.Equal(31, results[i].EndDate.Day);
                        break;

                    default:
                        break;
                }
            }
        }

        [Fact]
        public void WorkoutRepeatedDates_Week()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(69);
            RepeatDatesCalculationInput vm = new RepeatDatesCalculationInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Week
            };
            List<RepeatedDates> results = CommonUtility.WorkoutRepeatedDates(vm);

            Assert.Equal(10, results.Count);

            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(startdate.Year, results[i].StartDate.Year);
                Assert.Equal(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = ((DateTime)results[i].EndDate).Subtract(results[i].StartDate);
                Assert.Equal(6, ts.TotalDays);
            }
        }

        [Fact]
        public void WorkoutRepeatedDates_Fortnight()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(69);
            RepeatDatesCalculationInput vm = new RepeatDatesCalculationInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Fortnight
            };
            List<RepeatedDates> results = CommonUtility.WorkoutRepeatedDates(vm);

            Assert.Equal(5, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                Assert.Equal(startdate.Year, results[i].StartDate.Year);
                Assert.Equal(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = ((DateTime)results[i].EndDate).Subtract(results[i].StartDate);
                Assert.Equal(13, ts.TotalDays);
            }
        }

        [Fact]
        public void WorkoutRepeatedDates_Quarter()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(100);
            RepeatDatesCalculationInput vm = new RepeatDatesCalculationInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Quarter
            };
            List<RepeatedDates> results = CommonUtility.WorkoutRepeatedDates(vm);

            Assert.Equal(2, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                Assert.Equal(startdate.Year, results[i].StartDate.Year);
                Assert.Equal(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = ((DateTime)results[i].EndDate).Subtract(results[i].StartDate);
                Assert.Equal(90, ts.TotalDays);
            }
        }

        [Fact]
        public void WorkoutRepeatedDates_Day()
        {
            var startdate = new Date(2020, 1, 6);
            var enddate = new Date(2020, 1, 10);
            RepeatDatesCalculationInput vm = new RepeatDatesCalculationInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Day
            };
            List<RepeatedDates> results = CommonUtility.WorkoutRepeatedDates(vm);

            Assert.Equal(5, results.Count);

            Assert.Equal(startdate, results[0].StartDate);
            Assert.Equal(startdate, results[0].EndDate);

            Assert.Equal(new Date(2020, 1, 7), results[1].StartDate);
            Assert.Equal(new Date(2020, 1, 7), results[1].EndDate);

            Assert.Equal(new Date(2020, 1, 8), results[2].StartDate);
            Assert.Equal(new Date(2020, 1, 8), results[2].EndDate);

            Assert.Equal(new Date(2020, 1, 9), results[3].StartDate);
            Assert.Equal(new Date(2020, 1, 9), results[3].EndDate);

            Assert.Equal(enddate, results[4].StartDate);
            Assert.Equal(enddate, results[4].EndDate);
        }

        [Theory]
        [MemberData(nameof(RepeatedDatesWithAmountTestingData))]
        public void RepeatedDatesWithAmount_test(RepeatDatesWithAmountCalculationInput datInput)
        {
            var results = CommonUtility.WorkoutRepeatedDatesWithAmount(datInput);

            switch (datInput.RepeatType)
            {
                case RepeatFrequency.Month:
                    Assert.Equal(12, results.Count);
                    for (int i = 0; i < 12; i++)
                    {
                        Assert.Equal(datInput.StartDate.Year, results[i].TranDate.Year);
                        Assert.Equal(i + 1, results[i].TranDate.Month);
                        Assert.Equal(1, results[i].TranDate.Day);
                        Assert.Equal(i + 1, results[i].TranDate.Month);
                        Assert.Equal(100, results[i].TranAmount);
                    }
                    break;

                case RepeatFrequency.Week:
                    Assert.Equal(4, results.Count);
                    for(int i = 0; i < 4; i++)
                    {
                        Assert.Equal(300, results[i].TranAmount);
                    }
                    break;

                default:
                    break;
            }

        }

        [Fact]
        public void RepeatedDatesWithAmountTest_InputCheck()
        {
            // Scenario 0. No input
            List<RepeatedDatesWithAmount> results = null;
            // Act
            Action act = () => CommonUtility.WorkoutRepeatedDatesWithAmount(null);
            // Assert
            var exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Input the data!", exception.Message);

            // Scenario 1. Invalid data range
            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now
            };
            // Act
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmount(vm);
            // Assert
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Invalid data range", exception.Message);

            // Scenario 2. Invalid total amount
            vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                TotalAmount = 0
            };
            // Act
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmount(vm);
            // Assert
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Invalid total amount", exception.Message);

            // Scenario 3. Invalid desp
            vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                TotalAmount = 1000,
                Desp = String.Empty
            };
            // Act
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmount(vm);
            // Assert
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Invalid desp", exception.Message);
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Day()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            var vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate,
                EndDate = startdate.AddDays(10),
                RepeatType = RepeatFrequency.Day,
                TotalAmount = 10000,
                Desp = "Test_Day"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.Equal(10, results.Count);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddDays(i++), rst.TranDate);
                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Week()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddDays(70),
                RepeatType = RepeatFrequency.Week,
                TotalAmount = 10000,
                Desp = "Test_Week"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddDays(i), rst.TranDate);
                i += 7;

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Week2()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = new DateTime(2019, 1, 5);
            var enddate = new DateTime(2020, 1, 31);

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = enddate.Date,
                RepeatType = RepeatFrequency.Week,
                TotalAmount = 10000,
                Desp = "Test_Week"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 55);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddDays(i), rst.TranDate);
                i += 7;

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Fortnight()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddDays(140),
                RepeatType = RepeatFrequency.Fortnight,
                TotalAmount = 10000,
                Desp = "Test_Fortnight"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddDays(i), rst.TranDate);
                i += 14;

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Month()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddMonths(10),
                RepeatType = RepeatFrequency.Month,
                TotalAmount = 10000,
                Desp = "Test_Month"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddMonths(i++), rst.TranDate);

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Month2()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = new Date(2020, 1, 1),
                EndDate = new Date(2021, 1, 1),
                RepeatType = RepeatFrequency.Month,
                TotalAmount = 2219.00M,
                Desp = "Test_Month"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 12);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddMonths(i++), rst.TranDate);

                // Amount
                if (i == 1)
                    Assert.Equal(0, Decimal.Compare(184.88M, rst.TranAmount));
                else
                    Assert.Equal(0, Decimal.Compare(184.92M, rst.TranAmount));

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Quarter()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddMonths(30),
                RepeatType = RepeatFrequency.Quarter,
                TotalAmount = 10000,
                Desp = "Test_Quarter"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddMonths(i), rst.TranDate);
                i += 3;

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_HalfYear()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddMonths(60),
                RepeatType = RepeatFrequency.HalfYear,
                TotalAmount = 10000,
                Desp = "Test_HalfYear"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddMonths(i), rst.TranDate);
                i += 6;

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Year()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddYears(10),
                RepeatType = RepeatFrequency.Year,
                TotalAmount = 10000,
                Desp = "Test_Year"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.Equal(vm.StartDate.AddYears(i++), rst.TranDate);

                // Amount
                Assert.Equal(1000, rst.TranAmount);

                // Desp
                Assert.NotEqual(String.Empty, rst.Desp);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountTest_Manual()
        {
            List<RepeatedDatesWithAmount> results = null;
            var startdate = DateTime.Today;

            RepeatDatesWithAmountCalculationInput vm = new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate.Date,
                EndDate = startdate.AddYears(10),
                RepeatType = RepeatFrequency.Manual,
                TotalAmount = 10000,
                Desp = "Test_Manual"
            };

            results = CommonUtility.WorkoutRepeatedDatesWithAmount(vm);

            // Total count
            Assert.True(results.Count == 1);

            // Date
            Assert.Equal(vm.EndDate, results[0].TranDate);

            // Amount
            Assert.Equal(10000, results[0].TranAmount);

            // Desp
            Assert.NotEqual(String.Empty, results[0].Desp);
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_InterestFree()
        {
            RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                InterestFreeLoan = true,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 120000,
                EndDate = new DateTime(2021, 1, 1),
                TotalMonths = 12
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            Assert.Equal(12, results.Count);

            var realdate = results[0].TranDate;
            Assert.Equal(2020, realdate.Year);
            Assert.Equal(1, realdate.Month);
            Assert.Equal(1, realdate.Day);

            foreach (var rst in results)
            {
                Assert.Equal(10000, rst.TranAmount);
                Assert.True(rst.InterestAmount == 0);
            }
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_RepayDay()
        {
            RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                InterestFreeLoan = true,
                StartDate = new DateTime(2020, 1, 10),
                TotalAmount = 120000,
                EndDate = new DateTime(2021, 1, 10),
                TotalMonths = 12,
                RepayDayInMonth = 15
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            Assert.Equal(12, results.Count);

            var realdate = results[0].TranDate;
            Assert.Equal(2020, realdate.Year);
            Assert.Equal(1, realdate.Month);
            Assert.Equal(15, realdate.Day);

            foreach (var rst in results)
            {
                Assert.Equal(10000, rst.TranAmount);
                Assert.True(rst.InterestAmount == 0);
            }
        }

        //[Fact]
        //public void RepeatedDatesWithAmountAndInterestTest_FirstRepayDay()
        //{
        //    RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
        //    {
        //        RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
        //        InterestFreeLoan = false,
        //        StartDate = new DateTime(2020, 1, 10),
        //        TotalAmount = 120000,
        //        EndDate = new DateTime(2021, 1, 10),
        //        TotalMonths = 12,
        //        FirstRepayDate = new DateTime(2020, 2, 15)
        //    };
        //    List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

        //    Assert.Equal(12, results.Count);

        //    var realdate = results[0].TranDate;
        //    Assert.Equal(2020, realdate.Year);
        //    Assert.Equal(2, realdate.Month);
        //    Assert.Equal(15, realdate.Day);

        //    foreach (var rst in results)
        //    {
        //        Assert.Equal(10000, rst.TranAmount);
        //        Assert.True(rst.InterestAmount == 0);
        //    }
        //}

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_InterestFreeAndDue()
        {
            RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = LoanRepaymentMethod.DueRepayment,
                InterestFreeLoan = true,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 120000,
                EndDate = new DateTime(2021, 1, 1)
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            Assert.True(1 == results.Count);
            Assert.Equal(2021, results[0].TranDate.Year);
            Assert.Equal(120000, results[0].TranAmount);
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_InputChecks()
        {
            // Scenario 0. No input
            // Arrange
            List<RepeatedDatesWithAmountAndInterest> results = null;
            // Act
            Action act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(null);
            // Assert
            var exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Input the data!", exception.Message);

            // Scenario 1. Interest free VS Interest rate
            RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = true,
                StartDate = DateTime.Now,
                TotalAmount = 300000,
                InterestRate = 3
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Cannot input interest rate for Interest-Free loan", exception.Message);

            // Scenario 2. Negative interest rate
            vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 300000,
                InterestRate = -3
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Interest rate can not be negative", exception.Message);

            // Scenario 3. Missing Total amount
            vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 0,
                InterestRate = 3
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Total amount must large than zero!", exception.Message);

            // Scenario 4. Missing total month
            vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = DateTime.Now,
                TotalAmount = 20,
                InterestRate = 3
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Not supported method", exception.Message);

            // Scenario 5. Missing Start date
            //vm = new RepeatDatesWithAmountAndInterestCalInput
            //{
            //    InterestFreeLoan = false,
            //    TotalAmount = 20,
            //    InterestRate = 3,
            //    TotalMonths = 3,
            //    RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
            //};
            //act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            //exception = Assert.Throws<Exception>(act);
            //Assert.Null(results);
            //Assert.Equal("Start date is must", exception.Message);

            // Scenario 6. Different repay day with first repay date
            vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                TotalAmount = 10000,
                InterestRate = 3,
                TotalMonths = 12,
                StartDate = new DateTime(2020, 1, 3),
                EndDate = new DateTime(2021, 1, 3),
                RepayDayInMonth = 15,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                FirstRepayDate = new DateTime(2020, 2, 13)
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Inconsistency in first payment data and repay day", exception.Message);

            // Scenario 7. Invalid repay day
            vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                TotalAmount = 10000,
                InterestRate = 3,
                TotalMonths = 12,
                StartDate = new DateTime(2018, 1, 3),
                EndDate = new DateTime(2019, 1, 3),
                RepayDayInMonth = 30,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
            };
            act = () => CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);
            exception = Assert.Throws<Exception>(act);
            Assert.Null(results);
            Assert.Equal("Invalid repay. date", exception.Message);

            // Scenario 8. Others?
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_EqualCAndIAndFirstRepayDay()
        {
            RepeatDatesWithAmountAndInterestCalInput vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 8, 23),
                TotalAmount = 2680000,
                TotalMonths = 360,
                InterestRate = 0.0441M,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipalAndInterset,
                FirstRepayDate = new DateTime(2020, 10, 1)
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            Assert.Equal(360, results.Count);

            var realdate = results[0].TranDate;
            Assert.Equal(2020, realdate.Year);
            Assert.Equal(10, realdate.Month);
            Assert.Equal(1, realdate.Day);
            Assert.True(Math.Abs(16062.66M - results[0].TotalAmount) <= 0.01M); // Rounding
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_EqualCAndI()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                TotalMonths = 12,
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipalAndInterset
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            //1   8530.99 8168.50 362.50  91831.51
            Assert.True(Math.Abs(8168.50M - results[0].TranAmount) <= 0.01M);
            //Assert.Equal(8168.50M, results[0].TranAmount);
            Assert.True(Math.Abs(362.50M - results[0].InterestAmount) <= 0.01M);
            //Assert.Equal(362.50M, results[0].InterestAmount);
            Assert.True(Math.Abs(8530.99M - (results[0].TranAmount + results[0].InterestAmount)) <= 0.01M);

            //2   8530.99 8198.11 332.89  83633.41
            //Assert.Equal(8198.11M, results[1].TranAmount);
            //Assert.Equal(332.89M, results[1].InterestAmount);
            Assert.True(Math.Abs(8198.11M - results[1].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(332.89M - results[1].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[1].TranAmount + results[1].InterestAmount)) <= 0.01M);

            //3   8530.99 8227.82 303.17  75405.59
            //Assert.Equal(8227.82M, results[2].TranAmount);
            //Assert.Equal(303.17M, results[2].InterestAmount);
            Assert.True(Math.Abs(8227.82M - results[2].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(303.17M - results[2].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[2].TranAmount + results[2].InterestAmount)) <= 0.01M);

            //4   8530.99 8257.65 273.35  67147.95
            //Assert.Equal(8257.65M, results[3].TranAmount);
            //Assert.Equal(273.35M, results[3].InterestAmount);
            Assert.True(Math.Abs(8257.65M - results[3].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(273.35M - results[3].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[3].TranAmount + results[3].InterestAmount)) <= 0.01M);

            //5   8530.99 8287.58 243.41  58860.37
            Assert.True(Math.Abs(8287.58M - results[4].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(243.41M - results[4].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[4].TranAmount + results[4].InterestAmount)) <= 0.01M);

            //6   8530.99 8317.63 213.37  50542.75
            Assert.True(Math.Abs(8317.63M - results[5].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(213.37M - results[5].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[5].TranAmount + results[5].InterestAmount)) <= 0.01M);

            //7   8530.99 8347.78 183.22  42194.97
            Assert.True(Math.Abs(8347.78M - results[6].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(183.22M - results[6].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[6].TranAmount + results[6].InterestAmount)) <= 0.01M);

            //8   8530.99 8378.04 152.96  33816.94
            Assert.True(Math.Abs(8378.04M - results[7].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(152.96M - results[7].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[7].TranAmount + results[7].InterestAmount)) <= 0.01M);

            //9   8530.99 8408.41 122.59  25408.54
            Assert.True(Math.Abs(8408.41M - results[8].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(122.59M - results[8].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[8].TranAmount + results[8].InterestAmount)) <= 0.01M);

            //10  8530.99 8438.89 92.11   16969.65
            Assert.True(Math.Abs(8438.89M - results[9].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(92.11M - results[9].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[9].TranAmount + results[9].InterestAmount)) <= 0.01M);

            //11  8530.99 8469.48 61.51   8500.18
            Assert.True(Math.Abs(8469.48M - results[10].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(61.51M - results[10].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[10].TranAmount + results[10].InterestAmount)) <= 0.01M);

            //12  8530.99 8500.18 30.81   00.00
            Assert.True(Math.Abs(8500.18M - results[11].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(30.81M - results[11].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8530.99M - (results[11].TranAmount + results[11].InterestAmount)) <= 0.01M);
        }

        [Theory]
        [InlineData(50000.00, 0.05)]
        public void RepeatedDatesWithAmountAndInterestTest_EqualCEx(Decimal totalAmount, Decimal interestRate)
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = totalAmount,
                TotalMonths = 12,
                InterestRate = interestRate,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            var idx = 0;
            Decimal amtTotal = 0;
            foreach(var rst in results)
            {
                if (idx == 0)
                {
                    //Assert.True(Math.Abs(4166.71M - rst.TranAmount) <= 0.01M);
                }
                else
                {
                    Assert.True(Math.Abs(4166.67M - rst.TranAmount) <= 0.01M);
                }
                idx++;
                amtTotal += rst.TranAmount;
            }

            Assert.True(Math.Abs(amtTotal - totalAmount) <= 0.01M);
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_EqualC()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                TotalMonths = 12,
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            //1   8695.83 8333.34 362.50  91666.67
            Assert.True(Math.Abs(8333.37M - results[0].TranAmount) <= 0.01M);
            //Assert.Equal(8168.50M, results[0].TranAmount);
            Assert.True(Math.Abs(362.50M - results[0].InterestAmount) <= 0.01M);
            //Assert.Equal(362.50M, results[0].InterestAmount);
            Assert.True(Math.Abs(8695.87M - (results[0].TranAmount + results[0].InterestAmount)) <= 0.01M);

            //2   8665.63 8333.34 332.29  83333.33
            Assert.True(Math.Abs(8333.34M - results[1].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(332.29M - results[1].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8665.63M - (results[1].TranAmount + results[1].InterestAmount)) <= 0.01M);

            //3   8635.42 8333.34 302.08  75000.00
            Assert.True(Math.Abs(8333.34M - results[2].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(302.08M - results[2].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8635.42M - (results[2].TranAmount + results[2].InterestAmount)) <= 0.01M);

            //4   8605.21 8333.34 271.88  66666.67
            Assert.True(Math.Abs(8333.34M - results[3].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(271.88M - results[3].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8605.21M - (results[3].TranAmount + results[3].InterestAmount)) <= 0.01M);

            //5   8575.00 8333.34 241.67  58333.33
            Assert.True(Math.Abs(8333.34M - results[4].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(241.67M - results[4].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8575.00M - (results[4].TranAmount + results[4].InterestAmount)) <= 0.01M);

            //6   8544.79 8333.34 211.46  50000.00
            Assert.True(Math.Abs(8333.34M - results[5].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(211.46M - results[5].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8544.79M - (results[5].TranAmount + results[5].InterestAmount)) <= 0.01M);

            //7   8514.58 8333.34 181.25  41666.67
            Assert.True(Math.Abs(8333.34M - results[6].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(181.25M - results[6].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8514.58M - (results[6].TranAmount + results[6].InterestAmount)) <= 0.01M);

            //8   8484.38 8333.34 151.04  33333.33
            Assert.True(Math.Abs(8333.34M - results[7].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(151.04M - results[7].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8484.38M - (results[7].TranAmount + results[7].InterestAmount)) <= 0.01M);

            //9   8454.17 8333.34 120.83  25000.00
            Assert.True(Math.Abs(8333.34M - results[8].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(120.83M - results[8].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8454.17M - (results[8].TranAmount + results[8].InterestAmount)) <= 0.01M);

            //10  8423.96 8333.34 90.63   16666.67
            Assert.True(Math.Abs(8333.34M - results[9].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(90.63M - results[9].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8423.96M - (results[9].TranAmount + results[9].InterestAmount)) <= 0.01M);

            //11  8393.75 8333.34 60.42   8333.33
            Assert.True(Math.Abs(8333.34M - results[10].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(60.42M - results[10].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8393.75M - (results[10].TranAmount + results[10].InterestAmount)) <= 0.01M);

            //12  8363.54 8333.34 30.21   00.00
            Assert.True(Math.Abs(8333.34M - results[11].TranAmount) <= 0.01M);
            Assert.True(Math.Abs(30.21M - results[11].InterestAmount) <= 0.01M);
            Assert.True(Math.Abs(8363.54M - (results[11].TranAmount + results[11].InterestAmount)) <= 0.01M);

            // Total amount shall be equal
            decimal amttotalpaid = 0;
            foreach(var rst in results)
            {
                amttotalpaid += rst.TranAmount;
            }
            Assert.True(Math.Abs(amttotalpaid - vm.TotalAmount) < 0.01M);
        }

        [Fact]
        public void RepeatedDatesWithAmountAndInterestTest_DueRepayment()
        {
            var vm = new RepeatDatesWithAmountAndInterestCalInput
            {
                InterestFreeLoan = false,
                StartDate = new DateTime(2020, 1, 1),
                TotalAmount = 100000,
                EndDate = new DateTime(2021, 1, 1),
                InterestRate = 0.0435M,
                RepaymentMethod = LoanRepaymentMethod.DueRepayment
            };
            List<RepeatedDatesWithAmountAndInterest> results = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(vm);

            Assert.Equal<Decimal>(100000M, results[0].TranAmount);
            Assert.Equal<Decimal>(4350M, results[0].InterestAmount);
        }

        public static IEnumerable<object[]> RepeatedDateTestingData => new List<object[]>
        {
            new object[] 
            { 
                new RepeatDatesCalculationInput() 
                {
                    StartDate = new Date(2020, 1, 1),
                    EndDate = new Date(2020, 12, 31),
                    RepeatType = RepeatFrequency.Month
                }
            },
            new object[] 
            {
                new RepeatDatesCalculationInput()
                {
                    StartDate = new Date(2020, 1, 1),
                    EndDate = new Date(2020, 2, 1),
                    RepeatType = RepeatFrequency.Week
                }
            }
        };

        public static IEnumerable<object[]> RepeatedDatesWithAmountTestingData => new List<object[]>
        {
            new object[]
            {
                new RepeatDatesWithAmountCalculationInput()
                {
                    StartDate = new Date(2020, 1, 1),
                    EndDate = new Date(2021, 1, 1),
                    RepeatType = RepeatFrequency.Month,
                    TotalAmount = 1200,
                    Desp = "Test1"
                }
            },
            new object[]
            {
                new RepeatDatesWithAmountCalculationInput()
                {
                    StartDate = new Date(2020, 1, 1),
                    EndDate = new Date(2020, 2, 1),
                    RepeatType = RepeatFrequency.Week,
                    TotalAmount = 1200,
                    Desp = "Test2"
                }
            }
        };
    }
}
