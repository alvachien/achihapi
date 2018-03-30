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
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace achihapi.test.Controllers
{
    [TestClass]
    public class FinanceAssetBuyDocumentControllerTest
    {
        private static HttpClient _client;
        private static TestServer _server;
        private static String _contentRoot;
        private static String _connString;
        private const string _apiurl = "/api/FinanceAssetBuyDocument";

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

                String sqlCmd = SqlScriptHelper.FinanceAssetBuyDocument_Init;
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

                String sqlCmd = SqlScriptHelper.FinanceAssetBuyDocument_Cleanup;
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Close();
            }
        }

        public FinanceAssetBuyDocumentControllerTest()
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
            vm.DocType = FinanceDocTypeViewModel.DocType_AssetBuyIn;
            vm.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm.AccountVM = new FinanceAccountViewModel();
            vm.AccountVM.Name = "InvalidTest_InvalidInput";
            vm.AccountVM.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vm.AccountVM.ExtraInfo_AS.Name = "InvalidTest_InvalidInput";
            vm.AccountVM.ExtraInfo_AS.CategoryID = 1;
            var ditem = new FinanceDocumentItemUIViewModel();
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Buy asset";
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
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Buy asset";
            ditem.ItemID = 1;
            ditem.TranAmount = 2000;
            ditem.TranType = SqlScriptHelper.FinanceAssetBuy_TranType;
            ditem.ControlCenterID = SqlScriptHelper.FinanceAssetBuy_CCID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);

            // Check the table
            var nDocID = nvm.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT * FROM [dbo].[t_fin_document] WHERE [ID] = " + nDocID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account] WHERE [ID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);
                reader.Close();
                cmd.Dispose();

                // Account ext - asset
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                conn.Close();
            }
        }

        [TestMethod]
        public async Task Post_CreateMode_ItemWithOrder()
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
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Buy asset";
            ditem.ItemID = 1;
            ditem.TranAmount = 2000;
            ditem.TranType = SqlScriptHelper.FinanceAssetBuy_TranType;
            ditem.OrderID = SqlScriptHelper.FinanceAssetBuy_OrderID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);

            // Check the table
            var nDocID = nvm.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT * FROM [dbo].[t_fin_document] WHERE [ID] = " + nDocID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account] WHERE [ID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);
                reader.Close();
                cmd.Dispose();

                // Account ext - asset
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                conn.Close();
            }
        }

        [TestMethod]
        public async Task Post_CreateAndRead_ItemWithCC()
        {
            var vm = new FinanceAssetDocumentUIViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.DocType = FinanceDocTypeViewModel.DocType_AssetBuyIn;
            vm.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm.Desp = "Buy asset test 2";
            vm.TranDate = DateTime.Today;
            vm.CreatedBy = "Tester";
            vm.CreatedAt = DateTime.Now;
            vm.AccountVM = new FinanceAccountViewModel();
            vm.AccountVM.HID = SqlScriptHelper.HID_Tester;
            vm.AccountVM.Name = "Create_ItemWithCC 2";
            vm.AccountVM.CtgyID = FinanceAccountCtgyViewModel.AccountCategory_Asset;
            vm.AccountVM.Comment = vm.AccountVM.Name;
            vm.AccountVM.CreatedBy = "Tester";
            vm.AccountVM.CreatedAt = DateTime.Now;
            vm.AccountVM.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vm.AccountVM.ExtraInfo_AS.Name = "Create_ItemWithCC 2";
            vm.AccountVM.ExtraInfo_AS.CategoryID = 1;
            var ditem = new FinanceDocumentItemUIViewModel();
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Buy asset 2";
            ditem.ItemID = 1;
            ditem.TranAmount = 5000;
            ditem.TranType = SqlScriptHelper.FinanceAssetBuy_TranType;
            ditem.ControlCenterID = SqlScriptHelper.FinanceAssetBuy_CCID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);

            // Check the table
            var nDocID = nvm.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT * FROM [dbo].[t_fin_document] WHERE [ID] = " + nDocID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account] WHERE [ID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account ext - asset
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                conn.Close();
            }

            // Now do the reading
            response = await _client.GetAsync(_apiurl + "/" + nDocID.ToString() + "?hid=" + UnitTestUtility.UnitTestHomeID.ToString());
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            Assert.IsNotNull(response.Content);
            var result2 = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();
            Assert.IsNotNull(result2);
            Assert.AreEqual(nDocID, result2.ID);
            Assert.AreEqual(UnitTestUtility.UnitTestHomeID, result2.HID);
            Assert.IsNotNull(result2.AccountVM);
            Assert.IsNotNull(result2.AccountVM.ExtraInfo_AS);
            Assert.AreEqual(nvm.AccountVM.ID, result2.AccountVM.ID);
            Assert.AreEqual(nvm.AccountVM.ExtraInfo_AS.RefDocForBuy, result2.ID);
        }

        [TestMethod]
        public async Task Post_CreateAndPatch_ItemWithCC()
        {
            var vm = new FinanceAssetDocumentUIViewModel();
            vm.HID = SqlScriptHelper.HID_Tester;
            vm.DocType = FinanceDocTypeViewModel.DocType_AssetBuyIn;
            vm.TranCurr = SqlScriptHelper.UnitTest_Currency;
            vm.Desp = "Buy asset test 3";
            vm.TranDate = DateTime.Today;
            vm.CreatedBy = "Tester";
            vm.CreatedAt = DateTime.Now;
            vm.AccountVM = new FinanceAccountViewModel();
            vm.AccountVM.HID = SqlScriptHelper.HID_Tester;
            vm.AccountVM.Name = "Create_ItemWithCC 3";
            vm.AccountVM.CtgyID = FinanceAccountCtgyViewModel.AccountCategory_Asset;
            vm.AccountVM.Comment = vm.AccountVM.Name;
            vm.AccountVM.CreatedBy = "Tester";
            vm.AccountVM.CreatedAt = DateTime.Now;
            vm.AccountVM.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vm.AccountVM.ExtraInfo_AS.Name = "Create_ItemWithCC 3";
            vm.AccountVM.ExtraInfo_AS.CategoryID = 1;
            var ditem = new FinanceDocumentItemUIViewModel();
            ditem.AccountID = SqlScriptHelper.FinanceAssetBuy_AccountID;
            ditem.Desp = "Buy asset 3";
            ditem.ItemID = 1;
            ditem.TranAmount = 5000;
            ditem.TranType = SqlScriptHelper.FinanceAssetBuy_TranType;
            ditem.ControlCenterID = SqlScriptHelper.FinanceAssetBuy_CCID;
            vm.Items.Add(ditem);

            var response = await _client.PostAsJsonAsync(_apiurl, vm);
            var nvm = await response.Content.ReadAsJsonAsync<FinanceAssetDocumentUIViewModel>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(nvm);

            // Check the table
            var nDocID = nvm.ID;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT * FROM [dbo].[t_fin_document] WHERE [ID] = " + nDocID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account] WHERE [ID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                // Account ext - asset
                sqlCmd = @"SELECT * FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] = " + nvm.AccountVM.ID.ToString();
                cmd = new SqlCommand(sqlCmd, conn);
                reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);

                reader.Close();
                cmd.Dispose();

                conn.Close();
            }

            // Now do the patch
            var nTranDate = DateTime.Today.AddDays(7);
            var patchData = new JsonPatchDocument<FinanceAssetDocumentUIViewModel>();
            patchData.Operations.Add(new Operation<FinanceAssetDocumentUIViewModel>("replace", "/tranDate", null, nTranDate));
            response = await _client.PatchAsJsonAsync(_apiurl + "/" + nDocID.ToString() + "?hid=" + UnitTestUtility.UnitTestHomeID.ToString(), patchData);
            if (!response.IsSuccessStatusCode)
            {
                var errmsg = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(errmsg);
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(true, response.IsSuccessStatusCode);

            // Check in the DB directly
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Document
                String sqlCmd = @"SELECT [TRANDATE], [UPDATEDAT] FROM [dbo].[t_fin_document] WHERE [ID] = " + nDocID.ToString();
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                Assert.AreEqual(true, reader.HasRows);
                while(reader.Read())
                {
                    var ndate = reader.GetDateTime(0);
                    Assert.AreEqual(ndate.Date, nTranDate.Date);
                    ndate = reader.GetDateTime(1);
                    Assert.AreEqual(ndate.Date, DateTime.Today.Date);
                }

                reader.Close();
                cmd.Dispose();

                conn.Close();
            }
        }
    }
}
