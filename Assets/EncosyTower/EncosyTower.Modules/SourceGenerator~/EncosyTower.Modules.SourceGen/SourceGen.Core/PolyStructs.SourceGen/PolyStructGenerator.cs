using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.Modules.EnumExtensions.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    using InterfaceMap = Dictionary<ISymbol, InterfaceRef>;
    using InterfaceToStructMap = Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, StructRef>>;
    using StructMap = Dictionary<INamedTypeSymbol, StructRef>;

    [Generator]
    public partial class PolyStructGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(PolyStructGenerator);
        public const string POLY_INTERFACE_ATTRIBUTE = "global::EncosyTower.Modules.PolyStructs.PolyStructInterfaceAttribute";
        public const string POLY_STRUCT_ATTRIBUTE = "global::EncosyTower.Modules.PolyStructs.PolyStructAttribute";

        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.PolyStructs.PolyStructSourceGen.PolymorphicStructGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.PolyStructs.SkipSourceGenForAssemblyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var interfaceRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidInterfaceSyntax,
                transform: GetInterfaceRefSemanticMatch
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var structRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidStructSyntax,
                transform: GetStructRefSemanticMatch
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var combined = interfaceRefProvider.Collect()
                .Combine(structRefProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidInterfaceSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is InterfaceDeclarationSyntax syntax
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate("EncosyTower.Modules.PolyStructs", "PolyStructInterface");
        }

        public static InterfaceCandidate GetInterfaceRefSemanticMatch(GeneratorSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
                || context.Node is not InterfaceDeclarationSyntax syntax
            )
            {
                return new(null, null, default);
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);
            var attribute = symbol.GetAttribute(POLY_INTERFACE_ATTRIBUTE);

            if (attribute == null)
            {
                return new(null, null, default);
            }

            var verbose = false;

            foreach (var arg in attribute.NamedArguments)
            {
                if (arg.Key == "Verbose" && arg.Value.Value is bool value)
                {
                    verbose = value;
                }
            }

            return new(syntax, symbol, verbose);
        }

        private static bool IsValidStructSyntax(SyntaxNode node, CancellationToken _)
        {
            return node is StructDeclarationSyntax syntax
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate("EncosyTower.Modules.PolyStructs", "PolyStruct")
                && syntax.BaseList != null
                && syntax.BaseList.Types.Count > 0
                ;
        }

        public static StructCandidate GetStructRefSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
                || context.Node is not StructDeclarationSyntax syntax
                || syntax.BaseList == null
                || syntax.BaseList.Types.Count < 1
            )
            {
                return new(null, null);
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol.HasAttribute(POLY_STRUCT_ATTRIBUTE) == false)
            {
                return new(null, null);
            }

            foreach (var item in symbol.AllInterfaces)
            {
                if (item.HasAttribute(POLY_INTERFACE_ATTRIBUTE))
                {
                    return new(syntax, symbol);
                }
            }

            return new(null, null);
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , ImmutableArray<InterfaceCandidate> interfaces
            , ImmutableArray<StructCandidate> structs
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;

            BuildMaps(
                  context
                , interfaces
                , structs
                , out var interfaceMap
                , out var interfaceToStructMap
                , out var structRefs
                , out var count
            );

            var mergedFieldRefPool = new Queue<MergedFieldRef>(count);
            var mergedFieldRefList = new List<MergedFieldRef>(count);
            var enumMembers = new List<EnumMemberDeclaration>(count);
            var sb = new StringBuilder();

            foreach (var kv in interfaceToStructMap)
            {
                var interfaceSymbol = kv.Key;
                var structRefCount = kv.Value.Count;
                var structRefCollection = kv.Value.Values;

                BuildMergedFieldRefList(kv.Value, mergedFieldRefList, mergedFieldRefPool);

                if (interfaceMap.TryGetValue(interfaceSymbol, out var interfaceRef))
                {
                    interfaceMap.Remove(interfaceSymbol);

                    GenerateMergedStruct(
                          context
                        , compilationCandidate
                        , outputSourceGenFiles
                        , interfaceRef
                        , structRefCollection
                        , (ulong)structRefCount
                        , mergedFieldRefList
                        , enumMembers
                        , sb
                        , context.CancellationToken
                    );
                }

                ClearToPool(mergedFieldRefList, mergedFieldRefPool);
            }

            GenerateStructs(
                  context
                , compilationCandidate
                , outputSourceGenFiles
                , structRefs
            );

            GenerateEmptyMergedStruct(
                  context
                , compilationCandidate
                , outputSourceGenFiles
                , interfaceMap.Values
                , enumMembers
                , sb
                , context.CancellationToken
            );
        }

        private static void BuildMaps(
              SourceProductionContext context
            , ImmutableArray<InterfaceCandidate> interfaces
            , ImmutableArray<StructCandidate> structs
            , out InterfaceMap interfaceMap
            , out InterfaceToStructMap interfaceToStructMap
            , out ImmutableArray<StructRef> structRefs
            , out int maxStructCount
        )
        {
            interfaceMap = new(SymbolEqualityComparer.Default);

            foreach (var (syntax, symbol, verbose) in interfaces)
            {
                if (interfaceMap.ContainsKey(symbol) == false)
                {
                    interfaceMap.Add(symbol, new InterfaceRef(syntax, symbol, verbose));
                }
            }

            interfaceToStructMap = new InterfaceToStructMap(
                SymbolEqualityComparer.Default
            );

            maxStructCount = 0;

            using var structRefArrayBuilder = ImmutableArrayBuilder<StructRef>.Rent();

            foreach (var (syntax, symbol) in structs)
            {
                StructRef structRef = null;

                foreach (var @interface in symbol.AllInterfaces)
                {
                    if (interfaceMap.TryGetValue(@interface, out var interfaceRef) == false)
                    {
                        continue;
                    }

                    if (interfaceToStructMap.TryGetValue(@interface, out var structMap) == false)
                    {
                        structMap = new(SymbolEqualityComparer.Default);
                        interfaceToStructMap[@interface] = structMap;
                    }

                    if (structRef == null)
                    {
                        try
                        {
                            structRef = new StructRef(syntax, symbol);
                            structRefArrayBuilder.Add(structRef);
                        }
                        catch (Exception e)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  s_errorDescriptor_1
                                , syntax.GetLocation()
                                , e.ToUnityPrintableString()
                            ));
                        }
                    }

                    if (structRef.Interfaces.ContainsKey(@interface) == false)
                    {
                        structRef.Interfaces.Add(@interface, interfaceRef);
                    }

                    structMap.Add(symbol, structRef);

                    if (maxStructCount < structMap.Count)
                    {
                        maxStructCount = structMap.Count;
                    }
                }
            }

            structRefs = structRefArrayBuilder.ToImmutable();
        }

        private static void BuildMergedFieldRefList(
              StructMap structMap
            , List<MergedFieldRef> list
            , Queue<MergedFieldRef> pool
        )
        {
            var usedIndexesInList = new HashSet<int>();

            foreach (var kv in structMap)
            {
                usedIndexesInList.Clear();

                var structSymbol = kv.Key;
                var fields = kv.Value.Fields;

                for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
                {
                    var field = fields[fieldIndex];
                    var matchingListIndex = -1;

                    for (int listIndex = 0; listIndex < list.Count; listIndex++)
                    {
                        if (usedIndexesInList.Contains(listIndex) == false)
                        {
                            if (SymbolEqualityComparer.Default.Equals(field.Type, list[listIndex].Type))
                            {
                                matchingListIndex = listIndex;
                                break;
                            }
                        }
                    }

                    if (matchingListIndex < 0)
                    {
                        int newListIndex = list.Count;
                        MergedFieldRef mergedField;

                        if (pool.Count > 0)
                        {
                            mergedField = pool.Dequeue();
                        }
                        else
                        {
                            mergedField = new MergedFieldRef();
                        }

                        mergedField.Type = field.Type;
                        mergedField.Name = $"Field_{field.Type.ToValidIdentifier()}_{newListIndex}";
                        mergedField.StructToFieldMap.Add(structSymbol, field.Name);

                        field.MergedName = mergedField.Name;

                        list.Add(mergedField);
                        usedIndexesInList.Add(newListIndex);
                    }
                    else
                    {
                        var mergedField = list[matchingListIndex];
                        field.MergedName = mergedField.Name;

                        mergedField.StructToFieldMap.Add(structSymbol, field.Name);
                        usedIndexesInList.Add(matchingListIndex);
                    }
                }
            }
        }

        private static void ClearToPool(List<MergedFieldRef> list, Queue<MergedFieldRef> pool)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                item.Type = default;
                item.Name = default;
                item.StructToFieldMap.Clear();

                list.RemoveAt(i);
                pool.Enqueue(item);
            }

            list.Clear();
        }

        private static string GetEnumUnderlyingTypeName(ulong structRefCount)
        {
            string underlyingType;

            if (structRefCount <= 256)
            {
                underlyingType = "byte";
            }
            else if (structRefCount <= 65536)
            {
                underlyingType = "ushort";
            }
            else if (structRefCount <= 4294967296)
            {
                underlyingType = "uint";
            }
            else
            {
                underlyingType = "ulong";
            }

            return underlyingType;
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor_1
            = new("SG_POLYMORPHIC_STRUCT_01"
                , "Polymorphic Struct Generator Error"
                , "This error indicates a bug in the Polymorphic Struct source generators. Error message: '{0}'."
                , "EncosyTower.Modules.PolyStructs.SourceGen.PolyStructGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        public struct InterfaceCandidate
        {
            public InterfaceDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
            public bool verbose;

            public InterfaceCandidate(InterfaceDeclarationSyntax syntax, INamedTypeSymbol symbol, bool verbose)
            {
                this.syntax = syntax;
                this.symbol = symbol;
                this.verbose = verbose;
            }

            public void Deconstruct(out InterfaceDeclarationSyntax syntax, out INamedTypeSymbol symbol, out bool verbose)
            {
                syntax = this.syntax;
                symbol = this.symbol;
                verbose = this.verbose;
            }
        }

        public struct StructCandidate
        {
            public StructDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;

            public StructCandidate(StructDeclarationSyntax syntax, INamedTypeSymbol symbol)
            {
                this.syntax = syntax;
                this.symbol = symbol;
            }

            public void Deconstruct(out StructDeclarationSyntax syntax, out INamedTypeSymbol symbol)
            {
                syntax = this.syntax;
                symbol = this.symbol;
            }
        }
    }
}