using Xunit;
using hihapi.Models;

namespace hihapi.unittest.Home
{
    [Collection("HIHAPI_UnitTests#1")]
    public class HomeDefineTest
    {
        private SqliteDatabaseFixture fixture = null;

        public HomeDefineTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_CheckValid()
        {
            var home = new HomeDefine();
            var context = this.fixture.GetCurrentDataContext();
            var isvalid = home.IsValid(context);

            Assert.False(isvalid);
        }
    }
}
