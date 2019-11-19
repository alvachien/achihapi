using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using achihapi.ViewModels;
using achihapi.Controllers;
using achihapi.Utilities;

namespace achihapi.test
{
    [TestClass]
    public class CommonUtilityTest
    {
        [TestMethod]
        public void GetDates_Month()
        {
            var startyear = 2020;
            var endyear = 2021;
            RepeatFrequencyDateInput vm = new RepeatFrequencyDateInput
            {
                StartDate = new DateTime(startyear, 1, 1),
                EndDate = new DateTime(startyear, 12, 31),
                RptType = RepeatFrequency.Month
            };
            List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

            Assert.AreEqual(12, results.Count);

            for(int i = 0; i < 12; i ++)
            {
                Assert.AreEqual(startyear, results[i].StartDate.Year);
                Assert.AreEqual(results[i].StartDate.Year, results[i].EndDate.Year);
                Assert.AreEqual(results[i].StartDate.Month, results[i].EndDate.Month);
                Assert.AreEqual(i + 1, results[i].StartDate.Month);
                Assert.AreEqual(1, results[i].StartDate.Day);

                switch (i)
                {
                    case 0:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 1:
                        Assert.AreEqual(29, results[i].EndDate.Day);
                        break;

                    case 2:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 3:
                        Assert.AreEqual(30, results[i].EndDate.Day);
                        break;

                    case 4:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 5:
                        Assert.AreEqual(30, results[i].EndDate.Day);
                        break;

                    case 6:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 7:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 8:
                        Assert.AreEqual(30, results[i].EndDate.Day);
                        break;

                    case 9:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    case 10:
                        Assert.AreEqual(30, results[i].EndDate.Day);
                        break;

                    case 11:
                        Assert.AreEqual(31, results[i].EndDate.Day);
                        break;

                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public void GetDates_Week()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(69);
            RepeatFrequencyDateInput vm = new RepeatFrequencyDateInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RptType = RepeatFrequency.Week
            };
            List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

            Assert.AreEqual(10, results.Count);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(startdate.Year, results[i].StartDate.Year);
                Assert.AreEqual(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = results[i].EndDate.Subtract(results[i].StartDate);
                Assert.AreEqual(6, ts.TotalDays);
            }
        }

        [TestMethod]
        public void GetDates_Fortnight()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(69);
            RepeatFrequencyDateInput vm = new RepeatFrequencyDateInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RptType = RepeatFrequency.Fortnight
            };
            List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

            Assert.AreEqual(5, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(startdate.Year, results[i].StartDate.Year);
                Assert.AreEqual(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = results[i].EndDate.Subtract(results[i].StartDate);
                Assert.AreEqual(13, ts.TotalDays);
            }
        }

        [TestMethod]
        public void GetDates_Quarter()
        {
            var startdate = new DateTime(2020, 1, 1);
            var enddate = startdate.AddDays(100);
            RepeatFrequencyDateInput vm = new RepeatFrequencyDateInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RptType = RepeatFrequency.Quarter
            };
            List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

            Assert.AreEqual(2, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(startdate.Year, results[i].StartDate.Year);
                Assert.AreEqual(results[i].StartDate.Year, results[i].EndDate.Year);

                TimeSpan ts = results[i].EndDate.Subtract(results[i].StartDate);
                Assert.AreEqual(90, ts.TotalDays);
            }
        }

        [TestMethod]
        public void GetDates_Day()
        {
            var startdate = new DateTime(2020, 1, 6);
            var enddate = new DateTime(2020, 1, 10);
            RepeatFrequencyDateInput vm = new RepeatFrequencyDateInput
            {
                StartDate = startdate,
                EndDate = enddate,
                RptType = RepeatFrequency.Day
            };
            List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

            Assert.AreEqual(5, results.Count);

            Assert.AreEqual(startdate, results[0].StartDate.Date);
            Assert.AreEqual(startdate, results[0].EndDate.Date);

            Assert.AreEqual(new DateTime(2020, 1, 7), results[1].StartDate.Date);
            Assert.AreEqual(new DateTime(2020, 1, 7), results[1].EndDate.Date);

            Assert.AreEqual(new DateTime(2020, 1, 8), results[2].StartDate.Date);
            Assert.AreEqual(new DateTime(2020, 1, 8), results[2].EndDate.Date);

            Assert.AreEqual(new DateTime(2020, 1, 9), results[3].StartDate.Date);
            Assert.AreEqual(new DateTime(2020, 1, 9), results[3].EndDate.Date);

            Assert.AreEqual(enddate, results[4].StartDate.Date);
            Assert.AreEqual(enddate, results[4].EndDate.Date);
        }
    }
}
