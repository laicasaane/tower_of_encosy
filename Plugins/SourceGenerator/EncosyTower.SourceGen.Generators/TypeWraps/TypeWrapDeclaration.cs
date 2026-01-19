using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public partial struct TypeWrapDeclaration : IEquatable<TypeWrapDeclaration>
    {
        public const string OBSOLETE_ATTRIBUTE = "global::System.ObsoleteAttribute";
        public const string FIELD_NAME_FORMAT = "{0}Of{1}";

        public Location Location { get; }

        public string HintName { get; }

        public string SourceFilePath { get; }

        public string OpeningSource { get; }

        public string ClosingSource { get; }

        public string TypeName { get; }

        public string TypeNameWithTypeParams { get; }

        public string FullTypeName { get; }

        public string FieldTypeName { get; }

        public string FieldEnumUnderlyingTypeName { get; }

        public string FieldName { get; }

        public InterfaceKind IgnoreInterfaceMethods { get; }

        public OperatorKind IgnoreOperators { get; }

        public InterfaceKind ImplementInterfaces { get; }

        public OperatorKind ImplementOperators { get; }

        public SpecialMethodType ImplementSpecialMethods { get; }

        public SpecialType FieldSpecialType { get; }

        public SpecialType FieldUnderlyingSpecialType { get; }

        public bool IsRecord { get; }

        public bool IsStruct { get; }

        public bool IsRefStruct { get; }

        public bool FieldTypeIsInterface { get; }

        public bool ExcludeConverter { get; }

        public bool IsFieldDeclared { get; }

        public bool IsFieldEnum { get; }

        public bool IsReadOnly { get; }

        public bool FieldTypeIsReadOnly { get; }

        public bool IsSealed { get; }

        public bool EnableNullable { get; }

        public EquatableArray<FieldDeclaration> Fields { get; }

        public EquatableArray<PropertyDeclaration> Properties { get; }

        public EquatableArray<EventDeclaration> Events { get; }

        public EquatableArray<MethodDeclaration> Methods { get; }

        public Dictionary<OperatorKind, HashSet<Operator>> OperatorMap { get; }

        public readonly bool IsValid
            => string.IsNullOrEmpty(FullTypeName) == false
            && string.IsNullOrEmpty(FieldTypeName) == false
            && string.IsNullOrEmpty(FieldName) == false;

        public TypeWrapDeclaration(
              Location location
            , string hintName
            , string sourceFilePath
            , string openingSource
            , string closingSource
            , INamedTypeSymbol symbol
            , string typeName
            , string typeNameWithTypeParams
            , bool isStruct
            , bool isRefStruct
            , bool isRecord
            , INamedTypeSymbol fieldTypeSymbol
            , string fieldName
            , bool excludeConverter
            , bool enableNullable
        )
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = FIELD_NAME_FORMAT;
            }

            Location = location;
            HintName = hintName;
            SourceFilePath = sourceFilePath;
            OpeningSource = openingSource;
            ClosingSource = closingSource;
            TypeName = typeName;
            TypeNameWithTypeParams = typeNameWithTypeParams;
            FullTypeName = symbol.ToFullName();
            IsReadOnly = symbol.IsReadOnly;
            IsSealed = symbol.IsSealed;
            IsStruct = isStruct;
            IsRefStruct = isRefStruct;
            IsRecord = isRecord;
            EnableNullable = enableNullable;
            FieldTypeIsReadOnly = fieldTypeSymbol.IsReadOnly;
            FieldSpecialType = fieldTypeSymbol.SpecialType;
            FieldTypeIsInterface = fieldTypeSymbol.TypeKind == TypeKind.Interface;
            ImplementInterfaces = GetBuiltInInterfaces(fieldTypeSymbol);
            ImplementOperators = GetBuiltInOperators(fieldTypeSymbol);
            ImplementSpecialMethods = SpecialMethodType.CompareTo
                | SpecialMethodType.Equals
                | SpecialMethodType.GetHashCode
                | SpecialMethodType.ToString;

            if (fieldTypeSymbol.EnumUnderlyingType is INamedTypeSymbol underlyingTypeSymbol)
            {
                FieldSpecialType = SpecialType.System_Enum;
                FieldUnderlyingSpecialType = underlyingTypeSymbol.SpecialType;
            }
            else
            {
                FieldUnderlyingSpecialType = SpecialType.None;
            }

            var fieldTypeAsIdentifier = fieldTypeSymbol.ToSimpleNoSpecialTypeValidIdentifier();

            FieldName = string.Format(fieldName, isStruct ? "value" : "instance", fieldTypeAsIdentifier);
            FieldTypeName = fieldTypeSymbol.ToFullName();
            ExcludeConverter = excludeConverter;
            IsFieldEnum = fieldTypeSymbol.IsEnumType();
            FieldEnumUnderlyingTypeName = IsFieldEnum ? fieldTypeSymbol.EnumUnderlyingType.ToFullName() : string.Empty;

            var members = symbol.GetMembers();
            var definedMembers = new HashSet<string>(StringComparer.Ordinal);
            var globalFormat = SymbolExtensions.QualifiedMemberFormatWithGlobalPrefix;
            var implementSpecialMethods = ImplementSpecialMethods;
            var implementInterfaces = ImplementInterfaces;
            var ignoreInterfaceMethods = IgnoreInterfaceMethods = default;
            var ignoredOperators = IgnoreOperators = default;

            IsFieldDeclared = false;

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
                            else if (method.Parameters.Length == 1)
                            {
                                var paramType = method.Parameters[0].Type;

                                if (SymbolEqualityComparer.Default.Equals(paramType, symbol))
                                {
                                    switch (method.Name)
                                    {
                                        case "Equals":
                                        {
                                            ignoreInterfaceMethods |= InterfaceKind.EquatableT;
                                            implementInterfaces |= InterfaceKind.EquatableT;
                                            break;
                                        }

                                        case "CompareTo":
                                        {
                                            ignoreInterfaceMethods |= InterfaceKind.ComparableT;
                                            implementInterfaces |= InterfaceKind.ComparableT;
                                            break;
                                        }
                                    }
                                }
                                else if (paramType.ToFullName() == "object")
                                {
                                    switch (method.Name)
                                    {
                                        case "Equals":
                                        {
                                            implementSpecialMethods &= ~SpecialMethodType.Equals;
                                            break;
                                        }

                                        case "CompareTo":
                                        {
                                            implementSpecialMethods &= ~SpecialMethodType.CompareTo;
                                            ignoreInterfaceMethods |= InterfaceKind.Comparable;
                                            implementInterfaces |= InterfaceKind.Comparable;
                                            break;
                                        }
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

            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldDeclaration>.Rent();
            using var propertyArrayBuilder = ImmutableArrayBuilder<PropertyDeclaration>.Rent();
            using var eventArrayBuilder = ImmutableArrayBuilder<EventDeclaration>.Rent();
            using var methodArrayBuilder = ImmutableArrayBuilder<MethodDeclaration>.Rent();

            var fullTypeName = FullTypeName;
            var fieldTypeMembers = fieldTypeSymbol.GetMembers();
            var interfaces = fieldTypeSymbol.AllInterfaces;
            var memberMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var genericTypeArgs = new List<ITypeSymbol>();
            var format = SymbolExtensions.QualifiedMemberFormatWithType;
            var implementOperators = ImplementOperators;
            var hasBuiltInOperators = implementOperators != OperatorKind.None;
            var operatorMap = OperatorMap = new(OperatorKinds.All.Length);

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
                            fieldArrayBuilder.Add(FieldDeclaration.Create(field, fieldTypeSymbol));
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
                                propertyArrayBuilder.Add(PropertyDeclaration.Create(
                                      property
                                    , fieldTypeSymbol
                                    , IsStruct
                                    , IsReadOnly
                                ));
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
                                eventArrayBuilder.Add(EventDeclaration.Create(@event, fieldTypeSymbol));
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

                        if (ValidateSpecial(
                              fieldTypeSymbol
                            , method
                            , ref implementInterfaces
                            , ref implementSpecialMethods
                        ) == false)
                        {
                            continue;
                        }

                        if (definedMembers.Contains(method.ToDisplayString(globalFormat)))
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
                            if (operatorMap.TryGetValue(foundOp, out var operators) == false)
                            {
                                operatorMap[foundOp] = operators = new HashSet<Operator>();
                            }

                            var returnType = GetOpType(
                                method.ReturnType, fieldTypeSymbol, fullTypeName, RetainReturnType(foundOp)
                            );

                            var methodParams = method.Parameters;
                            var methodParamsLength = methodParams.Length;

                            if (methodParamsLength > 1)
                            {
                                var first = GetOpType(methodParams[0].Type, fieldTypeSymbol, fullTypeName, false);
                                var second = GetOpType(methodParams[1].Type, fieldTypeSymbol, fullTypeName, false);

                                if (isRecord && foundOp is OperatorKind.EqualCustom or OperatorKind.NotEqualCustom
                                    && first.IsWrapper && second.IsWrapper
                                )
                                {
                                    var second2 = new OpType(FieldTypeName);
                                    operators.Add(new Operator(returnType, new OpArgTypes(first, second2)));

                                    var first2 = new OpType(FieldTypeName);
                                    operators.Add(new Operator(returnType, new OpArgTypes(first2, second)));
                                }
                                else
                                {
                                    operators.Add(new Operator(returnType, new OpArgTypes(first, second)));
                                }
                            }
                            else if (methodParamsLength > 0)
                            {
                                var first = GetOpType(methodParams[0].Type, fieldTypeSymbol, fullTypeName, false);
                                operators.Add(new Operator(returnType, new OpArgTypes(first)));
                            }

                            continue;
                        }

                        if (method.IsStatic && method.Name.StartsWith("op_", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        methodArrayBuilder.Add(MethodDeclaration.Create(
                              method
                            , fieldTypeSymbol
                            , enableNullable
                        ));
                        break;
                    }
                }
            }

            ImplementSpecialMethods = implementSpecialMethods;
            IgnoreInterfaceMethods = ignoreInterfaceMethods;
            IgnoreOperators = ignoredOperators;
            ImplementInterfaces = implementInterfaces;
            ImplementOperators = implementOperators;
            Fields = fieldArrayBuilder.ToImmutable();
            Properties = propertyArrayBuilder.ToImmutable();
            Events = eventArrayBuilder.ToImmutable();
            Methods = methodArrayBuilder.ToImmutable();
        }

        private static bool ValidateSpecial(
              INamedTypeSymbol fieldTypeSymbol
            , IMethodSymbol method
            , ref InterfaceKind implementInterfaces
            , ref SpecialMethodType implementSpecialMethods
        )
        {
            var result = true;

            if (method.IsStatic == false && method.Parameters.Length == 1)
            {
                var paramType = method.Parameters[0].Type;

                if (SymbolEqualityComparer.Default.Equals(paramType, fieldTypeSymbol))
                {
                    switch (method.Name)
                    {
                        case "Equals":
                        {
                            implementInterfaces |= InterfaceKind.EquatableT;
                            break;
                        }

                        case "CompareTo":
                        {
                            implementInterfaces |= InterfaceKind.ComparableT;
                            break;
                        }
                    }
                }
                else if (paramType.ToFullName() == "object")
                {
                    switch (method.Name)
                    {
                        case "Equals":
                        {
                            implementSpecialMethods |= SpecialMethodType.Equals;
                            break;
                        }

                        case "CompareTo":
                        {
                            implementSpecialMethods |= SpecialMethodType.CompareTo;
                            implementInterfaces |= InterfaceKind.Comparable;
                            break;
                        }
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
            var specialType = type.IsEnumType() ? SpecialType.System_Enum : type.SpecialType;

            switch (specialType)
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
                    return InterfaceKind.EquatableT
                        | InterfaceKind.Comparable
                        | InterfaceKind.ComparableT
                        ;

                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                    return InterfaceKind.EquatableT;
            }

            return InterfaceKind.None;
        }

        private static OperatorKind GetBuiltInOperators(INamedTypeSymbol type)
        {
            var specialType = type.IsEnumType() ? SpecialType.System_Enum : type.SpecialType;

            switch (specialType)
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
                "op_Equality" => DoesReturnBool(method) ? OperatorKind.Equal : OperatorKind.EqualCustom,
                "op_Inequality" => DoesReturnBool(method) ? OperatorKind.NotEqual : OperatorKind.NotEqualCustom,
                "op_GreaterThan" => OperatorKind.Greater,
                "op_LessThan" => OperatorKind.Lesser,
                "op_GreaterThanOrEqual" => OperatorKind.GreaterEqual,
                "op_LessThanOrEqual" => OperatorKind.LesserEqual,
                _ => OperatorKind.None,
            };

            return result != OperatorKind.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DoesReturnBool(IMethodSymbol method)
        {
            return method.ReturnType.ToFullName() == "bool";
        }

        public readonly override bool Equals(object obj)
            => obj is TypeWrapDeclaration other && Equals(other);

        public readonly bool Equals(TypeWrapDeclaration other)
            => Location == other.Location
            && string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal)
            && string.Equals(FieldTypeName, other.FieldTypeName, StringComparison.Ordinal)
            && string.Equals(FieldEnumUnderlyingTypeName, other.FieldEnumUnderlyingTypeName, StringComparison.Ordinal)
            && string.Equals(FieldName, other.FieldName, StringComparison.Ordinal)
            && ExcludeConverter == other.ExcludeConverter
            && Fields.Equals(other.Fields)
            && Properties.Equals(other.Properties)
            && Events.Equals(other.Events)
            && Methods.Equals(other.Methods)
            ;

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(Location);
            hash.Add(FullTypeName);
            hash.Add(FieldTypeName);
            hash.Add(FieldEnumUnderlyingTypeName);
            hash.Add(FieldName);
            hash.Add(ExcludeConverter);
            hash.Add(Fields);
            hash.Add(Properties);
            hash.Add(Events);
            hash.Add(Methods);
            return hash.ToHashCode();
        }

        public readonly struct Operator : IEquatable<Operator>
        {
            public readonly OpType ReturnType;
            public readonly OpArgTypes ArgTypes;

            public Operator(OpType returnType, OpArgTypes argTypes)
            {
                ReturnType = returnType;
                ArgTypes = argTypes;
            }

            public void Deconstruct(out OpType returnType, out OpArgTypes argTypes)
            {
                returnType = ReturnType;
                argTypes = ArgTypes;
            }

            public bool Equals(Operator other)
                => ReturnType.Equals(other.ReturnType) && ArgTypes.Equals(other.ArgTypes);

            public override bool Equals(object obj)
                => obj is Operator other && Equals(other);

            public override int GetHashCode()
                => HashValue.Combine(ReturnType, ArgTypes);
        }
    }
}
