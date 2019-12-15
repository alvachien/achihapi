using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentsControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public FinanceDocumentsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1_Home1_UserA()
        {
            // 0. Prepare the context
            var control = new FinanceDocumentsController(this.fixture.CurrentDataContext);
            var user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // 0a. Prepare account
            var acntcontrol = new FinanceAccountsController(this.fixture.CurrentDataContext);
            acntcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var acnt1rst = await acntcontrol.Post(new FinanceAccount()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Account 1",
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Owner = DataSetupUtility.UserA
            });
            Assert.NotNull(acnt1rst);
            var acnt1 = Assert.IsType<CreatedODataResult<FinanceAccount>>(acnt1rst);

            // 0b. Prepare control center
            var cccontrol = new FinanceControlCentersController(this.fixture.CurrentDataContext);
            cccontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var cc1rst = await cccontrol.Post(new FinanceControlCenter()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Control Center 1",
                Comment = "Comment 1",
                Owner = DataSetupUtility.UserA
            });
            Assert.NotNull(cc1rst);
            var cc1 = Assert.IsType<CreatedODataResult<FinanceControlCenter>>(cc1rst);
            
            // 0c. Prepare order

            // 1. Create first account
            var doc = new FinanceDocument()
            {
                HomeID = DataSetupUtility.Home1ID,
                DocType = FinanceDocumentType.DocType_Normal,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Desp = "Test 1"
            };
            var item = new FinanceDocumentItem()
            {
                DocumentHeader = doc,
                ItemID = 1,
                Desp = "Item 1.1",
                TranType = 2, // Wage
                TranAmount = 10,
                AccountID = acnt1.Entity.ID,
                ControlCenterID = cc1.Entity.ID,
            };
            doc.Items.Add(item);
            var rst = await control.Post(doc);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, doc.TranCurr);
            var firstdocid = rst2.Entity.ID;
            Assert.True(firstdocid > 0);

            // 2. Now read the whole control centers
            var rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 3. Now create another one!
            doc = new FinanceDocument()
            {
                HomeID = DataSetupUtility.Home1ID,
                DocType = FinanceDocumentType.DocType_Normal,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Desp = "Test 2"
            };
            item = new FinanceDocumentItem()
            {
                DocumentHeader = doc,
                ItemID = 1,
                Desp = "Item 2.1",
                TranType = 2, // Wage
                TranAmount = 10,
                AccountID = acnt1.Entity.ID,
                ControlCenterID = cc1.Entity.ID,
            };
            doc.Items.Add(item);
            rst = await control.Post(doc);
            Assert.NotNull(rst);
            rst2 = Assert.IsType<CreatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, doc.TranCurr);
            var seconddocid = rst2.Entity.ID;
            Assert.True(seconddocid > 0);

            // 4. Change one document
            doc.Desp = "Change Test";
            rst = await control.Put(seconddocid, doc);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst4.Entity.Desp, doc.Desp);

            // 5. Delete the second document
            var rst5 = await control.Delete(seconddocid);
            Assert.NotNull(rst5);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 6. Now read the whole documents
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 7. Delete the first account
            rst5 = await control.Delete(firstdocid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Count());
        }
    }
}
