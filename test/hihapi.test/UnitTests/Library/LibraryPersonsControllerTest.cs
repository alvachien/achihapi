using Xunit;

namespace hihapi.unittest.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryPersonsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryPersonsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
