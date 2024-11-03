using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.Modules.EnumExtensions.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    partial class PolyStructGenerator
    {
        private static void GenerateMergedStruct(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , bool outputSourceGenFiles
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , IReadOnlyCollection<MergedFieldRef> mergedFieldRefList
            , List<EnumMemberDeclaration> enumMembers
            , StringBuilder sb
            , CancellationToken token
        )
        {
            try
            {
                var syntax = interfaceRef.Syntax;
                var symbol = interfaceRef.Symbol;
                var syntaxTree = syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;
                var refUnityCollections = compilationCandidate.references.unityCollections;
                var mergedStructName = $"{interfaceRef.FullContainingNameWithDot}{interfaceRef.StructName}";
                var enumUnderlyingTypeName = GetEnumUnderlyingTypeName(structRefCount);

                ToEnumMembers(interfaceRef, enumMembers, out var kindFixedStringBytes, structRefs);

                var enumExtensions = new EnumExtensionsDeclaration(refUnityCollections, kindFixedStringBytes) {
                    GeneratedCode = GENERATED_CODE,
                    Name = "TypeId",
                    ExtensionsName = $"{interfaceRef.StructName}_TypeIdExtensions",
                    ExtensionsWrapperName = $"{interfaceRef.StructName}_TypeIdExtensionsWrapper",
                    ParentIsNamespace = interfaceRef.Syntax.Parent is NamespaceDeclarationSyntax,
                    FullyQualifiedName = $"{mergedStructName}.TypeId",
                    UnderlyingTypeName = enumUnderlyingTypeName,
                    Members = enumMembers,
                    Accessibility = interfaceRef.Symbol.DeclaredAccessibility,
                    IsDisplayAttributeUsed = false,
                };

                var source = WriteMergedStruct(
                      interfaceRef
                    , structRefs
                    , structRefCount
                    , enumUnderlyingTypeName
                    , mergedFieldRefList
                    , enumExtensions
                    , sb
                    , token
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , source
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, interfaceRef.StructName)
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor_1
                    , interfaceRef.Syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static void ToEnumMembers(
              InterfaceRef interfaceRef
            , List<EnumMemberDeclaration> enumMembers
            , out int kindFixedStringBytes
            , IEnumerable<StructRef> structRefs = null
        )
        {
            kindFixedStringBytes = 32;
            enumMembers.Clear();

            enumMembers.Add(new EnumMemberDeclaration {
                name = "Undefined",
                order = 0u,
            });

            if (structRefs == null)
            {
                return;
            }

            var order = 1u;

            foreach (var structRef in structRefs)
            {
                var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;
                var byteCount = typeId.GetByteCount();

                if (byteCount > kindFixedStringBytes)
                {
                    kindFixedStringBytes = byteCount;
                }

                enumMembers.Add(new EnumMemberDeclaration {
                    name = typeId,
                    order = order,
                });

                order += 1;
            }
        }

        private static string WriteMergedStruct(
              InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , string enumUnderlyingType
            , IReadOnlyCollection<MergedFieldRef> mergedFieldRefList
            , EnumExtensionsDeclaration enumExtensions
            , StringBuilder sb
            , CancellationToken token
        )
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, interfaceRef.Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintLine("[global::System.Serializable]");
            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine();
            p.Print(interfaceRef.AccessKeyword)
                .Print($" partial struct {interfaceRef.StructName}")
                .Print($" : {interfaceRef.Symbol.ToFullName()}")
                .PrintEndLine();
            p.OpenScope();
            {
                WriteFields(ref p, mergedFieldRefList);
                WriteIsDefined(ref p, interfaceRef, structRefs, structRefCount);
                WriteIsType(ref p, interfaceRef, structRefs, structRefCount);
                WriteMembers(ref p, interfaceRef, structRefs, structRefCount, sb, token);
                WriteGenericMembers(ref p, interfaceRef, structRefs, structRefCount, sb, token);
                WriteGetTypeIdMethods(ref p, interfaceRef, structRefs);
                WriteEnum(ref p, interfaceRef, structRefs, enumUnderlyingType);
                WriteGenericTypeIdStruct(ref p, interfaceRef, structRefs, structRefCount);
            }
            p.CloseScope();
            p.PrintEndLine();

            enumExtensions.WriteCode(ref p);

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteFields(ref Printer p, IReadOnlyCollection<MergedFieldRef> mergedFieldRefList)
        {
            p.PrintLine("public TypeId CurrentTypeId;");
            p.PrintEndLine();

            if (mergedFieldRefList.Count < 1)
            {
                return;
            }

            foreach (var field in mergedFieldRefList)
            {
                p.PrintBeginLine("public ")
                    .Print(field.Type.ToFullName())
                    .Print(" ")
                    .Print(field.Name)
                    .PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        private static void WriteIsDefined(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly bool IsDefined");
            p.OpenScope();
            {
                p.PrintLine("get");
                p.OpenScope();
                {
                    p.PrintLine("return this.CurrentTypeId switch {");
                    p = p.IncreasedIndent();
                    {
                        foreach (var structRef in structRefs)
                        {
                            var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;
                            p.PrintLine($"TypeId.{typeId} => true,");
                        }

                        p.PrintLine("_ => false,");
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine("};");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteIsType(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
        )
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public readonly bool IsType<T>() where T : struct, {interfaceRef.FullName}");
            p.OpenScope();
            {
                p.PrintLine($"return GetTypeId<T>() == this.CurrentTypeId;");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var structRef in structRefs)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public readonly bool IsType({structRef.FullName} _)");
                p.OpenScope();
                {
                    p.PrintLine($"return GetTypeId<{structRef.FullName}>() == this.CurrentTypeId;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteGenericMembers(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , StringBuilder sb
            , CancellationToken token
        )
        {
            foreach (var member in interfaceRef.Members)
            {
                if (member is IMethodSymbol method
                    && method.RefKind == RefKind.None
                )
                {
                    WriteForMethod(
                          ref p
                        , interfaceRef
                        , method
                        , sb
                    );
                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    WriteForProperty(
                          ref p
                        , interfaceRef
                        , property
                    );
                    continue;
                }
            }

            static void WriteForMethod(
                  ref Printer p
                , InterfaceRef interfaceRef
                , IMethodSymbol method
                , StringBuilder sb
            )
            {
                var returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToFullName();
                var genericType = $"T{interfaceRef.StructName}";

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine($"private static {GetReturnRefKind(method.RefKind)}{returnType} ")
                    .Print($"Internal__{method.Name}<{genericType}>(");
                {
                    p.Print($"ref {genericType} instance");

                    WriteParameters(ref p, method);
                }
                p.PrintEndLine(")");
                p.WithIncreasedIndent()
                    .PrintLine($"where {genericType} : struct, {interfaceRef.FullName}")
                    .WithDecreasedIndent();
                p.OpenScope();
                {
                    p.PrintBeginLine();

                    if (method.ReturnsVoid == false)
                    {
                        p.Print("return ");

                        if (method.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                        {
                            p.Print("ref ");
                        }
                    }

                    p.Print(BuildCallClause(sb, method)).PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteForProperty(
                  ref Printer p
                , InterfaceRef interfaceRef
                , IPropertySymbol property
            )
            {
                var genericType = $"T{interfaceRef.StructName}";

                if (property.GetMethod is IMethodSymbol getMethod
                    && getMethod.RefKind == RefKind.None
                )
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"private static {property.Type.ToFullName()} Internal__{property.Name}<{genericType}>(ref {genericType} instance)");
                    p.WithIncreasedIndent()
                        .PrintLine($"where {genericType} : struct, {interfaceRef.FullName}")
                        .WithDecreasedIndent();
                    p.OpenScope();
                    {
                        p.PrintLine($"return instance.{property.Name};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (property.SetMethod != null)
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"private static void Internal__{property.Name}<{genericType}>(ref {genericType} instance, {property.Type.ToFullName()} value)");
                    p.WithIncreasedIndent()
                        .PrintLine($"where {genericType} : struct, {interfaceRef.FullName}")
                        .WithDecreasedIndent();
                    p.OpenScope();
                    {
                        p.PrintLine($"instance.{property.Name} = value;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }

            static void WriteParameters(
                  ref Printer p
                , IMethodSymbol method
                , bool outToRef = false
            )
            {
                var parameters = method.Parameters;
                var lastParamIndex = parameters.Length - 1;

                if (lastParamIndex >= 0)
                {
                    p.Print(", ");
                }

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    p.Print(GetRefKind(param.RefKind, outToRef))
                        .Print(param.Type.ToFullName())
                        .Print(" ")
                        .Print(param.Name);

                    if (i < lastParamIndex)
                    {
                        p.Print(", ");
                    }
                }
            }

            static string BuildCallClause(
                  StringBuilder sb
                , IMethodSymbol method
            )
            {
                var parameters = method.Parameters;
                var lastParamIndex = parameters.Length - 1;

                sb.Clear();
                sb.Append($"instance.{method.Name}(");

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    sb.Append($"{GetRefKind(param.RefKind)}{param.Name}");

                    if (i < lastParamIndex)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append(");");
                return sb.ToString();
            }
        }

        private static void WriteMembers(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , StringBuilder sb
            , CancellationToken token
        )
        {
            foreach (var member in interfaceRef.Members)
            {
                if (member is IMethodSymbol method)
                {
                    WriteMethod(
                          ref p
                        , interfaceRef
                        , structRefs
                        , structRefCount
                        , method
                        , sb
                        , token
                    );
                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    WriteProperty(
                          ref p
                        , interfaceRef
                        , structRefs
                        , structRefCount
                        , property
                        , token
                    );
                    continue;
                }
            }
        }

        private static void WriteMethod(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , IMethodSymbol method
            , StringBuilder sb
            , CancellationToken token
        )
        {
            var returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToFullName();

            WriteAttributes(ref p, method, token);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine($"public {GetReturnRefKind(method.RefKind)}{returnType} {method.Name}(");
            {
                WriteParameters(ref p, method);
            }
            p.PrintEndLine(")");

            var callClause = BuildCallClause(sb, interfaceRef, method);
            var assignOutParams = BuildAssignOutParams(sb, method);

            WriteMethodBody(
                  ref p
                , interfaceRef
                , structRefs
                , structRefCount
                , method
                , false
                , callClause
                , assignOutParams
            );

            p.PrintEndLine();

            p.PrintBeginLine($"partial void {GetDefaultMethodName(method)}(");
            {
                WriteParameters(ref p, method, outToRef: true);

                if (method.ReturnsVoid == false)
                {
                    if (method.Parameters.Length > 0)
                    {
                        p.Print(", ");
                    }

                    p.Print("ref ")
                        .Print(method.ReturnType.ToFullName())
                        .Print(" ")
                        .Print(GetDefaultResultVarName(method));
                }
            }
            p.PrintEndLine(");");

            p.PrintEndLine();

            static string BuildCallClause(
                  StringBuilder sb
                , InterfaceRef interfaceRef
                , IMethodSymbol method
            )
            {
                var parameters = method.Parameters;
                var lastParamIndex = parameters.Length - 1;

                sb.Clear();

                if (method.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                {
                    sb.Append($"instance.{method.Name}(");
                }
                else
                {
                    sb.Append(interfaceRef.StructName)
                        .Append(".Internal__").Append(method.Name)
                        .Append("(ref instance");

                    if (parameters.Length > 0)
                    {
                        sb.Append(", ");
                    }
                }

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    sb.Append($"{GetRefKind(param.RefKind)}{param.Name}");

                    if (i < lastParamIndex)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append(");");
                return sb.ToString();
            }

            static string BuildAssignOutParams(
                  StringBuilder sb
                , IMethodSymbol method
            )
            {
                sb.Clear();

                foreach (var param in method.Parameters)
                {
                    if (param.RefKind == RefKind.Out)
                    {
                        sb.Append($"{param.Name} = default;").Append('\n');
                    }
                }

                return sb.ToString();
            }
        }

        private static void WriteParameters(
              ref Printer p
            , IMethodSymbol method
            , bool outToRef = false
        )
        {
            var parameters = method.Parameters;
            var lastParamIndex = parameters.Length - 1;

            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                p.Print(GetRefKind(param.RefKind, outToRef))
                    .Print(param.Type.ToFullName())
                    .Print(" ")
                    .Print(param.Name);

                if (i < lastParamIndex)
                {
                    p.Print(", ");
                }
            }
        }

        private static void WriteProperty(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , IPropertySymbol property
            , CancellationToken token
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {GetReturnRefKind(property.RefKind)}{property.Type.ToFullName()} {property.Name}");
            p.OpenScope();
            {
                if (property.GetMethod is IMethodSymbol getMethod)
                {
                    string callClause;

                    if (getMethod.RefKind == RefKind.None)
                    {
                        callClause = $"{interfaceRef.StructName}.Internal__{property.Name}(ref instance);";
                    }
                    else
                    {
                        callClause = $"instance.{property.Name};";
                    }

                    WriteAttributes(ref p, property.GetMethod, token);

                    p.PrintLine("get");
                    WriteMethodBody(
                          ref p
                        , interfaceRef
                        , structRefs
                        , structRefCount
                        , property.GetMethod
                        , true
                        , callClause
                        , ""
                    );
                }

                if (property.SetMethod != null)
                {
                    WriteAttributes(ref p, property.SetMethod, token);

                    p.PrintLine("set");
                    WriteMethodBody(
                          ref p
                        , interfaceRef
                        , structRefs
                        , structRefCount
                        , property.SetMethod
                        , true
                        , $"{interfaceRef.StructName}.Internal__{property.Name}(ref instance, value);"
                        , ""
                    );
                }
            }
            p.CloseScope();
        }

        private static void WriteAttributes(
              ref Printer p
            , ISymbol symbol
            , CancellationToken token
        )
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.ApplicationSyntaxReference is not SyntaxReference syntaxRef)
                {
                    continue;
                }

                var syntax = syntaxRef.GetSyntax(token);
                p.PrintBeginLine("[").Print(syntax.ToFullString()).PrintEndLine("]");
            }
        }

        private static void WriteMethodBody(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
            , IMethodSymbol method
            , bool isPropertyBody
            , string callClause
            , string assignOutParams
        )
        {
            if (structRefCount < 1)
            {
                p.OpenScope();
                {
                    if (method.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                    {
                        p.PrintBeginLine("throw new global::System.InvalidOperationException(\"");
                        p.Print("Cannot return any reference from the default case.");
                        p.PrintEndLine("\");");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(assignOutParams) == false)
                        {
                            p.PrintLine(assignOutParams);
                        }

                        if (isPropertyBody)
                        {
                            p.PrintLine("return default;");
                        }
                        else
                        {
                            WriteDefaultMethodBranch(ref p, method);
                        }
                    }
                }
                p.CloseScope();

                return;
            }

            p.OpenScope();
            {
                p.PrintLine("switch (this.CurrentTypeId)");
                p.OpenScope();
                {
                    foreach (var structRef in structRefs)
                    {
                        var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;

                        p.PrintLine($"case TypeId.{typeId}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"{structRef.FullName} instance = this;");
                            p.PrintBeginLine();

                            if (method.ReturnsVoid == false)
                            {
                                if (method.RefKind == RefKind.Ref)
                                {
                                    p.Print("ref var result = ref ");
                                }
                                else if (method.RefKind == RefKind.RefReadOnly)
                                {
                                    p.Print("ref readonly var result = ref ");
                                }
                                else
                                {
                                    p.Print("var result = ");
                                }
                            }

                            p.Print(callClause).PrintEndLine();

                            if (structRef.Fields.Length > 0)
                            {
                                p.PrintLine("this = instance;");
                            }

                            p.PrintEndLine();

                            if (method.ReturnsVoid)
                            {
                                p.PrintLine("return;");
                            }
                            else
                            {
                                p.PrintBeginLine("return ");

                                if (method.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                                {
                                    p.Print("ref ");
                                }

                                p.PrintEndLine("result;");
                            }
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        if (method.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                        {
                            p.PrintBeginLine("throw new global::System.InvalidOperationException(\"");
                            p.Print("Cannot return any reference from the default case.");
                            p.PrintEndLine("\");");
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(assignOutParams) == false)
                            {
                                p.PrintLine(assignOutParams);
                            }

                            if (isPropertyBody)
                            {
                                if (method.ReturnsVoid)
                                {
                                    p.PrintLine("return;");
                                }
                                else
                                {
                                    p.PrintLine("return default;");
                                }
                            }
                            else
                            {
                                WriteDefaultMethodBranch(ref p, method);
                            }
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();

            static void WriteDefaultMethodBranch(
                  ref Printer p
                , IMethodSymbol method
            )
            {
                if (method.ReturnsVoid)
                {
                    WriteDefaultMethodCall(ref p, method);
                    p.PrintLine("return;");
                }
                else
                {
                    var resultVarName = GetDefaultResultVarName(method);
                    p.PrintLine($"var {resultVarName} = default({method.ReturnType.ToFullName()});");
                    p.PrintEndLine();
                    WriteDefaultMethodCall(ref p, method, resultVarName);
                    p.PrintEndLine();
                    p.PrintLine($"return {resultVarName};");
                }
            }

            static void WriteDefaultMethodCall(
                  ref Printer p
                , IMethodSymbol method
                , string resultVarName = ""
            )
            {
                var parameters = method.Parameters;
                var lastParamIndex = parameters.Length - 1;

                p.PrintBeginLine($"{GetDefaultMethodName(method)}(");

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    p.Print(GetRefKind(param.RefKind, true))
                        .Print(param.Name);

                    if (i < lastParamIndex)
                    {
                        p.Print(", ");
                    }
                }

                if (string.IsNullOrWhiteSpace(resultVarName) == false)
                {
                    if (parameters.Length > 0)
                    {
                        p.Print(", ");
                    }

                    p.Print("ref ").Print(resultVarName);
                }

                p.PrintEndLine(");");
            }
        }

        private static void WriteGetTypeIdMethods(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
        )
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static TypeId GetTypeId<T>() where T : struct, {interfaceRef.FullName}");
            p.OpenScope();
            {
                p.PrintLine("return TypeId<T>.Value;");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var structRef in structRefs)
            {
                var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static TypeId GetTypeId(in {structRef.FullName} _)");
                p.OpenScope();
                {
                    p.PrintLine($"return TypeId.{typeId};");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteEnum(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , string underlyingType
        )
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintLine($"public enum TypeId : {underlyingType}");
            p.OpenScope();
            {
                p.PrintLine("Undefined = 0,");

                foreach (var structRef in structRefs)
                {
                    var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;
                    p.PrintBeginLine().Print(typeId).PrintEndLine(",");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteGenericTypeIdStruct(
              ref Printer p
            , InterfaceRef interfaceRef
            , IEnumerable<StructRef> structRefs
            , ulong structRefCount
        )
        {
            if (structRefCount > 0)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"static {interfaceRef.StructName}()");
                p.OpenScope();
                {
                    foreach (var structRef in structRefs)
                    {
                        var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;
                        p.PrintLine($"TypeId<{structRef.FullName}>.Value = TypeId.{typeId};");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private struct TypeId<T> where T : struct, {interfaceRef.FullName}");
            p.OpenScope();
            {
                p.PrintLine($"public static {interfaceRef.StructName}.TypeId Value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string GetDefaultMethodName(IMethodSymbol method)
            => $"{method.Name}_Default";

        private static string GetDefaultResultVarName(IMethodSymbol method)
            => $"{GetDefaultMethodName(method)}_result";

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
