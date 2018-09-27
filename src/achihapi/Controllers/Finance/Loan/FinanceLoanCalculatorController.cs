using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Net;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
