using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using achihapi.ViewModels;
using achihapi.Controllers;

namespace achihapi.test
{
    [TestClass]
    public class FinanceCalcUtilityTest
    {
        [TestMethod]
        public void LoanCalcTest_EmptyInput()
        {
            LoanCalcViewModel vm = new LoanCalcViewModel();
            List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

            Assert.AreEqual(0, results.Count);
        }
    }
}
