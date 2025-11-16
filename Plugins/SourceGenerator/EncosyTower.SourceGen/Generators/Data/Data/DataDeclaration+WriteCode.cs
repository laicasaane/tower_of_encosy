using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Data.Data
{
    using static EncosyTower.SourceGen.Generators.Data.Helpers;

    partial class DataDeclaration
    {
        public string WriteCode()
        {
            var keyword = IsValueType ? "struct" : "class";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (HasSerializableAttribute == false)
            {
                p.PrintLine("[global::System.Serializable]");
            }

            p.PrintBeginLine()
                .Print($"partial {keyword} ").Print(TypeName)
                .Print(" : ")
                .PrintIf(HasBaseType, $"{BaseTypeName}");

            var secondInterface = HasBaseType;

            if (IsMutable == false)
            {
                p.PrintIf(secondInterface, ", ")
                    .Print(IREADONLY_DATA).Print("<").Print(TypeName).Print(">");

                secondInterface = true;
            }

            if (IdPropertyType != null)
            {
                p.PrintIf(secondInterface, ", ")
                    .Print("global::EncosyTower.Data.IDataWithId<")
                    .Print(IdPropertyType.ToFullName()).Print(">");

                secondInterface = true;
            }

            if (WithReadOnlyView)
            {
                p.PrintIf(secondInterface, ", ")
                    .Print("global::EncosyTower.Data.IDataWithReadOnlyView<")
                    .Print(ReadOnlyTypeName).Print(">");

                secondInterface = true;
            }
            else if (IsMutable == false)
            {
                p.PrintIf(secondInterface, ", ")
                    .Print("global::EncosyTower.Data.IDataWithReadOnlyView<")
                    .Print(TypeName).Print(">");

                secondInterface = true;
            }

            p.PrintIf(secondInterface, ", ").Print($"global::System.IEquatable<{TypeName}>");

            p.PrintEndLine();
            p.OpenScope();
            {
                WriteFieldOrPropertyByOrders(ref p);
                WriteGetHashCodeMethod(ref p);
                WriteGetHashCodeInternalMethod(ref p);
                WriteEqualsMethod(ref p);
                WriteIEquatableMethod(ref p);

                if (IsValueType == false)
                {
                    WriteEqualsInternalMethod(ref p);
                    WriteOverrideIEquatableMethod(ref p);
                }

                WriteSetValues_TypeMethod(ref p);
                WriteAsReadOnlyMethod(ref p);
                WriteEqualityOperators(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            WriteReadOnlyViewStruct(ref p);

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFieldOrPropertyByOrders(ref Printer p)
        {
            var orders = Orders;
            var propRefs = PropRefs;
            var fieldRefs = FieldRefs;
            var readonlyKeyword = IsValueType ? "readonly " : "";
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
            var hasGeneratePropertyBagAttribute = HasGeneratePropertyBagAttribute;
            var withDontCreateProperty = false;

            foreach (var (fullTypeName, attribute) in prop.ForwardedFieldAttributes)
            {
                if (fullTypeName == SERIALIZE_FIELD_ATTRIBUTE)
                {
                    withSerializeField = true;
                }
                else if (hasGeneratePropertyBagAttribute && fullTypeName == DONT_CREATE_PROPERTY_ATTRIBUTE)
                {
                    withDontCreateProperty = true;
                }

                p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
            }

            if (withSerializeField == false)
            {
                p.PrintLine($"[{SERIALIZE_FIELD_ATTRIBUTE}]");
            }

            if (hasGeneratePropertyBagAttribute
                && prop.DoesCreateProperty
                && withDontCreateProperty == false
            )
            {
                p.PrintLine($"[{DONT_CREATE_PROPERTY}]");
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
                p.PrintBeginLineIf(IsValueType, "readonly get => ", "get => ")
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
            p.PrintBeginLine("public override ")
                .PrintIf(IsValueType, "readonly ")
                .PrintEndLine("int GetHashCode()");
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

            p.PrintIf(IsValueType, "readonly ")
                .PrintEndLine("global::EncosyTower.Common.HashValue GetHashCodeInternal()");
            p.OpenScope();
            {
                p.PrintBeginLine("var hash = ")
                    .PrintIf(fromBase, "base.GetHashCodeInternal()", "new global::EncosyTower.Common.HashValue()")
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

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public override ")
                .PrintIf(IsValueType, "readonly ")
                .PrintEndLine("bool Equals(object obj)");
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
                p.PrintBeginLine("public override ")
                    .PrintIf(IsValueType, "readonly ")
                    .PrintEndLine($"bool Equals({typeName} other)");
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
            p.PrintBeginLineIf(IsSealed, "public ", "public virtual ")
                .PrintIf(IsValueType, "readonly ")
                .PrintEndLine($"bool Equals({TypeName} other)");
            p.OpenScope();
            {
                if (IsValueType)
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
            p.PrintBeginLine("public static bool operator ==(")
                .PrintIf(IsValueType, "in ")
                .Print($"{TypeName} left, ")
                .PrintIf(IsValueType, "in ")
                .PrintEndLine($"{TypeName} right)");
            p.OpenScope();
            {
                if (IsValueType == false)
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
            p.PrintBeginLine("public static bool operator !=(")
                .PrintIf(IsValueType, "in ")
                .Print($"{TypeName} left, ")
                .PrintIf(IsValueType, "in ")
                .PrintEndLine($"{TypeName} right)");
            p.OpenScope();
            {
                if (IsValueType == false)
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
            if (IsMutable && WithReadOnlyView == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine($"public ")
                .PrintIf(IsValueType, "readonly ")
                .PrintIf(IsMutable, ReadOnlyTypeName, TypeName)
                .PrintEndLine(" AsReadOnly()");
            p.OpenScope();
            {
                p.PrintLine("return this;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyViewStruct(ref Printer p)
        {
            if (WithReadOnlyView == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly partial struct ").Print(ReadOnlyTypeName)
                .Print(" : ")
                .Print(IREADONLY_DATA).Print("<").Print(TypeName).Print(">")
                .Print(", ")
                .Print($"global::System.IEquatable<{ReadOnlyTypeName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(TypeName).PrintEndLine(" _data;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine(Symbol.DeclaredAccessibility.ToKeyword())
                    .Print(" ReadOnly").Print(TypeIdentifier).Print("(")
                    .PrintIf(IsValueType, "in ")
                    .Print(TypeName)
                    .PrintEndLine(" data)");
                p.OpenScope();
                {
                    p.PrintLine("_data = data;");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteReadOnlyViewStruct_Properties(ref p);
                WriteReadOnlyViewStruct_Equality(ref p);
                WriteReadOnlyViewStruct_GetHashCodeMethod(ref p);
                WriteReadOnlyViewStruct_ImplicitOperator(ref p);
                WriteReadOnlyViewStruct_EqualityOperators(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyViewStruct_Properties(ref Printer p)
        {
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

        private void WriteReadOnlyViewStruct_Equality(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLineIf(IsValueType, AGGRESSIVE_INLINING);
            p.PrintLine($"public bool Equals({ReadOnlyTypeName} other)");
            p.OpenScope();
            {
                if (IsValueType == false)
                {
                    p.PrintLine("if (ReferenceEquals(other._data, null)) return false;");
                    p.PrintLine("if (ReferenceEquals(this, other._data)) return true;");
                    p.PrintEndLine();
                }

                p.PrintLine("return _data.Equals(other._data);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintLine($"return obj is {ReadOnlyTypeName} other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyViewStruct_GetHashCodeMethod(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = new global::EncosyTower.Common.HashValue();");
                p.PrintLine("hash.Add(_data);");
                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyViewStruct_ImplicitOperator(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static implicit operator ")
                .Print(ReadOnlyTypeName)
                .Print("(")
                .PrintIf(IsValueType, "in ")
                .PrintEndLine($"{TypeName} data)");
            p.OpenScope();
            {
                p.PrintBeginLine("return ")
                    .PrintIf(IsMutable, "new(data)", "data")
                    .PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteReadOnlyViewStruct_EqualityOperators(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator ==(in {ReadOnlyTypeName} left, in {ReadOnlyTypeName} right)");
            p.OpenScope();
            {
                p.PrintLine("return left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator !=(in {ReadOnlyTypeName} left, in {ReadOnlyTypeName} right)");
            p.OpenScope();
            {
                p.PrintLine("return !left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string GetFieldTypeName(ITypeSymbol typeSymbol, in CollectionRef collection)
        {
            var (kind, keyType, elementType) = collection;

            switch (kind)
            {
                case CollectionKind.ReadOnlyMemory:
                case CollectionKind.Memory:
                case CollectionKind.ReadOnlySpan:
                case CollectionKind.Span:
                case CollectionKind.Array:
                {
                    return $"{elementType.ToFullName()}[]";
                }

                case CollectionKind.ReadOnlyList:
                case CollectionKind.List:
                {
                    return $"{LIST_TYPE_T}{elementType.ToFullName()}>";
                }

                case CollectionKind.HashSet:
                {
                    return $"{HASH_SET_TYPE_T}{elementType.ToFullName()}>";
                }

                case CollectionKind.Stack:
                {
                    return $"{STACK_TYPE_T}{elementType.ToFullName()}>";
                }

                case CollectionKind.Queue:
                {
                    return $"{QUEUE_TYPE_T}{elementType.ToFullName()}>";
                }

                case CollectionKind.ReadOnlyDictionary:
                case CollectionKind.Dictionary:
                {
                    var keyTypeFullName = keyType.ToFullName();
                    var valueTypeFullName = elementType.ToFullName();
                    return $"{DICTIONARY_TYPE_T}{keyTypeFullName}, {valueTypeFullName}>";
                }

                default:
                {
                    return typeSymbol.ToFullName();
                }
            }
        }

        private static void GetTypeNames(
              ITypeSymbol typeSymbol
            , in CollectionRef collection
            , out string mutableTypeName
            , out string immutableTypeName
            , out bool sameType
        )
        {
            sameType = false;

            var (kind, keyType, elementType) = collection;

            switch (kind)
            {
                case CollectionKind.Array:
                {
                    mutableTypeName = $"{elementType.ToFullName()}[]";
                    immutableTypeName = $"global::System.ReadOnlyMemory<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.List:
                {
                    mutableTypeName = $"global::System.Collections.Generic.List<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyList<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Dictionary<{keyType.ToFullName()}, {elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyDictionary<{keyType.ToFullName()}, {elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.HashSet:
                {
                    mutableTypeName = $"global::System.Collections.Generic.HashSet<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Queue:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Queue<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Stack:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Stack<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
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
