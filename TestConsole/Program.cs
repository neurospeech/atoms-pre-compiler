using AtomsPreCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            PageCompiler pc = new PageCompiler(Test.TestPage);

            File.WriteAllText("test.html", pc.CompiledHtml);
            
        }
    }
}
