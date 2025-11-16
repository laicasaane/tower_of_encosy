namespace EncosyTower.SourceGen.Generators.Variants
{
    public readonly struct VariantPrinter
    {
        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string LOG_REGISTRIES = "ENCOSY_LOG_VARIANTS_REGISTRIES";
        public const string NAMESPACE = "EncosyTower.Variants";
        public const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        public const string META_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.VariantBase.META_OFFSET)]";
        public const string DATA_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.VariantBase.DATA_OFFSET)]";
        public const string VARIANT_TYPE = $"global::{NAMESPACE}.Variant";
        public const string VARIANT_DATA_TYPE = $"global::{NAMESPACE}.VariantData";
        public const string VARIANT_TYPE_KIND = $"global::{NAMESPACE}.VariantTypeKind";
        public const string DOES_NOT_RETURN = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        public const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

        private readonly string _generatedCodeAttribute;

        public VariantPrinter(string generatedCodeAttribute)
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
                p.PrintBeginLine("if (").Print(VARIANT_DATA_TYPE).Print(".BYTE_COUNT >= ")
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
                p.PrintLine($"  global::{NAMESPACE}.Converters.IVariantConverter<T> converter");
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

                p.PrintLine($"global::{NAMESPACE}.Converters.VariantConverter.TryRegister<T>(converter);");
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();

                p.PrintLine("if (result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.Log(\"Register variant for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.LogError(\"Cannot register variant for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();

                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        public void WriteVariantBody(
              ref Printer p
            , bool isValueType
            , bool hasImplicitFromStructToType
            , string typeName
            , string structName
            , string variantName
        )
        {
            p.OpenScope();
            {
                if (isValueType)
                {
                    WriteFieldsForValueType(ref p, typeName, variantName);
                    WriteConstructorForValueType(ref p, typeName, structName, variantName);
                }
                else
                {
                    WriteFieldsForRefType(ref p, typeName, variantName);
                    WriteConstructorForRefType(ref p, typeName, structName, variantName);
                }

                WriteOtherConstructors(ref p, structName, variantName);
                WriteValidateTypeIdMethod(ref p, typeName, variantName);

                WriteImplicitConversions(
                      ref p
                    , typeName
                    , structName
                    , variantName
                    , hasImplicitFromStructToType
                );

                WriteConverterClass(ref p, typeName, structName, variantName);
            }
            p.CloseScope();
        }

        private void WriteFieldsForValueType(ref Printer p, string typeName, string variantName)
        {
            p.PrintLine(META_OFFSET).PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {variantName} Variant;");
            p.PrintEndLine();

            p.PrintLine(DATA_OFFSET).PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {typeName} Value;");
            p.PrintEndLine();
        }

        private void WriteFieldsForRefType(ref Printer p, string typeName, string variantName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(PRESERVE);
            p.PrintLine($"public readonly {variantName} Variant;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {typeName} Value");
            p.OpenScope();
            {
                p.PrintLine("get");
                p.OpenScope();
                {
                    p.PrintLine($"if (this.Variant.Value.TryGetValue(out object obj) && obj is {typeName} objectT) return objectT;");
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
            , string variantName
        )
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}({typeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"this.Variant = new {VARIANT_TYPE}({VARIANT_TYPE_KIND}.ValueType, {variantName}.TypeId);");
                p.PrintLine("this.Value = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorForRefType(
              ref Printer p
            , string typeName
            , string structName
            , string variantName
        )
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}({typeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"this.Variant = new {VARIANT_TYPE}({variantName}.TypeId, (object)value);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteOtherConstructors(ref Printer p, string structName, string variantName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}(in {variantName} variant) : this()");
            p.OpenScope();
            {
                p.PrintLine("this.Variant = variant;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"public {structName}(in {VARIANT_TYPE} variant) : this()");
            p.OpenScope();
            {
                p.PrintLine("ValidateTypeId(variant);");
                p.PrintLine("this.Variant = variant;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteValidateTypeIdMethod(ref Printer p, string typeName, string variantName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintLine($"private static void ValidateTypeId(in {VARIANT_TYPE} variant)");
            p.OpenScope();
            {
                p.PrintLine($"if (variant.TypeId != {variantName}.TypeId)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfInvalidCast(variant);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(DOES_NOT_RETURN);
            p.PrintLine($"private static void ThrowIfInvalidCast(in {VARIANT_TYPE} variant)");
            p.OpenScope();
            {
                p.PrintLine("var type = global::EncosyTower.Types.TypeIdExtensions.ToType(variant.TypeId);");
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
            , string variantName
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
            p.PrintLine($"public static implicit operator {VARIANT_TYPE}(in {structName} value) => value.Variant;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            p.PrintLine($"public static implicit operator {variantName}(in {structName} value) => value.Variant;");
            p.PrintEndLine();

            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            p.PrintLine($"public static implicit operator {structName}(in {variantName} value) => new {structName}(value);");
            p.PrintEndLine();
        }

        private void WriteConverterClass(ref Printer p, string typeName, string structName, string variantName)
        {
            p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
            p.PrintBeginLine()
                .Print("public sealed class Converter")
                .Print($" : global::EncosyTower.Variants.Converters.IVariantConverter<{typeName}>")
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
                p.PrintLine($"public {VARIANT_TYPE} ToVariant({typeName} value) => new {structName}(value);");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public {variantName} ToVariantT({typeName} value) => new {structName}(value).Variant;");
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public {typeName} GetValue(in {VARIANT_TYPE} variant)");
                p.OpenScope();
                {
                    p.PrintLine($"if (variant.TypeId != {variantName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine("ThrowIfInvalidCast();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"var temp = new {structName}(variant);");
                    p.PrintLine("return temp.Value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(_generatedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public bool TryGetValue(in {VARIANT_TYPE} variant, out {typeName} result)");
                p.OpenScope();
                {
                    p.PrintLine($"if (variant.TypeId == {variantName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(variant);");
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
                p.PrintLine($"public bool TrySetValueTo(in {VARIANT_TYPE} variant, ref {typeName} result)");
                p.OpenScope();
                {
                    p.PrintLine($"if (variant.TypeId == {variantName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(variant);");
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
                p.PrintLine($"public string ToString(in {VARIANT_TYPE} variant)");
                p.OpenScope();
                {
                    p.PrintLine($"if (variant.TypeId == {variantName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var temp = new {structName}(variant);");
                        p.PrintLine("return temp.Value.ToString();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return global::EncosyTower.Types.TypeIdExtensions.ToType(variant.TypeId).ToString();");
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
                        p.PrintLine($"$\"Cannot get value of {{typeof({typeName})}} from the input variant.\"");
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
