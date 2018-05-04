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
    public class FinanceDocItemSearchControllerTest
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/FinanceDocItemSearch";

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

            // Buildup the database
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                String sqlCmd = SqlScriptHelper.FinanceDocItemSearch_Init;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }

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

                String sqlCmd = SqlScriptHelper.FinanceDocItemSearch_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }
        }

        public FinanceDocItemSearchControllerTest()
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
            var response = await _client.GetAsync(_apiurl + "/hid=" + UnitTestUtility.UnitTestHomeID.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
