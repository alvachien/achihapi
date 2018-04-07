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
    public class EventHabitCheckInControllerTest
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/EventHabitCheckIn";

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

                String sqlCmd = SqlScriptHelper.EventHabitCheckInController_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }
        }

        public EventHabitCheckInControllerTest()
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
            var vm = new EventHabitCheckInViewModel();
            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.AreEqual(false, response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Post_NormalCheckIn()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "CheckIn Test Monthly";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Check In Test";
            vm.Count = 2;
            vm.HID = SqlScriptHelper.HID_Tester;

            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);
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

            var vm2 = new EventHabitCheckInViewModel();
            vm2.HID = SqlScriptHelper.HID_Tester;
            vm2.HabitID = result.ID;
            vm2.Score = 80;
            vm2.TranDate = vm.StartDate.AddDays(7);
            response = await _client.PostAsJsonAsync(_apiurl, vm2);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result2 = await response.Content.ReadAsJsonAsync<EventHabitCheckInViewModel>();
            Assert.IsNotNull(response.Content);
            Assert.IsTrue(result2.ID > 0);
        }
    }
}
