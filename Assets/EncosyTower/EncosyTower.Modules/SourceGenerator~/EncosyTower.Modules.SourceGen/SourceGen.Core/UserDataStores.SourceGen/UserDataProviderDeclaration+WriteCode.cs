using System;
using System.Collections.Generic;
using System.Linq;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.Modules.UserDataStores.SourceGen
{
    partial class UserDataProviderDeclaration
    {
        public string WriteCode()
        {
            var accessDefs = AccessDefs;
            var storageDefs = new HashSet<StorageDefinition>();

            foreach (var accessDef in accessDefs)
            {
                foreach (var arg in accessDef.Args)
                {
                    if (arg.StorageDef.IsValid)
                    {
                        storageDefs.Add(arg.StorageDef);
                    }
                }
            }

            var orderedStorageDefs = storageDefs
                .OrderBy(x => x.DataType.Name)
                .ToArray()
                .AsSpan();

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            var staticKeyword = Symbol.IsStatic ? "static " : "";
            var fieldPrefix = Symbol.IsStatic ? "s_" : "_";

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").PrintEndLine(Syntax.Identifier.Text);
            p.OpenScope();
            {
                WriteFields(ref p, staticKeyword, fieldPrefix);
                WriteProperties(ref p, staticKeyword, accessDefs);
                WriteInitOnDomainReload(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteInitializeUserDataAccess(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteDataStorageClass(ref p, orderedStorageDefs);
                WriteCollectionsClass(ref p, orderedStorageDefs);
                WriteDataInspector(ref p, orderedStorageDefs);

            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFields(ref Printer p, string staticKeyword, string fieldPrefix)
        {
            p.PrintBeginLine("private ").Print(staticKeyword).Print("DataStorage ")
                .Print(fieldPrefix).PrintEndLine("storage;");
            p.PrintEndLine();
        }

        private void WriteProperties(ref Printer p, string staticKeyword, List<UserDataAccessDefinition> defs)
        {
            foreach (var def in defs)
            {
                var typeName = def.Symbol.ToFullName();
                var fieldName = def.FieldName;

                p.PrintBeginLine("public ").Print(staticKeyword).Print(typeName).Print(" ")
                    .Print(fieldName).PrintEndLine(" { get; private set; }")
                    .PrintEndLine();
            }
        }

        private void WriteInitOnDomainReload(
              ref Printer p
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            p.Print("#if UNITY_EDITOR").PrintEndLine();
            {
                p.PrintBeginLine("internal ").Print(staticKeyword).PrintEndLine("void InitOnDomainReload()");
                p.OpenScope();
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage = default;");
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.FieldName).PrintEndLine(" = null;");
                    }
                }
                p.CloseScope();
            }
            p.Print("#endif").PrintEndLine().PrintEndLine();
        }

        private static void WriteInitializeUserDataAccess(
              ref Printer p
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            var typeSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<UserDataAccessDefinition>(defs.Count);
            var loopMap = new Dictionary<ITypeSymbol, int>(defs.Count, SymbolEqualityComparer.Default);

            foreach (var def in defs)
            {
                queue.Enqueue(def);
            }

            p.PrintBeginLine("private ").Print(staticKeyword).PrintEndLine("void InitializeUserDataAccess()");
            p.OpenScope();
            {
                var loopBreakCondition = defs.Count;

                while (queue.Count > 0)
                {
                    var def = queue.Dequeue();
                    var needDependency = false;

                    foreach (var arg in def.Args)
                    {
                        if (arg.Type != null && typeSet.Contains(arg.Type) == false)
                        {
                            needDependency = true;
                            break;
                        }
                    }

                    if (needDependency)
                    {
                        if (loopMap.TryGetValue(def.Symbol, out var loop))
                        {
                            loop += 1;
                        }

                        if (loop == loopBreakCondition)
                        {
                            break;
                        }

                        loopMap[def.Symbol] = loop;
                        queue.Enqueue(def);
                        continue;
                    }

                    typeSet.Add(def.Symbol);
                    loopMap.Remove(def.Symbol);
                    Write(ref p, fieldPrefix, def);
                }

                foreach (var kv in loopMap)
                {
                    var type = kv.Key;

                    p.PrintBeginLine("global::EncosyTower.Modules.Logging.DevLogger.LogError(")
                        .Print("\"Detect cycling dependency in the constructor of type \\\"")
                        .Print(type.Name)
                        .PrintEndLine("\\\"\");");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            static void Write(ref Printer p, string fieldPrefix, UserDataAccessDefinition def)
            {
                if (def.Args.Count < 2)
                {
                    p.PrintBeginLine(def.FieldName).Print(" = new(");

                    foreach (var arg in def.Args)
                    {
                        if (arg.Type != null)
                        {
                            p.Print(arg.Type.Name);
                        }
                        else
                        {
                            p.Print(fieldPrefix).Print("storage.").Print(arg.StorageDef.DataType.Name);
                        }
                    }

                    p.PrintEndLine(");").PrintEndLine();
                }
                else
                {
                    p.PrintBeginLine(def.Symbol.Name).PrintEndLine(" = new(");
                    p = p.IncreasedIndent();
                    {
                        var args = def.Args;

                        for (var i = 0; i < args.Count; i++)
                        {
                            var arg = args[i];
                            var comma = i == 0 ? " " : ",";

                            p.PrintBeginLine(comma).Print(" ");

                            if (arg.Type != null)
                            {
                                p.Print(arg.Type.Name);
                            }
                            else
                            {
                                p.Print(fieldPrefix).Print("storage.").Print(arg.StorageDef.DataType.Name);
                            }

                            p.PrintEndLine();
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");").PrintEndLine();
                }
            }
        }

        private void WriteDataStorageClass(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintLine("partial class DataStorage : global::System.IDisposable");
            p.OpenScope();
            {
                p.PrintLine("private readonly global::EncosyTower.Modules.Encryption.RijndaelEncryption _encryption;");
                p.PrintEndLine();

                p.PrintLine("public DataStorage(string secret, byte[] iv)");
                p.OpenScope();
                {
                    p.PrintLine("var key = global::System.Convert.FromBase64String(secret);");
                    p.PrintLine("_encryption = new(key, iv);");
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).Print(" = new(")
                            .Print("Collections.").Print(StringUtils.ToSnakeCase(def.DataType.Name).ToUpperInvariant())
                            .PrintEndLine(", _encryption);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void Dispose()");
                p.OpenScope();
                {
                    p.PrintLine("_encryption.Dispose();");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    p.PrintBeginLine("public global::EncosyTower.Modules.UserDataStores.UserDataStorage<")
                        .Print(def.DataType.ToFullName()).Print("> ")
                        .Print(def.DataType.Name).PrintEndLine(" { get; }")
                        .PrintEndLine();
                }

                p.PrintLine("public void Initialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name)
                            .PrintEndLine(".Initialize();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void MarkDirty(bool isDirty)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".MarkDirty(isDirty);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public async global::Cysharp.Threading.Tasks.UniTask LoadFromDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("await ").Print(def.DataType.Name).PrintEndLine(".LoadFromDevice();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public async global::Cysharp.Threading.Tasks.UniTask LoadFromFirestore(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var def = defs[i];

                        if (i == 0)
                        {
                            p.PrintBeginLine("  ");
                        }
                        else
                        {
                            p.PrintBeginLine(", ");
                        }

                        p.Print("bool include").Print(def.DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine("if (include").Print(name).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".LoadFromFirestore();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public async global::Cysharp.Threading.Tasks.UniTask SaveToFirestoreAsync()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("await ").Print(def.DataType.Name).PrintEndLine(".SaveToFirestoreAsync();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void CreateData()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".CreateData();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void SaveToDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".SaveToDevice();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void Save(bool forceToFirestore)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".Save(forceToFirestore);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public async global::Cysharp.Threading.Tasks.UniTask SaveAsync(bool forceToFirestore)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("await ").Print(def.DataType.Name).PrintEndLine(".SaveAsync(forceToFirestore);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void DeepCloneDataFromFirestoreToDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".DeepCloneDataFromFirestoreToDevice();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void SetUserData(string userId, string version)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".SetUserData(userId, version);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteCollectionsClass(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintLine("public static class Collections");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    var name = StringUtils.ToSnakeCase(def.DataType.Name).ToUpperInvariant();

                    p.PrintBeginLine("public static readonly global::EncosyTower.Modules.NameKeys.NameKey<string> ").Print(name)
                        .Print(" = global::EncosyTower.Modules.NameKeys.NameToKey.Get<string>(nameof(").Print(name).PrintEndLine("));")
                        .PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteDataInspector(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            if (Symbol.IsStatic == false)
            {
                return;
            }

            p.Print("#if UNITY_EDITOR && ODIN_INSPECTOR").PrintEndLine();

            p.PrintLine("[global::System.Serializable]");
            p.PrintLine("[global::Sirenix.OdinInspector.InlineProperty]");
            p.PrintLine("partial class DataInspector");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    p.PrintLine("[global::UnityEngine.SerializeField]");
                    p.PrintBeginLine("public ").Print(def.DataType.ToFullName()).Print(" ")
                        .Print(def.DataType.Name).PrintEndLine(";");
                    p.PrintEndLine();
                }

                p.PrintLine("[global::Sirenix.OdinInspector.Button]");
                p.PrintLine("[global::Sirenix.OdinInspector.PropertyOrder(1000)]");
                p.PrintLine("private void FromDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine(name).Print(" = s_storage.")
                            .Print(name).PrintEndLine(".GetDataFromDevice();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("[Sirenix.OdinInspector.Button]");
                p.PrintLine("[Sirenix.OdinInspector.PropertyOrder(1001)]");
                p.PrintLine("private void FromFirestore()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine(name).Print(" = s_storage.")
                            .Print(name).PrintEndLine(".GetDataFromFirestore();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("private void SetToDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine("s_storage.").Print(name)
                            .Print(".SetToDevice(").Print(name).PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.Print("#endif").PrintEndLine().PrintEndLine();
        }
    }
}
