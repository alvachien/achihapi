using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using achihapi.ViewModels;

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
            //CultureInfo ci = new CultureInfo("");
            listCI.Add(new LanguageViewModel() { LCID = 4, Name="Chinese (Simplified)", NativeName="简体中文" });
            listCI.Add(new LanguageViewModel() { LCID = 9, Name = "English", NativeName = "English" });
            //listCI.Add(new LanguageViewModel() { LCID = 31748, Name = "Chinese (Traditional)", NativeName = "繁体中文" });
            //CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures);
            //foreach (CultureInfo cul in cinfo)
            //{
            //}
            return listCI;
        }
    }
}
