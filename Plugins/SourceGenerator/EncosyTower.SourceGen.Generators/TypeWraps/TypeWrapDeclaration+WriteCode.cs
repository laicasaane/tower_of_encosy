using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    partial struct TypeWrapDeclaration
    {
        public const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.SourceGen.Generators.TypeWraps.TypeWrapGenerator\"";
        private const string OBSOLETE_REF_STRUCT = "[Obsolete(\"Not supported for ref struct\")]";
        private const string IWRAP = "IWrap<";

        public readonly string WriteCode()
        {
            var p = new Printer(0, 1024 * 512);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                if (excludeConverter == false && isRefStruct == false)
                {
                    p.PrintBeginLine("[TypeConverter(typeof(")
                        .Print(fullTypeName).Print(".")
                        .Print(typeNameWithTypeParams).PrintEndLine("TypeConverter))]");
                }

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .PrintIf(isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(isRecord, "record ")
                    .PrintIf(isStruct, "struct ", "class ")
                    .Print(typeNameWithTypeParams);

                if (isRefStruct)
                {
                    p.PrintEndLine();
                }
                else
                {
                    p.Print(" : ").Print(IWRAP)
                        .Print(fieldTypeName)
                        .PrintEndLine(">");

                    WriteInterfaces(ref p);
                }

                p.OpenScope();
                {
                    if (isRecord == false)
                    {
                        WriteBackingField(ref p);
                        WritePrimaryConstructor(ref p);
                    }

                    WriteFields(ref p);
                    WriteProperties(ref p);
                    WriteEvents(ref p);
                    WriteMethods(ref p);
                    WriteConversionOperators(ref p);
                    WriteOperators(ref p);
                    WriteTypeConverter(ref p);
                }
                p.CloseScope();

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial ")
                    .PrintIf(isRecord, "record ")
                    .PrintIf(isStruct, "struct ", "class ")
                    .Print(typeNameWithTypeParams)
                    .PrintEndLine(" // Internals");
                p.OpenScope();
                {
                    WriteHelpers(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteInterfaces(ref Printer p)
        {
            p = p.IncreasedIndent();

            if (implementInterfaces.HasFlag(InterfaceKind.EquatableT))
            {
                p.PrintBeginLine(", ").Print("IEquatable<").Print(fullTypeName).PrintEndLine(">");
                p.PrintBeginLine(", ").Print("IEquatable<").Print(fieldTypeName).PrintEndLine(">");
            }

            var hasCompareToT = implementInterfaces.HasFlag(InterfaceKind.ComparableT);
            var hasCompareTo = implementInterfaces.HasFlag(InterfaceKind.Comparable)
                && implementSpecialMethods.HasFlag(SpecialMethodType.CompareTo);

            if (hasCompareToT || hasCompareTo)
            {
                p.PrintBeginLine(", ").PrintEndLine("IComparable");
            }

            if (implementInterfaces.HasFlag(InterfaceKind.ComparableT))
            {
                p.PrintBeginLine(", ").Print("IComparable<").Print(fullTypeName).PrintEndLine(">");
                p.PrintBeginLine(", ").Print("IComparable<").Print(fieldTypeName).PrintEndLine(">");
            }

            p = p.DecreasedIndent();
        }

        private readonly void WriteBackingField(ref Printer p)
        {
            if (isFieldDeclared)
            {
                return;
            }

            p.PrintBeginLine("public ")
                .PrintIf(isReadOnly || isStruct == false, "readonly ")
                .Print(fieldTypeName)
                .Print(" ")
                .Print(fieldName)
                .PrintEndLine($";");
            p.PrintEndLine();
        }

        private readonly void WritePrimaryConstructor(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine($"public {typeName}({fieldTypeName} value)");
            p.PrintEndLineIf(isStruct, " : this()", "");
            p.OpenScope();
            {
                p.PrintLine($"this.{fieldName} = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteFields(ref Printer p)
        {
            foreach (var field in fields)
            {
                WriteField(ref p, field);
            }
        }

        readonly void WriteField(ref Printer p, in FieldDeclaration field)
        {
            var returnTypeName = field.typeName;
            var sameType = field.sameType;
            var name = field.name;

            if (field.isConst)
            {
                if (sameType)
                {
                    p.PrintLine($"public static readonly {fullTypeName} {name} = new {fullTypeName}({fieldTypeName}.{name});");
                }
                else
                {
                    p.PrintLine($"public const {returnTypeName} {name} = {fieldTypeName}.{name};");
                }
            }
            else if (field.isStatic)
            {
                if (field.isReadOnly && sameType)
                {
                    p.PrintLine($"public static readonly {fullTypeName} {name} = new {fullTypeName}({fieldTypeName}.{name});");
                }
                else if (field.isReadOnly)
                {
                    p.PrintLine($"public static {returnTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => {fieldTypeName}.{name};");
                    }
                    p.CloseScope();
                }
                else if (sameType)
                {
                    p.PrintLine($"public static {fullTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => new {fullTypeName}({fieldTypeName}.{name});");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"set => {fieldTypeName}.{name} = value.{fieldName};");
                    }
                    p.CloseScope();
                }
                else
                {
                    p.PrintLine($"public static {returnTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => {fieldTypeName}.{name};");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"set => {fieldTypeName}.{name} = value;");
                    }
                    p.CloseScope();
                }
            }
            else
            {
                var isReadOnly = this.isReadOnly || field.isReadOnly;

                if (isReadOnly && sameType)
                {
                    p.PrintLine($"public readonly {fullTypeName} {name} = new {fullTypeName}(this.{fieldName}.{name});");
                }
                else if (isReadOnly)
                {
                    p.PrintLine($"public {returnTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => this.{fieldName}.{name};");
                    }
                    p.CloseScope();
                }
                else if (sameType)
                {
                    p.PrintLine($"public {fullTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => new {fullTypeName}(this.{fieldName}.{name});");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"set => this.{fieldName}.{name} = value.{fieldName};");
                    }
                    p.CloseScope();
                }
                else
                {
                    p.PrintLine($"public {returnTypeName} {name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => this.{fieldName}.{name};");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"set => this.{fieldName}.{name} = value;");
                    }
                    p.CloseScope();
                }
            }

            p.PrintEndLine();
        }

        private readonly void WriteProperties(ref Printer p)
        {
            foreach (var property in properties)
            {
                if (property.explicitInterfaceImplementationsLength > 0)
                {
                    continue;
                }

                WriteProperty(ref p, property);
            }
        }

        private readonly void WriteProperty(ref Printer p, in PropertyDeclaration property)
        {
            var name = property.name;
            var returnTypeName = property.typeName;
            var sameType = property.sameType;
            var hasParams = string.IsNullOrEmpty(property.parameters) == false;
            var isStatic = property.isStatic;
            var isReadOnly = property.isReadOnly;
            var refKind = property.refKind;
            var wrapperIsStruct = isStruct;
            var withoutSetter = property.withoutSetter;
            var getterSetterCanBeReadOnly = property.getterSetterCanBeReadOnly;
            var getterCanBeReadOnly = property.getterCanBeReadOnly;

            p.PrintBeginLineIf(property.isPublic, "public ", "");
            p.PrintIf(property.isUnsafe, "unsafe ");
            p.PrintIf(isStatic, "static ");

            if (refKind == RefKind.RefReadOnly)
            {
                getterSetterCanBeReadOnly = false;
                p.Print("ref readonly ");
            }
            else if (refKind == RefKind.Ref)
            {
                p.Print("ref ");
            }
            else if (isStruct && isStatic == false)
            {
                if (isReadOnly || getterCanBeReadOnly)
                {
                    getterSetterCanBeReadOnly = false;
                    p.Print("readonly ");
                }
            }

            var isRef = refKind is RefKind.Ref or RefKind.RefReadOnly;
            var canConvertType = wrapperIsStruct && sameType && isRef == false;

            p.PrintIf(canConvertType, fullTypeName, returnTypeName);
            p.Print(" ");

            var explicitTypeName = string.Empty;

            if (hasParams)
            {
                p.Print(property.parameters);
            }
            else
            {
                p.Print(name);
            }

            p.PrintEndLine();

            p.OpenScope();
            {
                var fieldName = string.IsNullOrEmpty(explicitTypeName)
                    ? $"this.{this.fieldName}"
                    : $"(({explicitTypeName})this.{this.fieldName})";

                var accessor = isStatic ? fieldTypeName : fieldName;

                if (hasParams)
                {
                    WriteIndexerBody(
                          ref p
                        , property
                        , accessor
                        , isRef
                        , getterSetterCanBeReadOnly
                        , withoutSetter
                    );
                }
                else
                {
                    WritePropertyBody(
                          ref p
                        , property
                        , accessor
                        , name
                        , isRef
                        , getterSetterCanBeReadOnly
                        , withoutSetter
                    );
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            static void WriteIndexerBody(
                  ref Printer p
                , in PropertyDeclaration property
                , string accessor
                , bool isRef
                , bool getterSetterCanBeReadOnly
                , bool withoutSetter
            )
            {
                if (property.hasGetter)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine();

                    if (getterSetterCanBeReadOnly && property.isGetterRefRO)
                    {
                        p.Print("ref readonly ");
                    }
                    else if (property.isGetterRef)
                    {
                        p.Print("ref ");
                    }
                    else if (getterSetterCanBeReadOnly && property.isGetterRO)
                    {
                        p.Print("readonly ");
                    }

                    p.Print("get => ");
                    p.PrintIf(isRef, "ref ");
                    p.Print(accessor).Print("[");
                    p.Print(property.arguments);
                    p.Print("];");
                    p.PrintEndLine();
                    p.PrintEndLine();
                }

                if (withoutSetter)
                {
                    return;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine();
                p.PrintIf(getterSetterCanBeReadOnly && property.isSetterRO, "readonly ");
                p.Print("set => ").Print(accessor).Print("[").Print(property.arguments).PrintEndLine("] = value;");
            }

            static void WritePropertyBody(
                  ref Printer p
                , in PropertyDeclaration property
                , string accessor
                , string propName
                , bool isRef
                , bool getterSetterCanBeReadOnly
                , bool withoutSetter
            )
            {
                if (property.hasGetter)
                {
                    var isGetterRef = property.isGetterRef;
                    var isGetterRefRO = property.isGetterRefRO;

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine();

                    if (getterSetterCanBeReadOnly && isGetterRefRO)
                    {
                        p.Print("ref readonly ");
                    }
                    else if (isGetterRef)
                    {
                        p.Print("ref ");
                    }
                    else if (getterSetterCanBeReadOnly && property.isGetterRO)
                    {
                        p.Print("readonly ");
                    }

                    p.Print("get => ");
                    p.PrintIf(isRef, "ref ");

                    p.Print($"{accessor}.{propName}");
                    p.Print(";").PrintEndLine().PrintEndLine();
                }

                if (withoutSetter)
                {
                    return;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine();
                p.PrintIf(getterSetterCanBeReadOnly && property.isSetterRO, "readonly ");
                p.PrintEndLine($"set => {accessor}.{propName} = value;");
            }
        }

        private readonly void WriteEvents(ref Printer p)
        {
            foreach (var evt in events)
            {
                if (evt.explicitInterfaceImplementationsLength > 0)
                {
                    continue;
                }

                WriteEvent(ref p, evt);
            }
        }

        private readonly void WriteEvent(ref Printer p, in EventDeclaration evt)
        {
            var name = evt.name;
            var returnTypeName = evt.typeName;
            var isPublic = evt.isPublic;

            p.PrintBeginLineIf(isPublic, "public ", "");
            p.PrintIf(evt.isStatic, "static ");
            p.Print("event ");

            p.Print(returnTypeName);
            p.Print(" ");

            var explicitTypeName = string.Empty;

            p.PrintEndLine(name);
            p.OpenScope();
            {
                var fieldName = string.IsNullOrEmpty(explicitTypeName)
                    ? $"this.{this.fieldName}"
                    : $"(({explicitTypeName})this.{this.fieldName})";

                var accessor = evt.isStatic ? returnTypeName : fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("add => ").Print(accessor).Print(".").Print(name).PrintEndLine(" += value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("remove => ").Print(accessor).Print(".").Print(name).PrintEndLine(" -= value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteMethods(ref Printer p)
        {
            SpecialMethodType writtenSpecialMethods = default;

            foreach (var method in methods)
            {
                if (method.explicitInterfaceImplementationsLength > 0)
                {
                    continue;
                }

                WriteMethod(ref p, method, ref writtenSpecialMethods);
            }

            WriteAdditionalMethods(ref p, writtenSpecialMethods);
        }

        private readonly void WriteMethod(
              ref Printer p
            , in MethodDeclaration method
            , ref SpecialMethodType writtenSpecialMethods
        )
        {
            var methodName = method.name;
            var returnTypeName = method.returnTypeName;
            var hasParams = string.IsNullOrEmpty(method.parameters) == false;

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLineIf(method.isPublic, "public ", "");
            p.PrintIf(method.isUnsafe, "unsafe ");
            p.PrintIf(method.isStatic, "static ");
            p.PrintIf(isStruct == false && method.isOverride, "override ");
            p.PrintIf(method.isReadOnly, "readonly ");
            p.PrintIf(method.refKind == RefKind.Ref, "ref ");
            p.PrintIf(method.refKind == RefKind.RefReadOnly, "ref readonly ");
            p.PrintIf(method.returnsVoid, "void", returnTypeName);

            p.Print(" ");

            p.Print(methodName).Print(method.typeParameters).Print("(");

            if (hasParams)
            {
                p.Print(method.parameters);
            }
            else
            {
                switch (methodName)
                {
                    case "GetHashCode":
                        writtenSpecialMethods |= SpecialMethodType.GetHashCode;
                        break;

                    case "ToString":
                        writtenSpecialMethods |= SpecialMethodType.ToString;
                        break;
                }
            }

            var hasTypeConstraints = string.IsNullOrEmpty(method.typeParameterConstraints) == false;

            p.PrintEndLine(")");
            p.PrintLineIf(hasTypeConstraints, method.typeParameterConstraints);
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("=> ");

                p.PrintIf(method.refKind == RefKind.Ref, "ref ");
                p.PrintIf(method.refKind == RefKind.RefReadOnly, "ref readonly ");

                if (method.isStatic)
                {
                    p.Print(fieldTypeName);
                }
                else
                {
                    p.Print($"this.{fieldName}");
                }

                p.Print(".").Print(method.name).Print(method.typeParameters).Print("(");

                if (hasParams)
                {
                    p.Print(method.arguments);
                }

                p.PrintEndLine(");");
            }
            p = p.DecreasedIndent();
            p.PrintEndLine();
        }

        private readonly void WriteAdditionalMethods(ref Printer p, SpecialMethodType writtenSpecialMethods)
        {
            var hasCompareToT = ignoreInterfaceMethods.HasFlag(InterfaceKind.ComparableT) == false
                && implementInterfaces.HasFlag(InterfaceKind.ComparableT);

            if (hasCompareToT)
            {
                if (isFieldEnum)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("int CompareTo(").Print(fieldTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> ((").Print(fieldEnumUnderlyingTypeName)
                            .Print(")this.").Print(fieldName).Print(").CompareTo((")
                            .Print(fieldEnumUnderlyingTypeName).PrintEndLine(")other);");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("int CompareTo(").Print(fullTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> this.CompareTo(other.").Print(fieldName).PrintEndLine(");");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
                else
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("int CompareTo(").Print(fullTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> this.").Print(fieldName).Print(".CompareTo(other.").Print(fieldName).PrintEndLine(");");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
            }

            var hasCompareTo = ignoreInterfaceMethods.HasFlag(InterfaceKind.Comparable) == false
                && implementInterfaces.HasFlag(InterfaceKind.Comparable)
                && implementSpecialMethods.HasFlag(SpecialMethodType.CompareTo);

            if ((hasCompareToT || hasCompareTo) && isRefStruct == false)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ").PrintEndLine("int CompareTo(object obj)");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("=> obj switch");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(typeNameWithTypeParams).PrintEndLine(" other => CompareTo(other),");
                        p.PrintBeginLine(fieldTypeName).Print(" other => this.").Print(fieldName).PrintEndLine(".CompareTo(other),");
                        p.PrintLine("_ => 1,");
                    }
                    p.CloseScope("};");
                }
                p = p.DecreasedIndent();
                p.PrintEndLine();
            }

            if (isStruct == false && isRecord)
            {
                return;
            }

            if (ignoreInterfaceMethods.HasFlag(InterfaceKind.EquatableT) == false
                && (implementOperators.HasFlag(OperatorKind.Equal)
                || implementInterfaces.HasFlag(InterfaceKind.EquatableT)
            ))
            {
                if (isFieldEnum)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("bool Equals(").Print(fieldTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> this.").Print(fieldName).Print(" == other").PrintEndLine(";");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("bool Equals(").Print(fullTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> this.").Print(fieldName).Print(" == other.").Print(fieldName).PrintEndLine(";");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
                else
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ", "virtual ")
                        .Print("bool Equals(").Print(fullTypeName).PrintEndLine(" other)");
                    p = p.IncreasedIndent();
                    {
                        if (implementOperators.HasFlag(OperatorKind.Equal))
                        {
                            p.PrintBeginLine("=> this.").Print(fieldName).Print(" == other.").Print(fieldName).PrintEndLine(";");
                        }
                        else
                        {
                            p.PrintBeginLine("=> this.").Print(fieldName).Print(".Equals(other.").Print(fieldName).PrintEndLine(");");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
            }

            var hasEquals = ignoreInterfaceMethods.HasFlag(InterfaceKind.EquatableT)
                || implementOperators.HasFlag(OperatorKind.Equal)
                || implementInterfaces.HasFlag(InterfaceKind.EquatableT);

            if (isRecord == false
                && hasEquals
                && implementSpecialMethods.HasFlag(SpecialMethodType.Equals)
                && isRefStruct == false
            )
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ").PrintEndLine("override bool Equals(object obj)");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("=> obj switch");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(typeNameWithTypeParams).PrintEndLine(" other => Equals(other),");
                        p.PrintBeginLine(fieldTypeName).Print(" other => this.").Print(fieldName).PrintEndLine(".Equals(other),");
                        p.PrintLine("_ => false,");
                    }
                    p.CloseScope("};");
                }
                p = p.DecreasedIndent();
                p.PrintEndLine();
            }

            if (writtenSpecialMethods.HasFlag(SpecialMethodType.GetHashCode) == false
                && implementSpecialMethods.HasFlag(SpecialMethodType.GetHashCode)
                && isRefStruct == false
            )
            {
                if (isFieldEnum)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ").PrintEndLine("override int GetHashCode()");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> ((").Print(fieldEnumUnderlyingTypeName).Print(")this.")
                            .Print(fieldName).PrintEndLine(").GetHashCode();");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
                else
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ").PrintEndLine("override int GetHashCode()");
                    p = p.IncreasedIndent();
                    {
                        p.PrintBeginLine("=> this.").Print(fieldName).PrintEndLine(".GetHashCode();");
                    }
                    p = p.DecreasedIndent();
                    p.PrintEndLine();
                }
            }

            if (writtenSpecialMethods.HasFlag(SpecialMethodType.ToString) == false
                && implementSpecialMethods.HasFlag(SpecialMethodType.ToString)
                && isRefStruct == false
            )
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").PrintIf(isStruct, "readonly ").PrintEndLine("override string ToString()");
                p = p.IncreasedIndent();
                {
                    p.PrintBeginLine("=> this.").Print(fieldName).PrintEndLine(".ToString();");
                }
                p = p.DecreasedIndent();
                p.PrintEndLine();
            }

            if (isRefStruct)
            {
                p.PrintLine(OBSOLETE_REF_STRUCT);
                p.PrintLine("public override int GetHashCode() => throw null;");
                p.PrintEndLine();

                p.PrintLine(OBSOLETE_REF_STRUCT);
                p.PrintLine("public override bool Equals(object other) => throw null;");
                p.PrintEndLine();
            }
        }

        private readonly void WriteConversionOperators(ref Printer p)
        {
            if (fieldTypeIsInterface)
            {
                return;
            }

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").PrintIf(isStruct, "implicit", "explicit")
                .Print(" operator ").Print(fullTypeName)
                .Print("(").Print(fieldTypeName).PrintEndLine(" value)");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("=> new ").Print(fullTypeName).PrintEndLine("(value);");
            }
            p = p.DecreasedIndent();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static implicit operator ").Print(fieldTypeName)
                .Print("(").Print(fullTypeName).PrintEndLine(" value)");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("=> value.").Print(fieldName).PrintEndLine(";");
            }
            p = p.DecreasedIndent();
            p.PrintEndLine();
        }

        private readonly void WriteOperators(ref Printer p)
        {
            var operatorKinds = OperatorKinds.All;
            var ignoreOperators = this.ignoreOperators;
            var implementOperators = this.implementOperators;
            var operatorMap = this.operatorMap;
            var fullTypeName = this.fullTypeName;
            var fieldTypeName = this.fieldTypeName;
            var fieldSpecialType = this.fieldSpecialType;
            var fieldUnderlyingSpecialType = this.fieldUnderlyingSpecialType;
            var fieldName = this.fieldName;

            foreach (var operatorKind in operatorKinds)
            {
                if (ignoreOperators.HasFlag(operatorKind)
                    || implementOperators.HasFlag(operatorKind) == false
                )
                {
                    continue;
                }

                if (operatorMap.TryGetValue(operatorKind, out var operators))
                {
                    foreach (var (returnType, argTypes) in operators)
                    {
                        WriteOperator(
                              ref p
                            , operatorKind
                            , fullTypeName
                            , fieldTypeName
                            , fieldName
                            , returnType
                            , argTypes
                            , fieldSpecialType
                        );
                    }
                }
                else
                {
                    var returnType = new OpType(DetermineReturnType(operatorKind, fullTypeName), true);
                    var argTypes = DetermineArgTypes(operatorKind, fullTypeName, fieldSpecialType, fieldUnderlyingSpecialType);

                    WriteOperator(
                          ref p
                        , operatorKind
                        , fullTypeName
                        , fieldTypeName
                        , fieldName
                        , returnType
                        , argTypes
                        , fieldSpecialType
                    );
                }
            }

            if (fieldSpecialType == SpecialType.System_Enum)
            {
                WriteEnumOperators(ref p, fullTypeName, fieldUnderlyingSpecialType, fieldName);
            }
        }

        private static void WriteOperator(
              ref Printer p
            , OperatorKind kind
            , string fullTypeName
            , string fieldTypeName
            , string fieldName
            , OpType opReturnType
            , OpArgTypes opArgTypes
            , SpecialType fieldSpecialType
        )
        {
            var (isValid, firstType, firstName, secondType, secondName) = opArgTypes;

            if (isValid == false)
            {
                return;
            }

            var op = GetOp(kind);

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").Print(opReturnType.Value).Print(" operator ").Print(op).Print("(");
            WriteArgs(ref p, opArgTypes);
            p.PrintEndLine(")");
            p.OpenScope();
            {
                switch (kind)
                {
                    case OperatorKind.UnaryPlus:
                    case OperatorKind.UnaryMinus:
                    case OperatorKind.Negation:
                    case OperatorKind.OnesComplement:
                    {
                        p.PrintBeginLine("return ");

                        if (opReturnType.IsWrapper)
                        {
                            p.Print("new ").Print(fullTypeName).Print("((").Print(fieldTypeName).Print(")(")
                                .Print(op).Print("(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(")))");
                        }
                        else
                        {
                            p.Print("(").Print(fieldTypeName).Print(")(").Print(op).Print("(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print("))");
                        }

                        p.PrintEndLine(";");
                        break;
                    }

                    case OperatorKind.Increment:
                    case OperatorKind.Decrement:
                    {
                        p.PrintBeginLine("var tempValue = ");
                        WriteParam(ref p, firstType, firstName, fieldName);
                        p.PrintEndLine(";");

                        p.PrintBeginLine("tempValue ")
                            .PrintIf(kind == OperatorKind.Increment, "++", "--")
                            .PrintEndLine(";");

                        if (opReturnType.IsWrapper)
                        {
                            p.PrintBeginLine("return new ").Print(fullTypeName).Print("((").Print(fieldTypeName)
                                .PrintEndLine(")(tempValue));");
                        }
                        else
                        {
                            p.PrintBeginLine("return (").Print(fieldTypeName).PrintEndLine(")tempValue;");
                        }
                        break;
                    }

                    case OperatorKind.True:
                    case OperatorKind.False:
                    {
                        p.PrintBeginLine("return ");
                        WriteParam(ref p, firstType, firstName, fieldName);
                        p.PrintEndLine(";");
                        break;
                    }

                    case OperatorKind.Addition:
                    {
                        if (fieldSpecialType != SpecialType.System_Enum)
                        {
                            goto case OperatorKind.Substraction;
                        }

                        p.PrintBeginLine("return ");

                        if (opReturnType.IsWrapper)
                        {
                            p.Print("new ").Print(fullTypeName).Print("((").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ").Print(secondName).Print("))");
                        }
                        else
                        {
                            p.Print("(").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ").Print(secondName);
                            p.Print(")");
                        }

                        p.PrintEndLine(";");
                        break;
                    }

                    case OperatorKind.Substraction:
                    case OperatorKind.Multiplication:
                    case OperatorKind.Division:
                    case OperatorKind.Remainder:
                    case OperatorKind.BitwiseAnd:
                    case OperatorKind.BitwiseOr:
                    case OperatorKind.BitwiseXor:
                    {
                        p.PrintBeginLine("return ");

                        if (opReturnType.IsWrapper)
                        {
                            p.Print("new ").Print(fullTypeName).Print("((").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ");
                            WriteParam(ref p, secondType, secondName, fieldName);
                            p.Print("))");
                        }
                        else
                        {
                            p.Print("(").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ");
                            WriteParam(ref p, secondType, secondName, fieldName);
                            p.Print(")");
                        }

                        p.PrintEndLine(";");
                        break;
                    }

                    case OperatorKind.LogicalAnd:
                    case OperatorKind.LogicalOr:
                    case OperatorKind.LogicalXor:
                    case OperatorKind.Equal:
                    case OperatorKind.NotEqual:
                    case OperatorKind.EqualCustom:
                    case OperatorKind.NotEqualCustom:
                    case OperatorKind.Greater:
                    case OperatorKind.Lesser:
                    case OperatorKind.GreaterEqual:
                    case OperatorKind.LesserEqual:
                    {
                        p.PrintBeginLine("return ");
                        WriteParam(ref p, firstType, firstName, fieldName);
                        p.Print(" ").Print(op).Print(" ");
                        WriteParam(ref p, secondType, secondName, fieldName);
                        p.PrintEndLine(";");
                        break;
                    }

                    case OperatorKind.LeftShift:
                    case OperatorKind.RightShift:
                    case OperatorKind.UnsignedRightShift:
                    {
                        p.PrintBeginLine("return ");

                        if (opReturnType.IsWrapper)
                        {
                            p.Print("new ").Print(fullTypeName).Print("((").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ");
                            WriteParam(ref p, secondType, secondName, fieldName);
                            p.Print("))");
                        }
                        else
                        {
                            p.Print("(").Print(fieldTypeName).Print(")(");
                            WriteParam(ref p, firstType, firstName, fieldName);
                            p.Print(" ").Print(op).Print(" ");
                            WriteParam(ref p, secondType, secondName, fieldName);
                            p.Print(")");
                        }

                        p.PrintEndLine(";");
                        break;
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            static void WriteArgs(ref Printer p, OpArgTypes opArgTypes)
            {
                var (isValid, firstType, firstName, secondType, secondName) = opArgTypes;

                if (isValid == false)
                {
                    return;
                }

                if (string.IsNullOrEmpty(secondName))
                {
                    WriteArg(ref p, firstType, firstName);
                }
                else
                {
                    WriteArg(ref p, firstType, firstName);
                    p.Print(", ");
                    WriteArg(ref p, secondType, secondName);
                }
            }

            static void WriteArg(ref Printer p, OpType type, string name)
            {
                p.Print(type.Value).Print(" ").Print(name);
            }

            static void WriteParam(ref Printer p, OpType type, string name, string fieldName)
            {
                p.Print(name);

                if (type.IsWrapper)
                {
                    p.Print(".").Print(fieldName);
                }
            }
        }

        private readonly void WriteEnumOperators(
              ref Printer p
            , string fullTypeName
            , SpecialType fieldUnderlyingSpecialType
            , string fieldName
        )
        {
            var map = operatorMap;

            {
                var kind = OperatorKind.Substraction;
                var op = GetOp(kind);

                if (map.TryGetValue(kind, out var operators))
                {
                    foreach (var (returnType, _) in operators)
                    {
                        Write(ref p, fullTypeName, fieldUnderlyingSpecialType, fieldName, op, returnType);
                    }
                }
                else
                {
                    var returnType = new OpType(DetermineReturnType(kind, fullTypeName), true);
                    Write(ref p, fullTypeName, fieldUnderlyingSpecialType, fieldName, op, returnType);
                }
            }

            static void Write(
                  ref Printer p
                , string fullTypeName
                , SpecialType fieldUnderlyingSpecialType
                , string fieldName
                , string op
                , OpType opReturnType
            )
            {
                var args = fieldUnderlyingSpecialType switch {
                    SpecialType.System_SByte => $"{fullTypeName} left, sbyte right",
                    SpecialType.System_Byte => $"{fullTypeName} left, byte right",
                    SpecialType.System_Int16 => $"{fullTypeName} left, short right",
                    SpecialType.System_UInt16 => $"{fullTypeName} left, ushort right",
                    SpecialType.System_Int32 => $"{fullTypeName} left, int right",
                    SpecialType.System_UInt32 => $"{fullTypeName} left, uint right",
                    SpecialType.System_Int64 => $"{fullTypeName} left, long right",
                    SpecialType.System_UInt64 => $"{fullTypeName} left, ulong right",
                    _ => $"{fullTypeName} left, sbyte right",
                };

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ").Print(opReturnType.Value).Print(" operator ").Print(op)
                    .Print("(").Print(args).PrintEndLine(")");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ");

                    if (opReturnType.IsWrapper)
                    {
                        p.Print("new ").Print(fullTypeName).Print("(")
                            .Print("left.").Print(fieldName)
                            .Print(" ").Print(op).Print(" ")
                            .Print("right")
                            .Print(")");
                    }
                    else
                    {
                        p.Print("left.").Print(fieldName)
                        .Print(" ").Print(op).Print(" ")
                        .Print("right");
                    }

                    p.PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static string GetOp(OperatorKind kind)
        {
            return kind switch {
                OperatorKind.UnaryPlus => "+",
                OperatorKind.UnaryMinus => "-",
                OperatorKind.Negation => "!",
                OperatorKind.OnesComplement => "~",
                OperatorKind.Increment => "++",
                OperatorKind.Decrement => "--",
                OperatorKind.True => "true",
                OperatorKind.False => "false",
                OperatorKind.Addition => "+",
                OperatorKind.Substraction => "-",
                OperatorKind.Multiplication => "*",
                OperatorKind.Division => "/",
                OperatorKind.Remainder => "%",
                OperatorKind.LogicalAnd => "&",
                OperatorKind.LogicalOr => "|",
                OperatorKind.LogicalXor => "^",
                OperatorKind.BitwiseAnd => "&",
                OperatorKind.BitwiseOr => "|",
                OperatorKind.BitwiseXor => "^",
                OperatorKind.LeftShift => "<<",
                OperatorKind.RightShift => ">>",
                OperatorKind.UnsignedRightShift => ">>>",
                OperatorKind.Equal => "==",
                OperatorKind.NotEqual => "!=",
                OperatorKind.EqualCustom => "==",
                OperatorKind.NotEqualCustom => "!=",
                OperatorKind.Greater => ">",
                OperatorKind.Lesser => "<",
                OperatorKind.GreaterEqual => ">=",
                OperatorKind.LesserEqual => "<=",
                _ => string.Empty,
            };
        }

        private readonly void WriteTypeConverter(ref Printer p)
        {
            if (excludeConverter || isRefStruct)
            {
                return;
            }

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private sealed class {typeNameWithTypeParams}TypeConverter : TypeConverter");
            p.OpenScope();
            {
                p.PrintLine($"private static readonly Type s_wrapperType = typeof({fullTypeName});");
                p.PrintLine($"private static readonly Type s_underlyingType = typeof({fieldTypeName});");

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)");
                p.OpenScope();
                {
                    p.PrintLine("if (sourceType == s_wrapperType || sourceType == s_underlyingType) return true;");
                    p.PrintLine("return base.CanConvertFrom(context, sourceType);");
                }
                p.CloseScope();

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)");
                p.OpenScope();
                {
                    p.PrintLine($"if (destinationType == s_wrapperType || destinationType == s_underlyingType) return true;");
                    p.PrintLine($"return base.CanConvertTo(context, destinationType);");
                }
                p.CloseScope();

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)");
                p.OpenScope();
                {
                    p.PrintLine("if (value != null)");
                    p.OpenScope();
                    {
                        p.PrintLine("var t = value.GetType();");
                        p.PrintLine($"if (t == typeof({fullTypeName})) return ({fullTypeName})value;");
                        p.PrintLine($"if (t == typeof({fieldTypeName})) return new {fullTypeName}(({fieldTypeName})value);");
                    }
                    p.CloseScope();

                    p.PrintLine("return base.ConvertFrom(context, culture, value);");
                }
                p.CloseScope();

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)");
                p.OpenScope();
                {
                    p.PrintLine($"if (value is {fullTypeName} wrappedValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (destinationType == s_wrapperType) return wrappedValue;");
                        p.PrintLine($"if (destinationType == s_underlyingType) return wrappedValue.{fieldName};");
                    }
                    p.CloseScope();

                    p.PrintLine("return base.ConvertTo(context, culture, value, destinationType);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteHelpers(ref Printer p)
        {
            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();
        }
    }
}
