namespace EncosyTower.SourceGen.Generators.Unions
{
    public readonly struct UnionPrinter
    {
        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string LOG_REGISTRIES = "ENCOSY_LOG_UNIONS_REGISTRIES";
        public const string NAMESPACE = "EncosyTower.Unions";
        public const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        public const string META_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.UnionBase.META_OFFSET)]";
        public const string DATA_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.UnionBase.DATA_OFFSET)]";
        public const string UNION_TYPE = $"global::{NAMESPACE}.Union";
        public const string UNION_DATA_TYPE = $"global::{NAMESPACE}.UnionData";
        public const string UNION_TYPE_KIND = $"global::{NAMESPACE}.UnionTypeKind";
        public const string DOES_NOT_RETURN = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        public const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

        private readonly string _generatedCodeAttribute;

        public UnionPrinter(string generatedCodeAttribute)
        {
            _generatedCodeAttribute = generatedCodeAttribute;
        }

        public void WriteRegister(
              ref Printer p
            , string typeName
            , string simpleTypeName
            , string converterDefault
            , int? unmanagedSize
        )
        {
            if (unmanagedSize.HasValue)
            {
                p.PrintBeginLine("if (").Print(UNION_DATA_TYPE).Print(".BYTE_COUNT >= ")
                    .Print(unmanagedSize.Value.ToString())
                    .PrintEndLine(")");
                p.OpenScope();
            }

            p.OpenScope($"Register<{typeName}>({converterDefault}");
            {
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintBeginLine(", \"").Print(simpleTypeName).PrintEndLine("\"");
                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope(");");

            if (unmanagedSize.HasValue)
            {
                p.CloseScope();
            }

            p.PrintEndLine();
        }

        public void WriteRegisterMethod(ref Printer p)
        {
            p.Print("#if !UNITY_EDITOR || !").Print(LOG_REGISTRIES).PrintEndLine();
            p.PrintLine(AGGRESSIVE_INLINING);
            p.Print("#endif").PrintEndLine();
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.OpenScope("private static void Register<T>(");
            {
                p.PrintLine($"  global::{NAMESPACE}.Converters.IUnionConverter<T> converter");
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintLine(", string typeName");
                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope(")");
            p.OpenScope();
            {
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintLine("var result =");
                p.Print("#endif").PrintEndLine();

                p.PrintLine($"global::{NAMESPACE}.Converters.UnionConverter.TryRegister<T>(converter);");
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();

                p.PrintLine("if (result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.Log(\"Register union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.LogError(\"Cannot register union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();

                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        public void WriteUnionBody(
              ref Printer p
            , bool isValueType
            , bool hasImplicitFromStructToType
            , string typeName
            , string structName
            , string unionName
        )
        {
            p.OpenScope();
            {
                if (isValueType)
                {
                    WriteFieldsForValueType(ref p, typeName, unionName);
                    WriteConstructorForValueType(ref p, typeName, structName, unionName);
                }
                else
                {
                    WriteFieldsForRefType(ref p, typeName, unionName);
                    WriteConstructorForRefType(ref p, typeName, structName, unionName);
                }

                WriteOtherConstructors(ref p, structName, unionName);
                WriteValidateTypeIdMethod(ref p, typeName, unionName);

                WriteImplicitConversions(
                      ref p
                    , typeName
                    , structName
                    , unionName
                    , hasImplicitFromStructToType
                );

                WriteConverterClass(ref p, typeName, structName, unionName);
            }
            p.CloseScope();
        }

        private void WriteFieldsForValueType(ref Printer p, string typeName, string unionName)
        {
            p.PrintLine(META_OFFSET).PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {unionName} Union;");
            p.PrintEndLine();

            p.PrintLine(DATA_OFFSET).PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {typeName} Value;");
            p.PrintEndLine();
        }

        private void WriteFieldsForRefType(ref Printer p, string typeName, string unionName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {unionName} Union;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {typeName} Value");
            p.OpenScope();
            {
                p.PrintLine("get");
                p.OpenScope();
                {
                    p.PrintLine($"if (this.Union.Value.TryGetValue(out object obj) && obj is {typeName} objectT) return objectT;");
                    p.PrintLine($"return default;");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorForValueType(
              ref Printer p
            , string typeName
            , string structName
            , string unionName
        )
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}({typeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"this.Union = new {UNION_TYPE}({UNION_TYPE_KIND}.ValueType, {unionName}.TypeId);");
                p.PrintLine("this.Value = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorForRefType(
              ref Printer p
            , string typeName
            , string structName
            , string unionName
        )
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}({typeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"this.Union = new {UNION_TYPE}({unionName}.TypeId, (object)value);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteOtherConstructors(ref Printer p, string structName, string unionName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}(in {unionName} union) : this()");
            p.OpenScope();
            {
                p.PrintLine("this.Union = union;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}(in {UNION_TYPE} union) : this()");
            p.OpenScope();
            {
                p.PrintLine("ValidateTypeId(union);");
                p.PrintLine("this.Union = union;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteValidateTypeIdMethod(ref Printer p, string typeName, string unionName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"private static void ValidateTypeId(in {UNION_TYPE} union)");
            p.OpenScope();
            {
                p.PrintLine($"if (union.TypeId != {unionName}.TypeId)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfInvalidCast(union);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(DOES_NOT_RETURN);
            p.PrintLine($"private static void ThrowIfInvalidCast(in {UNION_TYPE} union)");
            p.OpenScope();
            {
                p.PrintLine("var type = global::EncosyTower.Types.TypeIdExtensions.ToType(union.TypeId);");
                p.PrintEndLine();

                p.PrintLine("throw new global::System.InvalidCastException");
                p.OpenScope("(");
                {
                    p.PrintLine($"$\"Cannot cast {{type}} to {{typeof({typeName})}}\"");
                }
                p.CloseScope(");");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteImplicitConversions(
              ref Printer p
            , string typeName
            , string structName
            , string unionName
            , bool hasImplicitFromStructToType
        )
        {
            if (hasImplicitFromStructToType)
            {
                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public static implicit operator {structName}({typeName} value) => new {structName}(value);");
                p.PrintEndLine();
            }

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            p.PrintLine($"public static implicit operator {UNION_TYPE}(in {structName} value) => value.Union;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            p.PrintLine($"public static implicit operator {unionName}(in {structName} value) => value.Union;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            p.PrintLine($"public static implicit operator {structName}(in {unionName} value) => new {structName}(value);");
            p.PrintEndLine();
        }

        private void WriteConverterClass(ref Printer p, string typeName, string structName, string unionName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintBeginLine()
                .Print("public sealed class Converter")
                .Print($" : global::EncosyTower.Unions.Converters.IUnionConverter<{typeName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
                p.PrintLine("public static readonly Converter Default = new Converter();");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("private Converter() { }");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public {UNION_TYPE} ToUnion({typeName} value) => new {structName}(value);");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public {unionName} ToUnionT({typeName} value) => new {structName}(value).Union;");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public {typeName} GetValue(in {UNION_TYPE} union)");
                p.OpenScope();
                {
                    p.PrintLine($"if (union.TypeId != {unionName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine("ThrowIfInvalidCast();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"var temp = new {structName}(union);");
                    p.PrintLine("return temp.Value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public bool TryGetValue(in {UNION_TYPE} union, out {typeName} result)");
                p.OpenScope();
                {
                    p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(union);");
                        p.PrintLine("result = temp.Value;");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public bool TrySetValueTo(in {UNION_TYPE} union, ref {typeName} result)");
                p.OpenScope();
                {
                    p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(union);");
                        p.PrintLine("result = temp.Value;");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public string ToString(in {UNION_TYPE} union)");
                p.OpenScope();
                {
                    p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(union);");
                        p.PrintLine("return temp.Value.ToString();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return global::EncosyTower.Types.TypeIdExtensions.ToType(union.TypeId).ToString();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(DOES_NOT_RETURN);
                p.PrintLine("private static void ThrowIfInvalidCast()");
                p.OpenScope();
                {
                    p.PrintLine("throw new global::System.InvalidCastException");
                    p.OpenScope("(");
                    {
                        p.PrintLine($"$\"Cannot get value of {{typeof({typeName})}} from the input union.\"");
                    }
                    p.CloseScope(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
