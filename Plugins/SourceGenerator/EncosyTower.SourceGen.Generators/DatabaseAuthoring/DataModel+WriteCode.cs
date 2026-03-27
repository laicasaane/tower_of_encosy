using System.Collections.Generic;
using EncosyTower.SourceGen.Common.Data.Common;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DataModel
    {
        /// <summary>
        /// Writes the <c>__TypeName</c> partial class body into <paramref name="p"/>.
        /// </summary>
        /// <param name="dataMap">Full type name → DataModel for all IData types in scope.</param>
        /// <param name="horizontalListMap">
        /// targetTypeFullName → containingTypeFullName → property names.
        /// </param>
        /// <param name="containingTypeFullName">Full name of the table asset type (ITableAsset).</param>
        /// <param name="idTypeFullName">Full name of the Id type, or <c>null</c> when this is not a sheet-row class.</param>
        public readonly void WriteCode(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string containingTypeFullName
            , string idTypeFullName = null
        )
        {
            var typeName = simpleName;
            var typeFullName = fullName;
            var hasIdType = string.IsNullOrEmpty(idTypeFullName) == false;

            p.PrintLine(SERIALIZABLE);

            if (hasIdType)
            {
                p.PrintLine(string.Format(GENERATED_SHEET_ROW, idTypeFullName, typeFullName));
            }
            else
            {
                p.PrintLine(string.Format(GENERATED_DATA_ROW, typeFullName));
            }

            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintBeginLine("public partial class __").Print(typeName);

            if (hasIdType)
            {
                p.Print(" : SheetRow");

                if (dataMap.ContainsKey(idTypeFullName))
                {
                    p.Print($"<__{GetSimpleName(idTypeFullName)}>");
                }
                else
                {
                    p.Print($"<{idTypeFullName}>");
                }
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine($"public static readonly __{typeName} Default = new __{typeName}();");
                p.PrintEndLine();

                WriteConstructor(ref p, dataMap, horizontalListMap, typeName, containingTypeFullName);

                // Base type layers (outermost first = reversed order = ascending index)
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
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        // ── Constructor ────────────────────────────────────────────────────────────

        private readonly void WriteConstructor(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string typeName
            , string containingTypeFullName
        )
        {
            p.PrintLine($"public __{typeName}()");
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
            , Dictionary<string, DataModel> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , EquatableArray<MemberModel> memberModels
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
                        var collectionTypeName = VERTICAL_LIST_TYPE;

                        if (coll.kind == CollectionKind.Array
                            && horizontalListMap.TryGetValue(targetTypeFullName, out var innerMap)
                        )
                        {
                            if (innerMap.TryGetValue(containingTypeFullName, out var propertyNames)
                                && propertyNames.Contains(member.propertyName)
                            )
                            {
                                collectionTypeName = LIST_TYPE_T;
                            }
                        }

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        newExpression = $"new {collectionTypeName}{elemTypeName}>()";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyTypeName = dataMap.ContainsKey(coll.keyTypeName)
                            ? $"__{coll.keyTypeSimpleName}" : coll.keyTypeName;

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        newExpression = $"new {DICTIONARY_TYPE_T}{keyTypeName}, {elemTypeName}>()";
                        break;
                    }

                    default:
                    {
                        var fieldTypeFull = member.SelectTypeFullName();

                        if (dataMap.ContainsKey(fieldTypeFull))
                        {
                            newExpression = $"new __{member.SelectTypeSimpleName()}()";
                        }
                        else if (member.SelectTypeIsValueType())
                        {
                            newExpression = "default";
                        }
                        else if (fieldTypeFull == "string")
                        {
                            newExpression = "string.Empty";
                        }
                        else if (member.typeHasParameterlessConstructor)
                        {
                            newExpression = $"new {fieldTypeFull}()";
                        }
                        else
                        {
                            newExpression = "default";
                        }

                        break;
                    }
                }

                p.PrintLine($"this.{member.propertyName} = {newExpression};");
            }
        }

        // ── Properties ─────────────────────────────────────────────────────────────

        private static void WriteProperties(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , string containingTypeFullName
            , string idTypeFullName
            , EquatableArray<MemberModel> memberModels
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
                        var collectionTypeName = VERTICAL_LIST_TYPE;

                        if (coll.kind == CollectionKind.Array
                            && horizontalListMap.TryGetValue(containingTypeFullName, out var innerMap)
                        )
                        {
                            if (innerMap.TryGetValue(containingTypeFullName, out var propertyNames)
                                && propertyNames.Contains(member.propertyName)
                            )
                            {
                                collectionTypeName = LIST_TYPE_T;
                            }
                        }

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        propTypeName = $"{collectionTypeName}{elemTypeName}>";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyTypeName = dataMap.ContainsKey(coll.keyTypeName)
                            ? $"__{coll.keyTypeSimpleName}" : coll.keyTypeName;

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        propTypeName = $"{DICTIONARY_TYPE_T}{keyTypeName}, {elemTypeName}>";
                        break;
                    }

                    default:
                    {
                        var fieldTypeFull = member.SelectTypeFullName();
                        propTypeName = dataMap.ContainsKey(fieldTypeFull)
                            ? $"__{member.SelectTypeSimpleName()}" : fieldTypeFull;
                        break;
                    }
                }

                p.PrintLine($"public {propTypeName} {member.propertyName} {{ get; private set; }}");
                p.PrintEndLine();
            }
        }

        // ── Convert method ─────────────────────────────────────────────────────────

        private readonly void WriteConvertMethod(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
        )
        {
            var typeName = simpleName;
            var typeFullName = fullName;

            p.PrintLine($"public {typeFullName} To{typeName}()");
            p.OpenScope();
            {
                p.PrintLine($"var result = new {typeFullName}();");
                p.PrintEndLine();

                foreach (var layer in baseTypeLayers)
                {
                    WriteConvertType(ref p, dataMap, layer.validIdentifier, layer.propRefs, layer.fieldRefs);
                    p.PrintEndLine();
                }

                WriteConvertType(ref p, dataMap, validIdentifier, propRefs, fieldRefs);
                p.PrintEndLine();

                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteConvertType(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , string typeValidIdentifier
            , EquatableArray<MemberModel> props
            , EquatableArray<MemberModel> fields
        )
        {
            foreach (var member in fields)
            {
                WriteConvertMember(ref p, dataMap, typeValidIdentifier, member);
            }

            foreach (var member in props)
            {
                WriteConvertMember(ref p, dataMap, typeValidIdentifier, member);
            }
        }

        private static void WriteConvertMember(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , string typeValidIdentifier
            , MemberModel member
        )
        {
            var coll = member.SelectCollection();
            string expression;

            switch (coll.kind)
            {
                case CollectionKind.Array:
                {
                    if (dataMap.ContainsKey(coll.elementTypeName))
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "Array");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementTypeName;
                        expression = member.converter.Convert($"this.{member.propertyName}?.ToArray() ?? new {elemFull}[0]");
                    }
                    break;
                }

                case CollectionKind.List:
                {
                    if (dataMap.ContainsKey(coll.elementTypeName))
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "List");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementTypeName;
                        expression = member.converter.Convert($"this.{member.propertyName} ?? new {LIST_TYPE_T}{elemFull}>()");
                    }
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    var keyIsData = dataMap.ContainsKey(coll.keyTypeName);
                    var elemIsData = dataMap.ContainsKey(coll.elementTypeName);

                    if (keyIsData == false && elemIsData == false)
                    {
                        expression = member.converter.Convert($"this.{member.propertyName} ?? new {DICTIONARY_TYPE_T}{coll.keyTypeName}, {coll.elementTypeName}>()");
                    }
                    else
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "Dictionary");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    break;
                }

                case CollectionKind.HashSet:
                {
                    if (dataMap.ContainsKey(coll.elementTypeName))
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "HashSet");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementTypeName;
                        expression = member.converter.Convert($"this.{member.propertyName} == null ? new {HASH_SET_TYPE_T}{elemFull}>() : new {HASH_SET_TYPE_T}{elemFull}>(this.{member.propertyName})");
                    }
                    break;
                }

                case CollectionKind.Queue:
                {
                    if (dataMap.ContainsKey(coll.elementTypeName))
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "Queue");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementTypeName;
                        expression = member.converter.Convert($"this.{member.propertyName} == null ? new {QUEUE_TYPE_T}{elemFull}>() : new {QUEUE_TYPE_T}{elemFull}>(this.{member.propertyName})");
                    }
                    break;
                }

                case CollectionKind.Stack:
                {
                    if (dataMap.ContainsKey(coll.elementTypeName))
                    {
                        var methodName = GetToCollectionMethodName(member.propertyName, coll.elementTypeSimpleName, "Stack");
                        expression = member.converter.Convert($"this.{methodName}()");
                    }
                    else
                    {
                        var elemFull = coll.elementTypeName;
                        expression = member.converter.Convert($"this.{member.propertyName} == null ? new {STACK_TYPE_T}{elemFull}>() : new {STACK_TYPE_T}{elemFull}>(this.{member.propertyName})");
                    }
                    break;
                }

                default:
                {
                    var fieldTypeFull = member.SelectTypeFullName();
                    var fieldSimpleName = member.SelectTypeSimpleName();

                    if (dataMap.ContainsKey(fieldTypeFull))
                    {
                        expression = member.converter.Convert($"(this.{member.propertyName} ?? __{fieldSimpleName}.Default).To{fieldSimpleName}()");
                    }
                    else
                    {
                        var newSuffix = member.SelectTypeIsValueType()
                            ? "" : member.typeHasParameterlessConstructor
                            ? $" ?? new {fieldTypeFull}()" : " ?? default";

                        expression = member.converter.Convert($"this.{member.propertyName}{newSuffix}");
                    }
                    break;
                }
            }

            p.PrintLine($"result.SetValue_{typeValidIdentifier}_{member.propertyName}({expression});");
        }

        // ── ToCollection methods ────────────────────────────────────────────────────

        private static void WriteToCollectionMethod(
              ref Printer p
            , Dictionary<string, DataModel> dataMap
            , EquatableArray<MemberModel> memberModels
        )
        {
            foreach (var member in memberModels)
            {
                var coll = member.SelectCollection();

                switch (coll.kind)
                {
                    case CollectionKind.Array:      WriteToArrayMethod(ref p, member, coll, dataMap);      break;
                    case CollectionKind.List:       WriteToListMethod(ref p, member, coll, dataMap);       break;
                    case CollectionKind.Dictionary: WriteToDictionaryMethod(ref p, member, coll, dataMap); break;
                    case CollectionKind.HashSet:    WriteToHashSetMethod(ref p, member, coll, dataMap);    break;
                    case CollectionKind.Queue:      WriteToQueueMethod(ref p, member, coll, dataMap);      break;
                    case CollectionKind.Stack:      WriteToStackMethod(ref p, member, coll, dataMap);      break;
                }
            }
        }

        private static void WriteToArrayMethod(
              ref Printer p
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimpleName = coll.elementTypeSimpleName;
            var elemFullName = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimpleName, "Array");

            p.PrintLine($"private {elemFullName}[] {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {elemFullName}[0];");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {elemFullName}[count];");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                p.PrintLine($"result[i] = (rows[i] ?? __{elemSimpleName}.Default).To{elemSimpleName}();");
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToListMethod(
              ref Printer p
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimpleName = coll.elementTypeSimpleName;
            var elemFullName = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimpleName, "List");

            p.PrintLine($"private {LIST_TYPE_T}{elemFullName}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {LIST_TYPE_T}{elemFullName}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {LIST_TYPE_T}{elemFullName}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                p.PrintLine($"var item = (rows[i] ?? __{elemSimpleName}.Default).To{elemSimpleName}();");
                p.PrintLine("result.Add(item);");
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToDictionaryMethod(
              ref Printer p
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            var keyIsData = dataMap.ContainsKey(coll.keyTypeName);
            var elemIsData = dataMap.ContainsKey(coll.elementTypeName);

            if (keyIsData == false && elemIsData == false)
            {
                return;
            }

            var keySimple = coll.keyTypeSimpleName;
            var keyFull = coll.keyTypeName;
            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "Dictionary");

            p.PrintLine($"private {DICTIONARY_TYPE_T}{keyFull}, {elemFull}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {DICTIONARY_TYPE_T}{keyFull}, {elemFull}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {DICTIONARY_TYPE_T}{keyFull}, {elemFull}>(count);");
                p.PrintEndLine();

                p.PrintLine("foreach (var kv in rows)");
                p.OpenScope();
                {
                    if (keyIsData)
                    {
                        p.PrintLine($"var key = (kv.Key ?? __{keySimple}.Default).To{keySimple}();");
                    }
                    else
                    {
                        p.PrintLine("var key = kv.Key;");
                    }

                    if (elemIsData)
                    {
                        p.PrintLine($"var value = (kv.Value ?? __{elemSimple}.Default).To{elemSimple}();");
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
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "HashSet");

            p.PrintLine($"private {HASH_SET_TYPE_T}{elemFull}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {HASH_SET_TYPE_T}{elemFull}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {HASH_SET_TYPE_T}{elemFull}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                p.PrintLine($"var item = (rows[i] ?? __{elemSimple}.Default).To{elemSimple}();");
                p.PrintLine("result.Add(item);");
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToQueueMethod(
              ref Printer p
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "Queue");

            p.PrintLine($"private {QUEUE_TYPE_T}{elemFull}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {QUEUE_TYPE_T}{elemFull}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {QUEUE_TYPE_T}{elemFull}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                p.PrintLine($"var item = (rows[i] ?? __{elemSimple}.Default).To{elemSimple}();");
                p.PrintLine("result.Enqueue(item);");
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToStackMethod(
              ref Printer p
            , MemberModel member
            , CollectionModel coll
            , Dictionary<string, DataModel> dataMap
        )
        {
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "Stack");

            p.PrintLine($"private {STACK_TYPE_T}{elemFull}> {methodName}()");
            p.OpenScope();
            {
                p.PrintLine($"if (this.{member.propertyName} == null || this.{member.propertyName}.Count == 0)");
                p = p.IncreasedIndent();
                p.PrintLine($"return new {STACK_TYPE_T}{elemFull}>();");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                p.PrintLine($"var rows = this.{member.propertyName};");
                p.PrintLine("var count = rows.Count;");
                p.PrintLine($"var result = new {STACK_TYPE_T}{elemFull}>(count);");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < count; i++)");
                p.OpenScope();
                p.PrintLine($"var item = (rows[i] ?? __{elemSimple}.Default).To{elemSimple}();");
                p.PrintLine("result.Push(item);");
                p.CloseScope();

                p.PrintEndLine();
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

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
