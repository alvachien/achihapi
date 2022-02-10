using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceControlCentersControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public FinanceControlCentersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedID.Count > 0)
            {
                this.listCreatedID.ForEach(x => this.fixture.DeleteFinanceControlCenter(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedID.Clear();
            }
            this.fixture.GetCurrentDataContext().SaveChanges();
        }

        public static TheoryData<ControlCenterTestData> TestData => new TheoryData<ControlCenterTestData>
        {
            new ControlCenterTestData()
            {                
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                Comment = "Test 1 Comment",
                Owner = DataSetupUtility.UserA,
            },
            new ControlCenterTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1a",
                Comment = "Test 1a Comment",
                ParentID = DataSetupUtility.Home1ControlCenter1ID,
                Owner = DataSetupUtility.UserB,
            },
            new ControlCenterTestData()
            {
                HomeID = DataSetupUtility.Home2ID,
                Name = "Test 2",
                Comment = "Test 2 Comment",
                Owner = DataSetupUtility.UserB,
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestCase_StandardOperation(ControlCenterTestData testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            // Pre. setup
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceControlCentersController control = new FinanceControlCentersController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.Owner);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 2. Initial control center list
            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getallccs = Assert.IsAssignableFrom<IQueryable<FinanceControlCenter>>(getokresult.Value);

            // 3. Create
            FinanceControlCenter obj = new FinanceControlCenter();
            obj.HomeID = testdata.HomeID;
            obj.Name = testdata.Name;
            obj.Comment = testdata.Comment;
            obj.Owner = testdata.Owner;
            obj.ParentID = testdata.ParentID;

            var postrst = await control.Post(obj);
            Assert.NotNull(postrst);
            var postokrst = Assert.IsType<CreatedODataResult<FinanceControlCenter>>(postrst);
            var createdobj = Assert.IsAssignableFrom<FinanceControlCenter>(postokrst.Entity);
            Assert.Equal(obj.HomeID, createdobj.HomeID);
            Assert.Equal(obj.Name, createdobj.Name);
            Assert.Equal(obj.Comment, createdobj.Comment);
            Assert.Equal(obj.Owner, createdobj.Owner);
            Assert.Equal(obj.ParentID, createdobj.ParentID);
            var nccid = createdobj.ID;
            listCreatedID.Add(nccid);

            // 4. Get single
            var getsinglerst = control.Get(nccid);
            Assert.NotNull(getsinglerst);
            Assert.Equal(obj.HomeID, getsinglerst.HomeID);
            Assert.Equal(obj.Name, getsinglerst.Name);
            Assert.Equal(obj.Comment, getsinglerst.Comment);
            Assert.Equal(obj.Owner, getsinglerst.Owner);
            Assert.Equal(obj.ParentID, getsinglerst.ParentID);

            // 5. Change
            getsinglerst.Comment += "Changed";
            var chgresult = await control.Put(nccid, getsinglerst);
            Assert.NotNull(chgresult);
            var chgobjresult = Assert.IsType<UpdatedODataResult<FinanceControlCenter>>(chgresult);
            var chgedobj = Assert.IsType<FinanceControlCenter>(chgobjresult.Entity);
            Assert.Equal(getsinglerst.Comment, chgedobj.Comment);
            Assert.Equal(getsinglerst.HomeID, chgedobj.HomeID);
            Assert.Equal(getsinglerst.Name, chgedobj.Name);
            Assert.Equal(getsinglerst.Owner, chgedobj.Owner);
            Assert.Equal(getsinglerst.ParentID, chgedobj.ParentID);

            // 6. Delete
            var delresult = await control.Delete(nccid);
            Assert.NotNull(delresult);
            var delcoderst = Assert.IsType<StatusCodeResult>(delresult);
            Assert.Equal(204, delcoderst.StatusCode);

            await context.DisposeAsync();
        }
    }
}

