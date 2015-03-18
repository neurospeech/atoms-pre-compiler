using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsPreCompiler
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
    }
}
