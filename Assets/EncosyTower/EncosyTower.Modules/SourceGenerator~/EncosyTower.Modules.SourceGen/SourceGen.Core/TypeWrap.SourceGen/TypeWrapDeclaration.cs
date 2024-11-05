using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    public partial class TypeWrapDeclaration
    {
        public const string OBSOLETE_ATTRIBUTE = "global::System.ObsoleteAttribute";

        public TypeDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public INamedTypeSymbol FieldTypeSymbol { get; }

        public bool IsRecord { get; }

        public bool IsStruct { get; }

        public bool IsRefStruct { get; }

        public bool ExcludeConverter { get; }

        public string TypeName { get; }

        public string FullTypeName { get; }

        public string FieldTypeName { get; }

        public bool IsFieldDeclared { get; }

        public bool IsReadOnly { get; }

        public bool IsSealed { get; }

        public bool EnableNullable { get; }

        public InterfaceKind IgnoreInterfaces { get; }

        public OperatorKind IgnoreOperators { get; }

        public InterfaceKind ImplementInterfaces { get; }

        public OperatorKind ImplementOperators { get; }

        public SpecialMethodType ImplementSpecialMethods { get; }

        public string FieldName { get; }

        public ImmutableArray<IFieldSymbol> Fields { get; }

        public ImmutableArray<IPropertySymbol> Properties { get; }

        public ImmutableArray<IEventSymbol> Events { get; }

        public ImmutableArray<IMethodSymbol> Methods { get; }

        public Dictionary<OperatorKind, OpType> OperatorReturnTypeMap { get; }

        public Dictionary<OperatorKind, OpArgTypes> OperatorArgTypesMap { get; }

        public TypeWrapDeclaration(
              TypeDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , string typeName
            , bool isStruct
            , bool isRefStruct
            , bool isRecord
            , INamedTypeSymbol fieldTypeSymbol
            , string fieldName
            , bool excludeConverter
            , bool enableNullable
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            TypeName = typeName;
            FullTypeName = symbol.ToFullName();
            IsReadOnly = symbol.IsReadOnly;
            IsSealed = symbol.IsSealed;
            IsStruct = isStruct;
            IsRefStruct = isRefStruct;
            IsRecord = isRecord;
            EnableNullable = enableNullable;
            FieldTypeSymbol = fieldTypeSymbol;
            FieldName = fieldName;
            ImplementInterfaces = GetBuiltInInterfaces(fieldTypeSymbol);
            ImplementOperators = GetBuiltInOperators(fieldTypeSymbol);
            ImplementSpecialMethods |= SpecialMethodType.GetHashCode | SpecialMethodType.ToString;

            FieldTypeName = fieldTypeSymbol.ToFullName();
            ExcludeConverter = excludeConverter;

            var members = symbol.GetMembers();
            var definedMembers = new HashSet<string>(StringComparer.Ordinal);
            var globalFormat = SymbolExtensions.QualifiedMemberFormatWithGlobalPrefix;
            var implementSpecialMethods = ImplementSpecialMethods;
            var ignoreInterfaces = IgnoreInterfaces;
            var ignoredOperators = IgnoreOperators;

            foreach (var member in members)
            {
                switch (member)
                {
                    case IFieldSymbol field:
                    {
                        if (isRecord == false
                            && IsFieldDeclared == false
                            && field.Name == FieldName
                            && SymbolEqualityComparer.Default.Equals(field.Type, fieldTypeSymbol)
                        )
                        {
                            IsFieldDeclared = true;
                        }

                        definedMembers.Add(field.ToDisplayString(globalFormat));
                        break;
                    }

                    case IPropertySymbol property:
                    {
                        definedMembers.Add(property.ToDisplayString(globalFormat));
                        break;
                    }

                    case IEventSymbol @event:
                    {
                        definedMembers.Add(@event.ToDisplayString(globalFormat));
                        break;
                    }

                    case IMethodSymbol method:
                    {
                        if (NotSupported(method.MethodKind))
                        {
                            continue;
                        }

                        if (method.IsStatic == false && method.IsImplicitlyDeclared == false)
                        {
                            if (method.Parameters.Length == 0)
                            {
                                switch (method.Name)
                                {
                                    case "GetHashCode":
                                    {
                                        implementSpecialMethods &= ~SpecialMethodType.GetHashCode;
                                        break;
                                    }

                                    case "ToString":
                                    {
                                        implementSpecialMethods &= ~SpecialMethodType.ToString;
                                        break;
                                    }
                                }
                            }
                            else if (method.Parameters.Length == 1
                                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, symbol)
                            )
                            {
                                switch (method.Name)
                                {
                                    case "Equals":
                                    {
                                        ignoreInterfaces |= InterfaceKind.Equatable;
                                        break;
                                    }

                                    case "CompareTo":
                                    {
                                        ignoreInterfaces |= InterfaceKind.Comparable;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (method.IsImplicitlyDeclared == false && FindOperator(method, out var foundOp))
                        {
                            ignoredOperators |= foundOp;
                        }

                        definedMembers.Add(method.ToDisplayString(globalFormat));
                        break;
                    }
                }
            }

            if (isRecord)
            {
                ignoredOperators |= OperatorKind.Equal | OperatorKind.NotEqual;
            }

            definedMembers.Add("GetHashCode()");
            definedMembers.Add("ToString()");

            //using var interfaceArrayBuilder = ImmutableArrayBuilder<INamedTypeSymbol>.Rent();
            using var fieldArrayBuilder = ImmutableArrayBuilder<IFieldSymbol>.Rent();
            using var propertyArrayBuilder = ImmutableArrayBuilder<IPropertySymbol>.Rent();
            using var eventArrayBuilder = ImmutableArrayBuilder<IEventSymbol>.Rent();
            using var methodArrayBuilder = ImmutableArrayBuilder<IMethodSymbol>.Rent();

            var fullTypeName = FullTypeName;
            var fieldTypeMembers = fieldTypeSymbol.GetMembers();
            var interfaces = fieldTypeSymbol.AllInterfaces;
            var memberMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var genericTypeArgs = new List<ITypeSymbol>();
            var format = SymbolExtensions.QualifiedMemberFormatWithType;
            var implementOperators = ImplementOperators;
            var hasBuiltInOperators = implementOperators != OperatorKind.None;
            var implementInterfaces = ImplementInterfaces;
            var operatorReturnTypeMap = OperatorReturnTypeMap = new(OperatorKinds.All.Length);
            var operatorArgTypesMap = OperatorArgTypesMap = new(OperatorKinds.All.Length);

            foreach (var member in fieldTypeMembers)
            {
                if (member.HasAttribute(OBSOLETE_ATTRIBUTE))
                {
                    continue;
                }

                switch (member)
                {
                    case IFieldSymbol field:
                    {
                        if (field.DeclaredAccessibility == Accessibility.Public
                            && definedMembers.Contains(field.ToDisplayString(globalFormat)) == false
                        )
                        {
                            fieldArrayBuilder.Add(field);
                        }
                        break;
                    }

                    case IPropertySymbol property:
                    {
                        if (property.DeclaredAccessibility == Accessibility.Public
                            || property.ExplicitInterfaceImplementations.Length > 0
                        )
                        {
                            if (definedMembers.Contains(property.ToDisplayString(globalFormat)) == false)
                            {
                                propertyArrayBuilder.Add(property);
                            }
                        }
                        break;
                    }

                    case IEventSymbol @event:
                    {
                        if (@event.DeclaredAccessibility == Accessibility.Public
                            || @event.ExplicitInterfaceImplementations.Length > 0
                        )
                        {
                            if (definedMembers.Contains(@event.ToDisplayString(globalFormat)) == false)
                            {
                                eventArrayBuilder.Add(@event);
                            }
                        }
                        break;
                    }

                    case IMethodSymbol method:
                    {
                        if (method.DeclaredAccessibility != Accessibility.Public
                            && method.ExplicitInterfaceImplementations.Length < 1
                        )
                        {
                            continue;
                        }

                        if (Validate(method) == false)
                        {
                            continue;
                        }

                        if (definedMembers.Contains(method.ToDisplayString(globalFormat)))
                        {
                            continue;
                        }

                        if (ValidateSpecial(fieldTypeSymbol, method, ref implementInterfaces) == false)
                        {
                            continue;
                        }

                        if (hasBuiltInOperators == false && FindOperator(method, out var foundOp))
                        {
                            implementOperators |= foundOp;
                        }
                        else
                        {
                            foundOp = OperatorKind.None;
                        }

                        if (foundOp != OperatorKind.None)
                        {
                            operatorReturnTypeMap[foundOp] = GetOpType(
                                method.ReturnType, fieldTypeSymbol, fullTypeName, RetainReturnType(foundOp)
                            );

                            var methodParams = method.Parameters;
                            var methodParamsLength = methodParams.Length;

                            if (methodParamsLength > 1)
                            {
                                operatorArgTypesMap[foundOp] = new OpArgTypes(
                                      GetOpType(methodParams[0].Type, fieldTypeSymbol, fullTypeName, false)
                                    , GetOpType(methodParams[1].Type, fieldTypeSymbol, fullTypeName, false)
                                );
                            }
                            else if (methodParamsLength > 0)
                            {
                                operatorArgTypesMap[foundOp] = new OpArgTypes(
                                    GetOpType(methodParams[0].Type, fieldTypeSymbol, fullTypeName, false)
                                );
                            }

                            continue;
                        }

                        if (method.IsStatic && method.Name.StartsWith("op_", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        methodArrayBuilder.Add(method);
                        break;
                    }
                }
            }

            ImplementSpecialMethods = implementSpecialMethods;
            IgnoreInterfaces = ignoreInterfaces;
            IgnoreOperators = ignoredOperators;
            ImplementInterfaces = implementInterfaces;
            ImplementOperators = implementOperators;
            Fields = fieldArrayBuilder.ToImmutable();
            Properties = propertyArrayBuilder.ToImmutable();
            Events = eventArrayBuilder.ToImmutable();
            Methods = methodArrayBuilder.ToImmutable();
            //Interfaces = interfaceArrayBuilder.ToImmutable();
        }

        private static bool ValidateSpecial(
              INamedTypeSymbol fieldTypeSymbol
            , IMethodSymbol method
            , ref InterfaceKind implementInterfaces
        )
        {
            var result = true;

            if (method.IsStatic == false
                && method.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, fieldTypeSymbol)
            )
            {
                switch (method.Name)
                {
                    case "Equals":
                    {
                        implementInterfaces |= InterfaceKind.Equatable;
                        break;
                    }

                    case "CompareTo":
                    {
                        implementInterfaces |= InterfaceKind.Comparable;
                        break;
                    }
                }
            }

            return result;
        }

        private static OpType GetOpType(ITypeSymbol type, ITypeSymbol fieldType, string fullTypeName, bool retain)
        {
            if (retain == false && SymbolEqualityComparer.Default.Equals(type, fieldType))
            {
                return new OpType(fullTypeName, true);
            }

            return new OpType(type.ToFullName());
        }

        private static bool Validate(IMethodSymbol method)
        {
            if (NotSupported(method))
            {
                return false;
            }

            if (method.MethodKind is MethodKind.ExplicitInterfaceImplementation)
            {
                foreach (var explicitImpl in method.ExplicitInterfaceImplementations)
                {
                    if (NotSupported(explicitImpl))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool NotSupported(IMethodSymbol method)
        {
            if (method.DeclaredAccessibility == Accessibility.Public
                && method.Name is ("Equals" or "CompareTo")
                && method.Parameters.Length == 1
                && method.Parameters[0].Type.ToFullName() == "object"
            )
            {
                return true;
            }

            return NotSupported(method.MethodKind);
        }

        private static bool NotSupported(MethodKind kind)
        {
            return kind
                is MethodKind.PropertyGet
                or MethodKind.PropertySet
                or MethodKind.EventAdd
                or MethodKind.EventRemove
                or MethodKind.EventRaise
                or MethodKind.Destructor
                or MethodKind.Constructor;
        }

        private static InterfaceKind GetBuiltInInterfaces(INamedTypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Enum:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Boolean:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                    return InterfaceKind.Equatable
                        | InterfaceKind.Comparable
                        ;

                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                    return InterfaceKind.Equatable;
            }

            return InterfaceKind.None;
        }

        private static OperatorKind GetBuiltInOperators(INamedTypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Enum:
                    return OperatorKind.OnesComplement
                        | OperatorKind.Increment
                        | OperatorKind.Decrement
                        | OperatorKind.Addition // x + number equal or lesser than enum base type, equal to signed/unsigned
                        | OperatorKind.Substraction
                        | OperatorKind.BitwiseAnd
                        | OperatorKind.BitwiseOr
                        | OperatorKind.BitwiseXor
                        | OperatorKind.LeftShift
                        | OperatorKind.RightShift
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        | OperatorKind.Greater
                        | OperatorKind.Lesser
                        | OperatorKind.GreaterEqual
                        | OperatorKind.LesserEqual
                        ;

                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Decimal:
                    return OperatorKind.UnaryPlus
                        | OperatorKind.UnaryMinus
                        | OperatorKind.OnesComplement
                        | OperatorKind.Increment
                        | OperatorKind.Decrement
                        | OperatorKind.Addition
                        | OperatorKind.Substraction
                        | OperatorKind.Multiplication
                        | OperatorKind.Division
                        | OperatorKind.Remainder
                        | OperatorKind.BitwiseAnd
                        | OperatorKind.BitwiseOr
                        | OperatorKind.BitwiseXor
                        | OperatorKind.LeftShift // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators#shift-count-of-the-shift-operators
                        | OperatorKind.RightShift
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        | OperatorKind.Greater
                        | OperatorKind.Lesser
                        | OperatorKind.GreaterEqual
                        | OperatorKind.LesserEqual
                        ;

                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    return OperatorKind.UnaryPlus
                        | OperatorKind.OnesComplement
                        | OperatorKind.Increment
                        | OperatorKind.Decrement
                        | OperatorKind.Addition
                        | OperatorKind.Substraction
                        | OperatorKind.Multiplication
                        | OperatorKind.Division
                        | OperatorKind.Remainder
                        | OperatorKind.BitwiseAnd
                        | OperatorKind.BitwiseOr
                        | OperatorKind.BitwiseXor
                        | OperatorKind.LeftShift // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators#shift-count-of-the-shift-operators
                        | OperatorKind.RightShift
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        | OperatorKind.Greater
                        | OperatorKind.Lesser
                        | OperatorKind.GreaterEqual
                        | OperatorKind.LesserEqual
                        ;

                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                    return OperatorKind.Addition
                        | OperatorKind.Substraction
                        | OperatorKind.Multiplication
                        | OperatorKind.Division
                        | OperatorKind.Remainder
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        | OperatorKind.Greater
                        | OperatorKind.Lesser
                        | OperatorKind.GreaterEqual
                        | OperatorKind.LesserEqual
                        ;

                case SpecialType.System_Boolean:
                    return OperatorKind.Negation
                        | OperatorKind.True
                        | OperatorKind.False
                        | OperatorKind.LogicalAnd
                        | OperatorKind.LogicalOr
                        | OperatorKind.LogicalXor
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        ;

                case SpecialType.System_String:
                    return OperatorKind.Addition
                        | OperatorKind.Equal
                        | OperatorKind.NotEqual
                        ;

                case SpecialType.System_Object:
                case SpecialType.System_MulticastDelegate:
                case SpecialType.System_Delegate:
                case SpecialType.System_Array:
                case SpecialType.System_Collections_IEnumerable:
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_ICollection_T:
                case SpecialType.System_Collections_IEnumerator:
                case SpecialType.System_Collections_Generic_IEnumerator_T:
                case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                case SpecialType.System_IDisposable:
                    return OperatorKind.Equal
                        | OperatorKind.NotEqual
                        ;
            }

            return OperatorKind.None;
        }

        private static bool RetainReturnType(OperatorKind kind)
        {
            return kind is (
                   OperatorKind.True
                or OperatorKind.False
                or OperatorKind.LogicalAnd
                or OperatorKind.LogicalOr
                or OperatorKind.LogicalXor
                or OperatorKind.Equal
                or OperatorKind.NotEqual
                or OperatorKind.Greater
                or OperatorKind.Lesser
                or OperatorKind.GreaterEqual
                or OperatorKind.LesserEqual
            );
        }

        private static string DetermineReturnType(OperatorKind kind, string fullTypeName)
        {
            switch (kind)
            {
                case OperatorKind.True:
                case OperatorKind.False:
                case OperatorKind.LogicalAnd:
                case OperatorKind.LogicalOr:
                case OperatorKind.LogicalXor:
                case OperatorKind.Equal:
                case OperatorKind.NotEqual:
                case OperatorKind.Greater:
                case OperatorKind.Lesser:
                case OperatorKind.GreaterEqual:
                case OperatorKind.LesserEqual:
                    return "bool";
            }

            return fullTypeName;
        }

        private static OpArgTypes DetermineArgTypes(
              OperatorKind kind
            , string fullTypeName
            , SpecialType fieldSpecialType
            , SpecialType fieldUnderlyingSpecialType
        )
        {
            switch (kind)
            {
                case OperatorKind.UnaryPlus:
                case OperatorKind.UnaryMinus:
                case OperatorKind.Negation:
                case OperatorKind.OnesComplement:
                case OperatorKind.Increment:
                case OperatorKind.Decrement:
                case OperatorKind.True:
                case OperatorKind.False:
                    return new(new(fullTypeName, true));

                case OperatorKind.Addition:
                {
                    if (fieldSpecialType != SpecialType.System_Enum)
                    {
                        return new(new(fullTypeName, true), new(fullTypeName, true));
                    }

                    return fieldUnderlyingSpecialType switch {
                        SpecialType.System_SByte => new(new(fullTypeName, true), new("sbyte")),
                        SpecialType.System_Byte => new(new(fullTypeName, true), new("byte")),
                        SpecialType.System_Int16 => new(new(fullTypeName, true), new("short")),
                        SpecialType.System_UInt16 => new(new(fullTypeName, true), new("ushort")),
                        SpecialType.System_Int32 => new(new(fullTypeName, true), new("int")),
                        SpecialType.System_UInt32 => new(new(fullTypeName, true), new("uint")),
                        SpecialType.System_Int64 => new(new(fullTypeName, true), new("long")),
                        SpecialType.System_UInt64 => new(new(fullTypeName, true), new("ulong")),
                        _ => new(new(fullTypeName, true), new("sbyte")),
                    };
                }

                case OperatorKind.Substraction:
                case OperatorKind.Multiplication:
                case OperatorKind.Division:
                case OperatorKind.Remainder:
                case OperatorKind.LogicalAnd:
                case OperatorKind.LogicalOr:
                case OperatorKind.LogicalXor:
                case OperatorKind.BitwiseAnd:
                case OperatorKind.BitwiseOr:
                case OperatorKind.BitwiseXor:
                case OperatorKind.Equal:
                case OperatorKind.NotEqual:
                case OperatorKind.Greater:
                case OperatorKind.Lesser:
                case OperatorKind.GreaterEqual:
                case OperatorKind.LesserEqual:
                    return new(new(fullTypeName, true), new(fullTypeName, true));

                case OperatorKind.LeftShift:
                case OperatorKind.RightShift:
                case OperatorKind.UnsignedRightShift:
                    return new(new(fullTypeName, true), new("int"));

                default:
                    return default;
            }
        }
        private static bool FindOperator(IMethodSymbol method, out OperatorKind result)
        {
            if (method.IsStatic == false)
            {
                result = OperatorKind.None;
                return false;
            }

            var returnBool = method.ReturnType.SpecialType == SpecialType.System_Boolean;

            result = method.Name switch {
                "op_UnaryPlus" => OperatorKind.UnaryPlus,
                "op_UnaryNegation" => OperatorKind.UnaryMinus,
                "op_LogicalNot" => OperatorKind.Negation,
                "op_OnesComplement" => OperatorKind.OnesComplement,
                "op_Increment" => OperatorKind.Increment,
                "op_Decrement" => OperatorKind.Decrement,
                "op_True" => OperatorKind.True,
                "op_False" => OperatorKind.False,
                "op_Addition" => OperatorKind.Addition,
                "op_Subtraction" => OperatorKind.Substraction,
                "op_Multiply" => OperatorKind.Multiplication,
                "op_Division" => OperatorKind.Division,
                "op_Modulus" => OperatorKind.Remainder,
                "op_BitwiseAnd" => returnBool ? OperatorKind.LogicalAnd : OperatorKind.BitwiseAnd,
                "op_BitwiseOr" => returnBool ? OperatorKind.LogicalOr : OperatorKind.BitwiseOr,
                "op_ExclusiveOr" => returnBool ? OperatorKind.LogicalXor : OperatorKind.BitwiseXor,
                "op_LeftShift" => OperatorKind.LeftShift,
                "op_RightShift" => OperatorKind.RightShift,
                "op_UnsignedRightShift" => OperatorKind.UnsignedRightShift,
                "op_Equality" => OperatorKind.Equal,
                "op_Inequality" => OperatorKind.NotEqual,
                "op_GreaterThan" => OperatorKind.Greater,
                "op_LessThan" => OperatorKind.Lesser,
                "op_GreaterThanOrEqual" => OperatorKind.GreaterEqual,
                "op_LessThanOrEqual" => OperatorKind.LesserEqual,
                _ => OperatorKind.None,
            };

            return result != OperatorKind.None;
        }
    }
}
