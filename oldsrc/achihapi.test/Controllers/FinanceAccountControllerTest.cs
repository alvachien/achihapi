using System;
using System.Collections.Generic;
using System.Text;
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
    public class FinanceAccountControllerTest
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/FinanceAccount";

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

                String sqlCmd = SqlScriptHelper.FinanceAccount_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }
        }

        public FinanceAccountControllerTest()
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
        public async Task Create_InvalidCase1_EmptyInput()
        {
            var vm = new FinanceAccountViewModel();
            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_InvalidCase2_InvalidCategory()
        {
            var vm = new FinanceAccountViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.CtgyID = FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_InvalidCase3_WithoutName()
        {
            var vm = new FinanceAccountViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.CtgyID = 1;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_NormalAccount1()
        {
            var vm = new FinanceAccountViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.CtgyID = 1; // Cash
            vm.Name = "Test Create Normal 1";
            vm.Owner = "Tester";
            vm.Status = FinanceAccountStatus.Normal;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAccountUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);
            Assert.IsTrue(nvm.ID > 0);

            // Check in the database
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT * FROM [dbo].[t_fin_account] WHERE [ID] = " + nvm.ID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                conn.Close();
            }
        }

        [TestMethod]
        public async Task CreateAndRead_NormalAccount()
        {
            var vm = new FinanceAccountViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.CtgyID = 1; // Cash
            vm.Name = "Test Create and Read Normal";
            vm.Owner = "Tester";
            vm.Status = FinanceAccountStatus.Normal;

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAccountUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);
            Assert.IsTrue(nvm.ID > 0);

            // Check in the database
            response = await _client.GetAsync(_apiurl + "/" + nvm.ID.ToString() + "?hid=" + UnitTestUtility.UnitTestHomeID.ToString());
            var nvm2 = await response.Content.ReadAsJsonAsync<FinanceAccountUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm2);
            Assert.AreEqual(nvm.ID, nvm2.ID);
        }
    }
}
