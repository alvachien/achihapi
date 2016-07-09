using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using achihapi.ViewModels;
using achihapi.Models;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<LanguageViewModel> Get()
        {
            List<LanguageViewModel> listCI = new List<LanguageViewModel>();
            listCI.Add(new LanguageViewModel() { LCID = 4, ISOName="zh", EnglishName = "Chinese (Simplified)", NativeName="简体中文" });
            listCI.Add(new LanguageViewModel() { LCID = 9, ISOName = "en", EnglishName = "English", NativeName = "English" });

            //CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures);
            return listCI;
        }
    }
}
