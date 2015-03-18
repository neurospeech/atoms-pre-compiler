using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsPreCompiler
{
    public class PageCompiler
    {

        public HtmlDocument Document { get; set; }

        public HtmlNode Header { get; set; }

        public HtmlNode Body { get; set; }

        public StringWriter Writer { get; set; }

        public HtmlNode CompiledScript { get; set; }

        public int Index { get; set; }

        public List<HtmlNode> NodesToDelete { get; set; }
        public List<HtmlAttribute> AttributesToDelete { get; set; }

        public string CompiledHtml { get; set; }

        public PageCompiler(string html)
        {
            this.Document = new HtmlDocument();
            Document.LoadHtml(html);


            Header = Document.DocumentNode.DescendantsAndSelf().FirstOrDefault(x => x.Name.EqualsIgnoreCase("head"));
            if (Header == null)
            {
                throw new FormatException("head not found in the html document");
            }

            CompiledScript = Document.CreateElement("SCRIPT");
            CompiledScript.Attributes.Add("type", "text/javascript");

            Body = Document.DocumentNode.DescendantsAndSelf().FirstOrDefault(x => x.Name.EqualsIgnoreCase("body"));
            if (Body == null) {
                throw new FormatException("body not found in the html document");
            }

            NodesToDelete = new List<HtmlNode>();
            AttributesToDelete = new List<HtmlAttribute>();

            Index = 0;

            CreateSetup();

            CompileElements();

            CreateSetup();

            foreach (var item in AttributesToDelete)
            {
                item.Remove();
            }
            foreach (var item in NodesToDelete)
            {
                item.Remove();
            }

            using (StringWriter sw = new StringWriter()) {
                Document.Save(sw);
                CompiledHtml = sw.GetStringBuilder().ToString();
            }
        }

        private void CreateSetup()
        {

            if (Writer != null)
            {
                // push...

                var lastScript = Writer.GetStringBuilder().ToString().Trim();

                if (!string.IsNullOrWhiteSpace(lastScript))
                {

                    string script = CompiledScript.InnerText ?? "";
                    CompiledScript.RemoveAllChildren();

                    script += "\r\n";
                    script += "WebAtoms.PageSetup.a" + Index + "= function (e){";
                    script += lastScript;
                    script += "}\r\n";

                    CompiledScript.AppendChild(Document.CreateTextNode(script));

                    Index++;
                    
                }


            }
            else {
                Index++;
            }

            Writer = new StringWriter();


        }

        private void CompileElements()
        {
            var all = Body.DescendantsAndSelf().ToList();
            foreach (var element in all)
            {
                CompileElement(element);
            }
        }

        private void CompileElement(HtmlNode element)
        {
            if (element.Name.EqualsIgnoreCase("script")) {
                CompileScript(element);
                return;
            }

            CreateSetup();


            foreach (var att in element.Attributes)
            {
                CompileAttribute(att.Name.ToLower(), att);
            }

            if (Writer.GetStringBuilder().ToString().Trim().Length > 0) {
                element.Attributes.Add("data-atom-init", "a"+ Index );
            }
        }

        private void CompileAttribute(string name, HtmlAttribute att)
        {
            if (name == "atom-type") {
                // ignore...
                return;
            }

            string value = att.Value;
            bool bind = value.StartsWith("[");

            if (name.StartsWith("style-") || name.StartsWith("atom-")) {
                if (name.StartsWith("atom-"))
                {
                    name = name.Substring(5);
                }
                Writer.WriteLine("this.setValue('{0}',\"{1}\",{2},e);", name, value, bind);
                AttributesToDelete.Add(att);
            }

        }

        private void CompileScript(HtmlNode element)
        {
            var script = element.InnerText.Trim();
            if (script.StartsWith("({") && script.EndsWith("})"))
            {
                Writer.WriteLine("// Line " + element.Line);
                Writer.WriteLine("this.initScope(" + script + ");");

                NodesToDelete.Add(element);
            }
        }


    }
}
