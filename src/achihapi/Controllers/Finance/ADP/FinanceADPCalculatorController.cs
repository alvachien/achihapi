using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceADPCalculator")]
    public class FinanceADPCalculatorController : Controller
    {
        // POST: api/FinanceADPCalculator
        [HttpPost]
        [Authorize]
        public IActionResult Calculate([FromBody]ADPGenerateViewModel vm)
        {
            try
            {
                List<ADPGenerateResult> results = FinanceCalcUtility.GenerateAdvancePaymentTmps(vm);

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
