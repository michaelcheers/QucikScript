using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace QucikScript
{
    public class Program
    {
        public enum Scope
        {
            Local = 0,
            False = 0,
            Global = 1,
            True = 1
        }

        public Dictionary<string, object> globalVarsDefined = new Dictionary<string, object>();
        public Dictionary<string, Function> defined = new Dictionary<string, Function>();
        public CodeBlock workingMemoryWorking;
        public Function entry = new Function();
        public Function working;
        public Action<Program> selfReplace;
        public string written;
        public Random random;
        public Dictionary<string, CodeBlock> gotoLabels = new Dictionary<string, CodeBlock>();

        public void Decompile ()
        {
            written = "";
            written += entry.Decompile();
            foreach (var item in defined)
            {

            }
        }

        public Func<string, bool, QucikException> Error = (string exception, bool rethrow) =>
        {
            if (rethrow)
                throw new QucikException(exception);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception);
            Console.WriteLine("Press enter to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter) ;
            throw new QucikException(exception);
        };

        public Program (BinaryReader reader, Action<Program> selfReplace) : this(selfReplace)
        {
            entry = new Function(reader, this);
            random = new Random();
            var definedLength = reader.ReadInt32();
            for (int n = 0; n < definedLength; n++)
                defined.Add(reader.ReadString(), new Function(reader, this));
            reader.Close();
            this.selfReplace = selfReplace;
            working = entry;
        }

        public Program (Stream stream, Action<Program> selfReplace) : this (new BinaryReader(stream), selfReplace) { }

        public void Save (Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            entry.Save(writer);
            writer.Write(defined.Count);
            foreach (var item in defined)
            {
                writer.Write(item.Key);
                item.Value.Save(writer);
            }
            writer.Flush();
            writer.Close();
        }

        public void WriteLine (object value)
        {
            string written = value + "\n";
            Console.Write(written);
            this.written += written;
        }

        public string ReadLine ()
        {
            var readline = Console.ReadLine();
            written += readline + "\n";
            return readline;
        }

        public void Write (object value)
        {
            Console.Write(value);
            written += value;
        }

        public Program (Action<Program> selfReplace)
        {
            this.selfReplace = selfReplace;
            random = new Random();
            working = entry;
        }

        internal Dictionary<string, SoundPlayer> soundsPlayed = new Dictionary<string, SoundPlayer>();

        public object GetVariable (string name, Function function, int index)
        {
            var functionRuntime = function.runtime[index];
            if (functionRuntime != null && functionRuntime.localVars_runScope.ContainsKey(name))
                return functionRuntime.localVars_runScope[name];
            else if (globalVarsDefined.ContainsKey(name))
                return globalVarsDefined[name];
            else
                throw Error("Variable non-existant: \"" + name + "\"", rethrowexception);
        }

        public void SetVariable (string name, object value, Function function, int index)
        {
            var functionRuntime = function.runtime[index].localVars_runScope;
            if (functionRuntime.ContainsKey(name))
                functionRuntime[name] = value;
            else if (globalVarsDefined.ContainsKey(name))
                globalVarsDefined[name] = value;
            else
                throw Error("Variable non-existant: \"" + name + "\"", rethrowexception);
        }

        internal bool rethrowexception;

        public void EnterCommand (string value, Func<string> reInput)
        {
            var command = GetCommand(value, reInput, this, working);
            switch (command.functionName)
            {
                case "run":
                    {
                        Console.Clear();
                        if (command.args.ContainsKey("debug"))
                            Debugger.Break();
                        if (command.args.ContainsKey("rethrowexception"))
                            rethrowexception = true;
                        entry.Run(new Dictionary<string, CodeBlock>(), new CodeBlock[] { });
                        rethrowexception = false;
                        while (Console.ReadKey().Key != ConsoleKey.Enter) ;
                        Console.Clear();
                        Console.Write(written);
                        return;
                    }
                case "endblock":
                case "}":
                    {
                        working = working.stepUpContext;
                        return;
                    }
                case "while":
                case "if":
                case "else":
                case "switch":
                case "case":
                    {
                        command.baseMemory = new Function(working);
                        ((Function)command.baseMemory).isBaseMemoryOf = command;
                        working.blocks.Add(command);
                        working = (Function)command.baseMemory;
                        return;
                    }
                case "save":
                    {
                        var arg0 = command.GetArg(0, "path", 0).ToString();
                        File.WriteAllText(arg0, written);
                        return;
                    }
                case "compile":
                case "savequcik":
                    {
                        var arg0 = command.GetArg(0, "path", 0).ToString();
                        Save(File.OpenWrite(arg0));
                        return;
                    }
                case "load":
                    {
                        var arg0 = command.GetArg(0, "path", 0) as string;
                        var reader = File.OpenText(arg0);
                        while (!reader.EndOfStream)
                            EnterCommand(reader.ReadLine(), reader.ReadLine);
                        reader.Close();
                        return;
                    }
                case "decompile":
                case "loadqucik":
                    {
                        var arg0 = command.GetArg(0, "path", 0) as string;
                        selfReplace(new Program(File.OpenRead(arg0), selfReplace));
                        break;
                    }
                case "addfunc":
                    {
                        var function = new Function();
                        defined.Add(command.GetArg(0, "name", 0) as string, function);
                        working = function;
                        return;
                    }
                case "changefunc":
                    {
                        string name = command.GetArg(0, "name", 0) as string;
                        if (name == "main")
                            working = entry;
                        else
                            working = defined[name];
                        return;
                    }
                case "{":
                case "//":
                case "comment":
                case "commentout":
                case "":
                    return;
                case "purge":
                    written = "";
                    Console.Clear();
                    entry = new Function();
                    working = entry;
                    defined = new Dictionary<string, Function>();
                    Console.WriteLine("Purged successfully...");
                    return;
                default:
                    {
                        working.blocks.Add(command);
                        return;
                    }
            }
        }

        public static CodeBlock ReInputIfNeeded (string value, Func<string> reInput, Program context, Function functionContext)
        {
            if (value == "")
                return GetCommand(reInput(), reInput, context, functionContext);
            else if (value == "_empty")
                return CodeBlock.CreateLiteral(string.Empty);
            else
                return CodeBlock.CreateLiteral(value);
        }

        public static string[] DSplit (string value, char split, char quoteCharacter)
        {
            List<string> result = new List<string>();
            string current = "";
            bool quoting = false;

            foreach (var item in value)
            {
                if (item == split && !quoting)
                {
                    result.Add(current);
                    current = "";
                }
                else if (item == quoteCharacter)
                    quoting = !quoting;
                else
                    current += item;
            }
            result.Add(current);
            return result.ToArray();
        }

        public static CodeBlock GetCommand (string value, Func<string> reInput, Program context, Function functionContext)
        {
            var split = DSplit(value, ' ', '\"');
            string command = default(string);
            Dictionary<string, CodeBlock> args = new Dictionary<string, CodeBlock>();
            List<CodeBlock> argsList = new List<CodeBlock>();
            for (int n = 0; n < split.Length; n++)
            {
                var item = split[n];
                if (n == 0)
                {
                    command = item;
                    continue;
                }
                var itemColonSplit = item.Split(':');
                switch (itemColonSplit.Length)
                {
                    case 2:
                        {
                            if (command == "load")
                                goto case 1;
                            args.Add(itemColonSplit[0], ReInputIfNeeded(itemColonSplit[1], reInput, context, functionContext));
                            break;
                        }
                    case 1:
                        {
                            argsList.Add(ReInputIfNeeded(itemColonSplit[0], reInput, context, functionContext));
                            break;
                        }
                    default:
                        throw new ArgumentException("Error Occured.");
                }
            }
            return new CodeBlock { context = context, args = args, argsList = argsList.ToArray(), functionName = command, realFunctionContext = functionContext };
        }
    }
}
