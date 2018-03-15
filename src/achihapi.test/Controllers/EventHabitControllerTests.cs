using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task Post_WithGene()
        {
            var vm = new EventHabitViewModel();
            var response = await _client.PostAsJsonAsync("/api/EventHabit", vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
