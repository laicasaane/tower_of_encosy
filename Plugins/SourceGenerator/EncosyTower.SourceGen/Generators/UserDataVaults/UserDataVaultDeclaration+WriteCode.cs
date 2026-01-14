using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

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

            var isStatic = Symbol.IsStatic;
            var orderedStorageDefs = storageDefs
                .OrderBy(x => x.DataType.Name)
                .ToArray()
                .AsSpan();

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            var staticKeyword = isStatic ? "static " : "";
            var fieldPrefix = isStatic ? "s_" : "_";

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintIf(isStatic == false, " : IDisposable, IDeinitializable")
                .PrintEndLine();
            p.OpenScope();
            {
                WriteFields(ref p, isStatic, staticKeyword, fieldPrefix);
                WriteConstructor(ref p, isStatic, Syntax.Identifier.Text, accessDefs);
                WriteProperties(ref p, isStatic, staticKeyword, accessDefs);
                WriteInitializeAsync(ref p, isStatic, staticKeyword, fieldPrefix, accessDefs);
                WriteTryLoadUserDataAsync(ref p, isStatic, staticKeyword, fieldPrefix, accessDefs);
                WriteDeinitialize(ref p, isStatic, staticKeyword, fieldPrefix, accessDefs);
                WriteSave(ref p, staticKeyword, fieldPrefix);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region IDS").PrintEndLine();
            p.Print("#endregion").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // Ids");
            p.OpenScope();
            {
                WriteIds(ref p, isStatic, orderedStorageDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region DATA STORAGE").PrintEndLine();
            p.Print("#endregion =========").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // DataStorage");
            p.OpenScope();
            {
                WriteDataStorageClass(ref p, isStatic, orderedStorageDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region COLLECTION").PrintEndLine();
            p.Print("#endregion =======").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // Collection");
            p.OpenScope();
            {
                WriteCollection(ref p, orderedStorageDefs);
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
                WriteHelpers(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFields(ref Printer p, bool isStatic, string staticKeyword, string fieldPrefix)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("private ").Print(staticKeyword)
                .PrintIf(isStatic == false, "readonly ")
                .Print("DataStorage ").Print(fieldPrefix).PrintEndLine("storage;");
            p.PrintEndLine();

            if (isStatic == false)
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ")
                    .Print("Ids ").Print(fieldPrefix).PrintEndLine("ids;");
                p.PrintEndLine();
            }

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("private ").Print(staticKeyword)
                .Print("bool ").Print(fieldPrefix).PrintEndLine("isSavedOnLoaded;");
            p.PrintEndLine();
        }

        private static void WriteConstructor(
              ref Printer p
            , bool isStatic
            , string className
            , List<UserDataAccessDefinition> defs
        )
        {
            if (isStatic)
            {
                return;
            }

            var typeSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<UserDataAccessDefinition>(defs.Count);
            var loopMap = new Dictionary<ITypeSymbol, int>(defs.Count, SymbolEqualityComparer.Default);

            foreach (var def in defs)
            {
                queue.Enqueue(def);
            }

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(className).PrintEndLine("(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").PrintEndLine("StringVault stringVault");
                p.PrintLine(", string userId");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("_ids = new(stringVault);");
                p.PrintEndLine();

                p.PrintLine("OnBeginConstructor(encryption, logger, taskArrayPool, userId);");
                p.PrintEndLine();

                p.PrintLine("_storage = new(encryption, logger, taskArrayPool, _ids, userId);");

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
                    Write(ref p, def);
                }

                p.PrintEndLine();

                foreach (var kv in loopMap)
                {
                    p.PrintBeginLine("LogErrorCyclicDependency(logger, \"").Print(kv.Key.Name).PrintEndLine("\");");
                }

                if (loopMap.Count > 0)
                {
                    p.PrintEndLine();
                }

                p.PrintLine("OnFinishConstructor(encryption, logger, taskArrayPool, userId);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("partial void OnBeginConstructor(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", string userId");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            p.PrintLine("partial void OnFinishConstructor(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", string userId");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            return;

            static void Write(ref Printer p, UserDataAccessDefinition def)
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
                            p.Print("_storage.").Print(arg.StorageDef.DataType.Name);
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
                                p.Print("_storage.").Print(arg.StorageDef.DataType.Name);
                            }

                            p.PrintEndLine();
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
            }
        }

        private void WriteProperties(ref Printer p, bool isStatic, string staticKeyword, List<UserDataAccessDefinition> defs)
        {
            foreach (var def in defs)
            {
                var typeName = def.Symbol.ToFullName();
                var fieldName = def.FieldName;

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(staticKeyword).Print(typeName).Print(" ")
                    .Print(fieldName).Print(" { get; ").PrintIf(isStatic, "private set; ").PrintEndLine("}")
                    .PrintEndLine();
            }
        }

        private static void WriteInitializeAsync(
              ref Printer p
            , bool isStatic
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            if (isStatic == false)
            {
                return;
            }

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
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").PrintEndLine("StringVault stringVault");
                p.PrintLine(", string userId");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token = default");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("Ids.Initialize(stringVault);");
                p.PrintEndLine();

                p.PrintLine("UnityTask beginTask = UnityTasks.GetCompleted();");
                p.PrintLine("OnBeginInitializeAsync(encryption, logger, taskArrayPool, userId, token, ref beginTask);");
                p.PrintLine("await beginTask;");
                p.PrintEndLine();

                p.PrintLine("if (token.IsCancellationRequested) return;");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage ??= new(encryption, logger, taskArrayPool, userId);");

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
                    p.PrintBeginLine("LogErrorCyclicDependency(logger, \"").Print(kv.Key.Name).PrintEndLine("\");");
                }

                if (loopMap.Count > 0)
                {
                    p.PrintEndLine();
                }

                p.PrintLine("await TryLoadUserDataAsync(logger, userId, loadFromCloud, token);");
                p.PrintEndLine();

                p.PrintLine("if (token.IsCancellationRequested) return;");
                p.PrintEndLine();

                p.PrintLine("UnityTask finishTask = UnityTasks.GetCompleted();");
                p.PrintLine("OnFinishInitializeAsync(encryption, logger, taskArrayPool, userId, token, ref finishTask);");
                p.PrintLine("await finishTask;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnBeginInitializeAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", string userId");
                p.PrintLine(", CancellationToken token");
                p.PrintLine(", ref UnityTask returnTask");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnFinishInitializeAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                p.PrintLine(", string userId");
                p.PrintLine(", CancellationToken token");
                p.PrintLine(", ref UnityTask returnTask");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            return;

            static void Write(ref Printer p, string fieldPrefix, UserDataAccessDefinition def)
            {
                if (def.Args.Count < 2)
                {
                    p.PrintBeginLine(def.FieldName).Print(" ??= new(");

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
                    p.PrintBeginLine(def.Symbol.Name).PrintEndLine(" ??= new(");
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

        private static void WriteTryLoadUserDataAsync(
              ref Printer p
            , bool isStatic
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("async UnityTaskᐸboolᐳ TryLoadUserDataAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintLine(", string userId");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token = default");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("var result = false;");
                p.PrintEndLine();

                if (isStatic)
                {
                    p.PrintBeginLine("if (").Print(fieldPrefix).PrintEndLine("storage == null)");
                    p.OpenScope();
                    {
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("UnityTask beginTask = UnityTasks.GetCompleted();");
                p.PrintLine("OnBeginTryLoadUserDataAsync(logger, userId, loadFromCloud, token, ref beginTask);");
                p.PrintLine("await beginTask;");
                p.PrintEndLine();

                p.PrintLine("if (token.IsCancellationRequested) return result;");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.UserId = userId;");
                p.PrintEndLine();

                p.PrintLine("if (string.IsNullOrEmpty(userId))");
                p.OpenScope();
                {
                    p.PrintLine("result = false;");
                    p.PrintLine("LogWarningInvalidUserId(logger);");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.Initialize();");
                    p.PrintEndLine();

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

                    p.PrintLine("if (token.IsCancellationRequested == false)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(fieldPrefix).PrintEndLine("storage.CreateDataIfNotExist();");
                        p.PrintEndLine();

                        var initCount = 0;

                        foreach (var def in defs)
                        {
                            if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable"))
                            {
                                p.PrintBeginLine(def.FieldName).PrintIf(isStatic, "?").PrintEndLine(".Initialize();");
                                initCount += 1;
                            }
                        }

                        if (initCount > 0)
                        {
                            p.PrintEndLine();
                        }

                        p.PrintLine("await SaveAsync(token: token);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = token.IsCancellationRequested == false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (token.IsCancellationRequested == false)");
                p.OpenScope();
                {
                    p.PrintLine("UnityTask finishTask = UnityTasks.GetCompleted();");
                    p.PrintLine("OnFinishTryLoadUserDataAsync(logger, userId, loadFromCloud, token, ref finishTask);");
                    p.PrintLine("await finishTask;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnBeginTryLoadUserDataAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintLine(", string userId");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token");
                p.PrintLine(", ref UnityTask returnTask");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnFinishTryLoadUserDataAsync(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                p.PrintLine(", string userId");
                p.PrintLine(", bool loadFromCloud");
                p.PrintLine(", CancellationToken token");
                p.PrintLine(", ref UnityTask returnTask");
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
            p.PrintEndLine();
        }

        private void WriteDeinitialize(
              ref Printer p
            , bool isStatic
            , string staticKeyword
            , string fieldPrefix
            , List<UserDataAccessDefinition> defs
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void Deinitialize()");
            p.OpenScope();
            {
                p.PrintLine("OnBeginDeinitialize();");
                p.PrintEndLine();

                p.PrintBeginLine(fieldPrefix).PrintEndLine("isSavedOnLoaded = false;");
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable"))
                    {
                        p.PrintBeginLine(def.FieldName).PrintIf(isStatic, "?").PrintEndLine(".Deinitialize();");
                    }

                    if (isStatic)
                    {
                        p.PrintBeginLine(def.FieldName).PrintEndLine(" = null;");
                    }

                    p.PrintEndLine();
                }

                p.PrintBeginLine(fieldPrefix).Print("storage").PrintIf(isStatic, "?").PrintEndLine(".Dispose();");

                if (isStatic)
                {
                    p.PrintBeginLine(fieldPrefix).PrintEndLine("storage = null;");
                }

                p.PrintEndLine();

                p.PrintLine("OnFinishDeinitialize();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnBeginDeinitialize();");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).PrintEndLine("partial void OnFinishDeinitialize();");
            p.PrintEndLine();

            if (isStatic)
            {
                return;
            }

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public void Dispose()");
            p.WithIncreasedIndent().PrintLine("=> Deinitialize();");
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
                p.PrintLine("OnBeginSave(forceToCloud, token);");
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

                p.PrintLine("OnFinishSave(forceToCloud, token);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnBeginSave(bool forceToCloud, CancellationToken token);");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnFinishSave(bool forceToCloud, CancellationToken token);");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(staticKeyword)
                .PrintEndLine("async UnityTask SaveAsync(bool forceToCloud = false, CancellationToken token = default)");
            p.OpenScope();
            {
                p.PrintLine("UnityTask beginTask = UnityTasks.GetCompleted();");
                p.PrintLine("OnBeginSaveAsync(forceToCloud, token, ref beginTask);");
                p.PrintLine("await beginTask;");
                p.PrintEndLine();

                p.PrintLine("if (token.IsCancellationRequested) return;");
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

                p.PrintLine("if (token.IsCancellationRequested) return;");
                p.PrintEndLine();

                p.PrintLine("UnityTask finishTask = UnityTasks.GetCompleted();");
                p.PrintLine("OnFinishSaveAsync(forceToCloud, token, ref finishTask);");
                p.PrintLine("await finishTask;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnBeginSaveAsync(bool forceToCloud, CancellationToken token, ref UnityTask returnTask);");
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword)
                .PrintEndLine("partial void OnFinishSaveAsync(bool forceToCloud, CancellationToken token, ref UnityTask returnTask);");
            p.PrintEndLine();
        }

        private static void WriteIds(ref Printer p, bool isStatic, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine().PrintIf(isStatic, "static ", "readonly ").Print("partial ")
                .PrintIf(isStatic, "class", "struct").PrintEndLine(" Ids");
            p.OpenScope();
            {
                p.PrintBeginLineIf(
                      isStatic
                    , "internal static void Initialize"
                    , "public Ids"
                ).PrintEndLine("([NotNull] StringVault stringVault)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine(name)
                            .Print(" = stringVault.GetOrMakeId(nameof(").Print(name).PrintEndLine("));");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    p.PrintBeginLine("public ").PrintIf(isStatic, "static ")
                        .Print(STRING_ID).Print(" ").Print(def.DataType.Name)
                        .Print(" { get; ").PrintIf(isStatic, "private set; ").PrintEndLine("}");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteDataStorageClass(ref Printer p, bool isStatic, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial class DataStorage : IDisposable, IInitializable");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ").Print(ENCRYPTION_BASE).PrintEndLine(" _encryption;");
                p.PrintBeginLine("private readonly ").Print(ILOGGER).PrintEndLine(" _logger;");
                p.PrintBeginLine("private readonly ").Print(TASK_ARRAY_POOL).PrintEndLine(" _taskArrayPool;");

                if (isStatic == false)
                {
                    p.PrintLine("private readonly Ids _ids;");
                }

                p.PrintEndLine();

                p.PrintLine("private string _userId;");
                p.PrintEndLine();

                p.PrintLine("public DataStorage(");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");

                    if (isStatic == false)
                    {
                        p.PrintBeginLine(", ").Print(NOT_NULL).PrintEndLine(" Ids ids");
                    }

                    p.PrintLine(", string userId");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_encryption = encryption;");
                    p.PrintLine("_logger = logger;");
                    p.PrintLine("_taskArrayPool = taskArrayPool;");
                    p.PrintLineIf(isStatic == false, "_ids = ids;");
                    p.PrintLine("_userId = userId;");
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
                                .PrintIf(isStatic, "Ids.", "_ids.").Print(def.DataType.Name)
                                .Print(", encryption, logger, ignoreEncryption, storageArgs")
                                .PrintEndLine(") { UserId = userId };");
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

                p.PrintLine("public string UserId");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        p.PrintLine("return _userId;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("set");
                    p.OpenScope();
                    {
                        p.PrintLine("_userId = value;");
                        p.PrintEndLine();

                        foreach (var def in defs)
                        {
                            p.PrintBeginLine(def.DataType.Name)
                                .PrintEndLine(".UserId = value;");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("static partial void GetIgnoreEncryption(ref bool ignoreEncryption);");
                p.PrintEndLine();

                p.PrintBeginLine("static partial void GetArgsForStorage<TData, TStorage>(ref ")
                    .Print(STORAGE_ARGS).PrintEndLine(" storageArgs)");
                p.WithIncreasedIndent().PrintLine("where TData : IUserData");
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

                p.PrintLine("public void CloneDataFromCloudToDevice(");
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
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".CloneDataFromCloudToDevice();");
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

        private void WriteCollection(ref Printer p, ReadOnlySpan<StorageDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("[global::System.Serializable]");
            p.PrintLine("partial class Collection : IReadOnlyListᐸIUserDataᐳ, ICopyToSpanᐸIUserDataᐳ, ITryCopyToSpanᐸIUserDataᐳ");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    p.PrintLine("[global::UnityEngine.SerializeField]");
                    p.PrintBeginLine("public ").Print(def.DataType.ToFullName()).Print(" ")
                        .Print(def.DataType.Name).PrintEndLine(";");
                    p.PrintEndLine();
                }

                p.PrintLine("public IUserData this[int index] => index switch");
                p.OpenScope();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine().Print(i).Print(" => ").Print(defs[i].DataType.Name).PrintEndLine(",");
                    }

                    p.PrintLine("_ => throw ThrowHelper.CreateIndexOutOfRangeException_Collection()");

                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine("public int Count");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(defs.Length).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public string UserId { get; set; }");
                p.PrintEndLine();

                p.PrintLine("public bool IsCloud { get; set; }");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Enumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> new Enumerator(this);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumeratorᐸIUserDataᐳ IEnumerableᐸIUserDataᐳ.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumerator IEnumerable.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataᐳ destination, int length)");
                p.OpenScope();
                {
                    p.PrintLine("var count = Count - sourceStartIndex;");
                    p.PrintEndLine();

                    p.PrintLine("if (length < 0)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentOutOfRangeException_LengthNegative();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (count < length)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentException_SourceStartIndex_Length();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (destination.Length < length)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentException_DestinationTooShort();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("destination = destination[..length];");
                    p.PrintEndLine();

                    p.PrintLine("for (int i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("destination[i] = this[sourceStartIndex + i];");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataᐳ destination, int length)");
                p.OpenScope();
                {
                    p.PrintLine("var count = Count - sourceStartIndex;");
                    p.PrintEndLine();

                    p.PrintLine("if (length < 0 || count < length || destination.Length < length)");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("destination = destination[..length];");
                    p.PrintEndLine();

                    p.PrintLine("for (int i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("destination[i] = this[sourceStartIndex + i];");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void CloneDataFrom([NotNull] Collection source)");
                p.OpenScope();
                {
                    p.PrintLine("DataStorage sourceStorage = null;");
                    p.PrintLine("source.GetLoadedStorage(ref sourceStorage);");
                    p.PrintEndLine();

                    p.PrintLine("if (sourceStorage == null)");
                    p.OpenScope();
                    {
                        p.PrintLine("return;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine("if (sourceStorage.").Print(name).Print(".TryCloneData(out var cloned")
                            .Print(name).PrintEndLine("))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine(name).Print(" = cloned").Print(name).PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void RetrieveFromStorage()");
                p.OpenScope();
                {
                    p.PrintLine("DataStorage storage = null;");
                    p.PrintLine("GetLoadedStorage(ref storage);");
                    p.PrintEndLine();

                    p.PrintLine("if (storage == null)");
                    p.OpenScope();
                    {
                        p.PrintLine("return;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (IsCloud)");
                    p.OpenScope();
                    {
                        foreach (var def in defs)
                        {
                            var name = def.DataType.Name;

                            p.PrintBeginLine(name).Print(" = storage.")
                                .Print(name).PrintEndLine(".GetDataFromCloud();");
                        }
                    }
                    p.CloseScope();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        foreach (var def in defs)
                        {
                            var name = def.DataType.Name;

                            p.PrintBeginLine(name).Print(" = storage.")
                                .Print(name).PrintEndLine(".GetDataFromDevice();");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void ApplyToStorage()");
                p.OpenScope();
                {
                    p.PrintLine("DataStorage storage = null;");
                    p.PrintLine("GetLoadedStorage(ref storage);");
                    p.PrintEndLine();

                    p.PrintLine("if (storage == null)");
                    p.OpenScope();
                    {
                        p.PrintLine("return;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine("storage.").Print(name)
                            .Print(".SetData(").Print(name).PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("partial void GetLoadedStorage(ref DataStorage storage);");
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public struct Enumerator : IEnumeratorᐸIUserDataᐳ");
                p.OpenScope();
                {
                    p.PrintLine("private readonly Collection _collection;");
                    p.PrintLine("private int _index;");
                    p.PrintEndLine();

                    p.PrintLine("public Enumerator([NotNull] Collection collection)");
                    p.OpenScope();
                    {
                        p.PrintLine("_collection = collection;");
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public readonly IUserData Current => _collection[_index];");
                    p.PrintEndLine();

                    p.PrintLine("readonly object IEnumerator.Current => Current;");
                    p.PrintEndLine();

                    p.PrintLine("public void Dispose() { }");
                    p.PrintEndLine();

                    p.PrintLine("public bool MoveNext()");
                    p.OpenScope();
                    {
                        p.PrintLine("_index++;");
                        p.PrintLine("return (uint)_index < (uint)_collection.Count;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public void Reset()");
                    p.OpenScope();
                    {
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
        }

        private static void WriteHelpers(ref Printer p)
        {
            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintLine("private static void LogErrorCyclicDependency(ILogger logger, string name)");
            p.OpenScope();
            {
                p.PrintBeginLine("logger.LogError(")
                    .PrintEndLine("$\"Detect cyclic dependency in the constructor of type '{name}'\");");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("private static void LogWarningInvalidUserId(ILogger logger)");
            p.OpenScope();
            {
                p.PrintBeginLine("logger.LogWarning(")
                    .PrintEndLine("\"User data cannot be loaded because 'userId' is invalid.\");");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
