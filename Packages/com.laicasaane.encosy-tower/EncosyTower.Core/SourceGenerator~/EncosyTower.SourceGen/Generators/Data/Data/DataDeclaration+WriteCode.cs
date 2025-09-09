﻿using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Data.Data
{
    using static EncosyTower.SourceGen.Generators.Data.Helpers;

    partial class DataDeclaration
    {
        public string WriteCode()
        {
            var keyword = Symbol.IsValueType ? "struct" : "class";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine("[global::System.Serializable]");
            p.PrintBeginLine()
                .Print($"partial {keyword} ").Print(TypeName)
                .Print(" : ")
                .PrintIf(HasBaseType, $"{BaseTypeName}, ")
                .Print($"global::System.IEquatable<{TypeName}>");

            if (IdPropertyType != null)
            {
                p.Print(", global::EncosyTower.Data.IDataWithId<")
                    .Print(IdPropertyType.ToFullName()).Print(">");
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                WriteFieldOrPropertyByOrders(ref p);
                WriteGetHashCodeMethod(ref p);
                WriteGetHashCodeInternalMethod(ref p);
                WriteEqualsMethod(ref p);
                WriteIEquatableMethod(ref p);

                if (Symbol.IsValueType == false)
                {
                    WriteEqualsInternalMethod(ref p);
                    WriteOverrideIEquatableMethod(ref p);
                }

                WriteAsReadOnlyMethod(ref p);
                WriteSetValues_TypeMethod(ref p);
                WriteEqualityOperators(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            WriteReadOnlyStruct(ref p);

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFieldOrPropertyByOrders(ref Printer p)
        {
            var orders = Orders;
            var propRefs = PropRefs;
            var fieldRefs = FieldRefs;
            var readonlyKeyword = Symbol.IsValueType ? "readonly " : "";
            var accessKeyword = FieldPolicy switch {
                DataFieldPolicy.Public => "public",
                DataFieldPolicy.Internal => "internal",
                _ => "private",
            };

            foreach (var order in orders)
            {
                var index = order.index;

                if (order.isPropRef)
                {
                    WriteField(ref p, propRefs[index], accessKeyword, readonlyKeyword);
                }
                else
                {
                    WriteProperty(ref p, fieldRefs[index]);
                }
            }
        }

        private void WriteField(ref Printer p, PropertyRef prop, string accessKeyword, string readonlyKeyword)
        {
            if (prop.FieldIsImplemented)
            {
                return;
            }

            var fieldName = prop.FieldName;

            p.PrintLine(string.Format(GENERATED_FIELD_FROM_PROPERTY_ATTRIBUTE, prop.Property.Name));
            p.PrintLine(GENERATED_CODE);

            var withSerializeField = false;

            foreach (var (fullTypeName, attribute) in prop.ForwardedFieldAttributes)
            {
                if (fullTypeName == SERIALIZE_FIELD_ATTRIBUTE)
                {
                    withSerializeField = true;
                }

                p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
            }

            if (withSerializeField == false && ReferenceUnityEngine)
            {
                p.PrintLine($"[{SERIALIZE_FIELD_ATTRIBUTE}]");
            }

            var fieldTypeName = GetFieldTypeName(prop.FieldType, prop.FieldCollection);
            var propTypeName = prop.PropertyType.ToFullName();
            var mustCast = SymbolEqualityComparer.Default.Equals(prop.FieldType, prop.PropertyType) == false;
            var casting = mustCast ? $"({prop.FieldType.ToFullName()})" : string.Empty;

            GetTypeNames(
                  prop.PropertyType
                , prop.FieldCollection
                , out var mutableTypeName
                , out _
                , out var sameType
            );

            p.PrintLine($"{accessKeyword} {fieldTypeName} {prop.FieldName};");
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"private {readonlyKeyword}{propTypeName} Get_{prop.Property.Name}()");
            p.OpenScope();
            {
                p.PrintBeginLine($"return ")
                    .PrintIf(mustCast, $"({propTypeName})")
                    .PrintEndLine($"this.{fieldName};");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine($"private void Set_{prop.Property.Name}(")
                .PrintIf(prop.ImplicitlyConvertible, propTypeName, fieldTypeName)
                .PrintEndLine(" value)");
            p.OpenScope();
            {
                p.PrintBeginLine($"this.{fieldName} = ");

                if ((IsMutable && WithoutPropertySetters == false)
                    || (IsMutable && prop.FieldCollection.Kind == CollectionKind.Array)
                    || sameType
                )
                {
                    p.PrintIf(mustCast, casting).PrintEndLine("value;");
                }
                else if (prop.FieldCollection.Kind == CollectionKind.Array)
                {
                    p.PrintIf(mustCast, casting).PrintEndLine("value.ToArray();");
                }
                else if (mustCast)
                {
                    p.Print(casting).PrintEndLine("value;");
                }
                else
                {
                    p.PrintEndLine($"({mutableTypeName})value;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteProperty(ref Printer p, FieldRef field)
        {
            if (field.ImplementedProperty is not null)
            {
                return;
            }

            var fieldName = field.Field.Name;

            GetTypeNames(
                  field.PropertyType
                , field.FieldCollection
                , out var mutableTypeName
                , out var immutableTypeName
                , out var sameType
            );

            p.PrintLine(string.Format(GENERATED_PROPERTY_FROM_FIELD_ATTRIBUTE, fieldName, field.FieldType.ToFullName()));
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);

            foreach (var attribute in field.ForwardedPropertyAttributes)
            {
                p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
            }

            var mustCast = SymbolEqualityComparer.Default.Equals(field.FieldType, field.PropertyType) == false;
            var typeName = IsMutable ? mutableTypeName : immutableTypeName;

            p.PrintLine($"public {typeName} {field.PropertyName}");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ")
                    .PrintIf(mustCast, $"({typeName})")
                    .PrintEndLine($"this.{fieldName};");

                p.PrintEndLine();
                p.PrintLine(AGGRESSIVE_INLINING);

                var casting = mustCast ? $"({field.FieldType.ToFullName()})" : string.Empty;

                if (IsMutable && WithoutPropertySetters == false)
                {
                    p.PrintBeginLine($"set => this.{fieldName} = ")
                        .PrintIf(mustCast, casting)
                        .PrintEndLine("value;");
                }
                else if ((IsMutable && field.FieldCollection.Kind == CollectionKind.Array) || sameType)
                {
                    p.PrintBeginLine($"init => this.{fieldName} = ")
                        .PrintIf(mustCast, casting)
                        .PrintEndLine("value;");
                }
                else if (field.FieldCollection.Kind == CollectionKind.Array)
                {
                    p.PrintBeginLine($"init => this.{fieldName} = ")
                        .PrintIf(mustCast, casting)
                        .PrintEndLine("value.ToArray();");
                }
                else if (mustCast)
                {
                    p.PrintLine($"init => this.{fieldName} = {casting}value;");
                }
                else
                {
                    p.PrintLine($"init => this.{fieldName} = ({mutableTypeName})value;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetHashCodeMethod(ref Printer p)
        {
            if (HasGetHashCodeMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = GetHashCodeInternal();");
                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetHashCodeInternalMethod(ref Printer p)
        {
            var fromBase = false;

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);

            if (HasBaseType == false && IsSealed == false)
            {
                p.PrintBeginLine("protected virtual ");
            }
            else if (HasBaseType == true)
            {
                fromBase = true;
                p.PrintBeginLine("protected override ");
            }
            else
            {
                p.PrintBeginLine("private ");
            }

            p.PrintEndLine("global::System.HashCode GetHashCodeInternal()");
            p.OpenScope();
            {
                p.PrintBeginLine("var hash = ")
                    .PrintIf(fromBase, "base.GetHashCodeInternal()", "new global::System.HashCode()")
                    .PrintEndLine(";");

                foreach (var field in FieldRefs)
                {
                    p.PrintLine($"hash.Add({field.Field.Name});");
                }

                foreach (var prop in PropRefs)
                {
                    p.PrintLine($"hash.Add({prop.FieldName});");
                }

                p.PrintLine("return hash;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEqualsMethod(ref Printer p)
        {
            if (HasEqualsMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintLine($"return obj is {TypeName} other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteOverrideIEquatableMethod(ref Printer p)
        {
            foreach (var typeName in OverrideEquals)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public override bool Equals({typeName} other)");
                p.OpenScope();
                {
                    p.PrintLine($"if (other is not {TypeName} otherDerived) return false;");
                    p.PrintLine("if (ReferenceEquals(this, otherDerived)) return true;");
                    p.PrintEndLine();

                    p.PrintLine("return this.EqualsInternal(otherDerived);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteIEquatableMethod(ref Printer p)
        {
            if (HasIEquatableMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLineIf(IsSealed, "public ", "public virtual ");
            p.PrintEndLine($"bool Equals({TypeName} other)");
            p.OpenScope();
            {
                if (Symbol.IsValueType)
                {
                    p.PrintLine("return");
                    WriteEqualComparerLines(ref p, false);
                }
                else
                {
                    p.PrintLine("if (ReferenceEquals(other, null)) return false;");
                    p.PrintLine("if (ReferenceEquals(this, other)) return true;");
                    p.PrintEndLine();
                    p.PrintLine("return EqualsInternal(other);");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEqualsInternalMethod(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLineIf(IsSealed, "private ", "protected ")
                .PrintEndLine($"bool EqualsInternal({TypeName} other)");
            p.OpenScope();
            {
                p.PrintBeginLine("return")
                    .PrintEndLineIf(HasBaseType, " base.EqualsInternal(other)", "");

                WriteEqualComparerLines(ref p, HasBaseType);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEqualComparerLines(ref Printer p, bool previous)
        {
            p = p.IncreasedIndent();
            {
                for (var i = 0; i < FieldRefs.Length; i++)
                {
                    var fieldRef = FieldRefs[i];
                    var fieldName = fieldRef.Field.Name;
                    var equality = fieldRef.FieldEquality;
                    var fieldType = equality.IsNullable ? fieldRef.FieldType.GetTypeFromNullable() : fieldRef.FieldType;
                    var fieldTypeName = fieldType.ToFullName();
                    var and = i == 0 && previous == false ? "  " : "&&";
                    previous = true;

                    WriteEquality(ref p, fieldName, fieldTypeName, equality, and, "&&", fieldType.IsReferenceType);
                }

                for (var i = 0; i < PropRefs.Length; i++)
                {
                    var propRef = PropRefs[i];
                    var fieldName = propRef.FieldName;
                    var equality = propRef.FieldEquality;
                    var fieldType = equality.IsNullable ? propRef.FieldType.GetTypeFromNullable() : propRef.FieldType;
                    var fieldTypeName = GetFieldTypeName(fieldType, propRef.FieldCollection);
                    var and = i == 0 && previous == false ? "  " : "&&";
                    previous = true;

                    WriteEquality(ref p, fieldName, fieldTypeName, equality, and, "&&", fieldType.IsReferenceType);
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(";");

            static void WriteEquality(
                  ref Printer p
                , string fieldName
                , string fieldTypeName
                , Equality equality
                , string and
                , string and2
                , bool isReferenceType
            )
            {
                if (equality.IsNullable)
                {
                    p.PrintBeginLine(and).Print(" (this.").Print(fieldName).Print(".HasValue == other.")
                        .Print(fieldName).Print(".HasValue) ");

                    and = and2;

                    p.Print(and).Print(" this.").Print(fieldName).PrintEndLine(".HasValue");

                    fieldName = $"{fieldName}.Value";
                }

                switch (equality.Strategy)
                {
                    case EqualityStrategy.Operator:
                    {
                        p.PrintBeginLine(and).Print(" (this.").Print(fieldName).Print(" == other.")
                            .Print(fieldName).PrintEndLine(")");
                        break;
                    }

                    case EqualityStrategy.Equals:
                    {
                        if (equality.IsStatic)
                        {
                            p.PrintBeginLine(and).Print(" ").Print(fieldTypeName).Print(".Equals(this.").Print(fieldName)
                                .Print(", other.").Print(fieldName).PrintEndLine(")");
                        }
                        else if (isReferenceType)
                        {
                            p.PrintBeginLine(and).Print(" (this.").Print(fieldName)
                                .Print("?.Equals(other.")
                                .Print(fieldName).PrintEndLine(") ?? false)");
                        }
                        else
                        {
                            p.PrintBeginLine(and).Print(" this.").Print(fieldName)
                                .Print(".Equals(other.")
                                .Print(fieldName).PrintEndLine(")");
                        }
                        break;
                    }

                    default:
                    {
                        p.PrintBeginLine(and).Print(" global::System.Collections.Generic.EqualityComparer<")
                            .Print(fieldTypeName).Print(">.Default.Equals(this.").Print(fieldName).Print(", other.")
                            .Print(fieldName).PrintEndLine(")");
                        break;
                    }
                }
            }
        }

        private void WriteSetValues_TypeMethod(ref Printer p)
        {
            p.PrintLine("[global::System.Obsolete(\"This method is not intended to be used directly by user code.\")]");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"internal void SetValues_{Symbol.ToValidIdentifier()}(");
            p = p.IncreasedIndent();
            {
                var previous = false;

                for (var i = 0; i < FieldRefs.Length; i++)
                {
                    previous = true;
                    var fieldRef = FieldRefs[i];
                    var comma = i == 0 ? " " : ",";
                    p.PrintLine($"{comma} {fieldRef.FieldType.ToFullName()} {fieldRef.Field.Name}");
                }

                for (var i = 0; i < PropRefs.Length; i++)
                {
                    var propRef = PropRefs[i];
                    var comma = i == 0 && previous == false ? " " : ",";
                    p.PrintLine($"{comma} {GetFieldTypeName(propRef.FieldType, propRef.FieldCollection)} {propRef.FieldName}");
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                foreach (var field in FieldRefs)
                {
                    var fieldName = field.Field.Name;
                    p.PrintLine($"this.{fieldName} = {fieldName};");
                }

                foreach (var prop in PropRefs)
                {
                    var fieldName = prop.FieldName;
                    p.PrintLine($"this.{fieldName} = {fieldName};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEqualityOperators(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator ==({TypeName} left, {TypeName} right)");
            p.OpenScope();
            {
                if (Symbol.IsValueType == false)
                {
                    p.PrintLine("if (ReferenceEquals(left, null))");
                    p.OpenScope();
                    {
                        p.PrintLine("return ReferenceEquals(right, null);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator !=({TypeName} left, {TypeName} right)");
            p.OpenScope();
            {
                if (Symbol.IsValueType == false)
                {
                    p.PrintLine("if (ReferenceEquals(left, null))");
                    p.OpenScope();
                    {
                        p.PrintLine("return !ReferenceEquals(right, null);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return !left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteAsReadOnlyMethod(ref Printer p)
        {
            if (WithReadOnlyStruct == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine($"public ReadOnly").Print(TypeName).PrintEndLine(" AsReadOnly()");
            p.OpenScope();
            {
                p.PrintLine("return new(this);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyStruct(ref Printer p)
        {
            if (WithReadOnlyStruct == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly partial struct ReadOnly").PrintEndLine(TypeName);
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(TypeName).PrintEndLine(" _data;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE);
                p.PrintLine("private readonly bool _isValid;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine(Symbol.DeclaredAccessibility.ToKeyword())
                    .Print(" ReadOnly").Print(TypeName).Print("(")
                    .PrintIf(Symbol.IsValueType, "in ")
                    .Print(TypeName)
                    .PrintEndLine(" data)");
                p.OpenScope();
                {
                    p.PrintLine("_data = data;");

                    if (Symbol.IsValueType)
                    {
                        p.PrintLine("_isValid = true;");
                    }
                    else
                    {
                        p.PrintLine("_isValid = data is not null;");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool IsValid");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _isValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                var orders = Orders;
                var propRefs = PropRefs;
                var fieldRefs = FieldRefs;

                foreach (var order in orders)
                {
                    var index = order.index;

                    if (order.isPropRef)
                    {
                        var propRef = propRefs[index];

                        if (propRef.Property.DeclaredAccessibility != Accessibility.Public)
                        {
                            continue;
                        }

                        var propName = propRef.Property.Name;

                        GetTypeNames(
                              propRef.PropertyType
                            , propRef.FieldCollection
                            , out var _
                            , out var immutableTypeName
                            , out var _
                        );

                        p.PrintBeginLine("public readonly ").Print(immutableTypeName)
                            .Print(" ").PrintEndLine(propName);
                        p.OpenScope();
                        {
                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("get => _data.").Print(propName).PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                    else
                    {
                        var fieldRef = fieldRefs[index];

                        if (fieldRef.ImplementedProperty is IPropertySymbol prop
                            && prop.DeclaredAccessibility != Accessibility.Public
                        )
                        {
                            continue;
                        }

                        var propName = fieldRef.PropertyName;

                        GetTypeNames(
                              fieldRef.PropertyType
                            , fieldRef.FieldCollection
                            , out var _
                            , out var immutableTypeName
                            , out var _
                        );

                        p.PrintBeginLine("public readonly ").Print(immutableTypeName)
                            .Print(" ").PrintEndLine(propName);
                        p.OpenScope();
                        {
                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("get => _data.").Print(propName).PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string GetFieldTypeName(ITypeSymbol typeSymbol, CollectionRef collection)
        {
            switch (collection.Kind)
            {
                case CollectionKind.ReadOnlyMemory:
                case CollectionKind.Memory:
                case CollectionKind.ReadOnlySpan:
                case CollectionKind.Span:
                case CollectionKind.Array:
                {
                    return $"{collection.ElementType.ToFullName()}[]";
                }

                case CollectionKind.ReadOnlyList:
                case CollectionKind.List:
                {
                    return $"{LIST_TYPE_T}{collection.ElementType.ToFullName()}>";
                }

                case CollectionKind.HashSet:
                {
                    return $"{HASH_SET_TYPE_T}{collection.ElementType.ToFullName()}>";
                }

                case CollectionKind.Stack:
                {
                    return $"{STACK_TYPE_T}{collection.ElementType.ToFullName()}>";
                }

                case CollectionKind.Queue:
                {
                    return $"{QUEUE_TYPE_T}{collection.ElementType.ToFullName()}>";
                }

                case CollectionKind.ReadOnlyDictionary:
                case CollectionKind.Dictionary:
                {
                    var keyType = collection.KeyType.ToFullName();
                    var valueType = collection.ElementType.ToFullName();
                    return $"{DICTIONARY_TYPE_T}{keyType}, {valueType}>";
                }

                default:
                {
                    return typeSymbol.ToFullName();
                }
            }
        }

        private static void GetTypeNames(
              ITypeSymbol typeSymbol
            , CollectionRef collection
            , out string mutableTypeName
            , out string immutableTypeName
            , out bool sameType
        )
        {
            sameType = false;

            switch (collection.Kind)
            {
                case CollectionKind.Array:
                {
                    mutableTypeName = $"{collection.ElementType.ToFullName()}[]";
                    immutableTypeName = $"global::System.ReadOnlyMemory<{collection.ElementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.List:
                {
                    mutableTypeName = $"global::System.Collections.Generic.List<{collection.ElementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyList<{collection.ElementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Dictionary<{collection.KeyType.ToFullName()}, {collection.ElementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyDictionary<{collection.KeyType.ToFullName()}, {collection.ElementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.HashSet:
                {
                    mutableTypeName = $"global::System.Collections.Generic.HashSet<{collection.ElementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{collection.ElementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Queue:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Queue<{collection.ElementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{collection.ElementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Stack:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Stack<{collection.ElementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{collection.ElementType.ToFullName()}>";
                    break;
                }

                default:
                {
                    mutableTypeName = typeSymbol.ToFullName();
                    immutableTypeName = typeSymbol.ToFullName();
                    sameType = true;
                    break;
                }
            }
        }
    }
}
