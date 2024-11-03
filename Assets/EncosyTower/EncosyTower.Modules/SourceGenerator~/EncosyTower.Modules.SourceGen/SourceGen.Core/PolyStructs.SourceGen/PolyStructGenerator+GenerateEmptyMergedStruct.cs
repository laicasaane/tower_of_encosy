using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.Modules.EnumExtensions.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    partial class PolyStructGenerator
    {
        private static void GenerateEmptyMergedStruct(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , bool outputSourceGenFiles
            , IEnumerable<InterfaceRef> interfaceRefs
            , List<EnumMemberDeclaration> enumMembers
            , StringBuilder sb
            , CancellationToken token
        )
        {
            var refUnityCollections = compilationCandidate.references.unityCollections;

            foreach (var interfaceRef in interfaceRefs)
            {
                try
                {
                    var syntax = interfaceRef.Syntax;
                    var symbol = interfaceRef.Symbol;
                    var syntaxTree = syntax.SyntaxTree;
                    var compilation = compilationCandidate.compilation;
                    var assemblyName = compilation.Assembly.Name;
                    var mergedStructName = $"{interfaceRef.FullContainingNameWithDot}{interfaceRef.StructName}";
                    var enumUnderlyingTypeName = GetEnumUnderlyingTypeName(0);

                    var enumExtensions = new EnumExtensionsDeclaration(refUnityCollections, 32) {
                        GeneratedCode = GENERATED_CODE,
                        Name = "TypeId",
                        ExtensionsName = $"{interfaceRef.StructName}_TypeIdExtensions",
                        ExtensionsWrapperName = $"{interfaceRef.StructName}_TypeIdExtensionsWrapper",
                        ParentIsNamespace = interfaceRef.Syntax.Parent is NamespaceDeclarationSyntax,
                        FullyQualifiedName = $"{mergedStructName}.TypeId",
                        UnderlyingTypeName = enumUnderlyingTypeName,
                        Members = enumMembers,
                        Accessibility = interfaceRef.Symbol.DeclaredAccessibility,
                        IsDisplayAttributeUsed = false,
                    };

                    ToEnumMembers(interfaceRef, enumMembers, out var kindFixedStringBytes);

                    var source = WriteMergedStruct(
                          interfaceRef
                        , Array.Empty<StructRef>()
                        , 0
                        , GetEnumUnderlyingTypeName(0)
                        , Array.Empty<MergedFieldRef>()
                        , enumExtensions
                        , sb
                        , token
                    );

                    context.OutputSource(
                          outputSourceGenFiles
                        , syntax
                        , source
                        , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, interfaceRef.StructName)
                        , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
                    );
                }
                catch (Exception e)
                {
                    if (e is OperationCanceledException)
                    {
                        throw;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                          s_errorDescriptor_1
                        , interfaceRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }
    }
}
