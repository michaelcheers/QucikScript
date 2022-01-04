using QucikScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleQucikScript
{
    static class Program
    {
        public static QucikScript.Program program;
        static void Main(string[] args)
        {
            Console.Title = "Qucik Script Editor";
            switch (args.Length)
            {
                case 1:
                    {
                        program = new QucikScript.Program(File.OpenRead(args[0]), v => program = v);
                        break;
                    }
                case 2:
                    {
                        switch (args[0])
                        {
                            case "qtxt":
                                {
                                    program = new QucikScript.Program(v => program = v);
                                    var orgIn = Console.In;
                                    var reader = File.OpenText(args[1]);
                                    Console.SetIn(reader);
                                    while (!reader.EndOfStream)
                                        program.EnterCommand(program.ReadLine(), program.ReadLine);
                                    Console.SetIn(orgIn);
                                    Console.Write(program.written);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    {
                        program = new QucikScript.Program(v => program = v);
                        break;
                    }
            }
            while (true)
                program.EnterCommand(program.ReadLine(), program.ReadLine);
        }
    }
}
