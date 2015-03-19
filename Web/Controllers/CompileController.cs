using NeuroSpeech.AtomsPreCompiler;
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
        public ActionResult Page(string html, bool debug)
        {

            PageCompiler pc = new PageCompiler();
            pc.Debug = debug;
            pc.Compile(html);

            return Json(pc);
        }

        public ActionResult Component(string html, int start = 0, bool debug = false) {
            HtmlCompiler hc = new HtmlCompiler();
            hc.Index = start;
            hc.Debug = debug;
            hc.Compile(html);

            return Json(hc);
        }
    }
}