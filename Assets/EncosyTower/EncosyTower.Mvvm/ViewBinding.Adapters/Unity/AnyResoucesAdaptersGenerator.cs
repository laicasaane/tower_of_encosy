#if UNITY_EDITOR && ANNULUS_CODEGEN && ENCOSY_MVVM_ADAPTERS_GENERATOR

using System;
using EncosyTower.CodeGen;
using UnityCodeGen;

namespace EncosyTower.Editor.Mvvm.ViewBinding.Adapters.Unity
{
    [Generator]
    internal class AnyResoucesAdaptersGenerator : ICodeGenerator
    {
        public void Execute(GeneratorContext context)
        {
            var nameofGenerator = nameof(AnyResoucesAdaptersGenerator);

            if (CodeGenAPI.TryGetOutputFolderPath(nameofGenerator, out var outputPath) == false)
            {
                context.OverrideFolderPath("Assets");
                return;
            }

            var p = Printer.DefaultLarge;
            p.PrintAutoGeneratedBlock(nameofGenerator);
            p.PrintEndLine();
            p.PrintLine(@"#pragma warning disable

using System;
using EncosyTower.Annotations;
using EncosyTower.Unions.Converters;
");

            p.PrintEndLine();
            p.PrintLine("namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity");
            p.OpenScope();
            {
                var unityTypes = MvvmCodeGenAPI.UnityTypes.AsSpan();

                for (var i = 0; i < unityTypes.Length; i++)
                {
                    var type = unityTypes[i];
                    var name = type.Name;
                    var typeName = type.FullName;

                    p.PrintLine($"[Serializable]");
                    p.PrintLine($"[Label(\"Resources.Load<{typeName}>(string)\", \"Default\")]");
                    p.PrintLine($"[Adapter(sourceType: typeof(string), destType: typeof({typeName}), order: 0)]");
                    p.PrintLine($"public sealed class {name}ResourcesAdapter : ResourcesAdapter<{typeName}>");
                    p.OpenScope();
                    {
                        p.PrintBeginLine($"public ")
                            .Print(name)
                            .Print("ResourcesAdapter() : base(CachedUnionConverter<")
                            .Print(typeName)
                            .PrintEndLine(">.Default) { }");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            context.OverrideFolderPath(outputPath);
            context.AddCode($"AnyResoucesAdapters.gen.cs", p.Result);
        }
    }
}

#endif
