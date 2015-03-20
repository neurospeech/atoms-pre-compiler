using NeuroSpeech.AtomsPreCompiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //HtmlCompiler pc = new HtmlCompiler();
            //pc.Debug = true;
            //System.IO.File.WriteAllText("test.html", pc.Compile(Test.Template).Document);

            PageCompiler pc = new PageCompiler();
            pc.Debug = true;
            System.IO.File.WriteAllText("test.html", pc.Compile(Test.TestPage).Document);
            

        }
    }

    public class DynamicCompiler {

        public object DObject { get; set; }

        public DynamicCompiler(string path)
        {

            string typeName = "NeuroSpeech.AtomsPreCompiler.HtmlCompiler";

            string[] dependencies = new string[] { 
                "HtmlAgilityPack",
                "NeuroSpeech.AtomsPreCompiler"
            };

            System.Reflection.Assembly a = null;
            foreach (var item in dependencies)
            {

                if (!TryLoad(item, out a, path))
                    throw new System.IO.FileLoadException("Unable to load " + item);
            }

            var t = a.GetExportedTypes().FirstOrDefault(x => x.Namespace + "." + x.Name == typeName);

            DObject = Activator.CreateInstance(t);
        }

        public Result Compile(string html)
        {
            object r =  DObject.GetType().GetMethod("Compile").Invoke(DObject, new object[] { html });

            Type t = r.GetType();
            var result = new Result { };

            result.Document = (string)t.GetProperty("Document").GetValue(r);
            result.Script = (string)t.GetProperty("Script").GetValue(r);

            return result;
        }

        public class Result {
            public string Document { get; set; }
            public string Script { get; set; }
        }

        private bool TryLoad(string item, out System.Reflection.Assembly a, string root = null)
        {
            foreach (var f in System.IO.Directory.EnumerateFiles(root,"*.dll")) {
                if (f.EndsWith(item, StringComparison.OrdinalIgnoreCase)) {
                    a = System.Reflection.Assembly.LoadFrom(f);
                    return true;
                }
            }

            foreach (var d in System.IO.Directory.EnumerateDirectories(root))
            {
                if (TryLoad(item, out a, d)) { 
                    return true;
                }
            }
            a = null;
            return false;
        }

    }
}
