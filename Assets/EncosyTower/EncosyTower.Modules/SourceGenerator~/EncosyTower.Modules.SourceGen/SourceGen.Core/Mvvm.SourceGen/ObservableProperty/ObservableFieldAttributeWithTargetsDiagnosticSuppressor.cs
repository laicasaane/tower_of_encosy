// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static EncosyTower.Modules.Mvvm.SuppressionDescriptors;

namespace EncosyTower.Modules.Mvvm.ObservablePropertySourceGen
{
    /// <summary>
    /// <para>
    /// A diagnostic suppressor to suppress CS0657 warnings for fields with [ObservableProperty]
    /// using a [property:] or [method:] attribute list.
    /// </para>
    /// <para>
    /// That is, this diagnostic suppressor will suppress the following diagnostic:
    /// <code>
    /// public partial class MyViewModel : IObservableObject
    /// {
    ///     [ObservableProperty]
    ///     [property: JsonPropertyName("Name")]
    ///     private string _name;
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ObservableFieldAttributeWithTargetsDiagnosticSuppressor : DiagnosticSuppressor
    {
        private const string ATTRIBUTE = "EncosyTower.Modules.Mvvm.ComponentModel.ObservablePropertyAttribute";

        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => ImmutableArray.Create(PropertyAttributeListForObservableField);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken)
                    .FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [property:] over a field declaration with at least one variable,
                // which is the only case we are interested in
                if (syntaxNode is not AttributeTargetSpecifierSyntax attributeTarget
                    || attributeTarget.Parent.Parent is not FieldDeclarationSyntax fieldDeclaration
                    || fieldDeclaration.Declaration.Variables.Count < 1
                    || attributeTarget.Identifier.Kind() is not (SyntaxKind.PropertyKeyword or SyntaxKind.MethodKeyword)
                )
                {
                    continue;
                }

                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the field symbol from the first variable declaration
                var declaredSymbol = semanticModel.GetDeclaredSymbol(
                      fieldDeclaration.Declaration.Variables[0]
                    , context.CancellationToken
                );

                // Check if the field is using [ObservableProperty], in which case we should suppress the warning
                if (declaredSymbol is IFieldSymbol fieldSymbol
                    && semanticModel.Compilation.GetTypeByMetadataName(ATTRIBUTE) is INamedTypeSymbol attribTypeSymbol
                    && fieldSymbol.HasAttributeWithType(attribTypeSymbol)
                )
                {
                    context.ReportSuppression(Suppression.Create(PropertyAttributeListForObservableField, diagnostic));
                }
            }
        }
    }
}
