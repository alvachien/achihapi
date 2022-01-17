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
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

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
        public async Task TestCase_GetHomeDefineByUser(string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            HomeDefinesController control = new HomeDefinesController(context);

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
            Assert.NotNull(result);
            // Assert.IsType<ActionResult<IEnumerable<HomeDefine>>>(result);
            var okresult = Assert.IsType<OkObjectResult>(result);
            // Assert.IsAssignableFrom<IEnumerable<HomeDefine>>((result as OkObjectResult).Value as IEnumerable<HomeDefine>);

            var returnValue = Assert.IsAssignableFrom<IEnumerable<HomeDefine>>(okresult.Value as IEnumerable<HomeDefine>);
            var cnt = returnValue.Count();
            Assert.Equal(context.HomeMembers.Where(p => p.User == user).Count(), cnt);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1BaseCurrency)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1BaseCurrency)]
        public async Task TestCase_CreateAndUpdateAndDelete(string user, string curr)
        {
            var context = this.fixture.GetCurrentDataContext();
            var control = new HomeDefinesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            //List<HomeDefine> listObjectCreated = new List<HomeDefine>();
            var hid = 0;
            var homehost = user;

            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // Create
            var hd1 = new HomeDefine()
            {
                Name = "HomeDef.TestCase2." + user,
                Host = homehost,
                BaseCurrency = curr,
                Createdby = user,
            };
            var hm1 = new HomeMember()
            {
                Relation = HomeMemberRelationType.Self,
                DisplayAs = "Myself",
                User = homehost,
                HomeDefinition = hd1,
                Createdby = user,
            };
            hd1.HomeMembers.Add(hm1);
            var rst = await control.Post(hd1);
            var nhdobj = Assert.IsType<CreatedODataResult<HomeDefine>>(rst);            
            Assert.True(nhdobj.Entity.ID > 0);
            hid = nhdobj.Entity.ID;
            Assert.True(nhdobj.Entity.HomeMembers.Count == 1);
            Assert.True(nhdobj.Entity.HomeMembers.ElementAt(0).HomeID == nhdobj.Entity.ID);
            Assert.True(nhdobj.Entity.HomeMembers.ElementAt(0).Relation == HomeMemberRelationType.Self);
            Assert.True(nhdobj.Entity.HomeMembers.ElementAt(0).User == nhdobj.Entity.Host);

            // Read the single object
            var rst2 = control.Get(nhdobj.Entity.ID);
            // var nreadobjectrst = Assert.IsType<SingleResult<HomeDefine>>(rst2);
            Assert.IsType<HomeDefine>(rst2);
            // var nreadobjectrst = Assert.IsAssignableFrom<HomeDefine>(rst2 as HomeDefine);
            // var returnValue = Assert.IsAssignableFrom<HomeDefine>((rst2 as OkObjectResult).Value as HomeDefine);
            // Assert.Equal(nhdobj.Entity.Name, nreadobject.Queryable.)
            //Assert.Equal(1, nreadobjectrst.Queryable.Count<HomeDefine>());
            var nreadobj = rst2 as HomeDefine;
            Assert.Equal(nhdobj.Entity.Name, nreadobj.Name);
            Assert.Equal(nhdobj.Entity.Host, nreadobj.Host);
            Assert.True(nreadobj.HomeMembers.Count == 1);
            Assert.True(nreadobj.HomeMembers.ElementAt(0).HomeID == nreadobj.ID);
            Assert.True(nreadobj.HomeMembers.ElementAt(0).Relation == HomeMemberRelationType.Self);
            Assert.True(nreadobj.HomeMembers.ElementAt(0).User == nreadobj.Host);

            // How to test the $expand? Test in integration test!

            // Change the home define - Add new user
            var hm2 = new HomeMember()
            {
                HomeID = nreadobj.ID,
                Relation = HomeMemberRelationType.Couple,
                DisplayAs = "New Test",
                User = (user == DataSetupUtility.UserA) ? DataSetupUtility.UserB : DataSetupUtility.UserA,
                HomeDefinition = nreadobj,
                Createdby = user,
            };
            nreadobj.HomeMembers.Add(hm2);
            var rst3 = await control.Put(nreadobj.ID, nreadobj);
            var nupdobjectrst = Assert.IsType<UpdatedODataResult<HomeDefine>>(rst3);
            Assert.Equal(nreadobj.ID, nupdobjectrst.Entity.ID);
            //var memcnt = context.HomeMembers.Where(p => p.HomeID == nreadobj.ID).Count();
            //Assert.Equal(2, memcnt);
            Assert.Equal(2, nupdobjectrst.Entity.HomeMembers.Count);

            // Change the home define - change the host
            nreadobj.Host = hm2.User;
            homehost = nreadobj.Host;
            // Need the relationship....
            foreach (var mem in nreadobj.HomeMembers)
            {
                if (mem.User == nreadobj.Host)
                    mem.Relation = HomeMemberRelationType.Self;
                else
                    mem.Relation = HomeMemberRelationType.Couple;
            }
            rst3 = await control.Put(nreadobj.ID, nreadobj);
            nupdobjectrst = Assert.IsType<UpdatedODataResult<HomeDefine>>(rst3);
            Assert.Equal(nreadobj.Host, nupdobjectrst.Entity.Host);
            Assert.Equal(2, nupdobjectrst.Entity.HomeMembers.Count);
            foreach(var mem in nupdobjectrst.Entity.HomeMembers)
            {
                if (mem.User == nupdobjectrst.Entity.Host)
                {
                    Assert.Equal(HomeMemberRelationType.Self, mem.Relation);
                }
                else
                {
                    Assert.NotEqual(HomeMemberRelationType.Self, mem.Relation);
                }
            }

            // Change the home define - remove one user
            var memtoremove = nreadobj.HomeMembers.First(p => p.Relation != HomeMemberRelationType.Self);
            nreadobj.HomeMembers.Remove(memtoremove);
            Assert.Equal(1, nreadobj.HomeMembers.Count);
            Assert.Equal(nreadobj.Host, nreadobj.HomeMembers.ElementAt(0).User);
            rst3 = await control.Put(nreadobj.ID, nreadobj);
            nupdobjectrst = Assert.IsType<UpdatedODataResult<HomeDefine>>(rst3);
            Assert.Equal(1, nupdobjectrst.Entity.HomeMembers.Count);

            // Delete the home define (failed case)
            try
            {
                await control.Delete(hid);
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }

            // Switch to another user
            context = this.fixture.GetCurrentDataContext();
            var fakeCtgy = context.FinAccountCategories.Add(new FinanceAccountCategory()
            {
                HomeID = hid,
                Name = "Test 1",
                Comment = "Test 1"
            });
            await context.SaveChangesAsync();
            control = new HomeDefinesController(context);
            userclaim = DataSetupUtility.GetClaimForUser(homehost);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // Delete the home define (failed case 2)
            try
            {
                await control.Delete(hid);
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }
            context.FinAccountCategories.Remove(fakeCtgy.Entity);
            await context.SaveChangesAsync();

            // Delete the home define (success case)
            var rst9 = await control.Delete(hid);
            var rst9rst = Assert.IsType<StatusCodeResult>(rst9);
            Assert.Equal(204, rst9rst.StatusCode);

            Assert.Equal(0, context.HomeDefines.Where(p => p.ID == hid).Count());

            await context.DisposeAsync();
        }
    }
}

