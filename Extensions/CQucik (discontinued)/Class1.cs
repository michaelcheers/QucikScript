using QucikScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CQucik
{
    public class CCompiler
    {
        static CCompiler ()
        {
            cQucik = Assembly.GetExecutingAssembly();
            defaultStartC = new StreamReader(cQucik.GetManifestResourceStream("CQucik.startC.h")).ReadToEnd();
        }

        static Assembly cQucik;
        static string defaultStartC;
        string startC;
        public CCompiler ()
        {
            startC = defaultStartC;
        }
        
        public string CompileToC (Program program)
        {
            string result = startC;
            foreach (var item in program.defined)
            {

            }
        }

        public string CompileToC (Function function)
        {
            string result = "{";
            foreach (var item in function.blocks)
            {
                
            }
            return result += "}";
        }

        public string CompileToC (CodeBlock codeBlock)
        {
            string result = "";
            switch (codeBlock.functionName)
            {
                case "define":
                case "definevar":
                case "definevariable":
                    {
                        var name = codeBlock.GetArg(0, "name").ToString();
                        string value = null;
                        if (codeBlock.ContainsArg(1, "value"))
                            value = codeBlock.GetArg(1, "value").ToString();
                        if (value == null)
                            return "Object " + name + ";\n";
                        else
                            return "Object " + name + " = " + value + ";\n";
                    }
                case "write":
                    {
                        return "printf(\"" +  "\"";
                    }
                default:
                    break;
            }
        }

        public string CompileToC (Function function, string name)
        {
            return "Object " + name + "\n" + CompileToC(function);
        }
    }
}
