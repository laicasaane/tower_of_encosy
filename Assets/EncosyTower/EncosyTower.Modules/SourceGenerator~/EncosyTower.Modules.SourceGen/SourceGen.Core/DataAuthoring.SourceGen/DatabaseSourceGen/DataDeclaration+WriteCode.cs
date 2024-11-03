using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.Data.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    using static EncosyTower.Modules.DataAuthoring.SourceGen.Helpers;

    partial class DataDeclaration
    {
        /// <param name="horizontalListMap">
        /// TargetTypeFullName -> ContainingTypeFullName -> PropertyName (s)
        /// </param>
        public void WriteCode(
              ref Printer p
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , Dictionary<ITypeSymbol, Dictionary<ITypeSymbol, HashSet<string>>> horizontalListMap
            , ITypeSymbol containingType
            , ITypeSymbol objectType
            , ITypeSymbol idType = null
        )
        {
            var typeName = Symbol.Name;
            var typeFullName = FullName;
            var idTypeName = idType?.ToFullName() ?? string.Empty;

            p.PrintLine("[global::System.Serializable]");

            if (string.IsNullOrWhiteSpace(idTypeName) == false)
            {
                p.PrintLine($"[global::EncosyTower.Modules.Data.Authoring.SourceGen.GeneratedSheetRow(typeof({idTypeName}), typeof({typeFullName}))]");
            }
            else
            {
                p.PrintLine($"[global::EncosyTower.Modules.Data.Authoring.SourceGen.GeneratedDataRowAttribute(typeof({typeFullName}))]");
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine()
                .Print($"public partial class __{typeName}");

            if (idType != null)
            {
                p.Print(" : global::Cathei.BakingSheet.SheetRow");

                if (dataMap.ContainsKey(idType))
                {
                    p.Print($"<__{idType.Name}>");
                }
                else
                {
                    p.Print($"<{idTypeName}>");
                }
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public static readonly __{typeName} Default = new __{typeName}();");
                p.PrintEndLine();

                WriteConstructor(ref p, dataMap, horizontalListMap, typeName, Symbol, containingType, objectType);

                var baseTypeRefs = this.BaseTypeRefs;

                for (var i = baseTypeRefs.Length - 1; i >= 0; i--)
                {
                    var typeRef = baseTypeRefs[i];
                    WriteProperties(ref p, dataMap, horizontalListMap, Symbol, containingType, objectType, idType, typeRef.PropRefs);
                    WriteProperties(ref p, dataMap, horizontalListMap, Symbol, containingType, objectType, idType, typeRef.FieldRefs);
                }

                WriteProperties(ref p, dataMap, horizontalListMap, Symbol, containingType, objectType, idType, PropRefs);
                WriteProperties(ref p, dataMap, horizontalListMap, Symbol, containingType, objectType, idType, FieldRefs);

                WriteConvertMethod(ref p, dataMap);

                for (var i = baseTypeRefs.Length - 1; i >= 0; i--)
                {
                    var typeRef = baseTypeRefs[i];
                    WriteToCollectionMethod(ref p, dataMap, typeRef.PropRefs);
                    WriteToCollectionMethod(ref p, dataMap, typeRef.FieldRefs);
                }

                WriteToCollectionMethod(ref p, dataMap, PropRefs);
                WriteToCollectionMethod(ref p, dataMap, FieldRefs);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor(
              ref Printer p
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , Dictionary<ITypeSymbol, Dictionary<ITypeSymbol, HashSet<string>>> horizontalListMap
            , string typeName
            , ITypeSymbol targetType
            , ITypeSymbol containingType
            , ITypeSymbol objectType
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public __{typeName}()");
            p.OpenScope();
            {
                var baseTypeRefs = this.BaseTypeRefs;

                for (var i = baseTypeRefs.Length - 1; i >= 0; i--)
                {
                    var typeRef = baseTypeRefs[i];
                    Write(ref p, dataMap, horizontalListMap, typeRef.PropRefs, targetType, containingType, objectType);
                    Write(ref p, dataMap, horizontalListMap, typeRef.FieldRefs, targetType, containingType, objectType);
                }

                Write(ref p, dataMap, horizontalListMap, PropRefs, targetType, containingType, objectType);
                Write(ref p, dataMap, horizontalListMap, FieldRefs, targetType, containingType, objectType);
            }
            p.CloseScope();
            p.PrintEndLine();

            static void Write(
                  ref Printer p
                , Dictionary<ITypeSymbol, DataDeclaration> dataMap
                , Dictionary<ITypeSymbol, Dictionary<ITypeSymbol, HashSet<string>>> horizontalListMap
                , ImmutableArray<MemberRef> memberRefs
                , ITypeSymbol targetType
                , ITypeSymbol containingType
                , ITypeSymbol objectType
            )
            {
                foreach (var memberRef in memberRefs)
                {
                    var typeRef = memberRef.SelectTypeRef();
                    var collectionTypeRef = typeRef.CollectionTypeRef;

                    string newExpression;

                    switch (collectionTypeRef.Kind)
                    {
                        case CollectionKind.Array:
                        case CollectionKind.List:
                        case CollectionKind.HashSet:
                        case CollectionKind.Queue:
                        case CollectionKind.Stack:
                        {
                            var collectionTypeName = VERTICAL_LIST_TYPE;

                            if (collectionTypeRef.Kind == CollectionKind.Array
                                && horizontalListMap.TryGetValue(targetType, out var innerMap)
                            )
                            {
                                if (innerMap.TryGetValue(containingType, out var propertyNames)
                                    || innerMap.TryGetValue(objectType, out propertyNames)
                                )
                                {
                                    if (propertyNames.Contains(memberRef.PropertyName))
                                    {
                                        collectionTypeName = LIST_TYPE_T;
                                    }
                                }
                            }

                            var elemType = collectionTypeRef.ElementType;
                            var elemTypeName = dataMap.ContainsKey(elemType)
                                ? $"__{collectionTypeRef.ElementType.Name}" : elemType.ToFullName();

                            newExpression = $"new {collectionTypeName}{elemTypeName}>()";
                            break;
                        }

                        case CollectionKind.Dictionary:
                        {
                            var keyType = collectionTypeRef.KeyType;
                            var elemType = collectionTypeRef.ElementType;

                            var keyTypeName = dataMap.ContainsKey(keyType)
                                ? $"__{collectionTypeRef.KeyType.Name}" : keyType.ToFullName();

                            var elemTypeName = dataMap.ContainsKey(elemType)
                                ? $"__{collectionTypeRef.ElementType.Name}" : elemType.ToFullName();

                            newExpression = $"new {DICTIONARY_TYPE_T}{keyTypeName}, {elemTypeName}>()";
                            break;
                        }

                        default:
                        {
                            var fieldType = typeRef.Type;
                            var fieldTypeFullName = fieldType.ToFullName();

                            if (dataMap.ContainsKey(fieldType))
                            {
                                newExpression = $"new __{typeRef.Type.Name}()";
                            }
                            else if (typeRef.Type.IsValueType)
                            {
                                newExpression = "default";
                            }
                            else if (fieldTypeFullName == "string")
                            {
                                newExpression = "string.Empty";
                            }
                            else if (memberRef.TypeHasParameterlessConstructor)
                            {
                                newExpression = $"new {fieldTypeFullName}()";
                            }
                            else
                            {
                                newExpression = "default";
                            }

                            break;
                        }
                    }

                    p.PrintLine($"this.{memberRef.PropertyName} = {newExpression};");
                }
            }
        }

        private static void WriteProperties(
              ref Printer p
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , Dictionary<ITypeSymbol, Dictionary<ITypeSymbol, HashSet<string>>> horizontalListMap
            , ITypeSymbol targetType
            , ITypeSymbol containingType
            , ITypeSymbol objectType
            , ITypeSymbol idType
            , ImmutableArray<MemberRef> memberRefs
        )
        {
            foreach (var memberRef in memberRefs)
            {
                if (idType != null && memberRef.PropertyName == "Id")
                {
                    continue;
                }

                var typeRef = memberRef.SelectTypeRef();
                var collectionTypeRef = typeRef.CollectionTypeRef;

                string propTypeName;

                switch (collectionTypeRef.Kind)
                {
                    case CollectionKind.Array:
                    case CollectionKind.List:
                    case CollectionKind.HashSet:
                    case CollectionKind.Queue:
                    case CollectionKind.Stack:
                    {
                        var collectionTypeName = VERTICAL_LIST_TYPE;

                        if (collectionTypeRef.Kind == CollectionKind.Array
                            && horizontalListMap.TryGetValue(targetType, out var innerMap)
                        )
                        {
                            if (innerMap.TryGetValue(containingType, out var propertyNames)
                                || innerMap.TryGetValue(objectType, out propertyNames)
                            )
                            {
                                if (propertyNames.Contains(memberRef.PropertyName))
                                {
                                    collectionTypeName = LIST_TYPE_T;
                                }
                            }
                        }

                        var elemType = collectionTypeRef.ElementType;
                        var elemTypeName = dataMap.ContainsKey(elemType)
                            ? $"__{collectionTypeRef.ElementType.Name}" : elemType.ToFullName();

                        propTypeName = $"{collectionTypeName}{elemTypeName}>";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyType = collectionTypeRef.KeyType;
                        var elemType = collectionTypeRef.ElementType;

                        var keyTypeName = dataMap.ContainsKey(keyType)
                            ? $"__{collectionTypeRef.KeyType.Name}" : keyType.ToFullName();

                        var elemTypeName = dataMap.ContainsKey(elemType)
                            ? $"__{collectionTypeRef.ElementType.Name}" : elemType.ToFullName();

                        propTypeName = $"{DICTIONARY_TYPE_T}{keyTypeName}, {elemTypeName}>";
                        break;
                    }

                    default:
                    {
                        var fieldType = typeRef.Type;
                        propTypeName = dataMap.ContainsKey(fieldType)
                            ? $"__{typeRef.Type.Name}" : fieldType.ToFullName();

                        break;
                    }
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public {propTypeName} {memberRef.PropertyName} {{ get; private set; }}");
                p.PrintEndLine();
            }
        }

        private void WriteConvertMethod(
              ref Printer p
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeName = Symbol.Name;
            var typeFullName = Symbol.ToFullName();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {typeFullName} To{typeName}()");
            p.OpenScope();
            {
                p.PrintLine($"var result = new {typeFullName}();");
                p.PrintEndLine();

                var baseTypeRefs = this.BaseTypeRefs;

                for (var i = baseTypeRefs.Length - 1; i >= 0; i--)
                {
                    var typeRef = baseTypeRefs[i];
                    WriteType(ref p, typeRef, dataMap);
                    p.PrintEndLine();
                }

                WriteType(ref p, this, dataMap);
                p.PrintEndLine();

                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();

            static void WriteType(
                  ref Printer p
                , DataDeclaration typeRef
                , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            )
            {
                p.PrintLine($"result.SetValues_{typeRef.Symbol.ToValidIdentifier()}(");
                p = p.IncreasedIndent();
                {
                    var first = true;

                    foreach (var memberRef in typeRef.PropRefs)
                    {
                        var comma = first ? " " : ",";

                        first = false;

                        Write(ref p, dataMap, memberRef, comma);
                    }

                    foreach (var memberRef in typeRef.FieldRefs)
                    {
                        var comma = first ? " " : ",";

                        first = false;

                        Write(ref p, dataMap, memberRef, comma);
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(");");
            }

            static void Write(
                  ref Printer p
                , Dictionary<ITypeSymbol, DataDeclaration> dataMap
                , MemberRef memberRef
                , string comma
            )
            {
                var typeRef = memberRef.SelectTypeRef();
                var converterRef = memberRef.ConverterRef;
                var collectionTypeRef = typeRef.CollectionTypeRef;
                string expression;

                switch (collectionTypeRef.Kind)
                {
                    case CollectionKind.Array:
                    {
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(elemType))
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "Array");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        else
                        {
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName}?.ToArray() ?? new {elemTypeFullName}[0]");
                        }
                        break;
                    }

                    case CollectionKind.List:
                    {
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(elemType))
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "List");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        else
                        {
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName} ?? new {LIST_TYPE_T}{elemTypeFullName}>()");
                        }
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyType = collectionTypeRef.KeyType;
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(keyType) == false
                            && dataMap.ContainsKey(elemType) == false
                        )
                        {
                            var keyTypeFullName = keyType.ToFullName();
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName} ?? new {DICTIONARY_TYPE_T}{keyTypeFullName}, {elemTypeFullName}>()");
                        }
                        else
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "Dictionary");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        break;
                    }

                    case CollectionKind.HashSet:
                    {
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(elemType))
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "HashSet");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        else
                        {
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName} == null ? new {HASH_SET_TYPE_T}{elemTypeFullName}>() : new {HASH_SET_TYPE_T}{elemTypeFullName}>(this.{memberRef.PropertyName})");
                        }
                        break;
                    }

                    case CollectionKind.Queue:
                    {
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(elemType))
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "Queue");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        else
                        {
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName} == null ? new {QUEUE_TYPE_T}{elemTypeFullName}>() : new {QUEUE_TYPE_T}{elemTypeFullName}>(this.{memberRef.PropertyName})");
                        }
                        break;
                    }

                    case CollectionKind.Stack:
                    {
                        var elemType = collectionTypeRef.ElementType;

                        if (dataMap.ContainsKey(elemType))
                        {
                            var methodName = GetToCollectionMethodName(memberRef, "Stack");
                            expression = converterRef.Convert($"this.{methodName}()");
                        }
                        else
                        {
                            var elemTypeFullName = elemType.ToFullName();
                            expression = converterRef.Convert($"this.{memberRef.PropertyName} == null ? new {STACK_TYPE_T}{elemTypeFullName}>() : new {STACK_TYPE_T}{elemTypeFullName}>(this.{memberRef.PropertyName})");
                        }
                        break;
                    }

                    default:
                    {
                        var fieldType = typeRef.Type;

                        if (dataMap.ContainsKey(fieldType))
                        {
                            expression = converterRef.Convert($"(this.{memberRef.PropertyName} ?? __{typeRef.Type.Name}.Default).To{typeRef.Type.Name}()");
                        }
                        else
                        {
                            var fieldTypeFullName = fieldType.ToFullName();
                            var newExpression = typeRef.Type.IsValueType
                                ? "" : memberRef.TypeHasParameterlessConstructor
                                ? $" ?? new {fieldTypeFullName}()" : " ?? default";

                            expression = converterRef.Convert($"this.{memberRef.PropertyName}{newExpression}");
                        }
                        break;
                    }
                }

                p.PrintLine($"{comma} {expression}");
            }
        }

        private static void WriteToCollectionMethod(
              ref Printer p
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , ImmutableArray<MemberRef> memberRefs
        )
        {
            foreach (var memberRef in memberRefs)
            {
                var typeRef = memberRef.SelectTypeRef();
                var collectionTypeRef = typeRef.CollectionTypeRef;

                switch (collectionTypeRef.Kind)
                {
                    case CollectionKind.Array:
                    {
                        WriteToArrayMethod(ref p, memberRef, dataMap);
                        break;
                    }

                    case CollectionKind.List:
                    {
                        WriteToListMethod(ref p, memberRef, dataMap);
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        WriteToDictionaryMethod(ref p, memberRef, dataMap);
                        break;
                    }

                    case CollectionKind.HashSet:
                    {
                        WriteToHashSetMethod(ref p, memberRef, dataMap);
                        break;
                    }

                    case CollectionKind.Queue:
                    {
                        WriteToQueueMethod(ref p, memberRef, dataMap);
                        break;
                    }

                    case CollectionKind.Stack:
                    {
                        WriteToStackMethod(ref p, memberRef, dataMap);
                        break;
                    }
                }
            }
        }

        private static void WriteToArrayMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemType = collectionTypeRef.ElementType;

            if (dataMap.ContainsKey(elemType) == false)
            {
                return;
            }

            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "Array");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {elemTypeFullName}[] {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {elemTypeFullName}[0];");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {elemTypeFullName}[count];");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintLine($"result[i] = (rows[i] ?? __{elemTypeName}.Default).To{elemTypeName}();");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToListMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemType = collectionTypeRef.ElementType;

            if (dataMap.ContainsKey(elemType) == false)
            {
                return;
            }

            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "List");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {LIST_TYPE_T}{elemTypeFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {LIST_TYPE_T}{elemTypeFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {LIST_TYPE_T}{elemTypeFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintLine($"var item = (rows[i] ?? __{elemTypeName}.Default).To{elemTypeName}();");
                    p.PrintLine("result.Add(item);");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToDictionaryMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var keyType = collectionTypeRef.KeyType;
            var elemType = collectionTypeRef.ElementType;
            var keyIsData = dataMap.ContainsKey(keyType);
            var elemIsData = dataMap.ContainsKey(elemType);

            if (keyIsData == false && elemIsData == false)
            {
                return;
            }

            var keyTypeName = collectionTypeRef.KeyType.Name;
            var keyTypeFullName = keyType.ToFullName();
            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "Dictionary");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {DICTIONARY_TYPE_T}{keyTypeFullName}, {elemTypeFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {DICTIONARY_TYPE_T}{keyTypeFullName}, {elemTypeFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {DICTIONARY_TYPE_T}{keyTypeFullName}, {elemTypeFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("foreach (var kv in rows)");
                p.OpenScope();
                {
                    if (keyIsData)
                    {
                        p.PrintLine($"var key = (kv.Key ?? __{keyTypeName}.Default).To{keyTypeName}();");
                    }
                    else
                    {
                        p.PrintLine("var key = kv.Key;");
                    }

                    if (elemIsData)
                    {
                        p.PrintLine($"var value = (kv.Value ?? __{elemTypeName}.Default).To{elemTypeName}();");
                    }
                    else
                    {
                        p.PrintLine("var value = kv.Value;");
                    }

                    p.PrintEndLine();
                    p.PrintLine("result[key] = value;");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToHashSetMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemType = collectionTypeRef.ElementType;

            if (dataMap.ContainsKey(elemType) == false)
            {
                return;
            }

            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "HashSet");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {HASH_SET_TYPE_T}{elemTypeFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {HASH_SET_TYPE_T}{elemTypeFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {HASH_SET_TYPE_T}{elemTypeFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintLine($"var item = (rows[i] ?? __{elemTypeName}.Default).To{elemTypeName}();");
                    p.PrintLine("result.Add(item);");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToQueueMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemType = collectionTypeRef.ElementType;

            if (dataMap.ContainsKey(elemType) == false)
            {
                return;
            }

            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "Queue");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {QUEUE_TYPE_T}{elemTypeFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {QUEUE_TYPE_T}{elemTypeFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {QUEUE_TYPE_T}{elemTypeFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintLine($"var item = (rows[i] ?? __{elemTypeName}.Default).To{elemTypeName}();");
                    p.PrintLine("result.Enqueue(item);");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToStackMethod(
              ref Printer p
            , MemberRef memberRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemType = collectionTypeRef.ElementType;

            if (dataMap.ContainsKey(elemType) == false)
            {
                return;
            }

            var elemTypeName = collectionTypeRef.ElementType.Name;
            var elemTypeFullName = elemType.ToFullName();
            var methodName = GetToCollectionMethodName(memberRef, "Stack");

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private {STACK_TYPE_T}{elemTypeFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{memberRef.PropertyName} == null || this.{memberRef.PropertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {STACK_TYPE_T}{elemTypeFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{memberRef.PropertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {STACK_TYPE_T}{elemTypeFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintLine($"var item = (rows[i] ?? __{elemTypeName}.Default).To{elemTypeName}();");
                    p.PrintLine("result.Push(item);");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string GetToCollectionMethodName(MemberRef memberRef, string collectionName)
        {
            var typeRef = memberRef.SelectTypeRef();
            var collectionTypeRef = typeRef.CollectionTypeRef;
            var elemTypeName = collectionTypeRef.ElementType.Name;
            return $"To{elemTypeName}{collectionName}For{memberRef.PropertyName}";
        }
    }
}
