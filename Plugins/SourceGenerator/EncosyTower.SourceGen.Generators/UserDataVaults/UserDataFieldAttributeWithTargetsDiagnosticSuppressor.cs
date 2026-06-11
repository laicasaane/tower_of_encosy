// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static EncosyTower.SourceGen.Generators.UserDataVaults.Helpers;

    /// <summary>
    /// <para>
    /// A diagnostic suppressor to suppress CS0657 warnings for fields with [SerializeField] using a [property:] attribute list.
    /// </para>
    /// <para>
    /// That is, this diagnostic suppressor will suppress the following diagnostic:
    /// <code>
    /// [UserData]
    /// public partial class MyData
    /// {
    ///     [property: JsonIgnore]
    ///     private string _id;
    ///
    ///     [property: JsonIgnore]
    ///     private string _version;
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UserDataFieldAttributeWithTargetsDiagnosticSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor PropertyAttributeListForDataField = new(
              id: "SG_USER_DATA_SUPPRESS_0001"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Fields named 'id', '_id', 'm_id', 'version', '_version', 'm_version' " +
                "can use [property:] attribute lists to forward attributes to the generated properties"
        );

        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => ImmutableArray.Create(PropertyAttributeListForDataField);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                token.ThrowIfCancellationRequested();

                var syntaxNode = diagnostic.Location.SourceTree?
                    .GetRoot(token).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [property:] over a field declaration with at least one variable,
                // which is the only case we are interested in
                if (syntaxNode is not AttributeTargetSpecifierSyntax attributeTarget
                    || attributeTarget.Parent.Parent is not FieldDeclarationSyntax fieldDeclaration
                    || fieldDeclaration.Declaration.Variables.Count < 1
                    || ValidateFieldName(fieldDeclaration.Declaration.Variables[0].Identifier.Text) == false
                    || attributeTarget.Identifier.Kind() is not SyntaxKind.PropertyKeyword
                    || fieldDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration
                    || typeDeclaration.AttributeLists.Count < 1
                    || typeDeclaration.HasAttribute(NAMESPACE, "UserData", token) == false
                )
                {
                    continue;
                }



                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the field symbol from the first variable declaration
                var declaredSymbol = semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0], token);

                // Check if the field is using [SerializeField], in which case we should suppress the warning
                if (declaredSymbol is IFieldSymbol fieldSymbol
                    && fieldSymbol.ContainingType.HasAttribute(USER_DATA_ATTRIBUTE, token)
                )
                {
                    context.ReportSuppression(Suppression.Create(PropertyAttributeListForDataField, diagnostic));
                }
            }
        }

        private static bool ValidateFieldName(string fieldName)
        {
            return fieldName is "id"
                or "_id"
                or "m_id"
                or "version"
                or "_version"
                or "m_version"
                ;
        }
    }
}
