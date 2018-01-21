using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;
using achihapi.Controllers;
using achihapi.Utilities;

namespace achihapi.test
{
    [TestClass]
    public class EventUtilityTest
    {
        [TestMethod]
        public void RecurEventGenerateTest_InputCheck()
        {
            // Scenario 0. No input
            List<EventGenerationResultViewModel> results = null;
            try
            {
                results = EventUtility.GenerateEvents(null);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 1. Invalid data range
            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = DateTime.Now.AddDays(2),
                EndTimePoint = DateTime.Now
            };
            try
            {
                results = EventUtility.GenerateEvents(vm);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);

            // Scenario 2. Invalid name
            vm = new EventGenerationInputViewModel
            {
                StartTimePoint = DateTime.Now,
                EndTimePoint = DateTime.Now.AddMonths(1),
                Name = String.Empty
            };
            try
            {
                results = EventUtility.GenerateEvents(vm);

                Assert.Fail("UT Failed: throw exception when no inputts!");
            }
            catch (Exception exp)
            {
                Assert.IsNotNull(exp);
            }
            Assert.IsNull(results);
        }

        [TestMethod]
        public void RecurEventGenerateTest_Day()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddDays(10),
                RptType = RepeatFrequency.Day,
                Name = "Test_Day"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddDays(i++), rst.StartTimePoint);

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_Week()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddDays(70),
                RptType = RepeatFrequency.Week,
                Name = "Test_Week"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddDays(i*7), rst.StartTimePoint);
                i++;

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_Fortnight()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddDays(140),
                RptType = RepeatFrequency.Fortnight,
                Name = "Test_Fortnight"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddDays(i*14), rst.StartTimePoint);
                i++;
                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_Month()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddMonths(10),
                RptType = RepeatFrequency.Month,
                Name = "Test_Month"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddMonths(i++), rst.StartTimePoint);

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_Quarter()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddMonths(30),
                RptType = RepeatFrequency.Quarter,
                Name = "Test_Quarter"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddMonths(i * 3), rst.StartTimePoint);
                i++;

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_HalfYear()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddMonths(60),
                RptType = RepeatFrequency.HalfYear,
                Name = "Test_HalfYear"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddMonths(i*6), rst.StartTimePoint);
                i++;

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }

        [TestMethod]
        public void RecurEventGenerateTest_Year()
        {
            List<EventGenerationResultViewModel> results = null;
            var startdate = DateTime.Today;

            EventGenerationInputViewModel vm = new EventGenerationInputViewModel
            {
                StartTimePoint = startdate,
                EndTimePoint = startdate.AddYears(10),
                RptType = RepeatFrequency.Year,
                Name = "Test_Year"
            };

            results = EventUtility.GenerateEvents(vm);

            // Total count
            Assert.AreEqual(results.Count, 10);
            Int32 i = 0;
            foreach (var rst in results)
            {
                // Date
                Assert.AreEqual(vm.StartTimePoint.AddYears(i++), rst.StartTimePoint);

                // Desp
                Assert.AreNotEqual(String.Empty, rst.Name);
            }
        }
    }
}
