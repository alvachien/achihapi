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
    public class LearnCategoryTest
    {
        [Fact]
        public void TestCase_ValidCase1()
        {
            LearnCategory ctgy = new LearnCategory();
            ctgy.Name = "Test";
            ctgy.Comment = "Test";

            var isvalid = ctgy.IsValid(null);
            Assert.True(isvalid);
        }

        [Fact]
        public void TestCase_ValidCase2()
        {
            LearnCategory ctgy = new LearnCategory();
            ctgy.Name = "Test";
            ctgy.Comment = "Test";
            ctgy.ParentID = 1;
            ctgy.HomeID = 1;

            var isvalid = ctgy.IsValid(null);
            Assert.True(isvalid);
        }

        [Fact]
        public void TestCase_Invalid_NoName()
        {
            LearnCategory ctgy = new LearnCategory();
            ctgy.Comment = "Test";

            var isValid = ctgy.IsValid(null);
            Assert.False(isValid);
        }

        [Fact]
        public void TestCase_Invalid_WithParentNoHome()
        {
            LearnCategory ctgy = new LearnCategory();
            ctgy.Name = "Test";
            ctgy.Comment = "Test";
            ctgy.ParentID = 1;

            var isValid = ctgy.IsValid(null);
            Assert.False(isValid);
        }
    }
}
