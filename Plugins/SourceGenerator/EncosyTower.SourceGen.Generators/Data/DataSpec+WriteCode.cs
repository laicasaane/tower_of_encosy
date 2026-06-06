using EncosyTower.SourceGen.Helpers.Data;

namespace EncosyTower.SourceGen.Generators.Data
{
    using static EncosyTower.SourceGen.Helpers.Data.Helpers;

    partial struct DataSpec
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
                p.PrintLine("[S.Serializable]");
            }

            p.PrintBeginLine().Print("partial ").Print(keyword).Print(" ").Print(typeName).Print(" : ETD.IData");

            if (isMutable == false)
            {
                p.Print(", ").Print("ETD.IReadOnlyData").Print("<").Print(typeName).Print(">");
            }

            if (HasIdProperty)
            {
                p.Print(", ").Print("ETD.IDataWithId<").Print(idPropertyTypeName).Print(">");
            }

            if (withReadOnlyView)
            {
                p.Print(", ").Print("ETD.IDataWithReadOnlyView<").Print(readOnlyTypeName).Print(">");
            }
            else if (isMutable == false)
            {
                p.Print(", ").Print("ETD.IDataWithReadOnlyView<").Print(typeName).Print(">");
            }

            p.Print(", ").Print("S.IEquatable<").Print(typeName).Print(">");

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

                WriteAsReadOnlyMethod(ref p);
                WriteEqualityOperators(ref p);
                WriteValueSetterClass(ref p);
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
            var withSerializeField = false;
            var hasGenPropertyBagAttrib = hasGeneratePropertyBagAttribute;
            var withDontCreateProperty = false;
            var withManualAuthoring = false;
            var withConverter = false;

            foreach (var attr in prop.forwardedFieldAttributes)
            {
                switch (attr.fullTypeName)
                {
                    case SERIALIZE_FIELD_ATTRIBUTE:
                        withSerializeField = true;
                        break;

                    case DONT_CREATE_PROPERTY_ATTRIBUTE:
                        if (hasGenPropertyBagAttrib)
                        {
                            withDontCreateProperty = true;
                        }
                        break;

                    case DATA_MANUAL_AUTHORING_ATTRIBUTE:
                        withManualAuthoring = true;
                        break;

                    case DATA_AUTHORING_CONVERTER_ATTRIBUTE:
                        withConverter = true;
                        break;
                }

                p.PrintBeginLine("[").Print(attr.syntax).PrintEndLine("]");
            }

            if (withSerializeField == false)
            {
                p.PrintLine("[UE.SerializeField]");
            }

            if (withManualAuthoring == false && prop.manualAuthoringAttribute.HasValue)
            {
                p.PrintBeginLine("[").Print(prop.manualAuthoringAttribute.Value.syntax).PrintEndLine("]");
            }

            if (withConverter == false && prop.converterAttribute.HasValue)
            {
                p.PrintBeginLine("[").Print(prop.converterAttribute.Value.syntax).PrintEndLine("]");
            }

            if (hasGenPropertyBagAttrib
                && prop.doesCreateProperty
                && withDontCreateProperty == false
            )
            {
                p.PrintBeginLine("[").Print(DONT_CREATE_PROPERTY_ATTRIBUTE).PrintEndLine("]");
            }

            p.PrintBeginLine(string.Format(PR_GENERATED_FIELD_FROM_PROPERTY, prop.propertyName));
            p.PrintEndLine(PR_GENERATED_CODE);

            var fieldTypeName = prop.fieldTypeName;
            var propTypeName = prop.propertyTypeName;
            var mustCast = prop.typesAreDifferent;
            var mutableTypeName = prop.mutablePropertyTypeName;
            var sameType = prop.samePropertyType;

            p.PrintBeginLine(accessKeyword).Print(" ").Print(fieldTypeName)
                .Print(" ").Print(fieldName).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("private ").Print(readonlyKeyword).Print(propTypeName)
                .Print(" Get_").Print(prop.propertyName).PrintEndLine("()");
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

                p.PrintBeginLine("return ")
                    .PrintIf(mustCast, casting)
                    .Print("(this.").Print(fieldName)
                    .PrintEndLine(");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("private void Set_").Print(prop.propertyName).Print("(")
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

                p.PrintBeginLine("this.").Print(fieldName).Print(" = ");

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
                    p.Print($"(").Print(mutableTypeName).PrintEndLine(")(value);");
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
            var withManualAuthoring = false;
            var withConverter = false;

            p.PrintLine(string.Format(PR_GENERATED_PROPERTY_FROM_FIELD, fieldName, field.fieldTypeOriginalFullName));
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);

            foreach (var attr in field.forwardedPropertyAttributes)
            {
                if (attr.fullTypeName == DATA_MANUAL_AUTHORING_ATTRIBUTE)
                {
                    withManualAuthoring = true;
                }
                else if (attr.fullTypeName == DATA_AUTHORING_CONVERTER_ATTRIBUTE)
                {
                    withConverter = true;
                }

                p.PrintBeginLine("[").Print(attr.syntax).PrintEndLine("]");
            }

            if (withManualAuthoring == false && field.manualAuthoringAttribute.HasValue)
            {
                p.PrintBeginLine("[").Print(field.manualAuthoringAttribute.Value.syntax).PrintEndLine("]");
            }

            if (withConverter == false && field.converterAttribute.HasValue)
            {
                p.PrintBeginLine("[").Print(field.converterAttribute.Value.syntax).PrintEndLine("]");
            }

            var mustCast = field.typesAreDifferent;
            var propTypeName = isMutable ? mutableTypeName : immutableTypeName;

            p.PrintBeginLine("public ").Print(propTypeName).Print(" ").PrintEndLine(field.propertyName);
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

                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintBeginLineIf(isValueType, "readonly get => ", "get => ")
                        .PrintIf(mustCast, casting)
                        .Print("(this.").Print(fieldName).PrintEndLine(");");

                    p.PrintEndLine();
                    p.PrintLine(PR_AGGRESSIVE_INLINING);
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
                        p.PrintBeginLine("set => this.").Print(fieldName).Print(" = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value);");
                    }
                    else if ((isMutable && field.fieldCollection.kind == CollectionKind.Array) || sameType)
                    {
                        p.PrintBeginLine("init => this.").Print(fieldName).Print(" = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value);");
                    }
                    else if (field.fieldCollection.kind == CollectionKind.Array)
                    {
                        p.PrintBeginLine("init => this.").Print(fieldName).Print(" = ")
                            .PrintIf(mustCast, casting)
                            .PrintEndLine("(value.ToArray());");
                    }
                    else if (mustCast)
                    {
                        p.PrintBeginLine("init => this.").Print(fieldName)
                            .Print(" = ").Print(casting).PrintEndLine("(value);");
                    }
                    else
                    {
                        p.PrintBeginLine("init => this.").Print(fieldName)
                            .Print(" = (").Print(mutableTypeName).PrintEndLine(")(value);");
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);

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
                .PrintEndLine("ET.HashValue GetHashCodeInternal()");
            p.OpenScope();
            {
                p.PrintBeginLine("var hash = ")
                    .PrintIf(fromBase, "base.GetHashCodeInternal()", "new ET.HashValue()")
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
                        p.Print(PR_ARRAY_EXTENSIONS).Print(".AsReadOnlySpan");
                    }
                    else if (collectionKind == CollectionKind.List)
                    {
                        p.Print(PR_LIST_EXTENSIONS).Print(".AsReadOnlySpan");
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
                        p.Print(PR_ARRAY_EXTENSIONS).Print(".AsReadOnlySpan");
                    }
                    else if (collectionKind == CollectionKind.List)
                    {
                        p.Print(PR_LIST_EXTENSIONS).Print(".AsReadOnlySpan");
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

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public override ")
                .PrintIf(isValueType, "readonly ")
                .PrintEndLine("bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(typeName).PrintEndLine(" other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteOverrideIEquatableMethod(ref Printer p)
        {
            foreach (var baseTypeName in overrideEquals)
            {
                p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                p.PrintBeginLine("public override ")
                    .PrintIf(isValueType, "readonly ")
                    .Print("bool Equals(").Print(baseTypeName).PrintEndLine(" other)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (other is not ").Print(typeName).PrintEndLine(" otherDerived) return false;");
                    p.PrintEndLine();

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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLineIf(isSealed, "public ", "public virtual ")
                .PrintIf(isValueType, "readonly ")
                .Print("bool Equals(").Print(typeName).PrintEndLine(" other)");
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
                    p.PrintEndLine();

                    p.PrintLine("if (ReferenceEquals(this, other)) return true;");
                    p.PrintLine("return EqualsInternal(other);");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEqualsInternalMethod(ref Printer p)
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLineIf(isSealed, "private ", "protected ")
                .Print("bool EqualsInternal(").Print(typeName).PrintEndLine(" other)");
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
                    p.PrintBeginLine(and).Print(" ").Print(PR_MEMORY_EXTENSIONS).Print(".SequenceEqual(")
                        .PrintIf(collection.kind is CollectionKind.Array, PR_ARRAY_EXTENSIONS, PR_LIST_EXTENSIONS)
                        .Print(".AsReadOnlySpan")
                        .Print("(this.").Print(fieldName).Print("), ")
                        .PrintIf(collection.kind is CollectionKind.Array, PR_ARRAY_EXTENSIONS, PR_LIST_EXTENSIONS)
                        .Print(".AsReadOnlySpan")
                        .Print("(other.").Print(fieldName).PrintEndLine("))");
                    return true;
                }

                if (collection.kind is CollectionKind.HashSet or CollectionKind.Dictionary)
                {
                    p.PrintBeginLine(and).Print(" ")
                        .PrintIf(collection.kind is CollectionKind.HashSet, PR_HASH_SET_API, PR_DICTIONARY_EXTENSIONS)
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
                        p.PrintBeginLine(and).Print(" SCG.EqualityComparer<")
                            .Print(fieldTypeName).Print(">.Default.Equals(this.").Print(fieldName).Print(", other.")
                            .Print(fieldName).PrintEndLine(")");
                        break;
                    }
                }
            }
        }

        private readonly void WriteEqualityOperators(ref Printer p)
        {
            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public static bool operator ==(")
                .PrintIf(isValueType, "in ")
                .Print(typeName).Print(" left, ")
                .PrintIf(isValueType, "in ")
                .Print(typeName).PrintEndLine(" right)");
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

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public static bool operator !=(")
                .PrintIf(isValueType, "in ")
                .Print(typeName).Print(" left, ")
                .PrintIf(isValueType, "in ")
                .Print(typeName).PrintEndLine(" right)");
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

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public ")
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine(accessibilityKeyword).Print(" readonly partial struct ").Print(readOnlyTypeName)
                .Print(" : ")
                .Print("ETD.IReadOnlyData").Print("<").Print(typeName).Print(">")
                .Print(", ")
                .Print("S.IEquatable<").Print(readOnlyTypeName).Print(">")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(PR_GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(typeName).PrintEndLine(" _data;");
                p.PrintEndLine();

                p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
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
                        p.PrintLine(PR_AGGRESSIVE_INLINING);
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
                        p.PrintLine(PR_AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => _data.").Print(fieldRef.propertyName).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private readonly void WriteReadOnlyViewStruct_Equality(ref Printer p)
        {
            p.PrintBeginLineIf(isValueType, PR_AGGRESSIVE_INLINING, "").Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public bool Equals(").Print(readOnlyTypeName).PrintEndLine(" other)");
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

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(readOnlyTypeName).PrintLine(" other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct_GetHashCodeMethod(ref Printer p)
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = new ET.HashValue();");
                p.PrintLine("hash.Add(_data);");
                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteReadOnlyViewStruct_ImplicitOperator(ref Printer p)
        {
            p.PrintBeginLine(PR_GENERATED_CODE).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static implicit operator ")
                .Print(readOnlyTypeName)
                .Print("(")
                .PrintIf(isValueType, "in ")
                .Print(typeName).PrintEndLine(" data)");
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
            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public static bool operator ==(in ")
                .Print(readOnlyTypeName).Print(" left, in ")
                .Print(readOnlyTypeName).PrintEndLine(" right)");
            p.OpenScope();
            {
                p.PrintLine("return left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public static bool operator !=(in ")
                .Print(readOnlyTypeName).Print(" left, in ")
                .Print(readOnlyTypeName).PrintEndLine(" right)");
            p.OpenScope();
            {
                p.PrintLine("return !left.Equals(right);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteValueSetterClass(ref Printer p)
        {
            p.PrintLine("[S.Obsolete(\"This method is not intended to be used directly by user code.\")]");
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("internal ").Print("static class ")
                .Print(typeValidIdentifier).PrintEndLine("_ValueSetter");
            p.OpenScope();
            {
                foreach (var fieldRef in fieldRefs)
                {
                    var fn = fieldRef.fieldName;
                    var pn = fieldRef.propertyName;

                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static void Set_").Print(pn).Print("(").PrintIf(isValueType, "ref ")
                        .Print(typeName).Print(" @ref")
                        .Print(", ").Print(fieldRef.fieldTypeName).Print(" value_").Print(pn).PrintEndLine(")");
                    p.WithIncreasedIndent()
                        .PrintBeginLine("=> @ref.").Print(fn).Print(" = value_").Print(pn).PrintEndLine(";");
                    p.PrintEndLine();
                }

                foreach (var propRef in propRefs)
                {
                    var fn = propRef.fieldName;
                    var pn = propRef.propertyName;

                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static void Set_").Print(pn).Print("(").PrintIf(isValueType, "ref ")
                        .Print(typeName).Print(" @ref")
                        .Print(", ").Print(propRef.fieldTypeName).Print(" value_").Print(pn).PrintEndLine(")");
                    p.WithIncreasedIndent()
                        .PrintBeginLine("=> @ref.").Print(fn).Print(" = value_").Print(pn).PrintEndLine(";");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
