using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking
#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace EncosyTower.SourceGen.Analyzers.DatabaseAuthoring
{
    /// <summary>
    /// Analyzes types marked with <c>[AuthorDatabase]</c> and data types implementing
    /// <c>IData</c> for converter validation errors.
    /// <para>
    /// Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator prevents unnecessary source regeneration when only an error (and
    /// not the surrounding valid code) has changed.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DatabaseAuthoringDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        private const string DATABASES_AUTHORING_NAMESPACE = DATABASES_NAMESPACE + ".Authoring";
        private const string DATA_NAMESPACE = "EncosyTower.Data";

        private const string AUTHOR_DATABASE_ATTRIBUTE = "global::" + DATABASES_AUTHORING_NAMESPACE + ".AuthorDatabaseAttribute";
        private const string DATABASE_ATTRIBUTE        = "global::" + DATABASES_NAMESPACE + ".DatabaseAttribute";
        private const string TABLE_ATTRIBUTE           = "global::" + DATABASES_NAMESPACE + ".TableAttribute";
        private const string DATA_CONVERTER_ATTRIBUTE  = "global::" + DATA_NAMESPACE + ".DataConverterAttribute";
        private const string IDATA                     = "global::" + DATA_NAMESPACE + ".IData";

        // ── Diagnostic Descriptors ─────────────────────────────────────────────────

        public static readonly DiagnosticDescriptor MissingDefaultConstructor = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0010"
            , title: "Missing default constructor"
            , messageFormat: "The type \"{0}\" must contain a default (parameterless) constructor"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor StaticConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0020"
            , title: "Static \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public static methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InstancedConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0021"
            , title: "Instanced \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public instanced methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethod = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0030"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethodReturnType = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0031"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethodReturnType = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0040"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethodReturnType = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0041"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethod = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0042"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethod = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0043"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0050"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpressionAt = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0051"
            , title: "Not a typeof expression"
            , messageFormat: "The argument at position {0} must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor AbstractTypeNotSupported = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0060"
            , title: "Abstract type is not supported"
            , messageFormat: "The type \"{0}\" must not be abstract"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0061"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor ConverterAmbiguity = new DiagnosticDescriptor(
              id: "AUTHOR_DATABASE_0070"
            , title: "Converter ambiguity"
            , messageFormat: "The type \"{0}\" at position {3} will be ignored because a \"Convert\" method that returns a value of \"{2}\" has already been defined in \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        // ── DiagnosticAnalyzer boilerplate ─────────────────────────────────────────

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
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Validate database-level and table-level converter maps
            context.RegisterSymbolAction(AnalyzeAuthoringType, SymbolKind.NamedType);
        }

        // ── Analysis ───────────────────────────────────────────────────────────────

        private static void AnalyzeAuthoringType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol authoringSymbol)
                return;

            var authorAttrib = GetAttribute(authoringSymbol, AUTHOR_DATABASE_ATTRIBUTE);
            if (authorAttrib == null)
                return;

            if (authorAttrib.ConstructorArguments.Length != 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
            )
            {
                return;
            }

            var attribSyntax = authorAttrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);

            // Validate database-level converters
            var dbAttrib = GetAttribute(databaseSymbol, DATABASE_ATTRIBUTE);

            if (dbAttrib != null)
            {
                var dbConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in dbAttrib.ConstructorArguments)
                {
                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, dbAttrib, dbConverterMap, 0);
                        break;
                    }
                }
            }

            // Validate table-level converters
            foreach (var member in databaseSymbol.GetMembers())
            {
                if (member is not IPropertySymbol property)
                    continue;

                var tableAttrib = GetAttribute(member, TABLE_ATTRIBUTE);
                if (tableAttrib == null)
                    continue;

                var tableConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, tableAttrib, tableConverterMap, 2);
                        break;
                    }
                }

                // Validate [DataConverter] on IData members reachable from this table
                if (property.Type is INamedTypeSymbol tableType && tableType.BaseType != null)
                {
                    ValidateDataMembers(context, tableType);
                }
            }
        }

        private static void ValidateDataMembers(SymbolAnalysisContext context, INamedTypeSymbol tableType)
        {
            // Walk through type hierarchy to find IData members
            var visited = new HashSet<string>(System.StringComparer.Ordinal);
            var queue = new Queue<INamedTypeSymbol>();
            queue.Enqueue(tableType);

            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (visited.Contains(fullName))
                    continue;

                visited.Add(fullName);

                foreach (var member in type.GetMembers())
                {
                    ITypeSymbol memberType = null;

                    if (member is IPropertySymbol prop)
                        memberType = prop.Type;
                    else if (member is IFieldSymbol field)
                        memberType = field.Type;
                    else
                        continue;

                    var converterAttrib = GetAttribute(member, DATA_CONVERTER_ATTRIBUTE);

                    if (converterAttrib != null)
                    {
                        ValidateMemberConverter(context, converterAttrib, memberType);
                    }

                    // Queue nested IData types
                    if (memberType is INamedTypeSymbol namedMemberType && IsImplementingIData(namedMemberType))
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
            if (values.IsDefaultOrEmpty)
                return;

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);

            for (var i = 0; i < values.Length; i++)
            {
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
                    continue;

                // Find the return type of the Convert method to use as the map key
                if (TryFindConvertReturnType(converterType, out var returnTypeName) == false)
                    continue;

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

        private static bool TryFindConvertReturnType(INamedTypeSymbol converterType, out string returnTypeName)
        {
            foreach (var member in converterType.GetMembers("Convert"))
            {
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

        /// <returns><c>true</c> if the converter type is structurally valid; <c>false</c> if a diagnostic was reported.</returns>
        private static bool ValidateConverterType(
              SymbolAnalysisContext context
            , INamedTypeSymbol converterType
            , SyntaxNode reportSyntax
            , ITypeSymbol returnType
        )
        {
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

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
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
                        if (staticMethod != null) { staticMethod = null; multipleStatic = true; }
                        else staticMethod = method;
                    }
                }
                else
                {
                    if (multipleInstance == false)
                    {
                        if (instanceMethod != null) { instanceMethod = null; multipleInstance = true; }
                        else instanceMethod = method;
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
                context.ReportDiagnostic(Diagnostic.Create(desc, location, converterType.Name,
                      returnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty));
                return false;
            }

            if (convertMethod.Parameters.Length != 1
                || convertMethod.ReturnsVoid
                || (returnType != null && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false)
            )
            {
                DiagnosticDescriptor desc;

                if (convertMethod.IsStatic)
                    desc = returnType != null ? InvalidStaticConvertMethodReturnType : InvalidStaticConvertMethod;
                else
                    desc = returnType != null ? InvalidInstancedConvertMethodReturnType : InvalidInstancedConvertMethod;

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

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static AttributeData GetAttribute(ISymbol symbol, string fullMetadataName)
        {
            foreach (var attrib in symbol.GetAttributes())
            {
                if (attrib.AttributeClass?.HasFullName(fullMetadataName) == true)
                    return attrib;
            }

            return null;
        }

        private static bool IsImplementingIData(INamedTypeSymbol symbol)
        {
            foreach (var iface in symbol.AllInterfaces)
            {
                if (iface.HasFullName(IDATA))
                    return true;
            }

            return false;
        }
    }
}
