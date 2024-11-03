// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;
    using static EncosyTower.Modules.Data.SourceGen.SuppressionDescriptors;

    /// <summary>
    /// <para>
    /// A diagnostic suppressor to suppress CS0657 warnings for fields with [SerializeField] using a [property:] attribute list.
    /// </para>
    /// <para>
    /// That is, this diagnostic suppressor will suppress the following diagnostic:
    /// <code>
    /// public partial class MyData : IData
    /// {
    ///     [SerializeField]
    ///     [property: JsonPropertyName("Name")]
    ///     private string _name;
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DataFieldAttributeWithTargetsDiagnosticSuppressor : DiagnosticSuppressor
    {
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(PropertyAttributeListForDataField);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [property:] over a field declaration with at least one variable, which is the only case we are interested in
                if (syntaxNode is not AttributeTargetSpecifierSyntax attributeTarget
                    || attributeTarget.Parent.Parent is not FieldDeclarationSyntax fieldDeclaration
                    || fieldDeclaration.Declaration.Variables.Count <= 0
                    || attributeTarget.Identifier.Kind() is not SyntaxKind.PropertyKeyword
                )
                {
                    continue;
                }

                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the field symbol from the first variable declaration
                var declaredSymbol = semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0], context.CancellationToken);

                // Check if the field is using [SerializeField], in which case we should suppress the warning
                if (declaredSymbol is IFieldSymbol fieldSymbol
                    && (fieldSymbol.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE)
                        || fieldSymbol.HasAttribute(JSON_INCLUDE_ATTRIBUTE)
                        || fieldSymbol.HasAttribute(JSON_PROPERTY_ATTRIBUTE)
                    )
                )
                {
                    context.ReportSuppression(Suppression.Create(PropertyAttributeListForDataField, diagnostic));
                }
            }
        }
    }
}
