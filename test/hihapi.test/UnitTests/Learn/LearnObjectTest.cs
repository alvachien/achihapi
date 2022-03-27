using hihapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace hihapi.unittest.Learn
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LearnObjectTest
    {
        [Fact]
        public void TestCase_Valid1()
        {
            LearnObject obj = new LearnObject();
            obj.Name = "Test";
            obj.CategoryID = 1;
            obj.Content = "Test 2";
            obj.HomeID = 1;

            var isvalid = obj.IsValid(null);
            Assert.True(isvalid);
        }

        [Fact]
        public void TestCase_Invalid_NoHomeID()
        {
            LearnObject obj = new LearnObject();
            obj.Name = "Test";
            obj.CategoryID = 1;
            obj.Content = "Test 2";
            //obj.HomeID = 1;

            var isvalid = obj.IsValid(null);
            Assert.False(isvalid);
        }
    }
}
