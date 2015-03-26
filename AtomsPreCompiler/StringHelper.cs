using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeuroSpeech.AtomsPreCompiler
{
    internal static class StringHelper
    {

        internal static bool EqualsIgnoreCase(this string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a))
                return string.IsNullOrWhiteSpace(b);
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        internal static string Till(this string a, string b)
        {
            int index = a.LastIndexOf(b);
            if (index == -1)
                return a;
            return a.Substring(0, index);
        }

        internal static string ToCamelCase(this string a) {
            var t = string.Join("", a.ToLowerInvariant().Split('-').Select( (s,i)   => i==0 ? s : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant().Trim())));
            return t;
        }

        internal static string EscapeBinding(this string n) {
            if (n.StartsWith("$"))
            {
                n = n.Substring(1);
            }
            if (n.StartsWith("owner.", StringComparison.OrdinalIgnoreCase)) {
                n = n.Substring(6);
            }
            return n;
        }

        internal static string ToEncodedString(this string n) {
            return HttpUtility.JavaScriptStringEncode(n, true);
        }

        internal static string GetAtomType(this HtmlNode element) {
            return element.Attributes.Where(x => x.Name == "atom-type").Select(x => x.Value).FirstOrDefault();
        }

        internal static void AddComment(this HtmlNode element, string comment)
        {

            var c = element.OwnerDocument.CreateComment("<!-- " + comment + " -->");
            element.AppendChild(c);

        }

        internal static bool IsEmpty(this string n) {
            return string.IsNullOrWhiteSpace(n);
        }

    }
}
