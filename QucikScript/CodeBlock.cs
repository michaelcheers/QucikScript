using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QucikScript
{
    public class CodeBlock
    {
        public enum BaseMemoryType
        {
            String,
            Function,
            Unknown
        }

        public string functionName;
        public Dictionary<string, CodeBlock> args;
        public CodeBlock[] argsList;
        public Program context;
        public Function functionContext
        {
            get
            {
                return realFunctionContext.externalContext;
            }
        }
        public Function realFunctionContext;
        public object baseMemory;

        public object GetArg(int a, string b, int runtimeIndex) => GetArgWithoutRunning(a, b).Run(runtimeIndex);

        public CodeBlock GetArgWithoutRunning (int a, string b)
        {
            if (argsList != null && argsList.Length - 1 >= a)
                return argsList[a];
            else
                return args[b];
        }

        public TimeSpan GetArgTimeSpan (int a, string b, int runtimeIndex)
        {
            object obj = GetArg(a, b, runtimeIndex);
            if (obj is int)
                return TimeSpan.FromMilliseconds((int)obj);
            else if (obj is TimeSpan)
                return (TimeSpan)obj;
            else
                return TimeSpan.Parse(obj.ToString());
        }

        public bool ContainsArg(int a, string b) =>
            (argsList != null && argsList.Length - 1 >= a) || (args != null && args.ContainsKey(b));

        public Fraction GetArgDecimal(int a, string b, int runtimeIndex)
        {
            var arg = GetArg(a, b, runtimeIndex);
            if (arg is Fraction)
                return (Fraction)arg;
            else
                return Fraction.Parse(arg.ToString());
        }

        public static CodeBlock CreateLiteral(object value)
        {
            return new CodeBlock { functionName = "literal", baseMemory = value };
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(functionName);
            if (args != null)
            {
                writer.Write(args.Count);
                foreach (var item in args)
                {
                    writer.Write(item.Key);
                    item.Value.Save(writer);
                }
            }
            else
                writer.Write(0);
            if (argsList != null)
            {
                writer.Write(argsList.Length);
                foreach (var item in argsList)
                    item.Save(writer);
            }
            else
                writer.Write(0);
            if (baseMemory != null)
            {
                writer.Write(true);
                BaseMemoryType baseMemoryType;
                if (baseMemory is string)
                    baseMemoryType = BaseMemoryType.String;
                else if (baseMemory is Function)
                    baseMemoryType = BaseMemoryType.Function;
                else
                    baseMemoryType = BaseMemoryType.Unknown;
                writer.Write((byte)baseMemoryType);
                switch (baseMemoryType)
                {
                    case BaseMemoryType.String:
                        {
                            writer.Write(baseMemory as string);
                            break;
                        }
                    case BaseMemoryType.Function:
                        {
                            (baseMemory as Function).Save(writer);
                            break;
                        }
                    case BaseMemoryType.Unknown:
                        {
                            writer.Write(baseMemory.ToString());
                            break;
                        }
                }
            }
            else
                writer.Write(false);
        }

        public CodeBlock(Stream value, Program context, Function programContext) : this(new BinaryReader(value), context, programContext) { }

        public CodeBlock(BinaryReader reader, Program context, Function realFunctionContext)
        {
            this.context = context;
            this.realFunctionContext = realFunctionContext;
            List<CodeBlock> argsList = new List<CodeBlock>();
            functionName = reader.ReadString();
            int argsLength = reader.ReadInt32();
            args = new Dictionary<string, CodeBlock>();
            for (int n = 0; n < argsLength; n++)
                args.Add(reader.ReadString(), new CodeBlock(reader, context, realFunctionContext));
            int argsListLength = reader.ReadInt32();
            for (int n = 0; n < argsListLength; n++)
                argsList.Add(new CodeBlock(reader, context, realFunctionContext));
            this.argsList = argsList.ToArray();
            bool hasBaseMemory = reader.ReadBoolean();
            if (hasBaseMemory)
            {
                BaseMemoryType type = (BaseMemoryType)reader.ReadByte();
                switch (type)
                {
                    case BaseMemoryType.String:
                    case BaseMemoryType.Unknown:
                        {
                            baseMemory = reader.ReadString();
                            break;
                        }
                    case BaseMemoryType.Function:
                        {
                            baseMemory = new Function(reader, context, realFunctionContext);
                            break;
                        }
                }
            }
        }

        public CodeBlock()
        {
        }

        public string Decompile ()
        {
            string result = "";
            result += functionName;
            return default(string);
        }

        public object Run(int runtimeIndex)
        {
            switch (functionName)
            {
                case "if":
                    {
                        var baseMemoryFunction = ((Function)baseMemory);
                        if (GetArgBool(0, "condition", runtimeIndex))
                            baseMemoryFunction.RunWithoutParams();
                        else
                            foreach (var item in baseMemoryFunction.blocks)
                                if (item.functionName == "else")
                                    return ((Function)item.baseMemory).RunWithoutParams();
                        return null;
                    }
                case "sleep":
                    {
                        Thread.Sleep(GetArgTimeSpan(0, "time", runtimeIndex));
                        return null;
                    }
                case "label":
                    {
                        var name = GetArg(0, "name", runtimeIndex).ToString();
                        var codeBlock = GetArgWithoutRunning(1, "code");
                        context.gotoLabels.Add(name, codeBlock);
                        return codeBlock.Run(runtimeIndex);
                    }
                //case "goto":
                //    {

                //    }
                case "random":
                    return context.random.Next();
                case "timeout":
                case "length":
                case "count":
                case "getcount":
                case "getlength":
                    {
                        var value = GetArg(0, "value",  runtimeIndex);
                        if (value is Array)
                            return (value as Array).Length;
                        else
                            return value.ToString().Length;
                    }
                case "getpos":
                case "getposition":
                    {
                        var value = GetArg(0, "value", runtimeIndex);
                        var index = GetArgInt(1, "index", runtimeIndex);
                        if (value is Array)
                            return (value as object[])[index];
                        else
                            return value.ToString()[index];
                    }
                case "setpos":
                case "setposition":
                    {
                        var variableName = GetArg(0, "variable", runtimeIndex) as string;
                        var value = context.GetVariable(variableName, functionContext, runtimeIndex);
                        var position = GetArgInt(1, "position", runtimeIndex);
                        var setTo = GetArg(2, "position", runtimeIndex);
                        if (value is Array)
                        {
                            object[] valueArray = value as object[];
                            valueArray[position] = setTo;
                            context.SetVariable(variableName, valueArray, functionContext, runtimeIndex);
                            return valueArray;
                        }
                        else if (value is string)
                        {
                            char[] objVal = (value as string).ToCharArray();
                            objVal[position] = Convert.ToChar(setTo);
                            var result = new string(objVal);
                            context.SetVariable(variableName, result, functionContext, runtimeIndex);
                            return result;
                        }
                        else
                            throw context.Error("Bad data type.", context.rethrowexception);
                    }
                case "switch":
                    {
                        ((Function)baseMemory).switchValue = GetArg(0, "value", runtimeIndex);
                        return ((Function)baseMemory).RunWithoutParams();
                    }
                case "define":
                case "definevar":
                case "definevariable":
                    {
                        var name = GetArg(0, "name", runtimeIndex).ToString();
                        object value = null;
                        string scope = "0";
                        if (ContainsArg(1, "value"))
                            value = GetArg(1, "value", runtimeIndex);
                        if (ContainsArg(2, "condition"))
                            scope = GetArg(2, "scope", runtimeIndex).ToString();
                        int tryParse;
                        if (!int.TryParse(scope, out tryParse))
                            tryParse = (int)(Program.Scope)(Enum.Parse(typeof(Program.Scope), scope));
                        switch (tryParse)
                        {
                            case 0:
                                {
                                    functionContext.runtime[runtimeIndex].localVars_runScope.Add(name, value);
                                    break;
                                }
                            case 1:
                                {
                                    context.globalVarsDefined.Add(name, value);
                                    break;
                                }
                            default:
                                   throw context.Error("Fatal Error, scope undefined.", context.rethrowexception);
                        }
                        return value;
                    }
                case "set":
                case "setvar":
                case "setvariable":
                    {
                        string name = GetArg(0, "name", runtimeIndex).ToString();
                        var value = GetArg(1, "value", runtimeIndex);
                        context.SetVariable(name, value, functionContext, runtimeIndex);
                        return value;
                    }
                case "print":
                case "write":
                    {
                        Console.Out.Write(GetArg(0, "value", runtimeIndex));
                        return null;
                    }
                case "case":
                    {
                        if (realFunctionContext.switchValue == null)
                            context.Error("Switch expected.", context.rethrowexception);
                        if (realFunctionContext.switchValue.ToString() == GetArg(0, "value", runtimeIndex).ToString())
                            return new ReturnBlock(((Function)baseMemory).RunWithoutParams());
                        return null;
                    }
                case "getarg":
                    return functionContext.argsList[GetArgInt(0, "index", runtimeIndex)];
                case "get":
                case "getvar":
                case "getvariable":
                    {
                        string name = GetArg(0, "name", runtimeIndex) as string;
                        return context.GetVariable(name, functionContext, runtimeIndex);
                    }
                case "play":
                    {
                        string path = GetArg(0, "path", runtimeIndex).ToString();
                        if (context.soundsPlayed.ContainsKey(path))
                            context.soundsPlayed[path].PlaySync();
                        else
                            (context.soundsPlayed[path] = new SoundPlayer(path)).PlaySync();
                        return null;
                    }
                case "println":
                case "printline":
                case "writeln":
                case "writeline":
                    {
                        Console.Out.WriteLine(GetArg(0, "value", runtimeIndex));
                        return null;
                    }
                case "getline":
                case "readline":
                case "getln":
                case "readln":
                        return Console.ReadLine();
                case "floor":
                    {
                        var value = GetArgDecimal(0, "value", runtimeIndex);
                        return value.numerator / value.denominator;
                    }
                case "concat":
                case "concatinate":
                    return GetArg(0, "a", runtimeIndex).ToString() + GetArg(1, "b", runtimeIndex).ToString();
                case "mod":
                case "rem":
                    return GetArgDecimal(0, "a", runtimeIndex) % GetArgDecimal(1, "b", runtimeIndex);
                case "log":
                    return Fraction.Log(GetArgDecimal(0, "a", runtimeIndex), ContainsArg(1, "b") ? GetArgDecimal(1, "b", runtimeIndex) : new Fraction(10, 1));
                case "add":
                case "+":
                    return GetArgDecimal(0, "a", runtimeIndex) + GetArgDecimal(1, "b", runtimeIndex);
                case "subtract":
                case "-":
                    return GetArgDecimal(0, "a", runtimeIndex) - GetArgDecimal(1, "b", runtimeIndex);
                case "divide":
                case "/":
                    return GetArgDecimal(0, "a", runtimeIndex) / GetArgDecimal(1, "b", runtimeIndex);
                case "multiply":
                case "x":
                case "*":
                    return GetArgDecimal(0, "a", runtimeIndex) * GetArgDecimal(1, "b", runtimeIndex);
                case "literal":
                    return baseMemory;
                case "negate":
                case "neg":
                    return -GetArgDecimal(0, "value", runtimeIndex);
                case "equal":
                case "equals":
                case "==":
                    return GetArg(0, "a", runtimeIndex).ToString() == GetArg(1, "b", runtimeIndex).ToString();
                case "lessthan":
                case "<":
                    return GetArgDecimal(0, "a", runtimeIndex) < GetArgDecimal(1, "b", runtimeIndex);
                case "greaterthan":
                case ">":
                    return GetArgDecimal(0, "a", runtimeIndex) > GetArgDecimal(1, "b", runtimeIndex);
                case "greaterthanequal":
                case ">=":
                    return GetArgDecimal(0, "a", runtimeIndex) >= GetArgDecimal(1, "b", runtimeIndex);
                case "lessthanequal":
                case "<=":
                    return GetArgDecimal(0, "a", runtimeIndex) <= GetArgDecimal(1, "b", runtimeIndex);
                case "return":
                    return new ReturnBlock(GetArg(0, "value", runtimeIndex));
                case "while":
                    {
                        while (GetArgBool(0, "condition", runtimeIndex))
                            ((Function)baseMemory).RunWithoutParams();
                        return null;
                    }
                    //See if decleration.
                case "else":
                    return null;
                case "or":
                    return GetArgBool(0, "a", runtimeIndex) || GetArgBool(1, "b", runtimeIndex);
                case "and":
                    return GetArgBool(0, "a", runtimeIndex) && GetArgBool(1, "b", runtimeIndex);
                case "not":
                case "!":
                    return !GetArgBool(0, "value", runtimeIndex);
                default:
                    return context.defined[functionName].Run(args, argsList);
            }
        }

        public bool GetArgBool(int a, string b, int runtimeIndex)
        {
            var value = GetArg(a, b, runtimeIndex);
            int tryParseInt;

            if (value is bool)
                return (bool)value;
            else if (value is int)
                return Convert.ToBoolean((int)value);
            else if (int.TryParse(value.ToString(), out tryParseInt))
                return Convert.ToBoolean(tryParseInt);
            else
                return bool.Parse(value.ToString());
        }

        private int GetArgInt(int a, string b, int runtimeIndex)
        {
            object value = GetArg(a, b, runtimeIndex);
            if (value is int)
                return (int)(value);
            else
                return int.Parse(value.ToString());
        }
    }
}
