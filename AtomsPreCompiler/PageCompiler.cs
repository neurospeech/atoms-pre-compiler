using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.AtomsPreCompiler
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

        protected override void BeforeCompileNode(HtmlNode element)
        {
            base.BeforeCompileNode(element);
            var type = element.GetAtomType();

            if (IsFormLayout(element))
            {
                CreateFieldTemplate(element);
            }

        }

        private bool IsFormLayout(HtmlNode element) {
            var type = element.GetAtomType();
            switch (type)
            {
                case "AtomFormLayout":
                case "AtomFormRow":
                case "AtomFormTab":
                case "AtomFormGridLayout":
                case "AtomFormVerticalLayout":
                    return true;
            }
            return false;
        }

        private void CreateFieldTemplate(HtmlNode element)
        {
            foreach (var child in element.ChildNodes.ToArray())
            {
                if (child is HtmlTextNode)
                    continue;
                if (IsFormLayout(child)) {
                    continue;
                }
                var c = Document.CreateElement("div");
                c.Attributes.Add("data-atom-type", "AtomFormField");
                element.AppendChild(c);
                child.Remove();

                foreach (var at in child.Attributes.ToArray())
                {
                    string name = at.Name;
                    if (IsFormFieldAttribute(name))
                    {
                        at.Remove();
                        c.Attributes.Add(at);
                    }
                    else {
                        if (name == "atom-value") {
                            if (at.Value.StartsWith("$[")) {
                                c.Attributes.Add("atom-field-value", at.Value);
                            }
                        }
                    }
                }

                c.AppendChild(child);
            }
        }

        private bool IsFormFieldAttribute(string name)
        {
            switch (name)
            {
                case "atom-label":
                case "atom-description":
                case "atom-required":
                case "atom-regex":
                case "atom-data-type":
                case "atom-is-valid":
                case "atom-field-value":
                case "atom-field-visible":
                case "atom-field-class":
                case "atom-error":
                    return true;
                default:
                    break;
            }
            return false;
        }


    }
}
