using EncosyTower.SourceGen.Common.Data.Common;

namespace EncosyTower.SourceGen.Generators.Data
{
    using static EncosyTower.SourceGen.Common.Data.Common.Helpers;

    partial struct DataDeclaration
    {
        public readonly string WriteCode()
        {
            var keyword = isValueType ? "struct" : "class";

            var p = Printer.DefaultLarge;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (hasSerializableAttribute == false)
            {
                p.PrintLine("[Serializable]");
            }

            p.PrintBeginLine()
                .Print($"partial {keyword} ").Print(typeName)
                .Print(" : IData, ")
                .PrintIf(HasBaseType, ", ").PrintIf(HasBaseType, baseTypeName);

            if (isMutable == false)
            {
                p.Print(", ").Print(IREADONLY_DATA).Print("<").Print(typeName).Print(">");
            }

            if (HasIdProperty)
            {
                p.Print(", ").Print("IDataWithId<").Print(idPropertyTypeName).Print(">");
            }

            if (withReadOnlyView)
            {
                p.Print(", ").Print("IDataWithReadOnlyView<").Print(readOnlyTypeName).Print(">");
            }
            else if (isMutable == false)
            {
                p.Print(", ").Print("IDataWithReadOnlyView<").Print(typeName).Print(">");
            }

            p.Print(", ").Print($"IEquatable<{typeName}>");

            p.PrintEndLine();
            p.OpenScope();
            {
                WriteFieldOrPropertyByOrders(ref p);
                WriteGetHashCodeMethod(ref p);
                WriteGetHashCodeInternalMethod(ref p);
                WriteEqualsMethod(ref p);
                WriteIEquatableMethod(ref p);

                if (isValueType == false)
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

        private readonly void WriteFieldOrPropertyByOrders(ref Printer p)
        {
            var ordersArr = orders;
            var propRefsArr = propRefs;
            var fieldRefsArr = fieldRefs;
            var readonlyKeyword = isValueType ? "readonly " : "";
            var accessKeyword = fieldPolicy switch {
                DataFieldPolicy.Public => "public",
                DataFieldPolicy.Internal => "internal",
                _ => "private",
            };

            foreach (var order in ordersArr)
            {
                var index = order.index;

                if (order.isPropRef)
                {
                    WriteField(ref p, propRefsArr[index], accessKeyword, readonlyKeyword);
                }
                else
                {
                    WriteProperty(ref p, fieldRefsArr[index]);
                }
            }
        }

        private readonly void WriteField(ref Printer p, in PropRefData prop, string accessKeyword, string readonlyKeyword)
        {
            if (prop.fieldIsImplemented)
            {
                return;
            }

            var fieldName = prop.fieldName;

            p.PrintLine(string.Format(GENERATED_FIELD_FROM_PROPERTY_ATTRIBUTE, prop.propertyName));
            p.PrintLine(GENERATED_CODE);

            var withSerializeField = false;
            var hasGenPropertyBagAttrib = hasGeneratePropertyBagAttribute;
            var withDontCreateProperty = false;

            foreach (var attr in prop.forwardedFieldAttributes)
            {
                if (attr.fullTypeName == SERIALIZE_FIELD_ATTRIBUTE)
                {
                    withSerializeField = true;
                }
                else if (hasGenPropertyBagAttrib && attr.fullTypeName == DONT_CREATE_PROPERTY_ATTRIBUTE)
                {
                    withDontCreateProperty = true;
                }

                p.PrintLine($"[{attr.attributeSyntax}]");
            }

            if (withSerializeField == false)
            {
                p.PrintLine($"[{SERIALIZE_FIELD_ATTRIBUTE}]");
            }

            if (hasGenPropertyBagAttrib
                && prop.doesCreateProperty
                && withDontCreateProperty == false
            )
            {
                p.PrintLine($"[{DONT_CREATE_PROPERTY}]");
            }

            var fieldTypeName = prop.fieldTypeName;
            var propTypeName = prop.propertyTypeName;
            var mustCast = prop.typesAreDifferent;
            var mutableTypeName = prop.mutablePropertyTypeName;
            var sameType = prop.samePropertyType;

            p.PrintLine($"{accessKeyword} {fieldTypeName} {fieldName};");
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"private {readonlyKeyword}{propTypeName} Get_{prop.propertyName}()");
            p.OpenScope();
            {
                string casting;

                if (string.IsNullOrEmpty(prop.propertyConverter) == false)
                {
                    casting = prop.propertyConverter;
                    mustCast = true;
                }
                else
                {
                    casting = $"({propTypeName})";
                }

                if (mustCast == false)
                {
                    casting = string.Empty;
                }

                p.PrintBeginLine($"return ")
                    .PrintIf(mustCast, $"{casting}")
                    .PrintEndLine($"(this.{fieldName});");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine($"private void Set_{prop.propertyName}(")
                .PrintIf(prop.implicitlyConvertible, propTypeName, fieldTypeName)
                .PrintEndLine(" value)");
            p.OpenScope();
            {
                string casting;

                if (string.IsNullOrEmpty(prop.fieldConverter) == false)
                {
                    casting = prop.fieldConverter;
                    mustCast = true;
                }
                else
                {
                    casting = $"({fieldTypeName})";
                }

                if (mustCast == false)
                {
                    casting = string.Empty;
                }

                p.PrintBeginLine($"this.{fieldName} = ");

                if ((isMutable && withoutPropertySetters == false)
                    || (isMutable && prop.fieldCollection.kind == CollectionKind.Array)
                    || sameType
                )
                {
                    p.PrintIf(mustCast, casting).PrintEndLine("(value);");
                }
                else if (mustCast)
                {
                    p.Print(casting).PrintEndLine("(value);");
                }
                else
                {
                    p.PrintEndLine($"({mutableTypeName})(value);");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteProperty(ref Printer p, in FieldRefData field)
        {
            if (field.hasImplementedProperty)
            {
                return;
            }

            var fieldName = field.fieldName;
            var mutableTypeName = field.mutablePropertyTypeName;
            var immutableTypeName = field.immutablePropertyTypeName;
            var sameType = field.samePropertyType;

            p.PrintLine(string.Format(GENERATED_PROPERTY_FROM_FIELD_ATTRIBUTE, fieldName, field.fieldTypeOriginalFullName));
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);

            foreach (var attributeSyntax in field.forwardedPropertyAttributeSyntaxes)
            {
                p.PrintLine($"[{attributeSyntax}]");
            }

            var mustCast = field.typesAreDifferent;
            var propTypeName = isMutable ? mutableTypeName : immutableTypeName;

            p.PrintLine($"public {propTypeName} {field.propertyName}");
            p.OpenScope();
            {
                // getter
                {
                    string casting;

                    if (string.IsNullOrEmpty(field.propertyConverter) == false)
                    {
                        casting = field.propertyConverter;
                        mustCast = true;
                    }
                    else
                    {
                        casting = $"({propTypeName})";
                    }

                    if (mustCast == false)
                    {
                        casting = string.Empty;
                    }

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLineIf(isValueType, "readonly get => ", "get => ")
                        .PrintIf(mustCast, $"{casting}")
                        .PrintEndLine($"(this.{fieldName});");

                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                }

                // setter
                {
                    string casting;

                    if (field.fieldCollection.kind == CollectionKind.List && isMutable == false)
                    {
                        casting = $"{LIST_FAST_EXTENSIONS_UNSAFE}.GetListUnsafe";
                        mustCast = true;
                    }
                    else if (string.IsNullOrEmpty(field.fieldConverter) == false)
                    {
                        casting = field.fieldConverter;
                        mustCast = true;
                    }
                    else
                    {
                        casting = $"({field.fieldTypeName})";
                    }

                    if (mustCast == false)
                    {
                        casting = string.Empty;
                    }

                    if (isMutable && withoutPropertySetters == false)
                    {
                        p.PrintBeginLine($"set => this.{fieldName} = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value);");
                    }
                    else if ((isMutable && field.fieldCollection.kind == CollectionKind.Array) || sameType)
                    {
                        p.PrintBeginLine($"init => this.{fieldName} = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value);");
                    }
                    else if (field.fieldCollection.kind == CollectionKind.Array)
                    {
                        p.PrintBeginLine($"init => this.{fieldName} = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value.ToArray());");
                    }
                    else if (mustCast)
                    {
                        p.PrintLine($"init => this.{fieldName} = {casting}(value);");
                    }
                    else
                    {
                        p.PrintLine($"init => this.{fieldName} = ({mutableTypeName})(value);");
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteGetHashCodeMethod(ref Printer p)
        {
            if (hasGetHashCodeMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public override ")
                .PrintIf(isValueType, "readonly ")
                .PrintEndLine("int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = GetHashCodeInternal();");
                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteGetHashCodeInternalMethod(ref Printer p)
        {
            var fromBase = false;

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);

            if (HasBaseType == false && isSealed == false)
            {
                p.PrintBeginLine("protected virtual ");
            }
            else if (HasBaseType)
            {
                fromBase = true;
                p.PrintBeginLine("protected override ");
            }
            else
            {
                p.PrintBeginLine("private ");
            }

            p.PrintIf(isValueType, "readonly ")
                .PrintEndLine("HashValue GetHashCodeInternal()");
            p.OpenScope();
            {
                p.PrintBeginLine("var hash = ")
                    .PrintIf(fromBase, "base.GetHashCodeInternal()", "new HashValue()")
                    .PrintEndLine(";");

                foreach (var field in fieldRefs)
                {
                    var collectionKind = field.fieldCollection.kind;

                    p.PrintBeginLineIf(
                          collectionKind == CollectionKind.NotCollection
                        , "hash.Add("
                        , "hash.AddEach("
                    );

                    if (collectionKind == CollectionKind.Array)
                    {
                        p.Print(ARRAY_EXTENSIONS).Print(".AsReadOnlySpan");
                    }
                    else if (collectionKind == CollectionKind.List)
                    {
                        p.Print(LIST_EXTENSIONS).Print(".AsReadOnlySpan");
                    }

                    p.Print("(").Print(field.fieldName)
                        .PrintEndLine("));");
                }

                foreach (var prop in propRefs)
                {
                    var collectionKind = prop.fieldCollection.kind;

                    p.PrintBeginLineIf(
                          collectionKind == CollectionKind.NotCollection
                        , "hash.Add("
                        , "hash.AddEach("
                    );

                    if (collectionKind == CollectionKind.Array)
                    {
                        p.Print(ARRAY_EXTENSIONS).Print(".AsReadOnlySpan");
                    }
                    else if (collectionKind == CollectionKind.List)
                    {
                        p.Print(LIST_EXTENSIONS).Print(".AsReadOnlySpan");
                    }

                    p.Print("(").Print(prop.fieldName)
                        .PrintEndLine("));");
                }

                p.PrintLine("return hash;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEqualsMethod(ref Printer p)
        {
            if (hasEqualsMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public override ")
                .PrintIf(isValueType, "readonly ")
                .PrintEndLine("bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintLine($"return obj is {typeName} other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteOverrideIEquatableMethod(ref Printer p)
        {
            foreach (var baseTypeName in overrideEquals)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public override ")
                    .PrintIf(isValueType, "readonly ")
                    .PrintEndLine($"bool Equals({baseTypeName} other)");
                p.OpenScope();
                {
                    p.PrintLine($"if (other is not {typeName} otherDerived) return false;");
                    p.PrintLine("if (ReferenceEquals(this, otherDerived)) return true;");
                    p.PrintEndLine();

                    p.PrintLine("return this.EqualsInternal(otherDerived);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteIEquatableMethod(ref Printer p)
        {
            if (hasIEquatableMethod)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLineIf(isSealed, "public ", "public virtual ")
                .PrintIf(isValueType, "readonly ")
                .PrintEndLine($"bool Equals({typeName} other)");
            p.OpenScope();
            {
                if (isValueType)
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

        private readonly void WriteEqualsInternalMethod(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLineIf(isSealed, "private ", "protected ")
                .PrintEndLine($"bool EqualsInternal({typeName} other)");
            p.OpenScope();
            {
                p.PrintBeginLine("return")
                    .PrintEndLineIf(HasBaseType, " base.EqualsInternal(other)", "");

                WriteEqualComparerLines(ref p, HasBaseType);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEqualComparerLines(ref Printer p, bool previous)
        {
            p = p.IncreasedIndent();
            {
                for (var i = 0; i < fieldRefs.Count; i++)
                {
                    var fieldRef = fieldRefs[i];
                    var fieldName = fieldRef.fieldName;
                    var and = i == 0 && previous == false ? "  " : "&&";

                    if (string.IsNullOrEmpty(fieldRef.fieldEqualityComparer))
                    {
                        if (TryWriteCollectionEquality(ref p, fieldName, fieldRef.fieldCollection, and) == false)
                        {
                            WriteEquality(
                                  ref p
                                , fieldName
                                , fieldRef.fieldTypeFullNameForEquality
                                , fieldRef.fieldEquality
                                , and
                                , "&&"
                                , fieldRef.fieldTypeIsReferenceType
                            );
                        }
                    }
                    else
                    {
                        p.PrintBeginLine(and).Print(" ").Print(fieldRef.fieldEqualityComparer)
                            .Print("(this.").Print(fieldName)
                            .Print(", other.").Print(fieldName)
                            .PrintEndLine(")");
                    }

                    previous = true;
                }

                for (var i = 0; i < propRefs.Count; i++)
                {
                    var propRef = propRefs[i];
                    var fieldName = propRef.fieldName;
                    var and = i == 0 && previous == false ? "  " : "&&";

                    if (string.IsNullOrEmpty(propRef.fieldEqualityComparer))
                    {
                        if (TryWriteCollectionEquality(ref p, fieldName, propRef.fieldCollection, and) == false)
                        {
                            WriteEquality(
                                  ref p
                                , fieldName
                                , propRef.fieldTypeDeclNameForEquality
                                , propRef.fieldEquality
                                , and
                                , "&&"
                                , propRef.fieldTypeIsReferenceType
                            );
                        }
                    }
                    else
                    {
                        p.PrintBeginLine(and).Print(" ").Print(propRef.fieldEqualityComparer)
                            .Print("(this.").Print(fieldName)
                            .Print(", other.").Print(fieldName)
                            .PrintEndLine(")");
                    }
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(";");

            return;

            static bool TryWriteCollectionEquality(
                  ref Printer p
                , string fieldName
                , in FieldCollectionData collection
                , string and
            )
            {
                if (IsSupported(collection) == false)
                {
                    return false;
                }

                if (collection.kind is CollectionKind.Array or CollectionKind.List)
                {
                    p.PrintBeginLine(and).Print(" ").Print(MEMORY_EXTENSIONS).Print(".SequenceEqual(")
                        .PrintIf(collection.kind is CollectionKind.Array, ARRAY_EXTENSIONS, LIST_EXTENSIONS)
                        .Print(".AsReadOnlySpan")
                        .Print("(this.").Print(fieldName).Print("), ")
                        .PrintIf(collection.kind is CollectionKind.Array, ARRAY_EXTENSIONS, LIST_EXTENSIONS)
                        .Print(".AsReadOnlySpan")
                        .Print("(other.").Print(fieldName).PrintEndLine("))");
                    return true;
                }

                if (collection.kind is CollectionKind.HashSet or CollectionKind.Dictionary)
                {
                    p.PrintBeginLine(and).Print(" ")
                        .PrintIf(collection.kind is CollectionKind.HashSet, HASH_SET_API, DICTIONARY_EXTENSIONS)
                        .Print(".Overlaps(")
                        .Print("this.").Print(fieldName)
                        .Print(", other.").Print(fieldName).PrintEndLine(")");
                    return true;
                }

                return false;
            }

            static bool IsSupported(in FieldCollectionData collection)
            {
                return collection.kind switch {
                    CollectionKind.Array
                    or CollectionKind.List
                    or CollectionKind.HashSet => collection.isElementEquatable,
                    CollectionKind.Dictionary => collection.isKeyEquatable && collection.isElementEquatable,
                    _ => false,
                };
            }

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
                        p.PrintBeginLine(and).Print(" EqualityComparer<")
                            .Print(fieldTypeName).Print(">.Default.Equals(this.").Print(fieldName).Print(", other.")
                            .Print(fieldName).PrintEndLine(")");
                        break;
                    }
                }
            }
        }

        private readonly void WriteSetValues_TypeMethod(ref Printer p)
        {
            p.PrintLine("[Obsolete(\"This method is not intended to be used directly by user code.\")]");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"internal void SetValues_{typeValidIdentifier}(");
            p = p.IncreasedIndent();
            {
                var previous = false;

                for (var i = 0; i < fieldRefs.Count; i++)
                {
                    previous = true;
                    var fieldRef = fieldRefs[i];
                    var comma = i == 0 ? " " : ",";
                    p.PrintLine($"{comma} {fieldRef.fieldTypeName} {fieldRef.fieldName}");
                }

                for (var i = 0; i < propRefs.Count; i++)
                {
                    var propRef = propRefs[i];
                    var comma = i == 0 && previous == false ? " " : ",";
                    p.PrintLine($"{comma} {propRef.fieldTypeName} {propRef.fieldName}");
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                foreach (var field in fieldRefs)
                {
                    var fn = field.fieldName;
                    p.PrintLine($"this.{fn} = {fn};");
                }

                foreach (var prop in propRefs)
                {
                    var fn = prop.fieldName;
                    p.PrintLine($"this.{fn} = {fn};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEqualityOperators(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool operator ==(")
                .PrintIf(isValueType, "in ")
                .Print($"{typeName} left, ")
                .PrintIf(isValueType, "in ")
                .PrintEndLine($"{typeName} right)");
            p.OpenScope();
            {
                if (isValueType == false)
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
                .PrintIf(isValueType, "in ")
                .Print($"{typeName} left, ")
                .PrintIf(isValueType, "in ")
                .PrintEndLine($"{typeName} right)");
            p.OpenScope();
            {
                if (isValueType == false)
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

        private readonly void WriteAsReadOnlyMethod(ref Printer p)
        {
            if (isMutable && withReadOnlyView == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine($"public ")
                .PrintIf(isValueType, "readonly ")
                .PrintIf(isMutable, readOnlyTypeName, typeName)
                .PrintEndLine(" AsReadOnly()");
            p.OpenScope();
            {
                p.PrintLine("return this;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct(ref Printer p)
        {
            if (withReadOnlyView == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine(accessibilityKeyword).Print(" readonly partial struct ").Print(readOnlyTypeName)
                .Print(" : ")
                .Print(IREADONLY_DATA).Print("<").Print(typeName).Print(">")
                .Print(", ")
                .Print($"IEquatable<{readOnlyTypeName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(typeName).PrintEndLine(" _data;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine(accessibilityKeyword)
                    .Print(" ReadOnly").Print(typeIdentifier).Print("(")
                    .PrintIf(isValueType, "in ")
                    .Print(typeName)
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

        private readonly void WriteReadOnlyViewStruct_Properties(ref Printer p)
        {
            var ordersArr = orders;
            var propRefsArr = propRefs;
            var fieldRefsArr = fieldRefs;

            foreach (var order in ordersArr)
            {
                var index = order.index;

                if (order.isPropRef)
                {
                    var propRef = propRefsArr[index];

                    if (propRef.isPropertyPublic == false)
                    {
                        continue;
                    }

                    p.PrintBeginLine("public readonly ").Print(propRef.immutablePropertyTypeName)
                        .Print(" ").PrintEndLine(propRef.propertyName);
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => _data.").Print(propRef.propertyName).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                else
                {
                    var fieldRef = fieldRefsArr[index];

                    if (fieldRef.hasImplementedProperty && fieldRef.isImplementedPropertyPublic == false)
                    {
                        continue;
                    }

                    p.PrintBeginLine("public readonly ").Print(fieldRef.immutablePropertyTypeName)
                        .Print(" ").PrintEndLine(fieldRef.propertyName);
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => _data.").Print(fieldRef.propertyName).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private readonly void WriteReadOnlyViewStruct_Equality(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLineIf(isValueType, AGGRESSIVE_INLINING);
            p.PrintLine($"public bool Equals({readOnlyTypeName} other)");
            p.OpenScope();
            {
                if (isValueType == false)
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
                p.PrintLine($"return obj is {readOnlyTypeName} other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct_GetHashCodeMethod(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = new HashValue();");
                p.PrintLine("hash.Add(_data);");
                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct_ImplicitOperator(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static implicit operator ")
                .Print(readOnlyTypeName)
                .Print("(")
                .PrintIf(isValueType, "in ")
                .PrintEndLine($"{typeName} data)");
            p.OpenScope();
            {
                p.PrintBeginLine("return ")
                    .PrintIf(isMutable, "new(data)", "data")
                    .PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct_EqualityOperators(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator ==(in {readOnlyTypeName} left, in {readOnlyTypeName} right)");
            p.OpenScope();
            {
                p.PrintLine("return left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine($"public static bool operator !=(in {readOnlyTypeName} left, in {readOnlyTypeName} right)");
            p.OpenScope();
            {
                p.PrintLine("return !left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Collections.Generic;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Data;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
