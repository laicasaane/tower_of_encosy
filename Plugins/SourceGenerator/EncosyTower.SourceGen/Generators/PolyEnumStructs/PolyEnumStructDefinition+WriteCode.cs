using System;
using System.Collections.Generic;
using EncosyTower.SourceGen.Generators.EnumExtensions;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    partial struct PolyEnumStructDefinition
    {
        private const string METHOD_IMPL_OPTIONS = "MethodImplOptions";
        private const string INLINING = $"{METHOD_IMPL_OPTIONS}.AggressiveInlining";
        private const string GENERATOR = "\"EncosyTower.SourceGen.Generators.PolyEnumStructs.PolyEnumStructGenerator\"";

        private const string AGGRESSIVE_INLINING = "[MethodImpl(INLINING)]";
        private const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        private const string VALIDATION_ATTRIBUTES = "[HideInCallstack, StackTraceHidden, " +
            "Conditional(\"UNITY_EDITOR\"), Conditional(\"DEVELOPMENT_BUILD\")]";
        private const string UNDEFINED_NAME = "Undefined";
        private const string ENUM_CASE_NAME = "EnumCase";

        private static readonly List<ConstructionValue> s_emptyValues = new();

        public readonly string WriteCode(bool referenceUnityCollections)
        {
            var tempCollections = new TempCollections();

            GenerateMerged(
                  out var structRefs
                , out var mergedStructRef
                , out var partialInterfaceRef
                , out var undefinedType
                , out var enumCaseType
            );

            var p = new Printer(0, 1024 * 512);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.Print("#region ENUM CASE").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // EnumCase");
                p.OpenScope();
                {
                    WriteEnumCaseEnum(ref p, enumCaseType.UnderlyingType, undefinedType);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region INTERFACE ENUM CASE").PrintEndLine();
                p.Print("#endregion ================").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // IEnumCase");
                p.OpenScope();
                {
                    WriteInterface(ref p, partialInterfaceRef, mergedStructRef);
                }
                p.CloseScope();
                p.PrintEndLine();
                p.Print("#region CASE STRUCTS").PrintEndLine();
                p.Print("#endregion =========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // Case Structs");
                p.OpenScope();
                {
                    WriteUndefinedStruct(ref p, undefinedType, mergedStructRef);
                    WriteCaseStructs(ref p, structs, mergedStructRef);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region ENUM STRUCT").PrintEndLine();
                p.Print("#endregion ========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeName)
                    .Print(" : ").Print(typeName).Print(".IEnumCase");

                if (autoEquatable)
                {
                    p.Print(", IEquatable<").Print(typeName).Print(">");
                }

                p.PrintEndLine(" // Enum Struct");
                p.OpenScope();
                {
                    WriteMergedFields(ref p, mergedStructRef.FieldRefs);
                    WriteMergedProperties(ref p, mergedStructRef.PropertyDimMap, tempCollections.Properties);
                    WriteMergedIndexers(ref p, mergedStructRef.IndexerDimMap, tempCollections.Indexers);
                    WriteImplicitOperators(ref p, structRefs, mergedStructRef.Size);
                    WriteToCaseMethods(ref p, structRefs);
                    WriteConstructFromMethods(ref p, mergedStructRef, undefinedType);
                    WriteTryGetConstructionValueMethods(ref p, mergedStructRef, undefinedType);
                    WriteMergedMethods(ref p, mergedStructRef.MethodDimMap, tempCollections.Methods);
                    WriteAdditionalMethods(ref p, mergedStructRef);
                    WritePrivateStaticMethods(ref p);
                    WriteThrowMethods(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region ENUM CASE API").PrintEndLine();
                p.Print("#endregion ==========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // Enum Case API");
                p.OpenScope();
                {
                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintLine("private static partial class EnumCaseAPI");
                    p.OpenScope();
                    {
                        WriteEnumCaseApiProperties(ref p, tempCollections.Properties);
                        WriteEnumCaseApiIndexers(ref p, tempCollections.Indexers);
                        WriteEnumCaseApiMethods(ref p, tempCollections.Methods);
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // Internals");
                p.OpenScope();
                {
                    WriteHelperConstants(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteEnumCaseExtensions(ref p, referenceUnityCollections, enumCaseType);
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteEnumCaseEnum(
              ref Printer p
            , string underlyingType
            , string undefinedType
        )
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum EnumCase : ").PrintEndLine(underlyingType);
            p.OpenScope();
            {
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(typeName).Print(".").Print(undefinedType).PrintEndLine("\"/>");
                p.PrintBeginLine("/// <seealso cref=\"").Print(typeName).Print(".").Print(undefinedType).PrintEndLine("\"/>");
                p.PrintLine("Undefined = 0,");
                p.PrintEndLine();

                var structs = this.structs;
                var count = structs.Count - 1;

                for (var i = 0; i < count; i++)
                {
                    var def = structs[i];
                    var value = i + 1;

                    p.PrintBeginLine("/// <inheritdoc cref=\"").Print(typeName).Print(".").Print(def.name).PrintEndLine("\"/>");
                    p.PrintBeginLine("/// <seealso cref=\"").Print(typeName).Print(".").Print(def.name).PrintEndLine("\"/>");
                    p.PrintBeginLine(def.identifier).Print(" = ").Print(value).PrintEndLine(",");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteInterface(
              ref Printer p
            , PartialInterfaceRef interfaceRef
            , MergedStructRef mergedStructRef
        )
        {
            var interfaceDef = this.interfaceDef;
            var accessor = interfaceDef.definedInterface ? "" : "public ";

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine(accessor).Print("partial interface ").PrintEndLine(interfaceDef.name);
            p.OpenScope();
            {
                foreach (var def in interfaceRef.Properties)
                {
                    WriteProperty(ref p, def);
                }

                foreach (var def in interfaceRef.Indexers)
                {
                    WriteIndexer(ref p, def);
                }

                foreach (var def in interfaceRef.Methods)
                {
                    WriteMethod(ref p, def);
                }

                WriteTryGetConstructionValueMethods(ref p, mergedStructRef);

                p.PrintLine("EnumCase GetEnumCase();");
                p.PrintEndLine();

                p.PrintBeginLine(typeName).Print(" To").Print(typeName).PrintEndLine("();");
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteProperty(ref Printer p, PropertyDefinition def)
            {
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name).Print(" ")
                    .Print(def.name).Print(" { ")
                    .PrintIf(def.IsWriteOnly == false, "get; ")
                    .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                    .PrintEndLine("}");
                p.PrintEndLine();
            }

            static void WriteIndexer(ref Printer p, IndexerDefinition def)
            {
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name).Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.Print("] { ")
                    .PrintIf(def.IsWriteOnly == false, "get; ")
                    .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                    .PrintEndLine("}");
                p.PrintEndLine();
            }

            static void WriteMethod(ref Printer p, MethodDefinition def)
            {
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name).Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(");");
                p.PrintEndLine();
            }

            static void WriteTryGetConstructionValueMethods(ref Printer p, MergedStructRef mergedStructRef)
            {
                var typeToStructs = mergedStructRef.TypeToStructsMap;

                foreach (var kv in typeToStructs)
                {
                    var type = kv.Key;

                    p.PrintBeginLine("bool TryGetConstructionValue(out ")
                        .Print(type.name).PrintEndLine(" value, int index = default);");
                    p.PrintEndLine();
                }
            }
        }

        private readonly void WriteUndefinedStruct(ref Printer p, string structName, MergedStructRef mergedStructRef)
        {
            if (definedUndefinedStruct != DefinedUndefinedStruct.None)
            {
                return;
            }

            var structId = new StructId {
                name = structName,
                identifier = UNDEFINED_NAME
            };

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct ").Print(structName)
                .Print(" : ").Print(interfaceDef.name);

            if (autoEquatable)
            {
                p.Print(", IEquatable<").Print(structName).Print(">");
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                foreach (var def in mergedStructRef.PropertyDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteProperty(ref p, def, isRefReturn);
                }

                foreach (var def in mergedStructRef.IndexerDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteIndexer(ref p, def, isRefReturn);
                }

                foreach (var def in mergedStructRef.MethodDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteMethod(ref p, def, isRefReturn);
                }

                WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly EnumCase GetEnumCase()");
                p.OpenScope();
                {
                    p.PrintLine("return EnumCase.Undefined;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(typeName).Print(" To").Print(typeName).PrintEndLine("()");
                p.OpenScope();
                {
                    p.PrintLine("return this;");
                }
                p.CloseScope();

                if (autoEquatable)
                {
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override bool Equals(object obj)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return obj is ").Print(structName).PrintEndLine(" other && Equals(other);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly bool Equals(").Print(structName).PrintEndLine(" other)");
                    p.OpenScope();
                    {
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override int GetHashCode()");
                    p.OpenScope();
                    {
                        p.PrintLine("return 0;");
                    }
                    p.CloseScope();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteProperty(ref Printer p, PropertyDefinition def, bool isRefReturn)
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.IsReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ")
                    .PrintEndLine(def.name);
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("get");
                        p.OpenScope();

                        if (isRefReturn)
                        {
                            p.PrintBeginLine("throw new InvalidOperationException(\"")
                                .Print("Cannot return by reference from case 'Undefined'.")
                                .PrintEndLine("\");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("set { }");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteIndexer(ref Printer p, IndexerDefinition def, bool isRefReturn)
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.IsReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine("]");
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("get");
                        p.OpenScope();

                        if (isRefReturn)
                        {
                            p.PrintBeginLine("throw new InvalidOperationException(\"")
                                .Print("Cannot return any reference from the default case.")
                                .PrintEndLine("\");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("set { }");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteMethod(ref Printer p, MethodDefinition def, bool isRefReturn)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ")
                    .PrintIf(def.isReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var arg in def.parameters.AsReadOnlySpan())
                    {
                        if (arg.refKind is RefKind.Ref or RefKind.Out)
                        {
                            p.PrintBeginLine(arg.name).PrintEndLine(" = default;");
                        }
                    }

                    if (def.returnsVoid)
                    {
                        p.PrintLine("return;");
                    }
                    else if (isRefReturn)
                    {
                        p.PrintBeginLine("throw new InvalidOperationException(\"")
                            .Print("Cannot return any reference from the default case.")
                            .PrintEndLine("\");");
                    }
                    else
                    {
                        p.PrintLine("return default;");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMergedFields(ref Printer p, List<MergedFieldRef> fieldRefs)
        {
            foreach (var fieldRef in fieldRefs)
            {
                p.PrintBeginLine("public ")
                    .Print(fieldRef.Value.returnType.name)
                    .Print(" ")
                    .Print(fieldRef.Name)
                    .PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        private readonly void WriteImplicitOperators(ref Printer p, List<StructRef> structRefs, int mergedStructSize)
        {
            var mergedIn = mergedStructSize > 8 ? "in " : "";

            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;
                var structIn = def.size > 8 ? "in " : "";

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(typeName).Print("(")
                    .Print(structIn)
                    .Print(def.name)
                    .PrintEndLine(" @case)");
                p.OpenScope();
                {
                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("enumCase = EnumCase.").Print(def.identifier).PrintEndLine(",");

                        foreach (var kv in structRef.FieldToMergedFieldMap)
                        {
                            p.PrintBeginLine(kv.Value).Print(" = @case.").Print(kv.Key).PrintEndLine(",");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static explicit operator ").Print(def.name).Print("(")
                    .Print(mergedIn)
                    .Print(typeName)
                    .PrintEndLine(" @enum)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return @enum.ToCase").Print(def.identifier).PrintEndLine("OrThrow();");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteToCaseMethods(ref Printer p, List<StructRef> structRefs)
        {
            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;
                var structIn = def.size > 8 ? "in " : "";

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(def.name)
                    .Print(" ToCase").Print(def.identifier).PrintEndLine("OrThrow()");
                p.OpenScope();
                {
                    p.PrintBeginLine("ThrowIfCannotCastFromEnumStruct(this.enumCase, EnumCase.")
                        .Print(def.identifier).PrintEndLine(");");
                    p.PrintEndLine();

                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        foreach (var kv in structRef.FieldToMergedFieldMap)
                        {
                            p.PrintBeginLine(kv.Key).Print(" = this.").Print(kv.Value).PrintEndLine(",");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(def.name)
                    .Print(" ToCase").Print(def.identifier).Print("OrDefault(")
                    .Print(structIn).Print(def.name).PrintEndLine(" @default = default)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (CanCastFromEnumStruct(this.enumCase, EnumCase.")
                        .Print(def.identifier).PrintEndLine("))");
                    p.OpenScope();
                    {
                        p.PrintLine("return new()");
                        p.OpenScope();
                        {
                            foreach (var kv in structRef.FieldToMergedFieldMap)
                            {
                                p.PrintBeginLine(kv.Key).Print(" = this.").Print(kv.Value).PrintEndLine(",");
                            }
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return @default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryToCase(out ")
                    .Print(def.name).PrintEndLine(" @case)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (CanCastFromEnumStruct(this.enumCase, EnumCase.")
                        .Print(def.identifier).PrintEndLine("))");
                    p.OpenScope();
                    {
                        p.PrintLine("@case = new()");
                        p.OpenScope();
                        {
                            foreach (var kv in structRef.FieldToMergedFieldMap)
                            {
                                p.PrintBeginLine(kv.Key).Print(" = this.").Print(kv.Value).PrintEndLine(",");
                            }
                        }
                        p.CloseScope("};");
                        p.PrintEndLine();

                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("@case = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteConstructFromMethods(
              ref Printer p
            , MergedStructRef mergedStructRef
            , string undefinedType
        )
        {
            var creationMap = mergedStructRef.TypeValueToStructMap;

            foreach (var kv1 in creationMap)
            {
                var type = kv1.Key;

                p.PrintBeginLine("public static ").Print(typeName).Print(" ConstructFrom(")
                    .Print(type.name).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("return value switch");
                    p.OpenScope();
                    {
                        foreach (var kv2 in kv1.Value)
                        {
                            var value = kv2.Key;
                            var structName = kv2.Value;

                            p.PrintBeginLine()
                                .PrintIf(type.isEnum, type.name).PrintIf(type.isEnum, ".")
                                .Print(value.value).Print(" => new ").Print(structName.name).PrintEndLine("(),");
                        }

                        p.PrintBeginLine("_ => new ").Print(undefinedType).PrintEndLine("(),");
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMergedProperties(
              ref Printer p
            , Dictionary<PropertyDefinition, bool> dimMap
            , List<PropertyDefinition> dimList
        )
        {
            dimList.Clear();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(ref p, def, isDim, structs);

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(ref Printer p, PropertyDefinition def, bool isDim, ReadOnlySpan<StructDefinition> structs)
            {
                var readonlyWritten = def.IsReadOnly && def.CanHaveSetter == false;

                p.PrintBeginLine("public ")
                    .PrintIf(readonlyWritten, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ")
                    .PrintEndLine(def.name);
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintBeginLine()
                            .PrintIf(def.getter.isReadOnly && readonlyWritten == false, "readonly ")
                            .PrintEndLine("get");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case EnumCase.").Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteGetterCase(ref p, def, isDim, structDef);
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteGetterCase(ref p, def, isDim, structs[last]);
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine("set");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case EnumCase.").Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteSetterCase(ref p, def, isDim, structDef);
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteSetterCase(ref p, def, isDim, structs[last]);
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetterCase(
                      ref Printer p
                    , PropertyDefinition def
                    , bool isDim
                    , StructDefinition structDef
                )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print("var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind));
                {
                    if (isDim)
                    {
                        p.Print("EnumCaseAPI.Property_Get_").Print(def.name)
                            .Print("(ref enum_case)");
                    }
                    else
                    {
                        p.Print("enum_case.").Print(def.name);
                    }
                }
                p.PrintEndLine(";");

                p.PrintLineIf(def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintEndLine("result_for_enum_case;");
            }

            static void WriteSetterCase(
                  ref Printer p
                , PropertyDefinition def
                , bool isDim
                , StructDefinition structDef
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");

                {
                    if (isDim)
                    {
                        p.PrintBeginLine("EnumCaseAPI.Property_Set_").Print(def.name)
                            .PrintEndLine("(ref enum_case, value);");
                    }
                    else
                    {
                        p.PrintBeginLine("enum_case.").Print(def.name).PrintEndLine(" = value;");
                    }
                }

                p.PrintLineIf(def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintLine("return;");
            }
        }

        private readonly void WriteMergedIndexers(
              ref Printer p
            , Dictionary<IndexerDefinition, bool> dimMap
            , List<IndexerDefinition> dimList
        )
        {
            dimList.Clear();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(ref p, def, isDim, structs);

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(ref Printer p, IndexerDefinition def, bool isDim, ReadOnlySpan<StructDefinition> structs)
            {
                var readonlyWritten = def.IsReadOnly && def.CanHaveSetter == false;

                p.PrintBeginLine("public ")
                    .PrintIf(readonlyWritten, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine("]");
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintBeginLine()
                            .PrintIf(def.getter.isReadOnly && readonlyWritten == false, "readonly ")
                            .PrintEndLine("get");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case EnumCase.").Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteGetterCase(ref p, def, isDim, structDef);
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteGetterCase(ref p, def, isDim, structs[last]);
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine("set");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case EnumCase.").Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteSetterCase(ref p, def, isDim, structDef);
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteSetterCase(ref p, def, isDim, structs[last]);
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetterCase(
                  ref Printer p
                , IndexerDefinition def
                , bool isDim
                , StructDefinition structDef
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print("var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintIf(isDim, "EnumCaseAPI.Indexer_Get(", "enum_case[");
                {
                    p.PrintIf(isDim, "ref enum_case, ");
                    WriteArguments(ref p, def.parameters);
                }
                p.PrintEndLineIf(isDim, ");", "];");
                p.PrintLineIf(def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintEndLine("result_for_enum_case;");
            }

            static void WriteSetterCase(
                  ref Printer p
                , IndexerDefinition def
                , bool isDim
                , StructDefinition structDef
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");

                if (isDim)
                {
                    p.PrintBeginLine("EnumCaseAPI.Indexer_Set(");
                    {
                        p.Print("ref enum_case, value ");
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine(");");
                }
                else
                {
                    p.PrintBeginLine("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.Print("]").PrintEndLine(" = value;");
                }

                p.PrintLineIf(def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintLine("return;");
            }
        }

        private readonly void WriteTryGetConstructionValueMethods(
              ref Printer p
            , MergedStructRef mergedStructRef
            , string undefinedType
        )
        {
            var typeToStructs = mergedStructRef.TypeToStructsMap;

            foreach (var kv in typeToStructs)
            {
                var type = kv.Key;
                var structs = kv.Value;

                p.PrintBeginLine("public readonly bool TryGetConstructionValue(out ")
                    .Print(type.name).PrintEndLine(" value, int index = default)");
                p.OpenScope();
                {
                    p.PrintLine("switch (this.enumCase)");
                    p.OpenScope();
                    {
                        foreach (var structId in structs)
                        {
                            p.PrintBeginLine("case EnumCase.").Print(structId.identifier).PrintEndLine(":");
                            p.OpenScope();
                            {
                                p.PrintBeginLine(structId.name).Print(" enum_case = (")
                                    .Print(structId.name).PrintEndLine(")this;");
                                p.PrintLine("return enum_case.TryGetConstructionValue(out value, index);");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("value = default;");
                            p.PrintLine("return false;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMergedMethods(
              ref Printer p
            , Dictionary<MethodDefinition, bool> dimMap
            , List<MethodDefinition> dimList
        )
        {
            dimList.Clear();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(ref p, def, isDim, structs);

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(ref Printer p, MethodDefinition def, bool isDim, ReadOnlySpan<StructDefinition> structs)
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.isReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.OpenScope();
                {
                    p.PrintLine("switch (this.enumCase)");
                    p.OpenScope();
                    {
                        var length = structs.Length;
                        var last = length - 1;

                        for (var i = 0; i < last; i++)
                        {
                            var structDef = structs[i];

                            p.PrintBeginLine("case EnumCase.").Print(structDef.identifier).PrintEndLine(":");
                            p.OpenScope();
                            {
                                WriteCase(ref p, def, isDim, structDef);
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            WriteCase(ref p, def, isDim, structs[last]);
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCase(
                  ref Printer p
                , MethodDefinition def
                , bool isDim
                , StructDefinition structDef
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .PrintIf(def.returnsVoid == false, "var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintIf(isDim, "EnumCaseAPI.", "enum_case.").Print(def.name).Print("(");
                {
                    p.PrintIf(isDim, "ref enum_case, ");
                    WriteArguments(ref p, def.parameters);
                }
                p.PrintEndLine(");");
                p.PrintLineIf(def.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return")
                    .Print(GetAnyRefSpacePrefix(def.refKind))
                    .PrintIf(def.returnsVoid == false, " result_for_enum_case")
                    .PrintEndLine(";");
            }
        }

        private readonly void WriteAdditionalMethods(ref Printer p, MergedStructRef mergedStructRef)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly EnumCase GetEnumCase()");
            p.OpenScope();
            {
                p.PrintLine("return enumCase;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public readonly ").Print(typeName).Print(" To").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintLine("return this;");
            }
            p.CloseScope();

            if (autoEquatable == false)
            {
                return;
            }

            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(typeName).PrintEndLine(" other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public readonly bool Equals(").Print(typeName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return");
                p = p.IncreasedIndent();

                var fieldRefs = mergedStructRef.FieldRefs;
                var count = fieldRefs.Count;

                for (var i = 0; i < count; i++)
                {
                    var fieldRef = fieldRefs[i];
                    var fieldName = fieldRef.Name;
                    var op = i > 0 ? "&& " : "   ";

                    if (fieldRef.Value.returnType.isEnum)
                    {
                        p.PrintBeginLine(op).Print(fieldName).Print(" == other.")
                            .Print(fieldName).PrintEndLine("");
                    }
                    else
                    {
                        p.PrintBeginLine(op).Print(fieldName).Print(".Equals(other.")
                            .Print(fieldName).PrintEndLine(")");
                    }
                }

                p.PrintLine(";");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public readonly override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = new HashValue();");

                var fieldRefs = mergedStructRef.FieldRefs;
                var count = fieldRefs.Count;

                for (var i = 0; i < count; i++)
                {
                    var fieldName = fieldRefs[i].Name;
                    p.PrintBeginLine("hash.Add(").Print(fieldName).PrintEndLine(");");
                }

                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WritePrivateStaticMethods(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("private static bool CanCastFromEnumStruct(EnumCase enumStruct, EnumCase caseStruct)");
            p.OpenScope();
            {
                p.PrintLine("return enumStruct == caseStruct;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteThrowMethods(ref Printer p)
        {
            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine("private static void ThrowIfCannotCastFromEnumStruct")
                .PrintEndLine("(EnumCase enumStruct, EnumCase caseStruct)");
            p.OpenScope();
            {
                p.PrintLine("if (CanCastFromEnumStruct(enumStruct, caseStruct) == false)");
                p.OpenScope();
                {
                    p.PrintLine("throw new InvalidCastException(");
                    p.WithIncreasedIndent().PrintBeginLine("$\"")
                        .Print("Cannot cast '").Print(typeName).Print("' into '{caseStruct}' ")
                        .Print("because it currently stores a '{enumStruct}'.")
                        .PrintEndLine("\"");
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEnumCaseApiProperties(ref Printer p, List<PropertyDefinition> defs)
        {
            foreach (var def in defs)
            {
                WriteGetter(ref p, def);
                WriteSetter(ref p, def);
            }

            return;

            static void WriteGetter(ref Printer p, PropertyDefinition def)
            {
                if (def.getter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" Property_Get_").Print(def.name)
                    .PrintEndLine("<TEnumCase>(ref TEnumCase enum_case)");
                p.WithIncreasedIndent()
                    .PrintLine("where TEnumCase : struct, IEnumCase");
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .Print("return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case.").Print(def.name).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetter(ref Printer p, PropertyDefinition def)
            {
                if (def.setter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static void ")
                    .Print(def.returnType.name)
                    .Print(" Property_Set_").Print(def.name)
                    .Print("<TEnumCase>(ref TEnumCase enum_case, ")
                    .Print(def.returnType.name).Print(" setter_value")
                    .PrintEndLine(")");
                p.WithIncreasedIndent()
                    .PrintLine("where TEnumCase : struct, IEnumCase");
                p.OpenScope();
                {
                    p.PrintBeginLine("enum_case.").Print(def.name).PrintEndLine(" = setter_value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteEnumCaseApiIndexers(ref Printer p, List<IndexerDefinition> defs)
        {
            foreach (var def in defs)
            {
                WriteGetter(ref p, def);
                WriteSetter(ref p, def);
            }

            return;

            static void WriteGetter(ref Printer p, IndexerDefinition def)
            {
                if (def.getter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" Indexer_Get<TEnumCase>(");
                {
                    p.Print("ref TEnumCase enum_case, ");
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.WithIncreasedIndent()
                    .PrintLine("where TEnumCase : struct, IEnumCase");
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .Print("return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine("];");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetter(ref Printer p, IndexerDefinition def)
            {
                if (def.setter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static void ")
                    .Print(def.returnType.name)
                    .Print(" Indexer_Set<TEnumCase>(");
                {
                    p.Print("ref TEnumCase enum_case, ");
                    p.Print(def.returnType.name).Print(" setter_value, ");
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.WithIncreasedIndent()
                    .PrintLine("where TEnumCase : struct, IEnumCase");
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine("] = setter_value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteEnumCaseApiMethods(ref Printer p, List<MethodDefinition> defs)
        {
            foreach (var def in defs)
            {
                Write(ref p, def);
            }

            return;

            static void Write(ref Printer p, MethodDefinition def)
            {
                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name).Print("<TEnumCase>(");
                {
                    p.Print("ref TEnumCase enum_case, ");
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.WithIncreasedIndent()
                    .PrintLine("where TEnumCase : struct, IEnumCase");
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .PrintIf(def.returnsVoid == false, "return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case.").Print(def.name).Print("(");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteCaseStructs(
              ref Printer p
            , ReadOnlySpan<StructDefinition> defs
            , MergedStructRef mergedStructRef
        )
        {
            var defLength = defs.Length;

            for (var i = 0; i < defLength; i++)
            {
                var def = defs[i];

                if (def.implicitlyDeclared)
                {
                    continue;
                }

                var structId = new StructId {
                    name = def.name,
                    identifier = def.identifier,
                };

                p.PrintBeginLine("partial struct ").Print(def.name).Print(" : IEnumCase");

                if (autoEquatable)
                {
                    p.Print(", IEquatable<").Print(def.name).Print(">");
                }

                p.PrintEndLine();
                p.OpenScope();
                {
                    WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly EnumCase GetEnumCase()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return EnumCase.").Print(def.identifier).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly ").Print(typeName).Print(" To").Print(typeName).PrintEndLine("()");
                    p.OpenScope();
                    {
                        p.PrintLine("return this;");
                    }
                    p.CloseScope();

                    if (autoEquatable)
                    {
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("public readonly override bool Equals(object obj)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return obj is ").Print(def.name).PrintEndLine(" other && Equals(other);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public readonly bool Equals(").Print(def.name).PrintEndLine(" other)");
                        p.OpenScope();
                        {
                            p.PrintLine("return");
                            p = p.IncreasedIndent();

                            var fieldDefs = def.fields.AsReadOnlySpan();
                            var fieldDefLength = fieldDefs.Length;

                            for (var k = 0; k < fieldDefLength; k++)
                            {
                                var fieldDef = fieldDefs[k];
                                var fieldName = fieldDef.name;
                                var op = k > 0 ? "&& " : "   ";

                                if (fieldDef.returnType.isEnum)
                                {
                                    p.PrintBeginLine(op).Print(fieldName).Print(" == other.")
                                        .Print(fieldName).PrintEndLine("");
                                }
                                else
                                {
                                    p.PrintBeginLine(op).Print(fieldName).Print(".Equals(other.")
                                        .Print(fieldName).PrintEndLine(")");
                                }
                            }

                            p.PrintLine(";");
                            p = p.DecreasedIndent();
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("public readonly override int GetHashCode()");
                        p.OpenScope();
                        {
                            p.PrintLine("var hash = new HashValue();");

                            var fieldDefs = def.fields.AsReadOnlySpan();
                            var fieldDefLength = fieldDefs.Length;

                            for (var k = 0; k < fieldDefLength; k++)
                            {
                                var fieldDef = fieldDefs[k];
                                var fieldName = fieldDef.name;
                                p.PrintBeginLine("hash.Add(").Print(fieldName).PrintEndLine(");");
                            }

                            p.PrintLine("return hash.ToHashCode();");
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteEnumCaseExtensions(
              ref Printer p
            , bool referenceUnityCollections
            , EnumCaseType enumCaseType
        )
        {
            if (withEnumExtensions == false)
            {
                return;
            }

            var typeName = $"{this.typeName}_{ENUM_CASE_NAME}";
            var qualifiedName = $"{this.typeName}.{ENUM_CASE_NAME}";
            var extensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(typeName);
            var structName = EnumExtensionsDeclaration.GetNameExtendedStruct(typeName);

            p.PrintBeginLine("partial struct ").Print(structName).PrintEndLine(" // EnumCaseExtended");
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("static partial class ").Print(extensionsName).PrintEndLine(" // EnumCaseExtensions");
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region ENUM CASE - ENUM EXTENSIONS").PrintEndLine();
            p.Print("#endregion ========================").PrintEndLine();
            p.PrintEndLine();

            var enumExtensions = new EnumExtensionsDeclaration(referenceUnityCollections, enumCaseType.MaxByteCount) {
                GeneratedCode = GENERATED_CODE,
                ExcludeCoverage = EXCLUDE_COVERAGE,
                AggressiveInlining = AGGRESSIVE_INLINING,
                Name = typeName,
                ExtensionsName = extensionsName,
                StructName = structName,
                ParentIsNamespace = parentIsNamespace,
                FullyQualifiedName = qualifiedName,
                UnderlyingTypeName = enumCaseType.UnderlyingType,
                Accessibility = Accessibility.Public,
                IsDisplayAttributeUsed = false,
                Members = enumCaseType.Members,
            };

            enumExtensions.WriteCode(ref p);
        }

        private static void WriteHelperConstants(ref Printer p)
        {
            p.PrintBeginLine("private const ").Print(METHOD_IMPL_OPTIONS)
                .Print(" INLINING = ").Print(INLINING).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();
        }

        private static void WriteParameters(ref Printer p, ReadOnlySpan<ParameterDefinition> defs, bool outToRef = false)
        {
            var lastIndex = defs.Length - 1;

            for (var i = 0; i < defs.Length; i++)
            {
                var def = defs[i];

                p.Print(GetRefKind(def.refKind, outToRef))
                    .Print(def.type.name)
                    .Print(" ")
                    .Print(def.name);

                if (i < lastIndex)
                {
                    p.Print(", ");
                }
            }
        }

        private static void WriteArguments(ref Printer p, ReadOnlySpan<ParameterDefinition> defs)
        {
            var lastIndex = defs.Length - 1;

            for (var i = 0; i < defs.Length; i++)
            {
                var def = defs[i];

                p.Print(GetRefKind(def.refKind)).Print(def.name);

                if (i < lastIndex)
                {
                    p.Print(", ");
                }
            }
        }

        private static void WriteTryGetConstructionValueMethods(
              ref Printer p
            , MergedStructRef mergedStructRef
            , StructId structId
        )
        {
            if (mergedStructRef.StructToValuesMap.TryGetValue(structId, out var typeToValues) == false)
            {
                typeToValues = new();
            }

            foreach (var kv in mergedStructRef.TypeToStructsMap)
            {
                if (typeToValues.ContainsKey(kv.Key) == false)
                {
                    typeToValues.Add(kv.Key, s_emptyValues);
                }
            }

            Write(ref p, typeToValues);

            return;

            static void Write(
                  ref Printer p
                , Dictionary<SlimTypeDefinition, List<ConstructionValue>> typeToValues
            )
            {
                foreach (var kv in typeToValues)
                {
                    var type = kv.Key;
                    var values = kv.Value;
                    var valueCount = values.Count;

                    p.PrintBeginLine("public readonly bool TryGetConstructionValue(out ")
                        .Print(type.name).PrintEndLine(" value, int index = default)");
                    p.OpenScope();
                    {
                        if (valueCount > 1)
                        {
                            p.PrintLine("switch (index)");
                            p.OpenScope();
                            {
                                for (var orderIndex = 0; orderIndex < valueCount; orderIndex++)
                                {
                                    p.PrintBeginLine().Print(orderIndex).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        p.PrintBeginLine("value = ")
                                            .PrintIf(type.isEnum, type.name)
                                            .PrintIf(type.isEnum, ".")
                                            .Print(values[orderIndex]).PrintEndLine(";");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    p.PrintLine("value = default;");
                                    p.PrintLine("return false;");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        else if (valueCount == 1)
                        {
                            p.PrintBeginLine("value = ")
                                .PrintIf(type.isEnum, type.name)
                                .PrintIf(type.isEnum, ".")
                                .Print(values[0]).PrintEndLine(";");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("value = default;");
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private static bool IsReturnRefKind(RefKind refKind)
        {
            return refKind is RefKind.Ref or RefKind.Out or RefKind.RefReadOnly;
        }

        private static string GetAnyRef(RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.RefReadOnly => "ref ",
                _ => "",
            };
        }

        private static string GetAnyRefSpacePrefix(RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => " ref",
                RefKind.RefReadOnly => " ref",
                _ => "",
            };
        }

        private static string GetReturnRefKind(RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.RefReadOnly => "ref readonly ",
                _ => "",
            };
        }

        private static string GetRefKind(RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => "",
            };
        }

        private static string GetRefKind(RefKind refKind, bool outToRef)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => outToRef ? "ref " : "out ",
                RefKind.In => "in ",
                _ => "",
            };
        }
    }
}
