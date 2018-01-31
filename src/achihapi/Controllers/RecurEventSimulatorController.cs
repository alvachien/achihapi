using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/RecurEventSimulator")]
    [Authorize]
    public class RecurEventSimulatorController : Controller
    {
        // GET: api/RecurEventSimulator
        [HttpPost]
        public IActionResult Calculate([FromBody]EventGenerationInputViewModel datInput)
        {
            List<EventGenerationResultViewModel> listRsts = null;
            listRsts = EventUtility.GenerateEvents(datInput);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listRsts, setting);
        }
    }
}
