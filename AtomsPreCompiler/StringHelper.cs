using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            n = n.Substring(1);
            if (n.StartsWith("owner.", StringComparison.OrdinalIgnoreCase)) {
                n = n.Substring(6);
            }
            return n;
        }
    }
}
