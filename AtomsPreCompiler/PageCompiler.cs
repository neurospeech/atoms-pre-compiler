using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public int TemplateIndex { get; set; }        

        public List<HtmlNode> NodesToDelete { get; set; }
        public List<HtmlAttribute> AttributesToDelete { get; set; }

        public List<HtmlNode> PageTemplates { get; set; }

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
            TemplateIndex = 0;

            PageTemplates = new List<HtmlNode>();

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

            string script = "\r\n(function(window,WebAtoms){ \r\n WebAtoms.PageSetup = WebAtoms.PageSetup || {};\r\n(function (window,WebAtoms,Atom,AtomPromise){\r\n "+ CompiledScript.InnerText + "}).call(window.WebAtoms.PageSetup,window,window.WebAtoms,window.Atom,window.AtomPromise); \r\n})(window,window.WebAtoms);";

            CompiledScript.RemoveAllChildren();
            CompiledScript.AppendChild(Document.CreateTextNode(script));

            Header.AppendChild(CompiledScript);

            if (PageTemplates.Any()) {
                foreach (var item in PageTemplates)
                {
                    item.Remove();
                }

                var bodyTemplate = Document.CreateElement("div");
                bodyTemplate.Attributes.Add("style", "display:none;z-index:-1000000");
                int i = 0;
                foreach (var item in PageTemplates)
                {
                    item.Attributes.Add("id", "pt" + i);
                    bodyTemplate.AppendChild(item);
                    i++;
                }

                Body.AppendChild(bodyTemplate);
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
                    script += "this.a" + Index + "= function (e){\r\n";
                    script += lastScript;
                    script += "\r\n}\r\n";

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
                CompileAttribute(element, att.Name.ToLower(), att);
            }

            if (Writer.GetStringBuilder().ToString().Trim().Length > 0) {
                element.Attributes.Add("data-atom-init", "a"+ Index );
            }
        }

        private void CompileAttribute(HtmlNode element, string name, HtmlAttribute att)
        {
            if (!(name.StartsWith("atom-") || name.StartsWith("style-")))
                return;

            if (name == "atom-type" || name == "atom-dock" || name == "atom-template-name" || name == "atom-local-scope")
            {
                // ignore...
                return;
            }

            string value = att.Value;

            if (name == "atom-presenter")
            {
                CompilePresenter(element,att,name,value);
                return;
            }

            if (name == "atom-template") {
                CompileTemplate(element, name, value);
                return;
            }

            if (name.StartsWith("atom-")) {
                name = name.Substring(5);
            }
            name = name.ToCamelCase();

            if (value.StartsWith("{")) { 
                // one time binding...
                value = value.Substring(1).Till("}");
                CompileOneTimeBinding(att,name, value);
                return;
            }

            if (value.StartsWith("[")) { 
                // one way binding...
                value = value.Substring(1).Till("]");
                CompileOneWayBinding(att, name, value);
                return;
            }

            if (value.StartsWith("$["))
            {
                string events = "";
                value = value.Substring(2);
                int index = value.LastIndexOf(']');
                events = value.Substring(index+1);
                value = value.Substring(0, index);
                CompileTwoWayBinding(att, name, value, events);
                return;
            }

            if(value.StartsWith("^[")){
                value = value.Substring(2).Till("]");
                CompileTwoWayBinding(att, name, value, "keyup,keydown,keypress,blur,click");
                return;
            }

            CompileOneTimeBinding(att, name, "'" + value + "'");

        }

        private void CompileTemplate(HtmlNode element, string name, string value)
        {
            //PageTemplates.Add(element);
            //Writer.WriteLine("this['{0}'] = document.getElementById('pt{1}')",value, PageTemplates.Count );
        }

        private void CompilePresenter(HtmlNode element, HtmlAttribute att, string name, string value)
        {
            //Writer.WriteLine("\tthis[" + value + "] = e;");
        }

        private void CompileTwoWayBinding(HtmlAttribute att, string name, string value, string events)
        {
            value = value.TrimStart('$', '@');
            Writer.WriteLine("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);

            value = "[" + string.Join(", ", value.Split('.').Select( s=> "'" + s + "'" )) + "]";

            if (string.IsNullOrWhiteSpace(events))
            {
                Writer.WriteLine("\tthis.bind(e,'{0}',{1},true)",name,value);
            }
            else {
                Writer.WriteLine("\tthis.bind(e,'{0}',{1},true,null,{2})", name, value, events);
            }

            AttributesToDelete.Add(att);
        }

        private void CompileOneTimeBinding(HtmlAttribute att, string name, string value)
        {
            Writer.WriteLine("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);

            value = bindingRegex.Replace(value, (s) => "Atom.get(this,'" + s.Value.Substring(1) + "')");

            if (name.StartsWith("style"))
            {
                name = name.Substring(5).ToCamelCase();
                Writer.WriteLine("\te.style['{0}'] = {1};", name, value);
            }
            else
            {
                Writer.WriteLine("\tthis.setLocalValue('{0}', {1}, e);", name, value);
            }
            AttributesToDelete.Add(att);
        }

        public int BindingIndex { get; set; }

        private void CompileOneWayBinding(HtmlAttribute att, string name, string value)
        {
            List<Tuple<string, string>> variables = new List<Tuple<string, string>>();
            value = bindingRegex.Replace(value, (s) => {
                var v = variables.FirstOrDefault(x => "$" + x.Item1 == s.Value);
                if (v == null) {
                    v = new Tuple<string, string>(s.Value.Substring(1), "v" + (variables.Count + 1));
                    variables.Add(v);
                }
                return v.Item2;
            });

            if (variables.Count == 0)
            {
                CompileOneTimeBinding(att, name, value);
                return;
            }

            Writer.WriteLine("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);
            if (value == variables.First().Item2)
            {
                // no function.. simple binding...
                var bindingPath = string.Join(",", variables.Select(x => "\r\n\t[" + string.Join(", ", x.Item1.Split('.').Select(s => "'" + s + "'")) + "]"));


                Writer.WriteLine("\tthis.bind(e,'{0}',{1},false);", name, bindingPath);

            }
            else
            {
                string varList = string.Join(",", variables.Select(x => x.Item2));

                BindingIndex++;

                var bindingPath = string.Join(",", variables.Select(x => "\r\n\t[" + string.Join(", ", x.Item1.Split('.').Select(s=> "'" + s + "'")) + "]"));


                Writer.WriteLine("\tthis.bind(e,'{0}',[{1}],\r\n\t\t\tfalse, function({2}){{\r\n\t\t\t\t return {3}; \r\n\t\t\t}});",
                    name,
                    bindingPath,
                    varList,
                    value);
            }

            AttributesToDelete.Add(att);
        }


        Regex bindingRegex = new Regex("(\\$)(window|appScope|scope|data|owner|localScope)(\\.[a-zA-Z_][a-zA-Z_0-9]*)*", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);


        private void CompileScript(HtmlNode element)
        {
            var script = element.InnerText.Trim();
            if (script.StartsWith("({") && script.EndsWith("})"))
            {
                Writer.WriteLine("// Line " + element.Line);
                Writer.WriteLine("\tthis.initScope(" + script + ");");

                NodesToDelete.Add(element);
            }
        }


    }
}
