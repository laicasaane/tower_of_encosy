using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Collections.LowLevel.ILSupport.CodeGen
{
    internal class CollectionsUnsafeUtilityPostProcessor : ILPostProcessor
    {
        private static CollectionsUnsafeUtilityPostProcessor s_instance;

        public override ILPostProcessor GetInstance()
        {
            return s_instance ??= new CollectionsUnsafeUtilityPostProcessor();
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly.Name == Constants.ASSEMBLY_NAME;
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var assemblyDefinition = AssemblyDefinitionFor(compiledAssembly);

            TypeDefinition ilSupportType = null;

            foreach (var t in assemblyDefinition.MainModule.Types)
            {
                if (t.FullName == Constants.CLASS_NAME)
                {
                    ilSupportType = t;
                    break;
                }
            }

            if (ilSupportType == null)
            {
                throw new InvalidOperationException(
                    $"Could not find type {Constants.CLASS_NAME} in assembly {Constants.ASSEMBLY_NAME}"
                );
            }

            Inject_AddressOf_In(ilSupportType);
            Inject_AsRef_In(ilSupportType);
            Inject_NullRef(ilSupportType);
            Inject_IsNullRef_In(ilSupportType);
            Inject_SkipInit_Out(ilSupportType);

            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            var writerParameters = new WriterParameters {
                SymbolWriterProvider = new PortablePdbWriterProvider(), SymbolStream = pdb, WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()));
        }

        private void Inject_AddressOf_In(TypeDefinition ctx)
        {
            MethodDefinition method = null;

            foreach (var m in ctx.Methods)
            {
                if (m.HasParameters && m.Parameters.Count > 0 && m.Parameters[0].IsIn && m.Name.Equals("AddressOf"))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Could not find method {Constants.CLASS_NAME}.AddressOf<T>(in T)"
                );
            }

            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ret));
        }

        private void Inject_AsRef_In(TypeDefinition ctx)
        {
            MethodDefinition method = null;

            foreach (var m in ctx.Methods)
            {
                if (m.HasParameters && m.Parameters.Count > 0 && m.Parameters[0].IsIn && m.Name.Equals("AsRef"))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Could not find method {Constants.CLASS_NAME}.AsRef<T>(in T)"
                );
            }

            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ret));
        }

        private void Inject_NullRef(TypeDefinition ctx)
        {
            MethodDefinition method = null;

            foreach (var m in ctx.Methods)
            {
                if (m.HasParameters == false && m.Name.Equals("NullRef"))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Could not find method {Constants.CLASS_NAME}.NullRef<T>()"
                );
            }

            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Conv_U));
            il.Append(il.Create(OpCodes.Ret));
        }

        private void Inject_IsNullRef_In(TypeDefinition ctx)
        {
            MethodDefinition method = null;

            foreach (var m in ctx.Methods)
            {
                if (m.HasParameters && m.Parameters.Count > 0 && m.Parameters[0].IsIn && m.Name.Equals("IsNullRef"))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Could not find method {Constants.CLASS_NAME}.IsNullRef<T>(in T)"
                );
            }

            var il = GetILProcessorForMethod(method);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Conv_U));
            il.Append(il.Create(OpCodes.Ceq));
            il.Append(il.Create(OpCodes.Ret));
        }

        private void Inject_SkipInit_Out(TypeDefinition ctx)
        {
            MethodDefinition method = null;

            foreach (var m in ctx.Methods)
            {
                if (m.HasParameters && m.Parameters.Count > 0 && m.Parameters[0].IsOut && m.Name.Equals("SkipInit"))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Could not find method {Constants.CLASS_NAME}.SkipInit<T>(out T)"
                );
            }

            var il = GetILProcessorForMethod(method);

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

