using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Collections.LowLevel.ILSupport.CodeGen
{
    internal class CollectionsUnsafeUtilityPostProcessor : ILPostProcessor
    {
        private static CollectionsUnsafeUtilityPostProcessor s_Instance;

        public override ILPostProcessor GetInstance()
        {
            if (s_Instance == null)
                s_Instance = new CollectionsUnsafeUtilityPostProcessor();
            return s_Instance;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            if (compiledAssembly.Name == "Encosy.Core.LowLevel.ILSupport")
                return true;
            return false;
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var assemblyDefinition = AssemblyDefinitionFor(compiledAssembly);

            TypeDefinition ilSupportType = null;
            foreach (var t in assemblyDefinition.MainModule.Types)
            {
                if (t.FullName == "Encosy.Core.LowLevel.Unsafe.ILSupport")
                {
                    ilSupportType = t;
                    break;
                }
            }
            if (ilSupportType == null)
                throw new InvalidOperationException();

            InjectUtilityAddressOfIn(ilSupportType);
            InjectUtilityAsRefIn(ilSupportType);

            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(), SymbolStream = pdb, WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()));
        }

        private void InjectUtilityAddressOfIn(TypeDefinition ctx)
        {
            MethodDefinition method = null;
            foreach (var m in ctx.Methods)
            {
                if (m.Parameters[0].IsIn && m.Name.Equals("AddressOf"))
                {
                    method = m;
                    break;
                }
            }
            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ret));
        }

        private void InjectUtilityAsRefIn(TypeDefinition ctx)
        {
            MethodDefinition method = null;
            foreach (var m in ctx.Methods)
            {
                if (m.Parameters[0].IsIn && m.Name.Equals("AsRef"))
                {
                    method = m;
                    break;
                }
            }
            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ret));
        }

        internal static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
        {
            var readerParameters = new ReaderParameters
            {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                ReadingMode = ReadingMode.Immediate
            };

            var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

            return assemblyDefinition;
        }

        private static ILProcessor GetILProcessorForMethod(TypeDefinition ctx, string methodName, bool clear = true)
        {
            MethodDefinition method = null;
            foreach (var m in ctx.Methods)
            {
                if (m.HasGenericParameters && m.Name.Equals(methodName))
                {
                    method = m;
                    break;
                }
            }

            return GetILProcessorForMethod(method, clear);
        }

        private static ILProcessor GetILProcessorForMethod(MethodDefinition method, bool clear = true)
        {
            var ilProcessor = method.Body.GetILProcessor();

            if (clear)
            {
                ilProcessor.Body.Instructions.Clear();
                ilProcessor.Body.Variables.Clear();
                ilProcessor.Body.ExceptionHandlers.Clear();
            }

            return ilProcessor;
        }
    }
}

