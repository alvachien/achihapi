using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceLoanCalculator")]
    public class FinanceLoanCalculatorController : Controller
    {
        // POST: api/FinanceLoanCalculator
        [HttpPost]
        [Authorize]
        public IActionResult Calculate([FromBody]LoanCalcViewModel vm)
        {
            try
            {
                List<LoanCalcResult> results = FinanceCalcUtility.LoanCalculate(vm);

                var setting = new Newtonsoft.Json.JsonSerializerSettings
                {
                    DateFormatString = HIHAPIConstants.DateFormatPattern,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                return new JsonResult(results, setting);
            }
            catch (Exception exp)
            {
                return BadRequest(exp.Message);
            }
        }
    }
}
