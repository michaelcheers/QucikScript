using System;
using System.Collections.Generic;
using System.IO;

namespace QucikScript
{
    public class Function
    {
        public class Runtime
        {
            public Dictionary<string, object> localVars_runScope = new Dictionary<string, object>();
        }
        public List<CodeBlock> blocks;
        public List<Runtime> runtime = new List<Runtime>();
        public CodeBlock[] argsList;
        public Function stepUpContext;
        public object switchValue;
        public CodeBlock isBaseMemoryOf;
        public Function externalContext
        {
            get
            {
                Function result = this;
                while (result != (result = result.stepUpContext)) ;
                return result;
            }
        }

        public int ExternalContextNumber { get
            {
                Function current = this;
                int n = 0;
                while (current != current.externalContext)
                {
                    n++;
                    current = current.externalContext;
                }
                return n;
            }}

        public Function (List<CodeBlock> blocks = null)
        {
            stepUpContext = this;
            if (blocks == null)
                blocks = new List<CodeBlock>();
            this.blocks = blocks;
        }
        public Function (Function stepUpContext, List<CodeBlock> blocks = null) : this(blocks)
        {
            if (stepUpContext == null)
                stepUpContext = this;
            this.stepUpContext = stepUpContext;
        }
        public object Run (Dictionary<string, CodeBlock> args, CodeBlock[] argsList)
        {
            int index = externalContext.runtime.Count - ExternalContextNumber;
            externalContext.runtime.Add(new Runtime());
            foreach (var item in args)
                externalContext.runtime[index].localVars_runScope.Add(item.Key, item.Value.Run(index));
            this.argsList = argsList;
            foreach (var v in blocks)
            {
                var returned = v.Run(index);
                if (returned != null && returned is ReturnBlock)
                    return (returned as ReturnBlock).value;
            }
            return null;
        }

        public object RunWithoutParams ()
        {
            return Run(new Dictionary<string, CodeBlock>(), new CodeBlock[] { });
        }

        public Function (BinaryReader reader, Program context, Function stepUpFunction = null)
        {
            if (stepUpFunction == null)
                stepUpFunction = this;
            stepUpContext = stepUpFunction;
            blocks = new List<CodeBlock>();
            var blocksLength = reader.ReadInt32();
            for (int n = 0; n < blocksLength; n++)
            {
                blocks.Add(new CodeBlock(reader, context, this));
            }
        }

        public string Decompile () => string.Join(" ", blocks.ConvertAll(v => v.Decompile()));

        public Function (Stream stream, Program context, Function stepUpFunction = null) : this (new BinaryReader(stream), context, stepUpFunction) { }

        public void Save(BinaryWriter writer)
        {
            writer.Write(blocks.Count);   
            foreach (var item in blocks)
                item.Save(writer);
        }
    }
}