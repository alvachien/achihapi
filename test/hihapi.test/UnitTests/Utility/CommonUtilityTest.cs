using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using hihapi.Models;
using Microsoft.OData.Edm;
using hihapi.Utilities;

namespace hihapi.test.UnitTests
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

        [Theory]
        [MemberData(nameof(RepeatedDateWithAmountTestingData))]
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

        public static IEnumerable<object[]> RepeatedDateWithAmountTestingData => new List<object[]>
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
