using System.Collections.Generic;
using EncosyTower.SourceGen.Common.Data.Common;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DataSpec
    {
        /// <summary>
        /// Writes the <c>__TypeName</c> partial class body into <paramref name="p"/>.
        /// </summary>
        /// <param name="dataMap">Full type name → DataSpec for all IData types in scope.</param>
        /// <param name="horizontalListMap">
        /// targetTypeFullName → containingTypeFullName → property names.
        /// </param>
        /// <param name="containingTypeFullName">Full name of the table asset type (ITableAsset).</param>
        /// <param name="idTypeFullName">Full name of the Id type, or <c>null</c> when this is not a sheet-row class.</param>
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

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        newExpression = $"new {collectionTypeName}<{elemTypeName}>()";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyTypeName = dataMap.ContainsKey(coll.keyTypeName)
                            ? $"__{coll.keyTypeSimpleName}" : coll.keyTypeName;

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        newExpression = $"new {PR_DICTIONARY_T}<{keyTypeName}, {elemTypeName}>()";
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

                p.PrintBeginLine("this.").Print(member.propertyName)
                    .Print(" = ").Print(newExpression).PrintEndLine(";");
            }
        }

        // ── Properties ─────────────────────────────────────────────────────────────

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

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        propTypeName = $"{collectionTypeName}<{elemTypeName}>";
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        var keyTypeName = dataMap.ContainsKey(coll.keyTypeName)
                            ? $"__{coll.keyTypeSimpleName}" : coll.keyTypeName;

                        var elemTypeName = dataMap.ContainsKey(coll.elementTypeName)
                            ? $"__{coll.elementTypeSimpleName}" : coll.elementTypeName;

                        propTypeName = $"{PR_DICTIONARY_T}<{keyTypeName}, {elemTypeName}>";
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

                p.PrintBeginLine("public ").Print(propTypeName).Print(" ").Print(member.propertyName)
                    .PrintEndLine(" { get; private set; }");
                p.PrintEndLine();
            }
        }

        // ── Convert method ─────────────────────────────────────────────────────────

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
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , EquatableArray<MemberSpec> props
            , EquatableArray<MemberSpec> fields
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
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , MemberSpec member
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
                        expression = member.converter.Convert(
                            $"this.{member.propertyName} ?? new {PR_LIST_T}<{elemFull}>()"
                        );
                    }
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    var keyIsData = dataMap.ContainsKey(coll.keyTypeName);
                    var elemIsData = dataMap.ContainsKey(coll.elementTypeName);

                    if (keyIsData == false && elemIsData == false)
                    {
                        expression = member.converter.Convert(
                            $"this.{member.propertyName} ?? new {PR_DICTIONARY_T}<{coll.keyTypeName}, {coll.elementTypeName}>()"
                        );
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
                        expression = member.converter.Convert(
                            $"this.{member.propertyName} == null ? new {PR_HASH_SET_T}<{elemFull}>() : new {PR_HASH_SET_T}<{elemFull}>(this.{member.propertyName})"
                        );
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
                        expression = member.converter.Convert(
                            $"this.{member.propertyName} == null ? new {PR_QUEUE_T}<{elemFull}>() : new {PR_QUEUE_T}<{elemFull}>(this.{member.propertyName})"
                        );
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
                        expression = member.converter.Convert(
                            $"this.{member.propertyName} == null ? new {PR_STACK_T}<{elemFull}>() : new {PR_STACK_T}<{elemFull}>(this.{member.propertyName})"
                        );
                    }
                    break;
                }

                default:
                {
                    var fieldTypeFull = member.SelectTypeFullName();
                    var fieldSimpleName = member.SelectTypeSimpleName();

                    if (dataMap.ContainsKey(fieldTypeFull))
                    {
                        expression = member.converter.Convert(
                            $"(this.{member.propertyName} ?? __{fieldSimpleName}.Default).To{fieldSimpleName}()"
                        );
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

            p.PrintBeginLine("result.SetValue_").Print(typeValidIdentifier).Print("_")
                .Print(member.propertyName).Print("(")
                .Print(expression).PrintEndLine(");");
        }

        // ── ToCollection methods ────────────────────────────────────────────────────

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
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimpleName = coll.elementTypeSimpleName;
            var elemFullName = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimpleName, "Array");

            p.PrintBeginLine("private ").Print(elemFullName).Print("[] ").Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName).Print(" == null || this.")
                    .Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(elemFullName).PrintEndLine("[0];");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimpleName = coll.elementTypeSimpleName;
            var elemFullName = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimpleName, "List");

            p.PrintBeginLine("private ").Print(PR_LIST_T).Print("<").Print(elemFullName).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName)
                    .Print(" == null || this.").Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(PR_LIST_T).Print("<").Print(elemFullName).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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

            p.PrintBeginLine("private ").Print(PR_DICTIONARY_T).Print("<").Print(keyFull)
                .Print(", ").Print(elemFull).Print("> ").Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName).Print(" == null || this.")
                    .Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ").Print(PR_DICTIONARY_T).Print("<").Print(keyFull)
                    .Print(", ").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "HashSet");

            p.PrintBeginLine("private ").Print(PR_HASH_SET_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName)
                    .Print(" == null || this.").Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_HASH_SET_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "Queue");

            p.PrintBeginLine("private ").Print(PR_QUEUE_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName)
                    .Print(" == null || this.").Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_QUEUE_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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
            if (dataMap.ContainsKey(coll.elementTypeName) == false)
            {
                return;
            }

            var elemSimple = coll.elementTypeSimpleName;
            var elemFull = coll.elementTypeName;
            var methodName = GetToCollectionMethodName(member.propertyName, elemSimple, "Stack");

            p.PrintBeginLine("private ").Print(PR_STACK_T).Print("<").Print(elemFull).Print("> ")
                .Print(methodName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("if (this.").Print(member.propertyName)
                    .Print(" == null || this.").Print(member.propertyName).PrintEndLine(".Count == 0)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("return new ")
                    .Print(PR_STACK_T).Print("<").Print(elemFull).PrintEndLine(">();");
                p.PrintEndLine();

                p.PrintBeginLine("var rows = this.").Print(member.propertyName).PrintEndLine(";");
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
