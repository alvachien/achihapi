﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace hihapi.test.integrationtests
{
    [CollectionDefinition("HIHAPI_IntegrationTests#1")]
    public class IntegrationTestCollection : ICollectionFixture<SqliteDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
