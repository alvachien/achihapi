using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using achihapi;
using achihapi.Controllers;
using achihapi.test;
using achihapi.ViewModels;
using achihapi.Utilities;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace achihapi.test.Controllers
{
    [TestClass]
    public class EventHabitControllerTests
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/EventHabit";

        [ClassInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            var startupAssembly = typeof(achihapi.Startup).GetTypeInfo().Assembly;
            _contentRoot = TestFixture<achihapi.Startup>.GetProjectPath("src", startupAssembly);
            var jsonfile = new FileInfo(Path.Combine(_contentRoot, "appsettings.json"));
            var config = new ConfigurationBuilder()
                .AddJsonFile(jsonfile.FullName)
                .Build();
            _connString = config["ConnectionStrings:UnitTestConnection"];

            var builder = new WebHostBuilder()
                .UseConfiguration(config)
                .UseContentRoot(_contentRoot)
                .UseEnvironment("Test")
                .ConfigureServices(InitializeServices)
                .UseStartup(typeof(achihapi.Startup));

            _server = new TestServer(builder);

            _client = _server.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            if (_client != null)
                _client.Dispose();
            if (_server != null)
                _server.Dispose();

            // Clean the table.
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = SqlScriptHelper.EventHabitController_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
        }

        public EventHabitControllerTests()
        {
        }

        protected static void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(achihapi.Startup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. 
            // Overrides AddMvcCore() because it uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());            

            services.AddSingleton(manager);
        }

        [TestMethod]
        public async Task Post_InvalidCase1()
        {
            var vm = new EventHabitViewModel();
            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Post_InvalidCase2_Count()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Test";            

            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(false, response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Length);
            StringAssert.Equals("Count is must", result);
        }

        [TestMethod]
        public async Task Post_InvalidCase3_DateRange()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddDays(-1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Test";

            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(false, response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Length);
            StringAssert.Equals("Invalid data range", result);
        }

        [TestMethod]
        public async Task Post_GenerateMode()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;

            var response = await _client.PostAsJsonAsync(_apiurl + "?geneMode=true", vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var results = await response.Content.ReadAsJsonAsync<List<EventGenerationResultViewModel>>();
            Assert.IsNotNull(results);
            Assert.AreNotEqual(0, results.Count);
            Assert.AreEqual(12, results.Count);
        }

        [TestMethod]
        public async Task Post_CreateAndRead_MonthlyHabit()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test Monthly";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Test";
            vm.Count = 5;
            vm.HID = 1;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();                
                System.Diagnostics.Debug.WriteLine(errmsg);
            }
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result = await response.Content.ReadAsJsonAsync<EventHabitViewModel>();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Details.Count);
            Assert.AreEqual(12, result.Details.Count);

            // Check the table
            var nEvtID = result.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = @"SELECT * FROM [dbo].[t_event_habit] WHERE [ID] = " + nEvtID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                sqlCmd = @"SELECT * FROM [dbo].[t_event_habit_detail] WHERE [HabitID] = " + nEvtID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);
                Int32 cntdetail = 0;
                while(reader.Read())
                {
                    cntdetail++;
                }

                Assert.AreEqual(12, cntdetail);
            }

            // Do the read
            response = await _client.GetAsync(_apiurl + "/" + nEvtID.ToString() + "?hid=" + UnitTestUtility.UnitTestHomeID.ToString());
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result2 = await response.Content.ReadAsJsonAsync<EventHabitViewModel>();
            Assert.IsNotNull(result2);
            Assert.AreEqual(nEvtID, result2.ID);
            Assert.AreEqual(UnitTestUtility.UnitTestHomeID, result2.HID);
            Assert.AreEqual(vm.RptType, result2.RptType);
        }

        [TestMethod]
        public async Task Post_CreateMode_WeeklyHabit()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test Weekly";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddDays(14);
            vm.RptType = RepeatFrequency.Week;
            vm.Content = "Test";
            vm.Count = 5;
            vm.HID = 1;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result = await response.Content.ReadAsJsonAsync<EventHabitViewModel>();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Details.Count);
            Assert.AreEqual(2, result.Details.Count);

            // Check the table
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = @"SELECT * FROM [dbo].[t_event_habit] WHERE [ID] = " + result.ID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                sqlCmd = @"SELECT * FROM [dbo].[t_event_habit_detail] WHERE [HabitID] = " + result.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);
                Int32 cntdetail = 0;
                while (reader.Read())
                {
                    cntdetail++;
                }

                Assert.AreEqual(2, cntdetail);
            }
        }

        [TestMethod]
        public async Task Post_CreateAndDelete_Fortnight()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test Fortnight";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddDays(28);
            vm.RptType = RepeatFrequency.Fortnight;
            vm.Content = "Test";
            vm.Count = 5;
            vm.HID = 1;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result = await response.Content.ReadAsJsonAsync<EventHabitViewModel>();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Details.Count);
            Assert.AreEqual(2, result.Details.Count);

            // Check the table
            var nEvtID = result.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = @"SELECT * FROM [dbo].[t_event_habit] WHERE [ID] = " + nEvtID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();
            }

            response = await _client.DeleteAsync(_apiurl + "/" + nEvtID.ToString() + "?hid=" + UnitTestUtility.UnitTestHomeID.ToString());
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = @"SELECT * FROM [dbo].[t_event_habit] WHERE [ID] = " + nEvtID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Assert.AreEqual(false, reader.HasRows);

                reader.Close();
                cmd.Dispose();
            }
        }
    }
}
