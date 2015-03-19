using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AtomsPreCompiler.Controllers
{
    public class CompileController : Controller
    {
        // GET: Compile
        public ActionResult Page(string html)
        {

            PageCompiler pc = new PageCompiler();
            pc.Compile(html);

            return Json(pc);
        }

        public ActionResult Component(string html) {
            HtmlCompiler hc = new HtmlCompiler();
            hc.Compile(html);

            return Json(hc);
        }
    }
}