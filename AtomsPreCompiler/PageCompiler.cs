using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsPreCompiler
{

    public class PageCompiler : HtmlCompiler
    {

        public HtmlNode Header { get; set; }

        public HtmlNode Body { get; set; }

        public PageCompiler()
        {

        }

        protected override void OnBeforeCompile()
        {
            Header = Document.DocumentNode.DescendantsAndSelf().FirstOrDefault(x => x.Name.EqualsIgnoreCase("head"));
            if (Header == null)
            {
                throw new FormatException("head not found in the html document");
            }




            Body = Document.DocumentNode.DescendantsAndSelf().FirstOrDefault(x => x.Name.EqualsIgnoreCase("body"));
            if (Body == null)
            {
                throw new FormatException("body not found in the html document");
            }            
        }

        protected override CompilerResult CreateCompilerResult()
        {

            var result = base.CreateCompilerResult();
 	
            var pageScript = Document.CreateElement("SCRIPT");
            pageScript.Attributes.Add("type", "text/javascript");

            string script = "\r\n(function(window,WebAtoms){ \r\n WebAtoms.PageSetup = WebAtoms.PageSetup || {};\r\n(function (window,WebAtoms,Atom,AtomPromise){\r\n " 
                + result.Script
                + "}).call(window.WebAtoms.PageSetup,window,window.WebAtoms,window.Atom,window.AtomPromise); \r\n})(window,window.WebAtoms);";

            pageScript.AppendChild(Document.CreateTextNode(script));

            Header.AppendChild(pageScript);

            using (StringWriter sw = new StringWriter()) {
                Document.Save(sw);
                result.Document = sw.ToString();
            }

            result.Script = "";
            return result;
        }



    }
}
