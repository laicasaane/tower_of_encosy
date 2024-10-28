// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EncosyTower.Modules.SourceGen
{

    /// <summary>
    /// A model representing an attribute declaration.
    /// </summary>
    public sealed class AttributeInfo : IEquatable<AttributeInfo>
    {
        public string TypeName { get; }

        public EquatableArray<TypedConstantInfo> ConstructorArgumentInfo { get; }

        public EquatableArray<(string Name, TypedConstantInfo Value)> NamedArgumentInfo { get; }

        public AttributeInfo(string typeName, EquatableArray<TypedConstantInfo> constructorArgumentInfo, EquatableArray<(string Name, TypedConstantInfo Value)> namedArgumentInfo)
        {
            this.TypeName = typeName;
            this.ConstructorArgumentInfo = constructorArgumentInfo;
            this.NamedArgumentInfo = namedArgumentInfo;
        }

        /// <summary>
        /// Creates a new <see cref="AttributeInfo"/> instance from a given <see cref="AttributeData"/> value.
        /// </summary>
        /// <param name="attributeData">The input <see cref="AttributeData"/> value.</param>
        /// <returns>A <see cref="AttributeInfo"/> instance representing <paramref name="attributeData"/>.</returns>
        public static AttributeInfo From(AttributeData attributeData)
        {
            string typeName = attributeData.AttributeClass!.ToFullName();

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            // Get the constructor arguments
            foreach (TypedConstant typedConstant in attributeData.ConstructorArguments)
            {
                constructorArguments.Add(TypedConstantInfo.From(typedConstant));
            }

            // Get the named arguments
            foreach (KeyValuePair<string, TypedConstant> namedConstant in attributeData.NamedArguments)
            {
                namedArguments.Add((namedConstant.Key, TypedConstantInfo.From(namedConstant.Value)));
            }

            return new(
                typeName,
                constructorArguments.ToImmutable(),
                namedArguments.ToImmutable());
        }

        /// <summary>
        /// Creates a new <see cref="AttributeInfo"/> instance from a given syntax node.
        /// </summary>
        /// <param name="typeSymbol">The symbol for the attribute type.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="arguments">The sequence of <see cref="AttributeArgumentSyntax"/> instances to process.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <returns>A <see cref="AttributeInfo"/> instance representing the input attribute data.</returns>
        public static AttributeInfo From(INamedTypeSymbol typeSymbol, SemanticModel semanticModel, IEnumerable<AttributeArgumentSyntax> arguments, CancellationToken token)
        {
            string typeName = typeSymbol.ToFullName();

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            foreach (AttributeArgumentSyntax argument in arguments)
            {
                // The attribute expression has to have an available operation to extract information from
                if (semanticModel.GetOperation(argument.Expression, token) is not IOperation operation)
                {
                    continue;
                }

                var argumentInfo = TypedConstantInfo.From(operation, semanticModel, argument.Expression, token);

                // Try to get the identifier name if the current expression is a named argument expression. If it
                // isn't, then the expression is a normal attribute constructor argument, so no extra work is needed.
                if (argument.NameEquals?.Name.Identifier.ValueText is string argumentName)
                {
                    namedArguments.Add((argumentName, argumentInfo));
                }
                else
                {
                    constructorArguments.Add(argumentInfo);
                }
            }

            return new(
                typeName,
                constructorArguments.ToImmutable(),
                namedArguments.ToImmutable());
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeInfo other)
                return EqualityComparer<AttributeInfo>.Default.Equals(this, other);

            return false;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<AttributeInfo>.Default.GetHashCode(this);
        }

        public bool Equals(AttributeInfo other)
        {
            return EqualityComparer<AttributeInfo>.Default.Equals(this, other);
        }

        /// <summary>
        /// Gets an <see cref="AttributeSyntax"/> instance representing the current value.
        /// </summary>
        /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current value.</returns>
        public AttributeSyntax GetSyntax()
        {
            // Gather the constructor arguments
            IEnumerable<AttributeArgumentSyntax> arguments =
            ConstructorArgumentInfo
            .Select(static arg => AttributeArgument(arg.GetSyntax()));

            // Gather the named arguments
            IEnumerable<AttributeArgumentSyntax> namedArguments =
            NamedArgumentInfo.Select(static arg =>
                AttributeArgument(arg.Value.GetSyntax())
                .WithNameEquals(NameEquals(IdentifierName(arg.Name))));

            return Attribute(IdentifierName(TypeName), AttributeArgumentList(SeparatedList(arguments.Concat(namedArguments))));
        }
    }
}
