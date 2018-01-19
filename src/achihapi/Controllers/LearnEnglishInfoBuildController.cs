using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using achihapi.ViewModels;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    internal enum WordSource
    {
        Bing = 1,
        Iciba = 2
    }

    [Produces("application/json")]
    [Route("api/LearnEnglishInfoBuild")]
    public class LearnEnglishInfoBuildController : Controller
    {
        // GET: api/LearnEnglishInfoBuild
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return BadRequest();
        }

        // GET: api/LearnEnglishInfoBuild/5
        [HttpGet("{word}")]
        //[Authorize]
        public async Task<IActionResult> Get(String word, [FromQuery]Int32 hid = 0, [FromQuery]Int32 sid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            // Basic check
            WordSource src = (WordSource)sid;
            switch(src)
            {
                case WordSource.Bing:
                case WordSource.Iciba:
                    {
                        // Fetch the result
                        WordResult wr = await this.FetchWordFromSourceAsync(word, src);

                        // After then,
                        return new JsonResult(wr);
                    }

                default:
                    return BadRequest("No such source yet!");
            }
        }
        
        // POST: api/LearnEnglishInfoBuild
        [HttpPost]
        public void Post([FromBody]string value)
        {
            
        }
        
        // PUT: api/LearnEnglishInfoBuild/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private async Task<WordResult> FetchWordFromSourceAsync(String strword, WordSource wordsrc)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/html"));

            //Boolean useBing = true; // https://cn.bing.com/dict/search?q=university&qs=n&form=Z9LH5&sp=-1&pq=university&sc=8-10&sk=
            //Boolean useIciba = false; // http://www.iciba.com/
            //Boolean useYoudao = false; // US pron: https://dict.youdao.com/dictvoice?audio=take+part+in&type=2
            String resString = String.Empty;
            WordResult wr = new WordResult();

            try
            {
                Regex regex = new Regex(@"<[^>]*>");
                Regex regex2 = new Regex(@"</[^>]*>");

                if (wordsrc == WordSource.Bing)
                {
                    String strword2 = strword.Replace(" ", "+");
                    resString = await client.GetStringAsync("https://www.bing.com/dict/search?q=" + strword2);

                    wr.WordString = strword;

                    Int32 iPos = resString.IndexOf("<div class=\"lf_area\">");
                    Int32 iPos2 = resString.IndexOf("<div class=\"sidebar\">");
                    if (iPos == -1 || iPos2 == -1)
                    {
                        // Error occurs
                    }
                    else
                    {
                        String strContent = resString.Substring(iPos, iPos2 - iPos);
                        iPos = strContent.IndexOf("<div class=\"qdef\">");
                        iPos2 = strContent.IndexOf("<div class=\"se_div\">");

                        if (iPos != -1 && iPos2 != -1)
                        {
                            String strdef = strContent.Substring(iPos, iPos2 - iPos);
                            String strsent = strContent.Substring(iPos2);

                            // Pron
                            iPos = strdef.IndexOf("hd_prUS\">");
                            if (iPos != -1)
                            {
                                iPos2 = strdef.IndexOf('<', iPos);
                                wr.WordPronUS = strdef.Substring(iPos + 9, iPos2 - iPos - 9).Replace("&#160;", "");

                                iPos = strdef.IndexOf("https:", iPos2);
                                iPos2 = strdef.IndexOf(".mp3'", iPos);
                                wr.WordPronUSFile = strdef.Substring(iPos, iPos2 + 4 - iPos);

                                iPos = iPos2;
                            }

                            iPos = strdef.IndexOf("hd_pr\">", iPos == -1 ? 0 : iPos);
                            if (iPos != -1)
                            {
                                iPos2 = strdef.IndexOf('<', iPos);
                                wr.WordPronUK = strdef.Substring(iPos + 7, iPos2 - iPos - 7).Replace("&#160;", "");

                                iPos = strdef.IndexOf("https:", iPos2);
                                iPos2 = strdef.IndexOf(".mp3'", iPos);
                                wr.WordPronUKFile = strdef.Substring(iPos, iPos2 + 4 - iPos);

                                iPos = iPos2;
                            }

                            // Explains
                            iPos = strdef.IndexOf("<ul>", iPos == -1 ? 0 : iPos);
                            if (iPos != -1)
                            {
                                iPos2 = strdef.IndexOf("</ul>", iPos);
                                String strexp = strdef.Substring(iPos, iPos2 - iPos);

                                iPos = strexp.IndexOf("<li>");
                                while (iPos != -1)
                                {
                                    iPos2 = strexp.IndexOf("</li>", iPos);
                                    String expit = strexp.Substring(iPos, iPos2 - iPos);
                                    expit = regex.Replace(expit, "");
                                    expit = regex2.Replace(expit, "");
                                    wr.WordExplains.Add(expit);

                                    iPos = strexp.IndexOf("<li>", iPos2);
                                }

                                iPos = iPos2;
                            }

                            // Forms
                            iPos = strdef.IndexOf("<div class=\"hd_if\">", iPos == -1 ? 0 : iPos);
                            if (iPos != -1)
                            {
                                iPos2 = strdef.IndexOf("</div>", iPos);
                                string strform = strdef.Substring(iPos, iPos2 - iPos);
                                string[] forms = strform.Split("&nbsp;&nbsp;");
                                foreach (var frm in forms)
                                {
                                    var frm2 = regex.Replace(frm, "");
                                    frm2 = regex2.Replace(frm2, "");
                                    frm2 = frm2.Replace("&#160;&#160;", "");

                                    if (!String.IsNullOrEmpty(frm2))
                                        wr.WordForms.Add(frm2);
                                }

                                iPos = iPos2;
                            }

                            // Sentences
                            iPos = strsent.IndexOf("<div id=\"sentenceSeg\">");
                            if (iPos != -1)
                            {
                                iPos = strsent.IndexOf("<div class=\"se_li\">", iPos);
                                while (iPos != -1)
                                {
                                    iPos = strsent.IndexOf("<div class=\"sen_en\">", iPos);
                                    iPos2 = strsent.IndexOf("</div>", iPos);
                                    WordRefSent wrs = new WordRefSent();
                                    wrs.EnSent = strsent.Substring(iPos, iPos2 - iPos);
                                    wrs.EnSent = regex.Replace(wrs.EnSent, "");
                                    wrs.EnSent = regex2.Replace(wrs.EnSent, "");

                                    iPos = strsent.IndexOf("<div class=\"sen_cn\">", iPos2);
                                    iPos2 = strsent.IndexOf("</div>", iPos);
                                    wrs.CnSent = strsent.Substring(iPos, iPos2 - iPos);
                                    wrs.CnSent = regex.Replace(wrs.CnSent, "");
                                    wrs.CnSent = regex2.Replace(wrs.CnSent, "");

                                    wr.WordSentences.Add(wrs);

                                    iPos = strsent.IndexOf("<div class=\"se_li\">", iPos2);
                                }
                            }
                        }
                    }
                }
                else if (wordsrc == WordSource.Iciba)
                {
                    // Check whether there is a SPACE inside
                    String strword2 = strword.Trim();
                    strword2 = strword2.Replace(" ", "%20");
                    resString = await client.GetStringAsync("http://www.iciba.com/" + strword2);

                    Boolean bfailed = false;
                    wr.WordString = strword;

                    // Pron.
                    Int32 iPos = resString.IndexOf("<div class=\"base-speak\">");
                    Int32 iPos2 = -1;
                    if (iPos != -1)
                    {
                        iPos += "<div class=\"base-speak\">".Length;

                        iPos2 = resString.IndexOf("</div>", iPos);
                        String usPron = resString.Substring(iPos, iPos2 - iPos);

                        if (!String.IsNullOrEmpty(usPron))
                        {
                            iPos = usPron.IndexOf("<span>");
                            if (iPos != -1)
                                iPos += "<span>".Length;
                            if (iPos != -1)
                            {
                                usPron = usPron.Remove(0, iPos);
                                iPos = usPron.IndexOf("<span>");

                                if (iPos != -1)
                                {
                                    iPos2 = usPron.IndexOf("</span>", iPos);

                                    wr.WordPronUK = usPron.Substring(iPos + 6, iPos2 - iPos - 6);

                                    usPron = usPron.Remove(0, iPos2 + "</span>".Length);
                                    iPos = usPron.IndexOf("('");
                                    iPos2 = usPron.IndexOf("')");
                                    wr.WordPronUKFile = usPron.Substring(iPos + 2, iPos2 - iPos - 2);

                                    usPron = usPron.Remove(0, iPos2 + 2);
                                    iPos = usPron.IndexOf("<span>");
                                    usPron = usPron.Remove(0, iPos + "<span>".Length);

                                    iPos = usPron.IndexOf("<span>");
                                    iPos2 = usPron.IndexOf("</span>");
                                    wr.WordPronUS = usPron.Substring(iPos + 6, iPos2 - iPos - 6);
                                    usPron = usPron.Remove(0, iPos2 + "</span>".Length);
                                    iPos = usPron.IndexOf("('");
                                    iPos2 = usPron.IndexOf("')");
                                    wr.WordPronUSFile = usPron.Substring(iPos + 2, iPos2 - iPos - 2);
                                }
                                else
                                {
                                    wr.WordPronUS = usPron;
                                }
                            }
                        }
                        else
                        {
                            bfailed = true;
                        }
                    }

                    // Explain
                    if (!bfailed)
                    {
                        iPos = resString.IndexOf("<ul class=\"base-list switch_part\" class=\"\">");

                        if (iPos != -1)
                        {
                            iPos2 = resString.IndexOf("</ul>", iPos) + "</ul>".Length;
                            String expStr = resString.Substring(iPos, iPos2 - iPos);

                            if (!String.IsNullOrEmpty(expStr))
                            {
                                iPos = expStr.IndexOf("<li");
                                while (iPos != -1)
                                {
                                    iPos2 = expStr.IndexOf("</li>", iPos);

                                    String expitem = expStr.Substring(iPos + 3, iPos2 - iPos);
                                    Int32 j1 = expitem.IndexOf("<span");
                                    Int32 j2 = -1;
                                    String strexp = "";
                                    while (j1 != -1)
                                    {
                                        j2 = expitem.IndexOf("</span>", j1);
                                        j1 = expitem.IndexOf(">", j1);
                                        strexp += expitem.Substring(j1 + 1, j2 - j1 - 1);

                                        j1 = expitem.IndexOf("<span", j2);
                                    }
                                    if (!String.IsNullOrEmpty(strexp))
                                        wr.WordExplains.Add(strexp);

                                    iPos = expStr.IndexOf("<li", iPos2);
                                }
                            }
                            else
                            {
                                bfailed = true;
                            }
                        }
                    }

                    // Transforms
                    iPos = resString.IndexOf("<h1 class=\"base-word abbr chinese change-base\">");
                    if (iPos != -1)
                    {
                        iPos = resString.IndexOf("<p>", iPos);
                        iPos += 3;
                        iPos2 = resString.IndexOf("</p>", iPos);
                        String strForms = resString.Substring(iPos, iPos2 - iPos);

                        iPos = strForms.IndexOf("<span>");
                        while (iPos != -1)
                        {
                            iPos += 6; // "<span>".Length
                            iPos2 = strForms.IndexOf("</span>", iPos);

                            String strfi = strForms.Substring(iPos, iPos2 - iPos);
                            strfi = strfi.Trim();

                            Int32 f1 = strfi.IndexOf("<");
                            Int32 f2 = strfi.IndexOf(">", f1);
                            Int32 f3 = strfi.IndexOf("</a>", f2);
                            wr.WordForms.Add(strfi.Substring(0, f1).Trim() + " " + strfi.Substring(f2 + 1, f3 - f2 - 1).Trim());

                            iPos = strForms.IndexOf("<span>", iPos2);
                        }
                    }

                    // Sentences
                    iPos = resString.IndexOf("<div class='sentence-item'>");
                    while (iPos != -1)
                    {
                        iPos2 = resString.IndexOf("</div>", iPos);

                        String strSent = resString.Substring(iPos, iPos2 - iPos);
                        Int32 s1 = strSent.IndexOf("<p class='family-english'>");
                        s1 = strSent.IndexOf("<span>", s1);
                        Int32 s2 = strSent.IndexOf("</span>", s1);

                        WordRefSent wrs = new WordRefSent();
                        wrs.EnSent = strSent.Substring(s1 + 6, s2 - s1 - 6);
                        wrs.EnSent = wrs.EnSent.Replace("<b>", "");
                        wrs.EnSent = wrs.EnSent.Replace("</b>", "");

                        s1 = strSent.IndexOf("<p class='family-chinese size-chinese'>", s2);
                        s1 += "<p class='family-chinese size-chinese'>".Length;
                        s2 = strSent.IndexOf("</p>", s1);
                        wrs.CnSent = strSent.Substring(s1, s2 - s1);
                        wrs.CnSent = wrs.CnSent.Replace("<b>", "");
                        wrs.CnSent = wrs.CnSent.Replace("</b>", "");
                        wr.WordSentences.Add(wrs);

                        iPos = resString.IndexOf("<div class='sentence-item'>", iPos2);
                    }
                }
                else
                {
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error on " + strword + " : " + exp.Message);
            }

            return wr;
        }
    }
}
