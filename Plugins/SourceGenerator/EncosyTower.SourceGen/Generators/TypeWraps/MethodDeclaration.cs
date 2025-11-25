using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public struct MethodDeclaration
    {
        public string name;
        public string returnTypeName;
        public string typeParameters;
        public string typeParameterConstraints;
        public string parameters;
        public string arguments;
        public int explicitInterfaceImplementationsLength;
        public bool sameType;
        public bool isPublic;
        public bool isUnsafe;
        public bool isOverride;
        public bool isReadOnly;
        public bool isStatic;
        public bool returnsVoid;
        public RefKind refKind;

        public static MethodDeclaration Create(
              IMethodSymbol method
            , INamedTypeSymbol fieldTypeSymbol
            , bool enableNullable
        )
        {
            var p = Printer.DefaultLarge;
            var name = method.ToDisplayString(SymbolExtensions.MemberNameFormat);
            var typeParameters = string.Empty;
            var typeParameterConstraints = string.Empty;
            var parameters = string.Empty;
            var arguments = string.Empty;

            if (method.TypeParameters.Length > 0)
            {
                p.Clear();

                WriteTypeParams(ref p, method.TypeParameters);

                typeParameters = p.Result;

                p.Clear();

                WriteTypeParamConstraints(ref p, method.TypeParameters);

                typeParameterConstraints = p.Result;
            }

            var isUnsafe = false;

            if (method.ReturnType is IPointerTypeSymbol)
            {
                isUnsafe = true;
            }

            if (method.Parameters.Length > 0)
            {
                p.Clear();

                var propParams = method.Parameters;
                var last = propParams.Length - 1;

                for (var i = 0; i <= last; i++)
                {
                    var param = propParams[i];
                    var paramTypeName = param.Type.ToFullName();
                    var isNullable = param.NullableAnnotation == NullableAnnotation.Annotated && enableNullable;

                    WriteInlineAttributes(ref p, param);
                    p.PrintIf(param.RefKind == RefKind.Ref, "ref ");
                    p.PrintIf(param.RefKind == RefKind.Out, "out ");
                    p.PrintIf(param.RefKind == RefKind.In, "in ");
                    p.Print(paramTypeName);
                    p.PrintIf(isNullable, "? ", " ");
                    p.Print(param.Name);
                    p.PrintIf(i < last, ", ");

                    if (isUnsafe == false && param.Type is IPointerTypeSymbol)
                    {
                        isUnsafe = true;
                    }
                }

                parameters = p.Result;

                p.Clear();

                for (var i = 0; i <= last; i++)
                {
                    var param = propParams[i];

                    p.PrintIf(param.RefKind == RefKind.Ref, "ref ");
                    p.PrintIf(param.RefKind == RefKind.Out, "out ");
                    p.PrintIf(param.RefKind == RefKind.In, "in ");
                    p.Print(param.Name);
                    p.PrintIf(i < last, ", ");
                }

                arguments = p.Result;
            }

            return new MethodDeclaration {
                name = name,
                returnTypeName = method.ReturnType.ToFullName(),
                sameType = SymbolEqualityComparer.Default.Equals(method.ReturnType, fieldTypeSymbol),
                typeParameters = typeParameters,
                typeParameterConstraints = typeParameterConstraints,
                parameters = parameters,
                arguments = arguments,
                isPublic = method.DeclaredAccessibility == Accessibility.Public,
                isUnsafe = isUnsafe,
                isReadOnly = method.IsReadOnly,
                isStatic = method.IsStatic,
                isOverride = method.IsOverride,
                refKind = method.RefKind,
                explicitInterfaceImplementationsLength = method.ExplicitInterfaceImplementations.Length,
                returnsVoid = method.ReturnsVoid,
            };
        }

        static void WriteTypeParams(ref Printer p, ImmutableArray<ITypeParameterSymbol> typeParameters)
        {
            p.Print("<");

            var last = typeParameters.Length - 1;

            for (var i = 0; i <= last; i++)
            {
                var param = typeParameters[i];

                p.Print(param.Name);
                p.PrintIf(i < last, ", ");
            }

            p.Print(">");
        }

        static void WriteTypeParamConstraints(ref Printer p, ImmutableArray<ITypeParameterSymbol> typeParameters)
        {
            var last = typeParameters.Length - 1;
            var constraints = new List<string>(10);

            for (var i = 0; i <= last; i++)
            {
                constraints.Clear();

                var param = typeParameters[i];

                if (param.HasReferenceTypeConstraint)
                {
                    if (param.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated)
                    {
                        constraints.Add("class?");
                    }
                    else
                    {
                        constraints.Add("class");
                    }
                }

                if (param.HasValueTypeConstraint)
                {
                    constraints.Add("struct");
                }

                if (param.HasUnmanagedTypeConstraint)
                {
                    constraints.Add("unmanaged");
                }

                if (param.HasNotNullConstraint)
                {
                    constraints.Add("notnull");
                }

                var constraintTypes = param.ConstraintTypes;
                var constraintNullable = param.ConstraintNullableAnnotations;

                for (var k = 0; k < constraintTypes.Length; k++)
                {
                    var constraintType = constraintTypes[k];

                    if (constraintNullable[k] == NullableAnnotation.Annotated)
                    {
                        constraints.Add($"{constraintType.ToFullName()}?");
                    }
                    else
                    {
                        constraints.Add(constraintType.ToFullName());
                    }
                }

                if (param.HasConstructorConstraint)
                {
                    constraints.Add("new()");
                }

                if (constraints.Count > 0)
                {
                    p = p.IncreasedIndent();
                    p.PrintBeginLine("where ").Print(param.Name).Print(" : ");

                    var lastConstraint = constraints.Count - 1;

                    for (var k = 0; k <= lastConstraint; k++)
                    {
                        p.Print(constraints[k]);
                        p.PrintIf(k < lastConstraint, ", ");
                    }

                    p.PrintEndLine();
                    p = p.DecreasedIndent();
                }
            }
        }

        static void WriteInlineAttributes(ref Printer p, ISymbol symbol)
        {
            var attribs = symbol.GetAttributes();
            var attribsLength = attribs.Length;
            var attribLast = attribsLength - 1;

            for (var i = 0; i < attribsLength; i++)
            {
                var attrib = attribs[i];

                if (attrib.AttributeClass is not ITypeSymbol type)
                {
                    continue;
                }

                var attribName = type.ToFullName();

                if (attribName == "global::System.Runtime.CompilerServices.NullableAttribute")
                {
                    continue;
                }

                p.Print("[").Print(attribName).Print("(");

                var constructorArgs = attrib.ConstructorArguments;
                var length = constructorArgs.Length;
                var last = length - 1;

                for (var k = 0; k < length; k++)
                {
                    var arg = constructorArgs[k];

                    if (IsValid(arg) == false) continue;

                    WriteArgValue(ref p, arg);
                    p.PrintIf(k < last, ", ");
                }

                var namedArgs = attrib.NamedArguments;

                foreach (var item in namedArgs)
                {
                    var arg = item.Value;

                    if (IsValid(arg) == false) continue;

                    p.Print(", ").Print(item.Key).Print(": ");
                    WriteArgValue(ref p, arg);
                }

                p.Print(")]").PrintIf(i == last, " ");
            }

            static bool IsValid(TypedConstant arg)
            {
                return arg.Kind != TypedConstantKind.Error
                    && arg.Type != null;
            }

            static void WriteArgValue(ref Printer p, TypedConstant arg)
            {
                if (arg.IsNull)
                {
                    p.Print("null");
                    return;
                }

                switch (arg.Kind)
                {
                    case TypedConstantKind.Primitive:
                    {
                        if (arg.Value is bool valBool)
                        {
                            p.Print(valBool ? "true" : "false");
                        }
                        else if (arg.Value is string valStr)
                        {
                            p.Print("\"").Print(valStr).Print("\"");
                        }
                        else
                        {
                            p.Print(arg.Value.ToString());
                        }
                        break;
                    }

                    case TypedConstantKind.Enum:
                    {
                        p.Print("(").Print(arg.Type.ToFullName()).Print(")").Print(arg.Value.ToString());
                        break;
                    }

                    case TypedConstantKind.Type:
                    {
                        p.Print("typeof(").Print(((ITypeSymbol)arg.Value).ToFullName()).Print(")");
                        break;
                    }

                    case TypedConstantKind.Array:
                    {
                        p.Print("new ").Print(arg.Type.ToFullName()).Print(" { ");

                        var values = arg.Values;
                        var length = values.Length;
                        var last = length - 1;

                        for (var i = 0; i < length; i++)
                        {
                            WriteArgValue(ref p, values[i]);
                            p.PrintIf(i < last, ", ");
                        }

                        p.Print(" }");
                        break;
                    }
                }
            }
        }

    }
}
