using Xunit;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryOrganizationsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryOrganizationsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

    }
}
