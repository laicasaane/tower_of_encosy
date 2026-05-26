using System.Collections.Generic;
using EncosyTower.SourceGen.Helpers.Data;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DataSpec
    {
        public readonly void WriteCode(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string containingTypeFullName
            , string idTypeFullName = null
        )
        {
            var typeName = simpleName;
            var typeFullName = fullName;
            var hasIdType = string.IsNullOrEmpty(idTypeFullName) == false;

            p.PrintLine(PR_SERIALIZABLE);

            if (hasIdType)
            {
                p.PrintLine(string.Format(PR_GENERATED_SHEET_ROW, idTypeFullName, typeFullName));
            }
            else
            {
                p.PrintLine(string.Format(PR_GENERATED_DATA_ROW, typeFullName));
            }

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public partial class __").Print(typeName);

            if (hasIdType)
            {
                p.Print(" : CBS.SheetRow");

                if (dataMap.ContainsKey(idTypeFullName))
                {
                    p.Print("<__").Print(GetSimpleName(idTypeFullName)).Print(">");
                }
                else
                {
                    p.Print("<").Print(idTypeFullName).Print(">");
                }
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine("public static readonly __").Print(typeName)
                    .Print(" Default = new __").Print(typeName).PrintEndLine("();");
                p.PrintEndLine();

                WriteConstructor(ref p, dataMap, horizontalListMap, typeName, containingTypeFullName);

                foreach (var layer in baseTypeLayers)
                {
                    WriteProperties(ref p, dataMap, horizontalListMap, containingTypeFullName, idTypeFullName, layer.propRefs);
                    WriteProperties(ref p, dataMap, horizontalListMap, containingTypeFullName, idTypeFullName, layer.fieldRefs);
                }

                WriteProperties(ref p, dataMap, horizontalListMap, containingTypeFullName, idTypeFullName, propRefs);
                WriteProperties(ref p, dataMap, horizontalListMap, containingTypeFullName, idTypeFullName, fieldRefs);

                WriteConvertMethod(ref p, dataMap);

                foreach (var layer in baseTypeLayers)
                {
                    WriteToCollectionMethod(ref p, dataMap, layer.propRefs);
                    WriteToCollectionMethod(ref p, dataMap, layer.fieldRefs);
                }

                WriteToCollectionMethod(ref p, dataMap, propRefs);
                WriteToCollectionMethod(ref p, dataMap, fieldRefs);

                WriteSheetValueConverters(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteConstructor(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string typeName
            , string containingTypeFullName
        )
        {
            p.PrintBeginLine("public __").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                foreach (var layer in baseTypeLayers)
                {
                    WriteCtorMembers(ref p, dataMap, horizontalListMap, layer.propRefs, fullName, containingTypeFullName);
                    WriteCtorMembers(ref p, dataMap, horizontalListMap, layer.fieldRefs, fullName, containingTypeFullName);
                }

                WriteCtorMembers(ref p, dataMap, horizontalListMap, propRefs, fullName, containingTypeFullName);
                WriteCtorMembers(ref p, dataMap, horizontalListMap, fieldRefs, fullName, containingTypeFullName);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteCtorMembers(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , EquatableArray<MemberSpec> memberModels
            , string targetTypeFullName
            , string containingTypeFullName
        )
        {
            foreach (var member in memberModels)
            {
                var coll = member.SelectCollection();
                string newExpression;

                switch (coll.kind)
                {
                    case CollectionKind.Array:
                    case CollectionKind.List:
                    case CollectionKind.HashSet:
                    case CollectionKind.Queue:
                    case CollectionKind.Stack:
                    {
                        var collectionTypeName = PR_VERTICAL_LIST_T;

                        if (coll.kind == CollectionKind.Array
                            && horizontalListMap.TryGetValue(targetTypeFullName, out var innerMap)
                        )
                        {
                            if (innerMap.TryGetValue(containingTypeFullName, out var propertyNames)
                                && propertyNames.Contains(member.propertyName)
                            )
                            {
                                collectionTypeName = PR_LIST_T;
                            }
                        }

                        string elemTypeName;

                        if (dataMap.ContainsKey(coll.elementType.fullName))
                        {
                            elemTypeName = $"__{coll.elementType.simpleName}";
                        }
                        else
                        {
                            elemTypeName = coll.elementType.fullName;
                        }

                        newExpression = $"new {collectionTypeName}<{elemTypeName}>()";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        string keyTypeName;
                        string elemTypeName;

                        if (dataMap.ContainsKey(coll.keyType.fullName))
                        {
                            keyTypeName = $"__{coll.keyType.simpleName}";
                        }
                        else
                        {
                            keyTypeName = coll.keyType.fullName;
                        }

                        if (dataMap.ContainsKey(coll.elementType.fullName))
                        {
                            elemTypeName = $"__{coll.elementType.simpleName}";
                        }
                        else
                        {
                            elemTypeName = coll.elementType.fullName;
                        }

                        newExpression = $"new {PR_DICTIONARY_T}<{keyTypeName}, {elemTypeName}>()";
                        break;
                    }

                    default:
                    {
                        var type = member.SelectType();

                        if (dataMap.ContainsKey(type.fullName))
                        {
                            newExpression = $"new __{type.simpleName}()";
                        }
                        else if (type.isValueType)
                        {
                            newExpression = "default";
                        }
                        else if (type.fullName == "string")
                        {
                            newExpression = "string.Empty";
                        }
                        else if (type.hasParameterlessConstructor)
                        {
                            newExpression = $"new {type.fullName}()";
                        }
                        else
                        {
                            newExpression = "default";
                        }

                        break;
                    }
                }

                p.PrintBeginLine("this.").Print(member.propertyName)
                    .Print(" = ").Print(newExpression).PrintEndLine(";");
            }
        }

        private static void WriteProperties(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string containingTypeFullName
            , string idTypeFullName
            , EquatableArray<MemberSpec> memberModels
        )
        {
            var hasIdType = string.IsNullOrEmpty(idTypeFullName) == false;

            foreach (var member in memberModels)
            {
                if (hasIdType && member.propertyName == "Id")
                {
                    continue;
                }

                var coll = member.SelectCollection();
                string propTypeName;

                switch (coll.kind)
                {
                    case CollectionKind.Array:
                    case CollectionKind.List:
                    case CollectionKind.HashSet:
                    case CollectionKind.Queue:
                    case CollectionKind.Stack:
                    {
                        var collectionTypeName = PR_VERTICAL_LIST_T;

                        if (coll.kind == CollectionKind.Array
                            && horizontalListMap.TryGetValue(containingTypeFullName, out var innerMap)
                        )
                        {
                            if (innerMap.TryGetValue(containingTypeFullName, out var propertyNames)
                                && propertyNames.Contains(member.propertyName)
                            )
                            {
                                collectionTypeName = PR_LIST_T;
                            }
                        }

                        string elemTypeName;

                        if (dataMap.ContainsKey(coll.elementType.fullName))
                        {
                            elemTypeName = $"__{coll.elementType.simpleName}";
                        }
                        else
                        {
                            elemTypeName = coll.elementType.fullName;
                        }

                        propTypeName = $"{collectionTypeName}<{elemTypeName}>";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        string keyTypeName;
                        string elemTypeName;

                        if (dataMap.ContainsKey(coll.keyType.fullName))
                        {
                            keyTypeName = $"__{coll.keyType.simpleName}";
                        }
                        else
                        {
                            keyTypeName = coll.keyType.fullName;
                        }

                        if (dataMap.ContainsKey(coll.elementType.fullName))
                        {
                            elemTypeName = $"__{coll.elementType.simpleName}";
                        }
                        else
                        {
                            elemTypeName = coll.elementType.fullName;
                        }

                        propTypeName = $"{PR_DICTIONARY_T}<{keyTypeName}, {elemTypeName}>";
                        break;
                    }

                    default:
                    {
                        var type = member.SelectType();

                        if (dataMap.ContainsKey(type.fullName))
                        {
                            propTypeName = $"__{type.simpleName}";
                        }
                        else
                        {
                            propTypeName = type.fullName;
                        }
                        break;
                    }
                }

                if (member.sheetConverter.kind != ConverterKind.None)
                {
                    var converter = member.sheetConverter;
                    var hash = HashValue64.FNV1a(converter.converterTypeFullName);

                    p.PrintBeginLine("[CBS.SheetValueConverter(typeof(__SheetValueConverter_")
                        .Print(member.type.simpleName.ToValidIdentifier()).Print('_').Print(hash)
                        .PrintEndLine("))]");
                }

                p.PrintBeginLine("public ").Print(propTypeName).Print(" ").Print(member.propertyName)
                    .PrintEndLine(" { get; private set; }");
                p.PrintEndLine();
            }
        }

        private readonly void WriteSheetValueConverters(ref Printer p)
        {
            var hashes = new HashSet<ulong>();

            foreach (var layer in baseTypeLayers)
            {
                WriteSheetValueConverter(ref p, layer.propRefs, hashes);
                WriteSheetValueConverter(ref p, layer.fieldRefs, hashes);
            }

            WriteSheetValueConverter(ref p, propRefs, hashes);
            WriteSheetValueConverter(ref p, fieldRefs, hashes);

            static void WriteSheetValueConverter(
                  ref Printer p
                , EquatableArray<MemberSpec> members
                , HashSet<ulong> hashes
            )
            {
                foreach (var member in members)
                {
                    var converter = member.sheetConverter;

                    if (converter.kind == ConverterKind.None)
                    {
                        continue;
                    }

                    var hash = HashValue64.FNV1a(converter.converterTypeFullName);

                    if (hashes.Add(hash) == false)
                    {
                        continue;
                    }

                    var targetType = member.SelectType().fullName;

                    p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                    p.PrintBeginLine("public sealed class ").Print("__SheetValueConverter_")
                        .Print(member.type.simpleName.ToValidIdentifier()).Print('_').Print(hash)
                        .Print(" : CBS.SheetValueConverter<").Print(targetType).PrintEndLine(">");
                    p.OpenScope();
                    {
                        p.PrintLine(PR_AGGRESSIVE_INLINING);
                        p.PrintBeginLine("protected override ").Print(targetType)
                            .PrintEndLine(" StringToValue(S.Type type, string value, CBS.SheetValueConvertingContext context)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("=> ").Print(converter.Convert("value")).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine(PR_AGGRESSIVE_INLINING);
                        p.PrintBeginLine("protected override string ValueToString(S.Type type, ")
                            .Print(targetType).PrintEndLine(" value, CBS.SheetValueConvertingContext context)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("=> S.Convert.ToString(value, context.FormatProvider) ?? string.Empty;").PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private readonly void WriteConvertMethod(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
        )
        {
            var typeName = simpleName;
            var typeFullName = fullName;

            p.PrintBeginLine("public ").Print(typeFullName).Print(" To").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("var result = new ").Print(typeFullName).PrintEndLine("();");
                p.PrintEndLine();

                foreach (var layer in baseTypeLayers)
                {
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var resultBase = result as ").Print(layer.fullName).PrintEndLine(";");

                        WriteConvertType(
                              ref p
                            , layer.fullName
                            , "resultBase"
                            , dataMap
                            , layer.validIdentifier
                            , layer.propRefs
                            , layer.fieldRefs
                        );
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteConvertType(
                      ref p
                    , typeFullName
                    , "result"
                    , dataMap
                    , validIdentifier
                    , propRefs
                    , fieldRefs
                );

                p.PrintEndLine();

                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteConvertType(
              ref Printer p
            , string typeFullName
            , string resultName
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , EquatableArray<MemberSpec> props
            , EquatableArray<MemberSpec> fields
        )
        {
            foreach (var member in fields)
            {
                WriteConvertMember(ref p, typeFullName, resultName, dataMap, typeValidIdentifier, member);
            }

            foreach (var member in props)
            {
                WriteConvertMember(ref p, typeFullName, resultName, dataMap, typeValidIdentifier, member);
            }
        }

        private static void WriteConvertMember(
              ref Printer p
            , string typeFullName
            , string resultName
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , MemberSpec member
        )
        {
            var type = member.SelectType();
            var coll = member.SelectCollection();
            var memberPropName = member.propertyName;

            string expression;

            switch (coll.kind)
            {
                case CollectionKind.Array:
                {
                    if (dataMap.ContainsKey(coll.elementType.fullName))
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "Array");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementType.fullName;
                        expression = member.converter.Convert($"this.{memberPropName}?.ToArray() ?? new {elemFull}[0]");
                    }
                    break;
                }

                case CollectionKind.List:
                {
                    if (dataMap.ContainsKey(coll.elementType.fullName))
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "List");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementType.fullName;
                        expression = member.converter.Convert(
                            $"this.{memberPropName} ?? new {PR_LIST_T}<{elemFull}>()"
                        );
                    }
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    var keyIsData = dataMap.ContainsKey(coll.keyType.fullName);
                    var elemIsData = dataMap.ContainsKey(coll.elementType.fullName);

                    if (keyIsData == false && elemIsData == false)
                    {
                        expression = member.converter.Convert(
                            $"this.{memberPropName} ?? new {PR_DICTIONARY_T}<{coll.keyType.fullName}, {coll.elementType.fullName}>()"
                        );
                    }
                    else
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "Dictionary");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    break;
                }

                case CollectionKind.HashSet:
                {
                    if (dataMap.ContainsKey(coll.elementType.fullName))
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "HashSet");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementType.fullName;
                        expression = member.converter.Convert(
                            $"this.{memberPropName} == null ? new {PR_HASH_SET_T}<{elemFull}>() : new {PR_HASH_SET_T}<{elemFull}>(this.{memberPropName})"
                        );
                    }
                    break;
                }

                case CollectionKind.Queue:
                {
                    if (dataMap.ContainsKey(coll.elementType.fullName))
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "Queue");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementType.fullName;
                        expression = member.converter.Convert(
                            $"this.{memberPropName} == null ? new {PR_QUEUE_T}<{elemFull}>() : new {PR_QUEUE_T}<{elemFull}>(this.{memberPropName})"
                        );
                    }
                    break;
                }

                case CollectionKind.Stack:
                {
                    if (dataMap.ContainsKey(coll.elementType.fullName))
                    {
                        var methodName = GetToCollectionMethodName(memberPropName, coll.elementType.simpleName, "Stack");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementType.fullName;
                        expression = member.converter.Convert(
                            $"this.{memberPropName} == null ? new {PR_STACK_T}<{elemFull}>() : new {PR_STACK_T}<{elemFull}>(this.{memberPropName})"
                        );
                    }
                    break;
                }

                default:
                {
                    if (dataMap.ContainsKey(type.fullName))
                    {
                        expression = member.converter.Convert(
                            $"(this.{memberPropName} ?? __{type.simpleName}.Default).To{type.simpleName}()"
                        );
                    }
                    else if (type.isValueType)
                    {
                        expression = member.converter.Convert($"this.{memberPropName}");
                    }
                    else if (type.fullName == "string")
                    {
                        expression = member.converter.Convert($"this.{memberPropName} ?? string.Empty");
                    }
                    else
                    {
                        var newSuffix = type.hasParameterlessConstructor
                            ? $" ?? new {type.fullName}()"
                            : " ?? default";

                        expression = member.converter.Convert($"this.{memberPropName}{newSuffix}");
                    }
                    break;
                }
            }

            p.PrintBeginLine(typeFullName).Print(".").Print(typeValidIdentifier).Print("_ValueSetter.Set_")
                .Print(member.propertyName).Print("(ref ").Print(resultName)
                .Print(", ").Print(expression).PrintEndLine(");");
        }

        private static void WriteToCollectionMethod(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<MemberSpec> memberModels
        )
        {
            foreach (var member in memberModels)
            {
                var coll = member.SelectCollection();

                switch (coll.kind)
                {
                    case CollectionKind.Array: WriteToArrayMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.List: WriteToListMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.Dictionary: WriteToDictionaryMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.HashSet: WriteToHashSetMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.Queue: WriteToQueueMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.Stack: WriteToStackMethod(ref p, member, coll, dataMap); break;
                }
            }
        }

        private static void WriteToArrayMethod(
              ref Printer p
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementType.fullName) == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var elemSimpleName = coll.elementType.simpleName;
            var elemFullName = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimpleName, "Array");

            p.PrintBeginLine("private ").Print(elemFullName).Print("[] ").Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName).Print(" == null || this.")
                    .Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(elemFullName).PrintEndLine("[0];");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ").Print(elemFullName).PrintEndLine("[count];");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintBeginLine("result[i] = (rows[i] ?? __").Print(elemSimpleName).Print(".Default).To")
                        .Print(elemSimpleName).PrintEndLine("();");
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
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementType.fullName) == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var elemSimpleName = coll.elementType.simpleName;
            var elemFullName = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimpleName, "List");

            p.PrintBeginLine("private ").Print(PR_LIST_T).Print("<").Print(elemFullName).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName)
                    .Print(" == null || this.").Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(PR_LIST_T).Print("<").Print(elemFullName).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ")
                    .Print(PR_LIST_T).Print("<").Print(elemFullName).PrintEndLine(">(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var item = (rows[i] ?? __")
                        .Print(elemSimpleName).Print(".Default).To").Print(elemSimpleName).PrintEndLine("();");
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
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            var keyIsData = dataMap.ContainsKey(coll.keyType.fullName);
            var elemIsData = dataMap.ContainsKey(coll.elementType.fullName);

            if (keyIsData == false && elemIsData == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var keySimple = coll.keyType.simpleName;
            var keyFull = coll.keyType.fullName;
            var elemSimple = coll.elementType.simpleName;
            var elemFull = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimple, "Dictionary");

            p.PrintBeginLine("private ").Print(PR_DICTIONARY_T).Print("<").Print(keyFull)
                .Print(", ").Print(elemFull).Print("> ").Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName).Print(" == null || this.")
                    .Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(PR_DICTIONARY_T).Print("<").Print(keyFull)
                    .Print(", ").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ").Print(PR_DICTIONARY_T).Print("<").Print(keyFull)
                    .Print(", ").Print(elemFull).PrintEndLine(">(count);");
                p.PrintEndLine();

                p.PrintLine("foreach (var kv in rows)");
                p.OpenScope();
                {
                    if (keyIsData)
                    {
                        p.PrintBeginLine("var key = (kv.Key ?? __")
                            .Print(keySimple).Print(".Default).To")
                            .Print(keySimple).PrintEndLine("();");
                    }
                    else
                    {
                        p.PrintLine("var key = kv.Key;");
                    }

                    if (elemIsData)
                    {
                        p.PrintBeginLine("var value = (kv.Value ?? __").Print(elemSimple).Print(".Default).To")
                            .Print(elemSimple).PrintEndLine("();");
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
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementType.fullName) == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var elemSimple = coll.elementType.simpleName;
            var elemFull = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimple, "HashSet");

            p.PrintBeginLine("private ").Print(PR_HASH_SET_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName)
                    .Print(" == null || this.").Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_HASH_SET_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ")
                    .Print(PR_HASH_SET_T).Print("<").Print(elemFull).PrintEndLine(">(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var item = (rows[i] ?? __").Print(elemSimple).Print(".Default).To")
                        .Print(elemSimple).PrintEndLine("();");
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
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementType.fullName) == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var elemSimple = coll.elementType.simpleName;
            var elemFull = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimple, "Queue");

            p.PrintBeginLine("private ").Print(PR_QUEUE_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName)
                    .Print(" == null || this.").Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_QUEUE_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ")
                    .Print(PR_QUEUE_T).Print("<").Print(elemFull).PrintEndLine(">(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var item = (rows[i] ?? __").Print(elemSimple).Print(".Default).To")
                        .Print(elemSimple).PrintEndLine("();");
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
            , MemberSpec member
            , CollectionSpec coll
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementType.fullName) == false)
            {
                return;
            }

            var memberPropName = member.propertyName;
            var elemSimple = coll.elementType.simpleName;
            var elemFull = coll.elementType.fullName;
            var methodName = GetToCollectionMethodName(memberPropName, elemSimple, "Stack");

            p.PrintBeginLine("private ").Print(PR_STACK_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(memberPropName)
                    .Print(" == null || this.").Print(memberPropName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_STACK_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(memberPropName).PrintEndLine(";");
                p.PrintLine("var count = rows.Count;");
                p.PrintBeginLine("var result = new ")
                    .Print(PR_STACK_T).Print("<").Print(elemFull).PrintEndLine(">(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var item = (rows[i] ?? __").Print(elemSimple).Print(".Default).To")
                        .Print(elemSimple).PrintEndLine("();");
                    p.PrintLine("result.Push(item);");
                }
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string GetToCollectionMethodName(string propertyName, string elemSimpleName, string collectionName)
            => $"To{elemSimpleName}{collectionName}For{propertyName}";

        private static string GetSimpleName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            var dot = fullName.LastIndexOf('.');
            return dot >= 0 ? fullName.Substring(dot + 1) : fullName;
        }
    }
}
