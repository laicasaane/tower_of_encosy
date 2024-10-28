// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EncosyTower.Modules.SourceGen
{
    /// <summary>
    /// A model representing a typed constant item.
    /// </summary>
    /// <remarks>This model is fully serializable and comparable.</remarks>
    public abstract partial class TypedConstantInfo : IEquatable<TypedConstantInfo>
    {
        public override bool Equals(object obj)
        {
            if (obj is TypedConstantInfo other)
                return EqualityComparer<TypedConstantInfo>.Default.Equals(this, other);

            return false;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TypedConstantInfo>.Default.GetHashCode(this);
        }

        public bool Equals(TypedConstantInfo other)
        {
            return EqualityComparer<TypedConstantInfo>.Default.Equals(this, other);
        }

        /// <summary>
        /// Gets an <see cref="ExpressionSyntax"/> instance representing the current constant.
        /// </summary>
        /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current constant.</returns>
        public abstract ExpressionSyntax GetSyntax();

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing an array.
        /// </summary>
        /// <param name="ElementTypeName">The type name for array elements.</param>
        /// <param name="Items">The sequence of contained elements.</param>
        public sealed class Array : TypedConstantInfo
        {
            public string ElementTypeName { get; }

            public EquatableArray<TypedConstantInfo> Items { get; }

            public Array(string elementTypeName, EquatableArray<TypedConstantInfo> items)
            {
                this.ElementTypeName = elementTypeName;
                this.Items = items;
            }

            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax()
            {
                return
                    ArrayCreationExpression(
                    ArrayType(IdentifierName(ElementTypeName))
                    .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                    .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression)
                    .AddExpressions(Items.Select(static c => c.GetSyntax()).ToArray()));
            }
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a primitive value.
        /// </summary>
        public abstract class Primitive : TypedConstantInfo
        {
            /// <summary>
            /// A <see cref="TypedConstantInfo"/> type representing a <see cref="string"/> value.
            /// </summary>
            /// <param name="Value">The input <see cref="string"/> value.</param>
            public sealed class String : TypedConstantInfo
            {
                public string Value { get; }

                public String(string value)
                {
                    this.Value = value;
                }

                /// <inheritdoc/>
                public override ExpressionSyntax GetSyntax()
                {
                    return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Value));
                }
            }

            /// <summary>
            /// A <see cref="TypedConstantInfo"/> type representing a <see cref="bool"/> value.
            /// </summary>
            /// <param name="Value">The input <see cref="bool"/> value.</param>
            public sealed class Boolean : TypedConstantInfo
            {
                public bool Value { get; }

                public Boolean(bool value)
                {
                    this.Value = value;
                }

                /// <inheritdoc/>
                public override ExpressionSyntax GetSyntax()
                {
                    return LiteralExpression(Value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }
            }

            /// <summary>
            /// A <see cref="TypedConstantInfo"/> type representing a generic primitive value.
            /// </summary>
            /// <typeparam name="T">The primitive type.</typeparam>
            /// <param name="Value">The input primitive value.</param>
            public sealed class Of<T> : TypedConstantInfo
                where T : unmanaged, IEquatable<T>
            {
                public T Value { get; }

                public Of(T value)
                {
                    this.Value = value;
                }

                /// <inheritdoc/>
                public override ExpressionSyntax GetSyntax()
                {
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Value switch {
                        byte b => Literal(b),
                        char c => Literal(c),

                        // For doubles, we need to manually format it and always add the trailing "D" suffix.
                        // This ensures that the correct type is produced if the expression was assigned to
                        // an object (eg. the literal was used in an attribute object parameter/property).
                        double d => Literal(d.ToString("R", CultureInfo.InvariantCulture) + "D", d),

                        // For floats, Roslyn will automatically add the "F" suffix, so no extra work is needed
                        float f => Literal(f),
                        int i => Literal(i),
                        long l => Literal(l),
                        sbyte sb => Literal(sb),
                        short sh => Literal(sh),
                        uint ui => Literal(ui),
                        ulong ul => Literal(ul),
                        ushort ush => Literal(ush),
                        _ => throw new ArgumentException("Invalid primitive type")
                    });
                }
            }
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a type.
        /// </summary>
        /// <param name="TypeName">The input type name.</param>
        public sealed class Type : TypedConstantInfo
        {
            public string TypeName { get; }

            public Type(string typeName)
            {
                this.TypeName = typeName;
            }

            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax()
            {
                return TypeOfExpression(IdentifierName(TypeName));
            }
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing an enum value.
        /// </summary>
        /// <param name="TypeName">The enum type name.</param>
        /// <param name="Value">The boxed enum value.</param>
        public sealed class Enum : TypedConstantInfo
        {
            public string TypeName { get; }

            public object Value { get; }

            public Enum(string typeName, object value)
            {
                this.TypeName = typeName;
                this.Value = value;
            }

            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax()
            {
                return
                    CastExpression(
                        IdentifierName(TypeName),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, ParseToken(Value.ToString())));
            }
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a <see langword="null"/> value.
        /// </summary>
        public sealed class Null : TypedConstantInfo
        {
            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax()
            {
                return LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
        }
    }
}
