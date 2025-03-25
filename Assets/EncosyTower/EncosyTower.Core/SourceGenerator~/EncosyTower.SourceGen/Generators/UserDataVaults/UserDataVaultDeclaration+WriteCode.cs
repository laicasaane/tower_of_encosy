using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    partial class UserDataVaultDeclaration
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
            p.Print("#if UNITASK").PrintEndLine();
            p.Print("    using UnityTask = global::Cysharp.Threading.Tasks.UniTask;").PrintEndLine();
            p.Print("#elif UNITY_6000_0_OR_NEWER").PrintEndLine();
            p.Print("    using UnityTask = global::UnityEngine.Awaitable;").PrintEndLine();
            p.Print("#else").PrintEndLine();
            p.Print("    using UnityTask = global::System.Threading.Tasks.ValueTask;").PrintEndLine();
            p.Print("#endif").PrintEndLine();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").PrintEndLine(Syntax.Identifier.Text);
            p.OpenScope();
            {
                WriteFields(ref p, staticKeyword, fieldPrefix);
                WriteProperties(ref p, staticKeyword, accessDefs);
                WriteInitialize(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteOnUserDataLoaded(ref p, staticKeyword, accessDefs);
                WriteInitOnDomainReload(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteDataStorageClass(ref p, orderedStorageDefs);
                WriteIdsClass(ref p, orderedStorageDefs);
                WriteDataInspector(ref p, orderedStorageDefs);

            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFields(ref Printer p, string staticKeyword, string fieldPrefix)
        {
            p.PrintLine(GENERATED_CODE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(staticKeyword).Print(typeName).Print(" ")
                    .Print(fieldName).PrintEndLine(" { get; private set; }")
                    .PrintEndLine();
            }
        }

        private static void WriteInitialize(
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

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void Initialize(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("Ids.Initialize();");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage = new(encryption, logger, taskArrayPool);");
                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.Initialize();");
                p.PrintEndLine();

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

                    p.PrintBeginLine(LOG_ERROR).Print("(")
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

                    p.PrintEndLine(");");
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
                    p.PrintLine(");");
                }
            }
        }

        private static void WriteOnUserDataLoaded(
              ref Printer p
            , string staticKeyword
            , List<UserDataAccessDefinition> defs
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void OnUserDataLoaded()");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable"))
                    {
                        p.PrintBeginLine(def.FieldName).PrintEndLine("?.Initialize();");
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
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
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                p.PrintBeginLine("internal ").Print(staticKeyword).PrintEndLine("void InitOnDomainReload()");
                p.OpenScope();
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage?.Dispose();");
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

        private void WriteDataStorageClass(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintLine("partial class DataStorage : global::System.IDisposable");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(ENCRYPTION_BASE).PrintEndLine(" _encryption;")
                    .PrintEndLine();

                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(ILOGGER).PrintEndLine(" _logger;")
                    .PrintEndLine();

                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(TASK_ARRAY_POOL).PrintEndLine(" _taskArrayPool;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public DataStorage(");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_encryption = encryption;");
                    p.PrintLine("_logger = logger;");
                    p.PrintLine("_taskArrayPool = taskArrayPool;");
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).Print(" = new(")
                            .Print("Ids.").Print(StringUtils.ToSnakeCase(def.DataType.Name).ToUpperInvariant())
                            .PrintEndLine(", encryption, logger);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void Dispose()");
                p.OpenScope();
                {
                    p.PrintLine("_encryption.Dispose();");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    p.PrintBeginLine("public ")
                        .Print(def.StorageType.ToFullName()).Print(" ")
                        .Print(def.DataType.Name).PrintEndLine(" { get; }")
                        .PrintEndLine();
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ").PrintEndLine("UnityTask LoadFromDeviceAsync()");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintLine($"var tasks = _taskArrayPool.Rent({defs.Length});");
                        p.PrintEndLine();

                        for (var i = 0; i < defs.Length; i++)
                        {
                            p.PrintBeginLine($"tasks[{i}] = ").Print(defs[i].DataType.Name).PrintEndLine(".LoadFromDeviceAsync();");
                        }

                        p.PrintEndLine();
                        p.PrintBeginLine("await ").Print(WHEN_ALL_TASKS).PrintEndLine(";");

                        p.PrintEndLine();
                        p.PrintLine("_taskArrayPool.Return(tasks, true);");
                    }
                    else if (defs.Length == 1)
                    {
                        p.PrintBeginLine("await ").Print(defs[0].DataType.Name).PrintEndLine(".LoadFromDeviceAsync();");
                    }
                    else
                    {
                        p.PrintBeginLine("return ").Print(COMPLETED_TASK).PrintEndLine(";");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ").PrintEndLine("UnityTask LoadFromCloudAsync(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        if (i == 0)
                        {
                            p.PrintBeginLine("  ");
                        }
                        else
                        {
                            p.PrintBeginLine(", ");
                        }

                        p.Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintLine($"var tasks = _taskArrayPool.Rent({defs.Length});");
                        p.PrintEndLine();

                        var lastIndex = defs.Length - 1;

                        for (var i = 0; i < defs.Length; i++)
                        {
                            var name = defs[i].DataType.Name;

                            p.PrintBeginLine($"tasks[{i}] = include").PrintEndLine(name);
                            p = p.IncreasedIndent();
                            {
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".LoadFromCloudAsync()");
                                p.PrintBeginLine(": ").Print(COMPLETED_TASK).PrintEndLine(";");
                            }
                            p = p.DecreasedIndent();

                            p.PrintEndLineIf(i < lastIndex, "");
                        }

                        p.PrintEndLine();
                        p.PrintBeginLine("await ").Print(WHEN_ALL_TASKS).PrintEndLine(";");

                        p.PrintEndLine();
                        p.PrintLine("_taskArrayPool.Return(tasks, true);");
                    }
                    else if (defs.Length == 1)
                    {
                        var name = defs[0].DataType.Name;

                        p.PrintBeginLine("if (include").Print(name).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".LoadFromCloudAsync();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                    else
                    {
                        p.PrintBeginLine("return ").Print(COMPLETED_TASK).PrintEndLine(";");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ").PrintEndLine("UnityTask SaveToCloudAsync()");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintLine($"var tasks = _taskArrayPool.Rent({defs.Length});");
                        p.PrintEndLine();

                        for (var i = 0; i < defs.Length; i++)
                        {
                            p.PrintBeginLine($"tasks[{i}] = ").Print(defs[i].DataType.Name).PrintEndLine(".SaveToCloudAsync();");
                        }

                        p.PrintEndLine();
                        p.PrintBeginLine("await ").Print(WHEN_ALL_TASKS).PrintEndLine(";");

                        p.PrintEndLine();
                        p.PrintLine("_taskArrayPool.Return(tasks, true);");
                    }
                    else if (defs.Length == 1)
                    {
                        p.PrintBeginLine("await ").Print(defs[0].DataType.Name).PrintEndLine(".SaveToCloudAsync();");
                    }
                    else
                    {
                        p.PrintBeginLine("return ").Print(COMPLETED_TASK).PrintEndLine(";");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void Save(bool forceToCloud)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".Save(forceToCloud);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ").PrintEndLine("UnityTask SaveAsync(bool forceToCloud)");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintLine($"var tasks = _taskArrayPool.Rent({defs.Length});");
                        p.PrintEndLine();

                        for (var i = 0; i < defs.Length; i++)
                        {
                            p.PrintBeginLine($"tasks[{i}] = ").Print(defs[i].DataType.Name).PrintEndLine(".SaveAsync(forceToCloud);");
                        }

                        p.PrintEndLine();
                        p.PrintBeginLine("await ").Print(WHEN_ALL_TASKS).PrintEndLine(";");

                        p.PrintEndLine();
                        p.PrintLine("_taskArrayPool.Return(tasks, true);");
                    }
                    else if (defs.Length == 1)
                    {
                        p.PrintBeginLine("await ").Print(defs[0].DataType.Name).PrintEndLine(".SaveAsync(forceToCloud);");
                    }
                    else
                    {
                        p.PrintBeginLine("return ").Print(COMPLETED_TASK).PrintEndLine(";");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void DeepCloneDataFromCloudToDevice()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name).PrintEndLine(".DeepCloneDataFromCloudToDevice();");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

        private static void WriteIdsClass(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static class Ids");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    var name = StringUtils.ToSnakeCase(def.DataType.Name);
                    var nameUpper = name.ToUpperInvariant();

                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("private static ").Print(STRING_ID).Print(" s_").Print(name).PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static ").Print(STRING_ID).Print(" ").PrintEndLine(nameUpper);
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => s_").Print(name).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("internal static void Initialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = StringUtils.ToSnakeCase(def.DataType.Name);
                        var nameUpper = name.ToUpperInvariant();

                        p.PrintBeginLine("s_").Print(name).Print(" = ")
                            .Print(STRING_ID_MAKE).Print("(nameof(").Print(nameUpper).PrintEndLine("));");
                    }
                }
                p.CloseScope();
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

                p.PrintLine("[global::Sirenix.OdinInspector.Button]");
                p.PrintLine("[global::Sirenix.OdinInspector.PropertyOrder(1001)]");
                p.PrintLine("private void FromCloud()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine(name).Print(" = s_storage.")
                            .Print(name).PrintEndLine(".GetDataFromCloud();");
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
