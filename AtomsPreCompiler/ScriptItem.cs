using HtmlAgilityPack;
using System.IO;

namespace NeuroSpeech.AtomsPreCompiler
{
    public class ScriptItem
    {

        public StringWriter Writer { get; private set; }

        public ScriptItem(string key, HtmlNode element)
        {
            this.Writer = new StringWriter();
            this.Key = key;
            this.Element = element;
        }

        public string Key { get; private set; }

        public HtmlNode Element { get; private set; }

        public string Script
        {
            get
            {
                return Writer.GetStringBuilder().ToString().Trim();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Script.Length == 0;
            }
        }
    }
}