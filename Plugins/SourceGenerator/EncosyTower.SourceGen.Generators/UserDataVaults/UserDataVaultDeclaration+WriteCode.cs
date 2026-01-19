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
            var accessorDefs = AccessorDefs;
            var storeDefs = new HashSet<StoreDefinition>();

            foreach (var accessorDef in accessorDefs)
            {
                foreach (var arg in accessorDef.Args)
                {
                    if (arg.StoreDef.IsValid)
                    {
                        storeDefs.Add(arg.StoreDef);
                    }
                }
            }

            var orderedStoreDefs = storeDefs
                .OrderBy(x => x.DataType.Name)
                .ToArray()
                .AsSpan();

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            var staticKeyword = Symbol.IsStatic ? "static " : "";

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.Print("#region VAULT").PrintEndLine();
            p.Print("#endregion ==").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // Vault");
            p.OpenScope();
            {
                WriteVault(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region READ-ONLY VAULT").PrintEndLine();
            p.Print("#endregion ============").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // ReadOnlyVault");
            p.OpenScope();
            {
                WriteReadOnlyVault(ref p, Syntax.Identifier.Text);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region ID COLLECTION").PrintEndLine();
            p.Print("#endregion").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // IdCollection");
            p.OpenScope();
            {
                WriteIdCollection(ref p, orderedStoreDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region ACCESSOR COLLECTION").PrintEndLine();
            p.Print("#endregion ================").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // AccessorCollection");
            p.OpenScope();
            {
                WriteAccessorCollection(ref p, accessorDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region READ-ONLY ACCESSOR COLLECTION").PrintEndLine();
            p.Print("#endregion ==========================").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // ReadOnlyAccessorCollection");
            p.OpenScope();
            {
                WriteReadOnlyAccessorCollection(ref p, accessorDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region ACCESSOR ENUMERATOR").PrintEndLine();
            p.Print("#endregion ================").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // AccessorEnumerator");
            p.OpenScope();
            {
                WriteAccessorEnumerator(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region DATA DIRECTORY").PrintEndLine();
            p.Print("#endregion ===========").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // DataDirectory");
            p.OpenScope();
            {
                WriteDataDirectory(ref p, orderedStoreDefs);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region DATA COLLECTION").PrintEndLine();
            p.Print("#endregion ============").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial class ").Print(Syntax.Identifier.Text)
                .PrintEndLine(" // DataCollection");
            p.OpenScope();
            {
                WriteDataCollection(ref p, orderedStoreDefs);
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

        private static void WriteVault(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("internal partial class Vault : UserDataVaultBase");
            p.OpenScope();
            {
                p.PrintLine("internal readonly DataDirectory _directory;");
                p.PrintLine("internal readonly AccessorCollection _accessors;");
                p.PrintLine("internal readonly IdCollection _ids;");
                p.PrintEndLine();

                p.PrintLine("internal Vault(");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(STRING_VAULT).PrintEndLine(" stringVault");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                    p.PrintLine(", string userId");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_ids = new(stringVault);");
                    p.PrintLine("_directory = new(stringVault, encryption, logger, taskArrayPool, _ids, userId);");
                    p.PrintLine("_accessors = new(_directory);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public IdCollection Ids");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _ids;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ReadOnlyAccessorCollection Accessors");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _accessors.AsReadOnly();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("protected override IUserDataDirectory DataDirectory");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _directory;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("protected sealed override void OnDeinitialize()");
                p.OpenScope();
                {
                    p.PrintLine("_accessors.Deinitialize();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("protected sealed override void Dispose(bool disposing)");
                p.OpenScope();
                {
                    p.PrintLine("if (disposing)");
                    p.OpenScope();
                    {
                        p.PrintLine("_directory.Dispose();");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("protected sealed override UnityTaskᐸboolᐳ OnTryLoadAsync(");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                    p.PrintLine(", string userId");
                    p.PrintLine(", SourcePriority priority");
                    p.PrintLine(", SaveDestination destination");
                    p.PrintLine(", CancellationToken token");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_accessors.Initialize();");
                    p.PrintLine("return UnityTasks.GetCompleted(true);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public ReadOnlyVault AsReadOnly()");
                p.OpenScope();
                {
                    p.PrintLine("return new ReadOnlyVault(this);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteReadOnlyVault(ref Printer p, string outerTypeName)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct ReadOnlyVault : IIsCreated");
            p.OpenScope();
            {
                p.PrintLine("internal readonly Vault _vault;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("internal ReadOnlyVault([NotNull] Vault vault)");
                p.OpenScope();
                {
                    p.PrintLine("_vault = vault;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _vault != null;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public IdCollection Ids");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _vault.Ids;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ReadOnlyAccessorCollection Accessors");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _vault.Accessors;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public UnityTask SaveAsync")
                    .PrintEndLine("(SaveDestination destination, CancellationToken token = default)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCreated(IsCreated);");
                    p.PrintEndLine();

                    p.PrintLine("return _vault.SaveAsync(destination, token);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("private static void ThrowIfNotCreated([DoesNotReturnIf(false)] bool isCreated)");
                p.OpenScope();
                {
                    p.PrintLine("if (isCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("throw ThrowHelper.CreateInvalidOperationException_TypeNotCreatedCorrectly")
                            .Print("(\"").Print(outerTypeName).PrintEndLine("+ReadOnlyVault\");");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteIdCollection(ref Printer p, ReadOnlySpan<StoreDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct IdCollection : IUserDataIdCollection, IIsCreated");
            p.OpenScope();
            {
                foreach (var def in defs)
                {
                    p.PrintBeginLine("public readonly StringIdᐸstringᐳ ").Print(def.DataType.Name)
                        .PrintEndLine(";");
                }

                p.PrintEndLine();

                p.PrintLine("internal IdCollection([NotNull] StringVault stringVault)");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;

                        p.PrintBeginLine(name)
                            .Print(" = stringVault.GetOrMakeId(nameof(").Print(name).PrintEndLine("));");
                    }

                    p.PrintLine("IsCreated = true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public StringIdᐸstringᐳ this[int index] => index switch");
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

                p.PrintLine("public bool IsCreated { get; }");
                p.PrintEndLine();

                p.PrintLine("public int Count");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(defs.Length).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Enumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> new Enumerator(this);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumeratorᐸStringIdᐸstringᐳᐳ IEnumerableᐸStringIdᐸstringᐳᐳ.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumerator IEnumerable.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸStringIdᐸstringᐳᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸStringIdᐸstringᐳᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸStringIdᐸstringᐳᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸStringIdᐸstringᐳᐳ destination, int length)");
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
                p.PrintLine("public bool TryCopyTo(SpanᐸStringIdᐸstringᐳᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸStringIdᐸstringᐳᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸStringIdᐸstringᐳᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸStringIdᐸstringᐳᐳ destination, int length)");
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

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public struct Enumerator : IEnumeratorᐸStringIdᐸstringᐳᐳ");
                p.OpenScope();
                {
                    p.PrintLine("private readonly IdCollection _source;");
                    p.PrintLine("private int _index;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public Enumerator(IdCollection source)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (source.IsCreated == false)");
                        p.OpenScope();
                        {
                            p.PrintLine("throw ThrowHelper.CreateArgumentException_CollectionNotCreated(\"source\");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("_source = source;");
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public readonly StringIdᐸstringᐳ Current => _source[_index];");
                    p.PrintEndLine();

                    p.PrintLine("readonly object IEnumerator.Current => Current;");
                    p.PrintEndLine();

                    p.PrintLine("public void Dispose() { }");
                    p.PrintEndLine();

                    p.PrintLine("public bool MoveNext()");
                    p.OpenScope();
                    {
                        p.PrintLine("_index++;");
                        p.PrintLine("return (uint)_index < (uint)_source.Count;");
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
            p.PrintEndLine();
        }

        private static void WriteAccessorCollection(ref Printer p, List<UserDataAccessorDefinition> defs)
        {
            var typeSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<UserDataAccessorDefinition>(defs.Count);
            var loopMap = new Dictionary<ITypeSymbol, int>(defs.Count, SymbolEqualityComparer.Default);

            foreach (var def in defs)
            {
                queue.Enqueue(def);
            }

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("internal partial class AccessorCollection : IUserDataAccessorCollection");
            p.OpenScope();
            {
                p.PrintLine("internal AccessorCollection([NotNull] DataDirectory directory)");
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
                        Write(ref p, def);
                    }

                    p.PrintEndLine();

                    foreach (var kv in loopMap)
                    {
                        p.PrintBeginLine("LogErrorCyclicDependency(logger, \"").Print(kv.Key.Name).PrintEndLine("\");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public IUserDataAccessor this[int index] => index switch");
                p.OpenScope();
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        p.PrintBeginLine().Print(i).Print(" => ").Print(defs[i].FieldName).PrintEndLine(",");
                    }

                    p.PrintLine("_ => throw ThrowHelper.CreateIndexOutOfRangeException_Collection()");
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine("public int Count");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(defs.Count).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    var typeName = def.FullTypeName;
                    var fieldName = def.FieldName;

                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(typeName).Print(" ")
                        .Print(fieldName).PrintEndLine(" { get; }")
                        .PrintEndLine();
                }

                p.PrintLine("public void Initialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable"))
                        {
                            p.PrintBeginLine(def.FieldName).PrintEndLine(".Initialize();");
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public void Deinitialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        if (def.Symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable"))
                        {
                            p.PrintBeginLine(def.FieldName).PrintEndLine(".Deinitialize();");
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public AccessorEnumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> new AccessorEnumerator(new ReadOnlyAccessorCollection(this));");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumeratorᐸIUserDataAccessorᐳ IEnumerableᐸIUserDataAccessorᐳ.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumerator IEnumerable.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination, int length)");
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
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination, int length)");
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

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public ReadOnlyAccessorCollection AsReadOnly()");
                p.OpenScope();
                {
                    p.PrintLine("return new ReadOnlyAccessorCollection(this);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void Write(ref Printer p, UserDataAccessorDefinition def)
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
                            p.Print("directory.").Print(arg.StoreDef.DataType.Name);
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
                                p.Print("directory.").Print(arg.StoreDef.DataType.Name);
                            }

                            p.PrintEndLine();
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
            }
        }

        private void WriteReadOnlyAccessorCollection(ref Printer p, List<UserDataAccessorDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly partial struct ReadOnlyAccessorCollection")
                .PrintEndLine(" : IUserDataAccessorReadOnlyCollection, IIsCreated");
            p.OpenScope();
            {
                p.PrintLine("internal readonly AccessorCollection _accessors;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("internal ReadOnlyAccessorCollection([NotNull] AccessorCollection accessors)");
                p.OpenScope();
                {
                    p.PrintLine("_accessors = accessors;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _accessors != null;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public IUserDataAccessor this[int index] => index switch");
                p.OpenScope();
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        p.PrintBeginLine().Print(i).Print(" => ").Print(defs[i].FieldName).PrintEndLine(",");
                    }

                    p.PrintLine("_ => throw ThrowHelper.CreateIndexOutOfRangeException_Collection()");
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine("public int Count");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(defs.Count).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var def in defs)
                {
                    var typeName = def.FullTypeName;
                    var fieldName = def.FieldName;

                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(typeName).Print(" ").PrintEndLine(fieldName);
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => _accessors.").Print(fieldName).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public AccessorEnumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> new AccessorEnumerator(this);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumeratorᐸIUserDataAccessorᐳ IEnumerableᐸIUserDataAccessorᐳ.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("IEnumerator IEnumerable.GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> CopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void CopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> _accessors.CopyTo(sourceStartIndex, destination, length);");

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(0, destination, length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination)");
                p.WithIncreasedIndent().PrintLine("=> TryCopyTo(sourceStartIndex, destination, destination.Length);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool TryCopyTo(int sourceStartIndex, SpanᐸIUserDataAccessorᐳ destination, int length)");
                p.WithIncreasedIndent().PrintLine("=> _accessors.TryCopyTo(sourceStartIndex, destination, length);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteAccessorEnumerator(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct AccessorEnumerator : IEnumeratorᐸIUserDataAccessorᐳ");
            p.OpenScope();
            {
                p.PrintLine("private readonly ReadOnlyAccessorCollection _source;");
                p.PrintLine("private int _index;");
                p.PrintEndLine();

                p.PrintLine("internal AccessorEnumerator([NotNull] ReadOnlyAccessorCollection source)");
                p.OpenScope();
                {
                    p.PrintLine("if (source.IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentException_CollectionNotCreated(\"source\");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("_source = source;");
                    p.PrintLine("_index = -1;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly IUserDataAccessor Current => _source[_index];");
                p.PrintEndLine();

                p.PrintLine("readonly object IEnumerator.Current => Current;");
                p.PrintEndLine();

                p.PrintLine("public void Dispose() { }");
                p.PrintEndLine();

                p.PrintLine("public bool MoveNext()");
                p.OpenScope();
                {
                    p.PrintLine("_index++;");
                    p.PrintLine("return (uint)_index < (uint)_source.Count;");
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
            p.PrintEndLine();
        }

        private void WriteDataDirectory(ref Printer p, ReadOnlySpan<StoreDefinition> defs)
        {
            var generateCreateMethods = new List<StoreDefinition>(defs.Length);

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("internal partial class DataDirectory : IUserDataDirectory, IDisposable");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ").Print(STRING_VAULT).PrintEndLine(" _stringVault;");
                p.PrintBeginLine("private readonly ").Print(ENCRYPTION_BASE).PrintEndLine(" _encryption;");
                p.PrintBeginLine("private readonly ").Print(ILOGGER).PrintEndLine(" _logger;");
                p.PrintBeginLine("private readonly ").Print(TASK_ARRAY_POOL).PrintEndLine(" _taskArrayPool;");
                p.PrintLine("private readonly IdCollection _ids;");
                p.PrintEndLine();

                p.PrintLine("private string _userId;");
                p.PrintEndLine();

                p.PrintLine("internal DataDirectory(");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("  ").Print(NOT_NULL).Print(" ").Print(STRING_VAULT).PrintEndLine(" stringVault");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ENCRYPTION_BASE).PrintEndLine(" encryption");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(ILOGGER).PrintEndLine(" logger");
                    p.PrintBeginLine(", ").Print(NOT_NULL).Print(" ").Print(TASK_ARRAY_POOL).PrintEndLine(" taskArrayPool");
                    p.PrintLine(", IdCollection ids");
                    p.PrintLine(", string userId");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_stringVault = stringVault;");
                    p.PrintLine("_encryption = encryption;");
                    p.PrintLine("_logger = logger;");
                    p.PrintLine("_taskArrayPool = taskArrayPool;");
                    p.PrintLine("_ids = ids;");
                    p.PrintLine("_userId = userId;");
                    p.PrintEndLine();

                    p.PrintLine("bool ignoreEncryption = false;");
                    p.PrintEndLine();

                    p.Print("#if !FORCE_USER_DATA_ENCRYPTION").PrintEndLine();
                    p.PrintLine("GetIgnoreEncryption(ref ignoreEncryption);");
                    p.Print("#endif").PrintEndLine();
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        var nonDefaultConstructorCount = 0;
                        var defaultConstructorCount = 0;

                        foreach (var member in def.DataType.GetMembers())
                        {
                            if (member is not IMethodSymbol method)
                            {
                                continue;
                            }

                            if (method.MethodKind == MethodKind.Constructor)
                            {
                                if (method.Parameters.Length > 0)
                                {
                                    nonDefaultConstructorCount++;
                                }
                                else
                                {
                                    defaultConstructorCount++;
                                }
                            }
                        }

                        var hasDefaultConstructor = defaultConstructorCount > 0 || nonDefaultConstructorCount < 1;

                        if (hasDefaultConstructor == false)
                        {
                            generateCreateMethods.Add(def);
                        }

                        p.OpenScope();
                        {
                            p.PrintBeginLine("global::System.Func<").Print(def.FullDataTypeName).Print("> ")
                                .Print("createFunc = ");

                            if (hasDefaultConstructor)
                            {
                                p.Print("static () => new ").Print(def.FullDataTypeName).PrintEndLine("();");
                            }
                            else
                            {
                                p.Print("Create").Print(def.DataType.Name).PrintEndLine(";");
                            }

                            p.PrintBeginLine("var args = GetStoreArgs<")
                                .Print(def.FullDataTypeName).Print(", ").Print(def.FullStoreTypeName)
                                .PrintEndLine(">(createFunc);");

                            p.PrintBeginLine(def.DataType.Name).Print(" = new(")
                                .Print("_ids.").Print(def.DataType.Name)
                                .Print(", stringVault, encryption, logger, ignoreEncryption, args")
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
                        .Print(def.FullStoreTypeName).Print(" ")
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

                p.PrintBeginLine("private static partial ").Print(STORE_ARGS)
                    .PrintEndLine(" GetStoreArgs<TData, TStore>(global::System.Func<TData> createDataFunc)");
                p.WithIncreasedIndent().PrintLine("where TData : IUserData");
                p.WithIncreasedIndent().PrintBeginLine("where TStore : ").Print(USER_DATA_STORE_BASE).PrintEndLine("TData>;");
                p.PrintEndLine();

                foreach (var def in generateCreateMethods)
                {
                    p.PrintBeginLine("private static partial ").Print(def.FullDataTypeName)
                        .Print(" Create").Print(def.DataType.Name).PrintEndLine("();");
                    p.PrintEndLine();
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

                p.PrintLine("public void Deinitialize()");
                p.OpenScope();
                {
                    foreach (var def in defs)
                    {
                        p.PrintBeginLine(def.DataType.Name)
                            .PrintEndLine(".Deinitialize();");
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

                p.PrintLine("public UnityTask LoadEntireDirectoryAsync(SourcePriority priority, CancellationToken token = default)");
                p.WithIncreasedIndent().PrintLine("=> LoadAsync(priority, token);");
                p.PrintEndLine();

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask LoadAsync(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  SourcePriority priority");
                    p.PrintLine(", CancellationToken token = default");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine(", bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintBeginLine("var tasks = _taskArrayPool.Rent(").Print(defs.Length).PrintEndLine(");");
                        p.PrintEndLine();

                        var lastIndex = defs.Length - 1;

                        for (var i = 0; i < defs.Length; i++)
                        {
                            var name = defs[i].DataType.Name;

                            p.PrintBeginLine("tasks[").Print(i).Print("] = include").PrintEndLine(name);
                            p = p.IncreasedIndent();
                            {
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".LoadAsync(priority, token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".LoadAsync(priority, token);");
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

                p.PrintLine("public UnityTask SaveEntireDirectoryAsync(SaveDestination destination, CancellationToken token = default)");
                p.WithIncreasedIndent().PrintLine("=> SaveAsync(destination, token);");
                p.PrintEndLine();

                p.PrintBeginLine("public ").PrintIf(defs.Length > 0, "async ")
                    .PrintEndLine("UnityTask SaveAsync(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  SaveDestination destination");
                    p.PrintLine(", CancellationToken token = default");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine(", bool include").Print(defs[i].DataType.Name).PrintEndLine(" = true");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    if (defs.Length > 1)
                    {
                        p.PrintBeginLine("var tasks = _taskArrayPool.Rent(").Print(defs.Length).PrintEndLine(");");
                        p.PrintEndLine();

                        var lastIndex = defs.Length - 1;

                        for (var i = 0; i < defs.Length; i++)
                        {
                            var name = defs[i].DataType.Name;

                            p.PrintBeginLine("tasks[").Print(i).Print("] = include").PrintEndLine(name);
                            p = p.IncreasedIndent();
                            {
                                p.PrintBeginLine("? ").Print(name).PrintEndLine(".SaveAsync(destination, token)");
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
                            p.PrintBeginLine("await ").Print(name).PrintEndLine(".SaveAsync(destination, token);");
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

                p.PrintLine("public void CloneDataFromCloud(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  SourcePriority priority");

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
                            p.PrintBeginLine(def.DataType.Name).PrintEndLine(".TryCloneDataFromCloud();");
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

        private void WriteDataCollection(ref Printer p, ReadOnlySpan<StoreDefinition> defs)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("[Serializable]");
            p.PrintLine("public partial struct DataCollection : IUserDataCollection, IIsCreated");
            p.OpenScope();
            {
                p.PrintLine("[SerializeField] internal string _userId;");

                foreach (var def in defs)
                {
                    p.PrintBeginLine("[SerializeField] internal ").Print(def.FullDataTypeName).Print(" ")
                        .Print(def.DataFieldName).PrintEndLine(";");
                }

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public DataCollection(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  [NotNull] string userId");
                    p.PrintLine(", DataCollection source");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("if (source.IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentException_CollectionNotCreated(\"source\");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("_userId = userId;");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        var def = defs[i];

                        p.PrintBeginLine(def.DataFieldName).Print(" = source.").Print(def.DataFieldName).PrintEndLine(";");
                    }

                    p.PrintEndLine();
                    p.PrintLine("IsCreated = true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("internal DataCollection(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  [NotNull] string userId");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        var def = defs[i];

                        p.PrintBeginLine(", ").Print(def.FullDataTypeName).Print(" ").PrintEndLine(def.DataArgName);
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("_userId = userId;");

                    for (var i = 0; i < defs.Length; i++)
                    {
                        var def = defs[i];

                        p.PrintBeginLine(def.DataFieldName).Print(" = ").Print(def.DataArgName).PrintEndLine(";");
                    }

                    p.PrintEndLine();
                    p.PrintLine("IsCreated = true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public IUserData this[int index] => index switch");
                p.OpenScope();
                {
                    for (var i = 0; i < defs.Length; i++)
                    {
                        p.PrintBeginLine().Print(i).Print(" => ").Print(defs[i].DataFieldName).PrintEndLine(",");
                    }

                    p.PrintLine("_ => throw ThrowHelper.CreateIndexOutOfRangeException_Collection()");

                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine("public bool IsCreated { get; }");
                p.PrintEndLine();

                p.PrintLine("public int Count");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(defs.Length).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public string UserId");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _userId;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public static DataCollection GetFrom(ReadOnlyVault vault, SourcePriority priority)");
                p.OpenScope();
                {
                    p.PrintLine("if (vault.IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentNullException(\"vault\");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("var directory = vault._vault._directory;");
                    p.PrintEndLine();

                    p.PrintLine("return new DataCollection(");
                    p = p.IncreasedIndent();
                    {
                        p.PrintLine("  directory.UserId");

                        foreach (var def in defs)
                        {
                            var name = def.DataType.Name;

                            p.PrintBeginLine(", directory.").Print(name).PrintEndLine(".GetData(priority)");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public static DataCollection CloneFrom(ReadOnlyVault vault, SourcePriority priority)");
                p.OpenScope();
                {
                    p.PrintLine("if (vault.IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentNullException(\"vault\");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("var directory = vault._vault._directory;");
                    p.PrintEndLine();

                    p.PrintLine("return new DataCollection(");
                    p = p.IncreasedIndent();
                    {
                        p.PrintLine("  directory.UserId");

                        foreach (var def in defs)
                        {
                            var name = def.DataType.Name;
                            p.PrintBeginLine(", directory.").Print(name)
                                .PrintEndLine(".TryCloneData(priority).GetValueOrDefault()");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public DataCollection ChangeUserId([NotNull] string userId)");
                p.WithIncreasedIndent().PrintLine("=> new DataCollection(userId, this);");
                p.PrintEndLine();

                p.PrintLine("public void SetTo(ReadOnlyVault vault)");
                p.OpenScope();
                {
                    p.PrintLine("if (IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateInvalidOperationException_CollectionNotCreated();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (vault.IsCreated == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("throw ThrowHelper.CreateArgumentNullException(\"vault\");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("var directory = vault._vault._directory;");
                    p.PrintEndLine();

                    foreach (var def in defs)
                    {
                        var name = def.DataType.Name;
                        var fieldName = def.DataFieldName;

                        p.PrintBeginLine("directory.").Print(name)
                            .Print(".SetData(").Print(fieldName).PrintEndLine(");");
                    }
                }
                p.CloseScope();
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

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public struct Enumerator : IEnumeratorᐸIUserDataᐳ");
                p.OpenScope();
                {
                    p.PrintLine("private readonly DataCollection _source;");
                    p.PrintLine("private int _index;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public Enumerator([NotNull] DataCollection source)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (source.IsCreated == false)");
                        p.OpenScope();
                        {
                            p.PrintLine("throw ThrowHelper.CreateArgumentException_CollectionNotCreated(\"source\");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("_source = source;");
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public readonly IUserData Current => _source[_index];");
                    p.PrintEndLine();

                    p.PrintLine("readonly object IEnumerator.Current => Current;");
                    p.PrintEndLine();

                    p.PrintLine("public void Dispose() { }");
                    p.PrintEndLine();

                    p.PrintLine("public bool MoveNext()");
                    p.OpenScope();
                    {
                        p.PrintLine("_index++;");
                        p.PrintLine("return (uint)_index < (uint)_source.Count;");
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

            p.PrintLine(HIDE_IN_CALL_STACK);
            p.PrintLine("private static void LogErrorCyclicDependency(ILogger logger, string name)");
            p.OpenScope();
            {
                p.PrintBeginLine("logger.LogError(")
                    .PrintEndLine("$\"Detect cyclic dependency in the constructor of type '{name}'\");");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
