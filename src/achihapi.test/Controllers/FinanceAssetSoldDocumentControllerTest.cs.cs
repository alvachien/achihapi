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
    public class FinanceAssetSoldDocumentControllerTest
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/FinanceAssetSoldDocument";
        private const string _buyapiurl = "/api/FinanceAssetBuyDocument";

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

                String sqlCmd = SqlScriptHelper.FinanceAssetSoldDocument_Init;
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

                String sqlCmd = SqlScriptHelper.FinanceAssetSoldDocument_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }
        }

        public FinanceAssetSoldDocumentControllerTest()
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
        public async Task Post_InvalidCase1_EmptyInput()
        {
            var vm = new FinanceAssetDocumentUIViewModel();
            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Post_InvalidCase2_InvalidInput()
        {
            var vm = new FinanceAssetDocumentUIViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.DocType = FinanceDocTypeViewModel.DocType_AssetSoldOut;
            vm.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm.AccountVM = new FinanceAccountViewModel();
            vm.AccountVM.Name = "InvalidTest_InvalidInput";
            vm.AccountVM.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vm.AccountVM.ExtraInfo_AS.Name = "InvalidTest_InvalidInput";
            vm.AccountVM.ExtraInfo_AS.CategoryID = 1;
            var ditem = new FinanceDocumentItemUIViewModel();
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Sold asset";
            ditem.ItemID = 1;
            ditem.TranAmount = 2000;
            ditem.ControlCenterID = SqlScriptHelper.FinanceAssetBuy_CCID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_apiurl, vm);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode); // Invalid input data in items! => TranType
        }

        [TestMethod]
        public async Task Post_CreateMode_ItemWithCC()
        {
            var vm = new FinanceAssetDocumentUIViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.DocType = FinanceDocTypeViewModel.DocType_AssetBuyIn;
            vm.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm.Desp = "Buy asset test";
            vm.TranDate = DateTime.Today;
            vm.CreatedBy = "Tester";
            vm.CreatedAt = DateTime.Now;
            vm.AccountVM = new FinanceAccountViewModel();
            vm.AccountVM.HID = SqlScriptHelper.HID_Tester;
            vm.AccountVM.Name = "Create_ItemWithCC";
            vm.AccountVM.CtgyID = FinanceAccountCtgyViewModel.AccountCategory_Asset;
            vm.AccountVM.Comment = vm.AccountVM.Name;
            vm.AccountVM.CreatedBy = "Tester";
            vm.AccountVM.CreatedAt = DateTime.Now;
            vm.AccountVM.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vm.AccountVM.ExtraInfo_AS.Name = "Create_ItemWithCC";
            vm.AccountVM.ExtraInfo_AS.CategoryID = 1;
            var ditem = new FinanceDocumentItemUIViewModel();
            ditem.AccountID = SqlScriptHelper.FinanceAssetSold_AccountID;
            ditem.Desp = "Buy asset";
            ditem.ItemID = 1;
            ditem.TranAmount = 2000;
            ditem.TranType = SqlScriptHelper.FinanceAssetBuy_TranType;
            ditem.ControlCenterID = SqlScriptHelper.FinanceAssetSold_CCID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_buyapiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);

            // Now do the sold out
            var nDocID = nvm.ID;
            var nAcntID = nvm.AccountVM.ID;

            var vm2 = new FinanceAssetDocumentUIViewModel();
            vm2.HID = SqlScriptHelper.HID_Tester;
            vm2.DocType = FinanceDocTypeViewModel.DocType_AssetSoldOut;
            vm2.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm2.Desp = "Sold asset test";
            vm2.TranDate = DateTime.Today;
            vm2.CreatedBy = "Tester";
            vm2.CreatedAt = DateTime.Now;
            vm2.AccountVM = new FinanceAccountViewModel();
            vm2.AccountVM.ID = nAcntID;
            vm2.AccountVM.HID = SqlScriptHelper.HID_Tester;
            var ditem2 = new FinanceDocumentItemUIViewModel();
            ditem2.AccountID = SqlScriptHelper.FinanceAssetSold_AccountID;
            ditem2.Desp = "Sold asset";
            ditem2.ItemID = 1;
            ditem2.TranAmount = 1000;
            ditem2.TranType = SqlScriptHelper.FinanceAssetSold_TranType;
            ditem2.ControlCenterID = SqlScriptHelper.FinanceAssetSold_CCID;
            vm2.Items.Add(ditem2);

            response = await _client.PostAsJsonAsync(_apiurl, vm2);
            nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);
        }
    }
}
