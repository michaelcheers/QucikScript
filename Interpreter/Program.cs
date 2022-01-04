using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = args[0];
            QucikScript.Program program = new QucikScript.Program(File.OpenRead(args[0]), v => program = v);
            Console.Clear();
            program.entry.Run(new Dictionary<string, QucikScript.CodeBlock>(), new QucikScript.CodeBlock[] { });
            while (Console.ReadKey().Key != ConsoleKey.Enter) ;
        }
    }
}
