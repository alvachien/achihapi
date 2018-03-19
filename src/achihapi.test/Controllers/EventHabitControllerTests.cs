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
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace achihapi.test.Controllers
{
    [TestClass]
    public class EventHabitControllerTests
    {
        private HttpClient _client;
        private TestServer _server;

        [TestInitialize]
        public void TestInitialize()
        {
            var startupAssembly = typeof(achihapi.Startup).GetTypeInfo().Assembly;
            var contentRoot = TestFixture<achihapi.Startup>.GetProjectPath("src", startupAssembly);

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .UseEnvironment("Test")
                .ConfigureServices(InitializeServices)
                .UseStartup(typeof(achihapi.Startup));

            _server = new TestServer(builder);

            _client = _server.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_client != null)
                _client.Dispose();
            if (_server != null)
                _server.Dispose();
        }

        public EventHabitControllerTests()
        {
        }

        protected virtual void InitializeServices(IServiceCollection services)
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
            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);

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

            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);

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
            vm.EndDate = DateTime.Today.AddYears(-1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Test";

            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(false, response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Length);
            StringAssert.Equals("Date range is invaid", result);
        }

        [TestMethod]
        public async Task Post_GenerateMode()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;

            var response = await _client.PostAsJsonAsync("/api/EventHabit?geneMode=true", vm);

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
        public async Task Post_CreateModeSucceed()
        {
            var vm = new EventHabitViewModel();
            vm.Name = "Test";
            vm.StartDate = DateTime.Today;
            vm.EndDate = DateTime.Today.AddYears(1);
            vm.RptType = RepeatFrequency.Month;
            vm.Content = "Test";
            vm.Count = 1;

            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result = await response.Content.ReadAsJsonAsync<EventHabitViewModel>();
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Details.Count);
            Assert.AreEqual(12, result.Details.Count);
        }
    }
}
