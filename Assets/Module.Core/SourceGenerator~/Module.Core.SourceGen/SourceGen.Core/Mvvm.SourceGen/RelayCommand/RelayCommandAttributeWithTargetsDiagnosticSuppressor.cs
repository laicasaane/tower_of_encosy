// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Module.Core.SourceGen;

using static Module.Core.Mvvm.SuppressionDescriptors;

namespace Module.Core.Mvvm.RelayCommandSourceGen
{
    /// <summary>
    /// <para>
    /// A diagnostic suppressor to suppress CS0657 warnings for methods with [RelayCommand] using a [field:] or [property:] attribute list.
    /// </para>
    /// <para>
    /// That is, this diagnostic suppressor will suppress the following diagnostic:
    /// <code>
    /// public partial class MyViewModel : IObservableObject
    /// {
    ///     [RelayCommand]
    ///     [field: JsonIgnore]
    ///     [property: SomeOtherAttribute]
    ///     private void DoSomething()
    ///     {
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RelayCommandAttributeWithTargetsDiagnosticSuppressor : DiagnosticSuppressor
    {
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(FieldOrPropertyAttributeListForRelayCommandMethod);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [field:] or [property:] over a method declaration, which is the case we're looking for
                if (syntaxNode is not AttributeTargetSpecifierSyntax attributeTarget
                    || attributeTarget.Parent.Parent is not MethodDeclarationSyntax methodDeclaration
                    || attributeTarget.Identifier.Kind() is not SyntaxKind.FieldKeyword and not SyntaxKind.PropertyKeyword
                )
                {
                    continue;
                }

                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the method symbol from the first variable declaration
                var declaredSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                // Check if the method is using [RelayCommand], in which case we should suppress the warning
                if (declaredSymbol is IMethodSymbol methodSymbol
                    && semanticModel.Compilation.GetTypeByMetadataName("Module.Core.Mvvm.Input.RelayCommandAttribute") is INamedTypeSymbol relayCommandSymbol
                    && methodSymbol.HasAttributeWithType(relayCommandSymbol)
                )
                {
                    context.ReportSuppression(Suppression.Create(FieldOrPropertyAttributeListForRelayCommandMethod, diagnostic));
                }
            }
        }
    }
}
