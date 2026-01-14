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
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").PrintEndLine(Syntax.Identifier.Text);
            p.OpenScope();
            {
                WriteFields(ref p, staticKeyword, fieldPrefix);
                WriteProperties(ref p, staticKeyword, accessDefs);
                WriteInitialize(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteDeinitialize(ref p, staticKeyword, fieldPrefix, accessDefs);
                WriteOnUserDataLoaded(ref p, staticKeyword, accessDefs);
                WriteSave(ref p, staticKeyword, fieldPrefix);
                WriteDataStorageClass(ref p, orderedStorageDefs);
                WriteIdsClass(ref p, orderedStorageDefs);
                WriteDataInspector(ref p, orderedStorageDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region INTERNALS").PrintEndLine();
            p.Print("#endregion ======").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // Internals");
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
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

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("private ").Print(staticKeyword).Print("bool ")
                .Print(fieldPrefix).PrintEndLine("isSavedOnLoaded;");
            p.PrintEndLine();
        }

        private void WriteProperties(ref Printer p, string staticKeyword, List<UserDataAccessDefinition> defs)
        {
            foreach (var def in defs)
            {
                var typeName = def.Symbol.ToFullName();
                var fieldName = def.FieldName;

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
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

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("async UnityTask InitializeAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token = default");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("Ids.Initialize();");
                p.PrintLine("OnBeginInitializing(encryption, logger, taskArrayPool, loadFromCloud, token);");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine(
                    "storage = new(encryption, logger, taskArrayPool);"
                );

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

                p.PrintEndLine();

                foreach (var kv in loopMap)
                {
                    var type = kv.Key;

                    p.PrintBeginLine("logger.LogError(")
                        .Print("\"Detect cycling dependency in the constructor of type \\\"")
                        .Print(type.Name)
                        .PrintEndLine("\\\"\");");
                    p.PrintEndLine();
                }

                p.PrintLine("if (loadFromCloud)");
                p.OpenScope();
                {
                    p.PrintBeginLine("await ").Print(fieldPrefix).PrintEndLine("storage.LoadFromCloudAsync(token: token);");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("await ").Print(fieldPrefix).PrintEndLine("storage.LoadFromDeviceAsync(token: token);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.CreateDataIfNotExist();");
                p.PrintLine("OnUserDataLoaded();");
                p.PrintLine("Save(token: token);");
                p.PrintEndLine();

                p.PrintLine("OnFinishInitializing(encryption, logger, taskArrayPool, loadFromCloud, token);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnBeginInitializing(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnFinishInitializing(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            return;

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

        private void WriteDeinitialize(
              ref Printer p
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void Deinitialize()");
            p.OpenScope();
            {
                p.PrintLine("OnBeginDeinitializing();");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("isSavedOnLoaded = false;");
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable"))
                    {
                        p.PrintBeginLine(def.FieldName).PrintEndLine("?.Deinitialize();");
                    }
                }

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage?.Dispose();");
                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage = default;");
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    p.PrintBeginLine(def.FieldName).PrintEndLine(" = null;");
                }

                p.PrintEndLine();
                p.PrintLine("OnFinishDeinitializing();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnBeginDeinitializing();");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnFinishDeinitializing();");
            p.PrintEndLine();
        }

        private void WriteSave(
              ref Printer p
            , string staticKeyword
            , string fieldPrefix
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword)
                .PrintEndLine("void Save(bool forceToCloud = false, CancellationToken token = default)");
            p.OpenScope();
            {
                p.PrintLine("OnBeginSaving(forceToCloud, token);");
                p.PrintEndLine();

                p.PrintBeginLine("if (").Print(fieldPrefix).PrintEndLine("isSavedOnLoaded == false)");
                p.OpenScope();
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("isSavedOnLoaded = true;");
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.MarkDirty(true);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.Save(forceToCloud, token: token);");
                p.PrintEndLine();

                p.PrintLine("OnFinishSaving(forceToCloud, token);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword)
                .PrintEndLine("async UnityTask SaveAsync(bool forceToCloud = false, CancellationToken token = default)");
            p.OpenScope();
            {
                p.PrintLine("OnBeginSaving(forceToCloud, token);");
                p.PrintEndLine();

                p.PrintBeginLine("if (").Print(fieldPrefix).PrintEndLine("isSavedOnLoaded == false)");
                p.OpenScope();
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("isSavedOnLoaded = true;");
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.MarkDirty(true);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("await ").Print(fieldPrefix).PrintEndLine("storage.SaveAsync(forceToCloud, token: token);");
                p.PrintEndLine();

                p.PrintLine("OnFinishSaving(forceToCloud, token);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnBeginSaving(bool forceToCloud, CancellationToken token);");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnFinishSaving(bool forceToCloud, CancellationToken token);");
            p.PrintEndLine();
        }

        private static void WriteOnUserDataLoaded(
              ref Printer p
            , string staticKeyword
            , List<UserDataAccessDefinition> defs
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private ").Print(staticKeyword).PrintEndLine("void OnUserDataLoaded()");
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

        private void WriteDataStorageClass(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial class DataStorage : global::System.IDisposable");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ").Print(ENCRYPTION_BASE).PrintEndLine(" _encryption;");
                p.PrintBeginLine("private readonly ").Print(ILOGGER).PrintEndLine(" _logger;");
                p.PrintBeginLine("private readonly ").Print(TASK_ARRAY_POOL).PrintEndLine(" _taskArrayPool;");

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

                    p.PrintLine("bool ignoreEncryption = false;");
                    p.PrintBeginLine(STORAGE_ARGS).PrintEndLine(" storageArgs = null;");
                    p.PrintEndLine();

                    p.Print("#if !FORCE_USER_DATA_ENCRYPTION").PrintEndLine();
                    p.PrintLine("GetIgnoreEncryption(ref ignoreEncryption);");
                    p.Print("#endif").PrintEndLine();
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        p.OpenScope();
                        {
                            p.PrintLine("storageArgs = null;");
                            p.PrintBeginLine("GetArgsForStorage<")
                                .Print(def.DataType.ToFullName()).Print(", ").Print(def.StorageType.ToFullName())
                                .PrintEndLine(">(ref storageArgs);");

                            p.PrintBeginLine(def.DataType.Name).Print(" = new(")
                                .Print("Ids.").Print(StringUtils.ToSnakeCase(def.DataType.Name).ToUpperInvariant())
                                .Print(", encryption, logger, ignoreEncryption, storageArgs")
                                .PrintEndLine(");");
                        }
                        p.CloseScope();
                    }
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

                p.PrintLine("static partial void GetIgnoreEncryption(ref bool ignoreEncryption);");
                p.PrintEndLine();

                p.PrintBeginLine("static partial void GetArgsForStorage<TData, TStorage>(ref ")
                    .Print(STORAGE_ARGS).PrintEndLine(" storageArgs)");
                p.WithIncreasedIndent().PrintBeginLine("where TData : ").PrintEndLine(IUSER_DATA);
                p.WithIncreasedIndent().PrintBeginLine("where TStorage : ").Print(USER_DATA_STORAGE_BASE).PrintEndLine("TData>;");
                p.PrintEndLine();

                p.PrintLine("public void Dispose()");
                p.OpenScope();
                {
                    p.PrintLine("_encryption.Dispose();");
                }
                p.CloseScope();
                p.PrintEndLine();

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

                p.PrintLine("public void CreateDataIfNotExist()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("if (").Print(def.DataType.Name).PrintEndLine(".IsDataValid == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".CreateData();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
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

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask LoadFromCloudAsync(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var comma = i == 0 ? "  " : ", ";

                        p.PrintBeginLine(comma).Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
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
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".LoadFromCloudAsync(token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".LoadFromCloudAsync(token);");
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

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask LoadFromDeviceAsync(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var comma = i == 0 ? "  " : ", ";

                        p.PrintBeginLine(comma).Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
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
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".LoadFromDeviceAsync(token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".LoadFromDeviceAsync(token);");
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

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask SaveToCloudAsync(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var comma = i == 0 ? "  " : ", ";

                        p.PrintBeginLine(comma).Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
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
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".SaveToCloudAsync(token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".SaveToCloudAsync(token);");
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

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask SaveToDeviceAsync(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var comma = i == 0 ? "  " : ", ";

                        p.PrintBeginLine(comma).Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
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
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".SaveToDeviceAsync(token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".SaveToDeviceAsync(token);");
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

                p.PrintLine("public void Save(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  bool forceToCloud");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine(", bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("if (include").Print(def.DataType.Name).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".Save(forceToCloud, token);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask SaveAsync(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  bool forceToCloud");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine(", bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }

                    p.PrintLine(", CancellationToken token = default");
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
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".SaveAsync(forceToCloud, token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".SaveAsync(forceToCloud, token);");
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

                p.PrintLine("public void DeepCloneDataFromCloudToDevice(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        var comma = i == 0 ? "  " : ", ";
                        p.PrintBeginLine(comma).Print("bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("if (include").Print(def.DataType.Name).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".DeepCloneDataFromCloudToDevice();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void SetUserData(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  string userId");
                    p.PrintLine(", string version");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine(", bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine("if (include").Print(def.DataType.Name).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".SetUserData(userId, version);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
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
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static class Ids");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    var name = StringUtils.ToSnakeCase(def.DataType.Name);
                    var nameUpper = name.ToUpperInvariant();

                    p.PrintBeginLine("private static ").Print(STRING_ID).Print(" s_").Print(name).PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("public static ").Print(STRING_ID).Print(" ").PrintEndLine(nameUpper);
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => s_").Print(name).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("internal static void Initialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = StringUtils.ToSnakeCase(def.DataType.Name);
                        var nameUpper = name.ToUpperInvariant();

                        p.PrintBeginLine("s_").Print(name).Print(" = ")
                            .Print(STRING_ID_GET).Print("(nameof(").Print(nameUpper).PrintEndLine("));");
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

        private static void WriteHelperConstants(ref Printer p)
        {
            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();
        }
    }
}
