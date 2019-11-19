using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepeatFrequencyDatesController : ControllerBase
    {
        // POST: api/RepeatFrequencyDates
        [HttpPost]
        public IActionResult Post([FromBody]RepeatFrequencyDateInput vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<RepeatFrequencyDateViewModel> results = CommonUtility.GetDates(vm);

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
