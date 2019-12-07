using System;
using System.Collections.Generic;
using System.Text;
using hihapi.test;
using Xunit;

namespace hihapi.test.UnitTests
{
    [CollectionDefinition("HIHAPI_UnitTests#1")]
    public class UnitTestCollection: ICollectionFixture<SqliteDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
