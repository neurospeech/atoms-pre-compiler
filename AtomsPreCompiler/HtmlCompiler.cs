﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeuroSpeech.AtomsPreCompiler
{
    public class HtmlCompiler
    {

        public HtmlDocument Document { get; set; }


        public StringWriter Writer { get; set; }


        public int Index { get; set; }

        public string Prefix { get; set; }

        public List<HtmlNode> NodesToDelete { get; set; }
        public List<HtmlAttribute> AttributesToDelete { get; set; }

        public List<ScriptItem> CompiledScripts { get; set; }

        public HtmlNode Root { get; set; }

        public bool Debug { get; set; }

        public HtmlCompiler()
        {
            Prefix = "a";
        }

        public CompilerResult Compile(string html) {

            Writer = null;
            this.CompiledScripts = new List<ScriptItem>();

            this.Document = new HtmlDocument();
            Document.LoadHtml(html);

            Root = Document.DocumentNode;

            OnBeforeCompile();

            NodesToDelete = new List<HtmlNode>();
            AttributesToDelete = new List<HtmlAttribute>();

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

            OnAfterCompile();

            return CreateCompilerResult();
        }

        protected virtual CompilerResult CreateCompilerResult()
        {
            using(StringWriter sw= new StringWriter()){
                Document.Save(sw);
                return new CompilerResult{ 
                    Document = sw.ToString(),
                    JsonMLDocument = JsonML.Compile(Document),
                    Script = string.Join("\r\n", CompiledScripts.Select((x, i) => x.StringBuilder.Length > 0 ? "this." + x.Key + "= function(e){\r\n" + x.StringBuilder.ToString() + "\r\n};\r\n" : ""))
                };
            }
        }

        protected virtual void OnBeforeCompile()
        {
            
        }

        protected virtual void OnAfterCompile()
        {
            
        }

        private void CreateSetup()
        {

            if (Writer != null)
            {
                // push...

                var lastScript = Writer.GetStringBuilder().ToString().Trim();

                if (!string.IsNullOrWhiteSpace(lastScript))
                {

                    Index++;
                    Writer = new StringWriter();
                    CompiledScripts.Add(new ScriptItem { Key=Prefix + Index, StringBuilder = Writer.GetStringBuilder() });
                    return;

                }


            }
            else
            {
                Index++;


                Writer = new StringWriter();
                CompiledScripts.Add(new ScriptItem { Key = Prefix + Index, StringBuilder = Writer.GetStringBuilder() });
            }


        }

        private void CompileElements()
        {
            var all = Root.DescendantsAndSelf().ToList();
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
                if (att.Name.StartsWith("atom-")) {
                    att.Name = "data-" + att.Name;
                }
            }

            if (Writer.GetStringBuilder().ToString().Trim().Length > 0) {
                element.Attributes.Add("data-atom-init", Prefix + Index );
            }
        }

        private void CompileAttribute(HtmlNode element, string name, HtmlAttribute att)
        {
            if (!(name.StartsWith("atom-") || name.StartsWith("style-") || name.StartsWith("event-")))
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

            CompileOneTimeBinding(att, name, "'" + value + "'",true);

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

            value = HtmlEntity.DeEntitize(value);

            DebugLog("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);

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

        private void DebugLog(string format, params object[] args)
        {
            if (Debug) {
                Writer.WriteLine(format, args);
            }
        }

        private void CompileOneTimeBinding(HtmlAttribute att, string name, string value, bool constant = false)
        {
            DebugLog("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);

            value = HtmlEntity.DeEntitize(value);

            value = bindingRegex.Replace(value, (s) => "Atom.get(this,'" + s.Value.EscapeBinding() + "')");

            // only if it is constant, ignore setLocalValue
            // setLocalValue is necessary to evaluate promise value
            if (constant)
            {
                if (name.StartsWith("style"))
                {
                    name = name.Substring(5).ToCamelCase();
                    Writer.WriteLine("\te.style['{0}'] = {1};", name, value);
                }
                else
                {
                    switch (name)
                    {
                        case "text":
                        case "value":
                        case "isEnabled":
                        case "checked":
                        case "html":
                        case "absPos":
                        case "relPos":
                            Writer.WriteLine("\tAtomProperties.{0}(e,{1});", name, value);
                            break;
                        case "class":
                            Writer.WriteLine("\tAtomProperties.{0}(e,{1});", name, value);
                            break;
                        default:
                            Writer.WriteLine("\tthis.setLocalValue('{0}', {1}, e);", name, value);
                            break;
                    }
                }
            }
            else {
                Writer.WriteLine("\tthis.setLocalValue('{0}', {1}, e);", name, value);
            }
            AttributesToDelete.Add(att);
        }

        public int BindingIndex { get; set; }


        private void CompileOneWayBinding(HtmlAttribute att, string name, string value)
        {

            value = HtmlEntity.DeEntitize(value);

            List<Tuple<string, string>> variables = new List<Tuple<string, string>>();
            value = bindingRegex.Replace(value, (s) => {
                var v = variables.FirstOrDefault(x => "$" + x.Item1 == s.Value);
                if (v == null) {
                    v = new Tuple<string, string>(s.Value.EscapeBinding(), "v" + (variables.Count + 1));
                    variables.Add(v);
                }
                return v.Item2;
            });

            if (variables.Count == 0)
            {
                CompileOneTimeBinding(att, name, value);
                return;
            }

            DebugLog("/* Line {0}, {1}=\"{2}\" */", att.Line, att.Name, att.Value);
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
                DebugLog("// Line " + element.Line);

                script = HtmlEntity.DeEntitize(script);

                Writer.WriteLine("\tthis.initScope(" + script + ");");

                NodesToDelete.Add(element);
            }
        }


    }

    public class ScriptItem {
        public string Key { get; set; }
        public StringBuilder StringBuilder { get; set; }
    }
}
