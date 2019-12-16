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
using Microsoft.AspNet.OData;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class HomeDefinesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public HomeDefinesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.UserD)]
        public void TestCase_GetHomeDefineByUser(string user)
        {
            HomeDefinesController control = new HomeDefinesController(this.fixture.CurrentDataContext);
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }

            var userclaim = DataSetupUtility.GetClaimForUser(user);

            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var result = control.Get();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IQueryable<HomeDefine>>(okResult.Value);
            var cnt = returnValue.Count();
            Assert.Equal(fixture.CurrentDataContext.HomeMembers.Where(p => p.User == user).Count(), cnt);
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1BaseCurrency)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1BaseCurrency)]
        public async Task TestCase_CreateAndUpdateAndDelete(string user, string curr)
        {
            var control = new HomeDefinesController(this.fixture.CurrentDataContext);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            List<HomeDefine> listObjectCreated = new List<HomeDefine>();

            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // Create
            var hd1 = new HomeDefine()
            {
                Name = "HomeDef.Test.1",
                Host = user,
                BaseCurrency = curr,
            };
            var hm1 = new HomeMember()
            {
                Relation = HomeMemberRelationType.Self,
                DisplayAs = "Myself",
                User = user,
                HomeDefinition = hd1,
            };
            hd1.HomeMembers.Add(hm1);
            var rst = await control.Post(hd1);
            var nhdobj = Assert.IsType<CreatedODataResult<HomeDefine>>(rst);
            Assert.True(nhdobj.Entity.ID > 0);
            listObjectCreated.Add(nhdobj.Entity);

            // Read the single object
            var rst2 = control.Get(nhdobj.Entity.ID);
            var nreadobjectrst = Assert.IsType<SingleResult<HomeDefine>>(rst2);
            // Assert.Equal(nhdobj.Entity.Name, nreadobject.Queryable.)
            Assert.Equal(1, nreadobjectrst.Queryable.Count<HomeDefine>());
            var nreadobj = nreadobjectrst.Queryable.First<HomeDefine>();
            Assert.Equal(nhdobj.Entity.Name, nreadobj.Name);
            Assert.Equal(nhdobj.Entity.Host, nreadobj.Host);

            // How to test the $expand? Test in integration test!

            // Change the home define - Add new user

            // Change the home define - change the host

            // Change the home define - change the name

            // Change the home define - remove one user
            
            // Delete the home define (failed case)

            // Delete the home define (success case)

            // Delete all objects
            this.fixture.CurrentDataContext.HomeDefines.RemoveRange(listObjectCreated);
        }
    }
}

