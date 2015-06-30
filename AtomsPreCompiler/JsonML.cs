using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.AtomsPreCompiler
{
    class JsonML
    {
        private HtmlDocument Document;

        public JsonML(HtmlDocument Document)
        {
            this.Document = Document;
        }
        internal static string Compile(HtmlDocument Document)
        {
            JsonML j = new JsonML(Document);
            return j.Compile();
        }

        private string Compile() {

            var root = Document.DocumentNode;

            var container = root.FirstChild;

            if (container.Name == "container")
            {
                return "[" + string.Join(", ", container.ChildNodes.Where( x => !(x is HtmlTextNode) ).Select(x => WriteElement(x))) + "]";
            }

            // this is template...
            return "[" + WriteElement(container) + "]";

        }

        private string WriteElement(HtmlNode e, string postFix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[\"{0}\",\r\n", e.Name);
            List<string> list = new List<string>();
            foreach (HtmlAttribute a in e.Attributes)
            {
                list.Add(string.Format("\"{0}\": \"{1}\"", a.Name, encode(a.Value)));
            }
            sb.AppendFormat("{{ {0} }}\r\n", string.Join(", ", list));
            list.Clear();
            if (e.ChildNodes.Any())
            {
                foreach (HtmlNode child in e.ChildNodes)
                {
                    if (child is HtmlTextNode)
                    {
                        string text = (child as HtmlTextNode).Text.Trim('\n', '\r');
                        if (text.IsEmpty())
                            continue;
                        list.Add(string.Format("\"{0}\"", encode(text)));
                    }
                    else
                    {
                        list.Add(WriteElement((HtmlNode)child));
                    }
                }
                sb.AppendFormat(",{0}", string.Join(", ", list));
            }
            sb.AppendFormat("]");
            return sb.ToString();
        }

        private string encode(string txt)
        {
            return txt.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

    }
}
