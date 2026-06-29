using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking
#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace EncosyTower.SourceGen.Analyzers.DatabaseAuthoring
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DatabaseAuthoringDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        private const string DATABASES_AUTHORING_NAMESPACE = DATABASES_NAMESPACE + ".Authoring";
        private const string DATA_NAMESPACE = "EncosyTower.Data";

        private const string AUTHOR_DATABASE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.AuthorDatabaseAttribute";
        private const string CONVERTER_FOR_TABLE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForTableAttribute";
        private const string CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForDataPropertyAttribute";
        private const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        private const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";
        private const string DATA_AUTHORING_CONVERTER_ATTRIBUTE = $"global::{DATA_NAMESPACE}.Authoring.DataAuthoringConverterAttribute";
        private const string IDATA = $"global::{DATA_NAMESPACE}.IData";

        public static readonly DiagnosticDescriptor MissingDefaultConstructor = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0010"
            , title: "Missing default constructor"
            , messageFormat: "The type \"{0}\" must contain a default (parameterless) constructor"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor StaticConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0020"
            , title: "Static \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public static methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InstancedConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0021"
            , title: "Instanced \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public instanced methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0030"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0031"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0040"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0041"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0042"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0043"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0050"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpressionAt = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0051"
            , title: "Not a typeof expression"
            , messageFormat: "The argument at position {0} must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor AbstractTypeNotSupported = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0060"
            , title: "Abstract type is not supported"
            , messageFormat: "The type \"{0}\" must not be abstract"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0061"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor ConverterAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0070"
            , title: "Converter ambiguity"
            , messageFormat: "The type \"{0}\" at position {3} will be ignored because a \"Convert\" method that returns a value of \"{2}\" has already been defined in \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DuplicateDataPropertyConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0090"
            , title: "Conflicting data property converters"
            , messageFormat: "Multiple different converters are specified by \"ConverterForDataProperty\" for property \"{0}\" of data type \"{1}\" within {2}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor RedundantDataPropertyConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0091"
            , title: "Redundant data property converter"
            , messageFormat: "The converter \"{0}\" is specified more than once by \"ConverterForDataProperty\" for property \"{1}\" of data type \"{2}\" within {3}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DuplicateTableConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0092"
            , title: "Conflicting table converters"
            , messageFormat: "Multiple different converters are specified by \"ConverterForTable\" for source type \"{0}\" within {1}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor RedundantTableConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0093"
            , title: "Redundant table converter"
            , messageFormat: "The converter \"{0}\" is specified more than once by \"ConverterForTable\" for source type \"{1}\" within {2}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MissingDefaultConstructor
                , StaticConvertMethodAmbiguity
                , InstancedConvertMethodAmbiguity
                , MissingConvertMethod
                , MissingConvertMethodReturnType
                , InvalidStaticConvertMethodReturnType
                , InvalidInstancedConvertMethodReturnType
                , InvalidStaticConvertMethod
                , InvalidInstancedConvertMethod
                , NotTypeOfExpression
                , NotTypeOfExpressionAt
                , AbstractTypeNotSupported
                , OpenGenericTypeNotSupported
                , ConverterAmbiguity
                , DuplicateDataPropertyConverter
                , RedundantDataPropertyConverter
                , DuplicateTableConverter
                , RedundantTableConverter
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeAuthoringType, SymbolKind.NamedType);
        }


        private static void AnalyzeAuthoringType(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol authoringSymbol)
            {
                return;
            }

            var authorAttrib = authoringSymbol.GetAttribute(AUTHOR_DATABASE_ATTRIBUTE, token);

            if (authorAttrib == null)
            {
                return;
            }

            // AuthorDatabaseAttribute is (Type databaseType, params Type[] converters), so real usages have
            // two constructor arguments. Only the first (the database type) is required here.
            if (authorAttrib.ConstructorArguments.Length < 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
            )
            {
                return;
            }

            var dbAttrib = databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE, token);

            if (dbAttrib != null)
            {
                var dbConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in dbAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, dbAttrib, dbConverterMap, 0);
                        break;
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            AnalyzeConverterAttributes(context, authoringSymbol, databaseSymbol, token);

            token.ThrowIfCancellationRequested();

            foreach (var member in databaseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE, token);

                if (tableAttrib == null)
                {
                    continue;
                }

                var tableConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, tableAttrib, tableConverterMap, 2);
                        break;
                    }
                }

                if (property.Type is INamedTypeSymbol tableType && tableType.BaseType != null)
                {
                    ValidateDataMembers(context, tableType);
                }
            }
        }

        private static void ValidateDataMembers(SymbolAnalysisContext context, INamedTypeSymbol tableType)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            var visited = new HashSet<string>(System.StringComparer.Ordinal);
            var queue = new Queue<INamedTypeSymbol>();
            queue.Enqueue(tableType);

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var type = queue.Dequeue();
                var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (visited.Contains(fullName))
                {
                    continue;
                }

                visited.Add(fullName);

                foreach (var member in type.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    ITypeSymbol memberType;

                    if (member is IPropertySymbol prop)
                    {
                        memberType = prop.Type;
                    }
                    else if (member is IFieldSymbol field)
                    {
                        memberType = field.Type;
                    }
                    else
                    {
                        continue;
                    }

                    var converterAttrib = member.GetAttribute(DATA_AUTHORING_CONVERTER_ATTRIBUTE, token);

                    if (converterAttrib != null)
                    {
                        ValidateMemberConverter(context, converterAttrib, memberType);
                    }

                    if (memberType is INamedTypeSymbol namedMemberType
                        && namedMemberType.ImplementsInterface(IDATA, false, token)
                    )
                    {
                        queue.Enqueue(namedMemberType);
                    }
                }
            }
        }

        private static void ValidateMemberConverter(
              SymbolAnalysisContext context
            , AttributeData converterAttrib
            , ITypeSymbol targetType
        )
        {
            if (converterAttrib.ConstructorArguments.Length != 1)
                return;

            var syntax = converterAttrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);

            if (converterAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                if (syntax != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          NotTypeOfExpression
                        , syntax.GetLocation()
                    ));
                }
                return;
            }

            ValidateConverterType(context, converterType, syntax, targetType);
        }

        private static void ValidateConverterMapArguments(
              SymbolAnalysisContext context
            , ImmutableArray<TypedConstant> values
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> converterMap
            , int offset
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (values.IsDefaultOrEmpty)
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);

            for (var i = 0; i < values.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (values[i].Value is not INamedTypeSymbol converterType)
                {
                    if (syntax != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              NotTypeOfExpressionAt
                            , syntax.GetLocation()
                            , offset + i
                        ));
                    }

                    continue;
                }

                if (ValidateConverterType(context, converterType, syntax, returnType: null) == false)
                {
                    continue;
                }

                if (TryFindConvertReturnType(converterType, out var returnTypeName, token) == false)
                {
                    continue;
                }

                if (converterMap.TryGetValue(returnTypeName, out var existing))
                {
                    if (syntax != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              ConverterAmbiguity
                            , syntax.GetLocation()
                            , converterType.Name
                            , existing.Name
                            , returnTypeName
                            , offset + i
                        ));
                    }
                }
                else
                {
                    converterMap[returnTypeName] = converterType;
                }
            }
        }

        private static void AnalyzeConverterAttributes(
              SymbolAnalysisContext context
            , INamedTypeSymbol authoringSymbol
            , INamedTypeSymbol databaseSymbol
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var tableTypeByPropName = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

            foreach (var member in databaseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IPropertySymbol property
                    && property.GetAttribute(TABLE_ATTRIBUTE, token) != null
                    && property.Type is INamedTypeSymbol tableType
                )
                {
                    tableTypeByPropName[property.Name] = tableType;
                }
            }

            var dataPropGroups = new Dictionary<string, List<ConverterEntry>>(System.StringComparer.Ordinal);
            var tableGroups = new Dictionary<string, List<ConverterEntry>>(System.StringComparer.Ordinal);

            foreach (var attrib in authoringSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attribClass = attrib.AttributeClass;

                if (attribClass == null)
                {
                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE, token))
                {
                    CollectDataPropertyConverter(context, attrib, tableTypeByPropName, dataPropGroups, token);
                }
                else if (attribClass.HasFullName(CONVERTER_FOR_TABLE_ATTRIBUTE, token))
                {
                    CollectTableConverter(context, attrib, tableTypeByPropName, tableGroups, token);
                }
            }

            ReportDuplicateGroups(context, dataPropGroups, DuplicateDataPropertyConverter, RedundantDataPropertyConverter, isDataProperty: true);
            ReportDuplicateGroups(context, tableGroups, DuplicateTableConverter, RedundantTableConverter, isDataProperty: false);
        }

        private static void CollectDataPropertyConverter(
              SymbolAnalysisContext context
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
            , Dictionary<string, List<ConverterEntry>> groups
            , CancellationToken token
        )
        {
            var args = attrib.ConstructorArguments;

            if (args.Length < 3
                || args[0].Value is not INamedTypeSymbol dataType
                || args[1].Value is not string propertyName
                || string.IsNullOrWhiteSpace(propertyName)
                || args[2].Value is not INamedTypeSymbol converterType
            )
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);
            ValidateConverterType(context, converterType, syntax, returnType: null);

            var location = syntax?.GetLocation() ?? Location.None;
            var converterFullName = converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var dataTypeFullName = dataType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            ReadTableScope(args, 3, out var tableName, out var tableType);

            foreach (var scopeKey in ResolveScopeKeys(tableName, tableType, tableTypeByPropName))
            {
                token.ThrowIfCancellationRequested();

                var groupKey = $"{dataTypeFullName}|{propertyName}|{scopeKey}";

                AddEntry(groups, groupKey, new ConverterEntry {
                    location = location,
                    converterTypeFullName = converterFullName,
                    descPrimary = propertyName,
                    descSecondary = dataTypeFullName,
                    descScope = ScopeLabel(scopeKey),
                });
            }
        }

        private static void CollectTableConverter(
              SymbolAnalysisContext context
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
            , Dictionary<string, List<ConverterEntry>> groups
            , CancellationToken token
        )
        {
            var args = attrib.ConstructorArguments;

            if (args.Length != 2 || args[1].Value is not INamedTypeSymbol converterType)
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);
            ValidateConverterType(context, converterType, syntax, returnType: null);

            if (TryFindConvertSourceType(converterType, out var sourceTypeFullName, token) == false)
            {
                return;
            }

            ReadTableScope(args, 0, out var tableName, out var tableType);

            if (tableName == null && tableType == null)
            {
                return;
            }

            var location = syntax?.GetLocation() ?? Location.None;
            var converterFullName = converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            foreach (var scopeKey in ResolveScopeKeys(tableName, tableType, tableTypeByPropName))
            {
                token.ThrowIfCancellationRequested();

                var groupKey = $"{scopeKey}|{sourceTypeFullName}";

                AddEntry(groups, groupKey, new ConverterEntry {
                    location = location,
                    converterTypeFullName = converterFullName,
                    descPrimary = sourceTypeFullName,
                    descSecondary = string.Empty,
                    descScope = ScopeLabel(scopeKey),
                });
            }
        }

        private static void ReportDuplicateGroups(
              SymbolAnalysisContext context
            , Dictionary<string, List<ConverterEntry>> groups
            , DiagnosticDescriptor conflictDescriptor
            , DiagnosticDescriptor redundantDescriptor
            , bool isDataProperty
        )
        {
            foreach (var pair in groups)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var entries = pair.Value;

                if (entries.Count < 2)
                {
                    continue;
                }

                var distinctConverters = new HashSet<string>(System.StringComparer.Ordinal);

                foreach (var entry in entries)
                {
                    distinctConverters.Add(entry.converterTypeFullName);
                }

                var conflict = distinctConverters.Count > 1;

                foreach (var entry in entries)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (conflict)
                    {
                        context.ReportDiagnostic(isDataProperty
                            ? Diagnostic.Create(conflictDescriptor, entry.location, entry.descPrimary, entry.descSecondary, entry.descScope)
                            : Diagnostic.Create(conflictDescriptor, entry.location, entry.descPrimary, entry.descScope)
                        );
                    }
                    else
                    {
                        context.ReportDiagnostic(isDataProperty
                            ? Diagnostic.Create(redundantDescriptor, entry.location, entry.converterTypeFullName, entry.descPrimary, entry.descSecondary, entry.descScope)
                            : Diagnostic.Create(redundantDescriptor, entry.location, entry.converterTypeFullName, entry.descPrimary, entry.descScope)
                        );
                    }
                }
            }
        }

        private static void ReadTableScope(
              ImmutableArray<TypedConstant> args
            , int index
            , out string tableName
            , out INamedTypeSymbol tableType
        )
        {
            tableName = null;
            tableType = null;

            if (args.Length <= index)
            {
                return;
            }

            if (args[index].Value is string nameValue)
            {
                tableName = nameValue;
            }
            else if (args[index].Value is INamedTypeSymbol typeValue)
            {
                tableType = typeValue;
            }
        }

        private static List<string> ResolveScopeKeys(
              string tableName
            , INamedTypeSymbol tableType
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
        )
        {
            var keys = new List<string>(1);

            if (tableName != null)
            {
                keys.Add(tableName);
                return keys;
            }

            if (tableType != null)
            {
                foreach (var pair in tableTypeByPropName)
                {
                    if (SymbolEqualityComparer.Default.Equals(pair.Value, tableType))
                    {
                        keys.Add(pair.Key);
                    }
                }

                if (keys.Count == 0)
                {
                    keys.Add(tableType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                return keys;
            }

            keys.Add("*");
            return keys;
        }

        private static string ScopeLabel(string scopeKey)
            => string.Equals(scopeKey, "*", System.StringComparison.Ordinal)
                ? "all tables"
                : $"table \"{scopeKey}\"";

        private static void AddEntry(Dictionary<string, List<ConverterEntry>> groups, string key, ConverterEntry entry)
        {
            if (groups.TryGetValue(key, out var list) == false)
            {
                groups[key] = list = new List<ConverterEntry>(1);
            }

            list.Add(entry);
        }

        private static bool TryFindConvertSourceType(
              INamedTypeSymbol converterType
            , out string sourceTypeName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method
                    && method.DeclaredAccessibility == Accessibility.Public
                    && method.IsGenericMethod == false
                    && method.Parameters.Length == 1
                    && method.ReturnsVoid == false
                )
                {
                    sourceTypeName = method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    return true;
                }
            }

            sourceTypeName = null;
            return false;
        }

        private struct ConverterEntry
        {
            public Location location;
            public string converterTypeFullName;
            public string descPrimary;
            public string descSecondary;
            public string descScope;
        }

        private static bool TryFindConvertReturnType(
              INamedTypeSymbol converterType
            , out string returnTypeName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method
                    && method.DeclaredAccessibility == Accessibility.Public
                    && method.IsGenericMethod == false
                    && method.Parameters.Length == 1
                    && method.ReturnsVoid == false
                )
                {
                    returnTypeName = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    return true;
                }
            }

            returnTypeName = null;
            return false;
        }

        private static bool ValidateConverterType(
              SymbolAnalysisContext context
            , INamedTypeSymbol converterType
            , SyntaxNode reportSyntax
            , ITypeSymbol returnType
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            var location = reportSyntax?.GetLocation() ?? Location.None;

            if (converterType.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(AbstractTypeNotSupported, location, converterType.Name));
                return false;
            }

            if (converterType.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(OpenGenericTypeNotSupported, location, converterType.Name));
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var hasPublicParameterlessCtor = false;

                foreach (var ctor in converterType.GetMembers(".ctor"))
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is IMethodSymbol m
                        && m.DeclaredAccessibility == Accessibility.Public
                        && m.Parameters.Length == 0
                    )
                    {
                        hasPublicParameterlessCtor = true;
                        break;
                    }
                }

                if (hasPublicParameterlessCtor == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingDefaultConstructor, location, converterType.Name));
                    return false;
                }
            }

            token.ThrowIfCancellationRequested();

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public
                )
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    if (multipleStatic == false)
                    {
                        if (staticMethod != null)
                        {
                            staticMethod = null;
                            multipleStatic = true;
                        }
                        else
                        {
                            staticMethod = method;
                        }
                    }
                }
                else
                {
                    if (multipleInstance == false)
                    {
                        if (instanceMethod != null)
                        {
                            instanceMethod = null;
                            multipleInstance = true;
                        }
                        else
                        {
                            instanceMethod = method;
                        }
                    }
                }
            }

            if (multipleStatic || (multipleStatic == false && multipleInstance))
            {
                var desc = multipleStatic ? StaticConvertMethodAmbiguity : InstancedConvertMethodAmbiguity;
                context.ReportDiagnostic(Diagnostic.Create(desc, location, converterType.Name));
                return false;
            }

            var convertMethod = staticMethod ?? instanceMethod;

            if (convertMethod == null)
            {
                var desc = returnType != null ? MissingConvertMethod : MissingConvertMethodReturnType;
                context.ReportDiagnostic(Diagnostic.Create(
                    desc
                    , location
                    , converterType.Name
                    , returnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty
                ));

                return false;
            }

            if (convertMethod.Parameters.Length != 1
                || convertMethod.ReturnsVoid
                || (returnType != null && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false)
            )
            {
                DiagnosticDescriptor desc;

                if (convertMethod.IsStatic)
                {
                    desc = returnType != null ? InvalidStaticConvertMethodReturnType : InvalidStaticConvertMethod;
                }
                else
                {
                    desc = returnType != null ? InvalidInstancedConvertMethodReturnType : InvalidInstancedConvertMethod;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      desc
                    , location
                    , converterType.Name
                    , returnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty
                ));

                return false;
            }

            return true;
        }
    }
}
