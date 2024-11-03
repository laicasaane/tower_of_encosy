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
    /// A diagnostic suppressor to suppress CS0657 warnings for properties with [ObservableProperty]
    /// using a [field:] attribute list.
    /// </para>
    /// <para>
    /// That is, this diagnostic suppressor will suppress the following diagnostic:
    /// <code>
    /// public partial class MyViewModel : IObservableObject
    /// {
    ///     [ObservableProperty]
    ///     [field: JsonPropertyName("Name")]
    ///     public string Name { get => Get_Name(); set => Set_Name(value); }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ObservablePropertyAttributeWithTargetsDiagnosticSuppressor : DiagnosticSuppressor
    {
        private const string ATTRIBUTE = "EncosyTower.Modules.Mvvm.ComponentModel.ObservablePropertyAttribute";

        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => ImmutableArray.Create(FieldAttributeListForObservableProperty);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken)
                    .FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [field:] over a property declaration with at least one variable,
                // which is the only case we are interested in
                if (syntaxNode is not AttributeTargetSpecifierSyntax attributeTarget
                    || attributeTarget.Parent.Parent is not PropertyDeclarationSyntax propertyDeclaration
                    || attributeTarget.Identifier.Kind() is not SyntaxKind.FieldKeyword
                )
                {
                    continue;
                }

                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the property symbol from the first variable declaration
                var declaredSymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

                // Check if the property is using [ObservableProperty], in which case we should suppress the warning
                if (declaredSymbol is IPropertySymbol propertySymbol
                    && semanticModel.Compilation.GetTypeByMetadataName(ATTRIBUTE) is INamedTypeSymbol attribTypeSymbol
                    && propertySymbol.HasAttributeWithType(attribTypeSymbol)
                )
                {
                    context.ReportSuppression(Suppression.Create(FieldAttributeListForObservableProperty, diagnostic));
                }
            }
        }
    }
}
