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

    }
}
