using Xunit;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryBookLocationsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryBookLocationsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
