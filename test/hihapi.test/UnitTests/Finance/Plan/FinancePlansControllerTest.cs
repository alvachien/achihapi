using System;
using Xunit;
using System.Collections.Generic;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinancePlansControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public FinancePlansControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedID.Count > 0)
            {
                this.listCreatedID.ForEach(x => this.fixture.DeleteFinanceOrder(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedID.Clear();
            }
            this.fixture.GetCurrentDataContext().SaveChanges();
        }
    }
}
