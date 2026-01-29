using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.SourceGen.Generators.EnumExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    internal partial class UnionIdDeclaration
    {
        private const string UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdAttribute";
        private const string UNION_ID_KIND_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdKindAttribute";
        private const string ENUM_EXTENSIONS_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumExtensionsAttribute";

        public StructDeclarationSyntax Syntax { get; }

        public bool IsInvalid { get; }

        public UnionIdSize Size { get; }

        public bool PreserveIdKindOrder { get; }

        public string DisplayNameForId { get; }

        public string DisplayNameForKind { get; }

        public string RawTypeName { get; }

        public string IdRawUnsignedTypeName { get; }

        public string IdRawSignedTypeName { get; }

        public bool KindEnumIsEmpty { get; }

        public string KindRawTypeName { get; }

        public char Separator { get; }

        public int TypeSize { get; }

        public int KindFieldOffset { get; }

        public ParsableStructConverterSettings ConverterSettings { get; }

        public List<KindRef> KindRefs { get; }

        public References References { get; }

        public EnumExtensionsDeclaration KindExtensionsRef { get; }

        public List<EnumExtensionsDeclaration> IdEnumExtensionsRefs { get; }

        public string FixedStringType { get; }

        public UnionIdDeclaration(
              SourceProductionContext context
            , IdCandidate idCandidate
            , ImmutableArray<KindCandidate> kindCandidates
            , References references
        )
        {
            Syntax = idCandidate.syntax;
            Size = idCandidate.size;
            Separator = idCandidate.separator;
            PreserveIdKindOrder = idCandidate.kindSettings.HasFlag(UnionIdKindSettings.PreserveOrder);
            ConverterSettings = idCandidate.converterSettings;
            DisplayNameForId = string.IsNullOrWhiteSpace(idCandidate.displayNameForId) ? string.Empty : idCandidate.displayNameForId;
            DisplayNameForKind = string.IsNullOrWhiteSpace(idCandidate.displayNameForKind) ? string.Empty : idCandidate.displayNameForKind;
            References = references;

            var idSymbol = idCandidate.symbol;
            var candidates = new List<KindCandidate>(kindCandidates);
            TryGetKinds(context, idSymbol, candidates);

            var comparer = SymbolEqualityComparer.Default;
            var kindRefs = KindRefs = new(candidates.Count);
            var enumMembers = new List<EnumMemberDeclaration>(candidates.Count);
            var kinds = new HashSet<INamedTypeSymbol>(comparer);
            var uniqueKinds = new Dictionary<string, INamedTypeSymbol>(candidates.Count);
            var idExtensionsRefs = IdEnumExtensionsRefs = new List<EnumExtensionsDeclaration>(candidates.Count);
            var removeSuffix = idCandidate.kindSettings.HasFlag(UnionIdKindSettings.RemoveSuffix);
            var maxIdSize = 0;
            var kindFixedStringBytes = 0;
            var idFixedStringBytes = 0;
            var kindHasDisplayName = false;
            ulong maxKeyOrder = 0;

            foreach (var candidate in candidates)
            {
                if (comparer.Equals(idSymbol, candidate.idSymbol) == false)
                {
                    continue;
                }

                var kindSymbol = candidate.kindSymbol;

                if (comparer.Equals(kindSymbol, idSymbol))
                {
                    context.ReportDiagnostic(
                          KindTypeCannotBeIdType
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , idSymbol.ToSimpleName()
                    );
                    continue;
                }

                if (kinds.Contains(kindSymbol))
                {
                    context.ReportDiagnostic(
                          TypeAlreadyDeclared
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , kindSymbol.ToSimpleName()
                    );
                    continue;
                }

                var kindName = kindSymbol.Name;

                if (uniqueKinds.TryGetValue(kindName, out var otherKind))
                {
                    context.ReportDiagnostic(
                          SameKindIsIgnored
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , kindName
                        , otherKind.ToSimpleName()
                    );
                    continue;
                }

                kinds.Add(kindSymbol);
                uniqueKinds.Add(kindName, kindSymbol);

                if (removeSuffix)
                {
                    kindName = RemoveTypeKindSuffix(kindName);
                }

                var order = candidate.order;

                var size = 0;
                kindSymbol.GetUnmanagedSize(ref size);

                var nameByteCount = kindName.GetByteCount();
                kindFixedStringBytes = Math.Max(kindFixedStringBytes, nameByteCount);

                var isEnum = kindSymbol.TypeKind == TypeKind.Enum;
                var toStringMethods = candidate.toStringMethods;
                var enumExtensionsName = string.Empty;
                var nestedEnumExtensions = false;
                var tryParseSpan = default(MemberExistence);
                var equality = default(Equality);

                if (isEnum)
                {
                    var extensions = new EnumExtensionsDeclaration(
                          kindSymbol
                        , parentIsNamespace: false
                        , extensionsName: EnumExtensionsDeclaration.GetNameExtensionsClass(kindName)
                        , accessibility: Accessibility.Private
                        , references.unityCollections
                    ) {
                        GeneratedCode = GENERATED_CODE,
                        OnlyNames = true,
                        NoDocumentation = true,
                        OnlyClass = true,
                    };

                    if (kindSymbol.TryGetAttribute(ENUM_EXTENSIONS_ATTRIBUTE, out _))
                    {
                        enumExtensionsName = $"{kindSymbol.ToFullName()}Extensions";
                    }
                    else
                    {
                        enumExtensionsName = extensions.ExtensionsName;
                        idExtensionsRefs.Add(extensions);
                        nestedEnumExtensions = true;
                    }

                    idFixedStringBytes = Math.Max(idFixedStringBytes, extensions.FixedStringBytes);
                    toStringMethods |= ToStringMethods.ToDisplayString;

                    if (references.unityCollections)
                    {
                        toStringMethods |= ToStringMethods.ToFixedString | ToStringMethods.ToDisplayString;
                    }
                }
                else if (kindSymbol.HasAttribute(UNION_ID_ATTRIBUTE))
                {
                    tryParseSpan = new MemberExistence(true, false, false, 4);
                    equality = new Equality(EqualityStrategy.Equals, false, false);
                    toStringMethods |= ToStringMethods.All;
                    idFixedStringBytes = 128;
                }
                else
                {
                    tryParseSpan = candidate.tryParseSpan switch {
                        TryParseMethodType.Instance => new MemberExistence(true, false, false, 4),
                        TryParseMethodType.Static => new MemberExistence(true, true, false, 4),
                        _ => kindSymbol.FindTryParseSpan().DefaultIfNullableIs(true),
                    };

                    equality = kindSymbol.DetermineEquality();

                    if (toStringMethods.HasFlag(ToStringMethods.ToDisplayString) == false
                        && CheckToDisplayFixedString(kindSymbol)
                    )
                    {
                        toStringMethods |= ToStringMethods.ToDisplayString;
                    }

                    if (references.unityCollections)
                    {
                        var byteCount = 128;
                        var displayByteCount = 128;

                        if (toStringMethods.HasFlag(ToStringMethods.ToFixedString) == false
                            && CheckToFixedString(kindSymbol, "ToFixedString", out byteCount)
                        )
                        {
                            toStringMethods |= ToStringMethods.ToFixedString;
                        }

                        if (toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString) == false
                            && CheckToFixedString(kindSymbol, "ToDisplayFixedString", out displayByteCount)
                        )
                        {
                            toStringMethods |= ToStringMethods.ToDisplayFixedString;
                        }

                        idFixedStringBytes = Math.Max(Math.Max(idFixedStringBytes, displayByteCount), byteCount);
                    }
                }

                kindRefs.Add(new KindRef {
                    name = kindName,
                    fullName = kindSymbol.ToFullName(),
                    fullNameFromNullable = equality.IsNullable ? kindSymbol.GetTypeFromNullable().ToFullName() : "",
                    displayName = candidate.displayName,
                    enumExtensionsName = enumExtensionsName,
                    order = order,
                    size = size,
                    isEnum = isEnum,
                    signed = candidate.signed,
                    toStringMethods = toStringMethods,
                    tryParseSpan = tryParseSpan,
                    equality = equality,
                    nestedEnumExtensions = nestedEnumExtensions,
                });

                enumMembers.Add(new EnumMemberDeclaration {
                    name = kindName,
                    order = order,
                    displayName = candidate.displayName,
                });

                if (string.IsNullOrWhiteSpace(candidate.displayName) == false)
                {
                    kindHasDisplayName = true;
                }

                if (size > maxIdSize)
                {
                    maxIdSize = size;
                }

                if (order > maxKeyOrder)
                {
                    maxKeyOrder = order;
                }

                if (maxIdSize >= (int)UnionIdSize.ULong)
                {
                    context.ReportDiagnostic(
                          KindSizeMustBeSmallerThan7Bytes
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , kindName
                    );

                    IsInvalid = true;
                    return;
                }
            }

            kindRefs.Sort(Compare);

            var allowEmptyKind = idCandidate.kindSettings.HasFlag(UnionIdKindSettings.AllowEmpty);
            KindEnumIsEmpty = allowEmptyKind && kindRefs.Count <= 1;

            var idSize = NormalizeSize(maxIdSize);

            if (KindEnumIsEmpty)
            {
                var typeSize = TypeSize = ToSize(Size, idSize);
                RawTypeName = ToUnsignedTypeName(typeSize);
                IdRawUnsignedTypeName = kindRefs.Count > 0 ? kindRefs[0].fullName : ToUnsignedTypeName(idSize);
                IdRawSignedTypeName = kindRefs.Count > 0 ? kindRefs[0].fullName : ToSignedTypeName(idSize);
                KindRawTypeName = RawTypeName;
                KindFieldOffset = 0;
            }
            else
            {
                var kindSizeCandidate = ItemCountToSize(Math.Max((ulong)kindRefs.Count, maxKeyOrder));
                var typeSize = TypeSize = Math.Max(GetTypeSize(idSize + kindSizeCandidate, Size), 2);
                var kindSizeCapacity = typeSize - idSize;
                var kindSize = kindSizeCandidate <= kindSizeCapacity
                    ? kindSizeCandidate
                    : NormalizeSize(kindSizeCapacity);

                var removeCount = kindRefs.Count - SizeToIntCount(kindSize);

                for (; removeCount >= 0; removeCount--)
                {
                    kindRefs.RemoveAt(kindRefs.Count - 1);
                }

                RawTypeName = ToUnsignedTypeName(typeSize);
                IdRawUnsignedTypeName = ToUnsignedTypeName(idSize);
                IdRawSignedTypeName = ToSignedTypeName(idSize);
                KindRawTypeName = ToUnsignedTypeName(kindSize);
                KindFieldOffset = typeSize - kindSize;
            }

            var kindEnumName = $"{idSymbol.ToSimpleValidIdentifier()}_IdKind";

            KindExtensionsRef = new EnumExtensionsDeclaration(references.unityCollections, kindFixedStringBytes) {
                GeneratedCode = GENERATED_CODE,
                Name = kindEnumName,
                ExtensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(kindEnumName),
                StructName = EnumExtensionsDeclaration.GetNameExtendedStruct(kindEnumName),
                ParentIsNamespace = idCandidate.syntax.Parent is BaseNamespaceDeclarationSyntax,
                FullyQualifiedName = $"{idSymbol.ToFullName()}.IdKind",
                UnderlyingTypeName = KindRawTypeName,
                Members = enumMembers,
                Accessibility = idSymbol.DeclaredAccessibility,
                IsDisplayAttributeUsed = kindHasDisplayName,
            };

            FixedStringType = GeneratorHelpers.GetFixedStringFullyQualifiedTypeName(kindFixedStringBytes + idFixedStringBytes);
        }

        private static void TryGetKinds(
              SourceProductionContext context
            , INamedTypeSymbol symbol
            , List<KindCandidate> output
        )
        {
            var attributes = symbol.GetAttributes(UNION_ID_KIND_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                if (attrib == null)
                {
                    continue;
                }

                var args = attrib.ConstructorArguments;

                if (args.Length < 1)
                {
                    continue;
                }

                var typeArg = args[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol kindSymbol
                    || kindSymbol.IsUnmanagedType == false
                )
                {
                    context.ReportDiagnostic(
                          MustBeUnmanagedType
                        , attrib.ApplicationSyntaxReference.GetSyntax()
                    );
                    continue;
                }

                var candidate = new KindCandidate {
                    kindSymbol = kindSymbol,
                    idSymbol = symbol,
                    attributeData = attrib,
                };

                for (var i = 1; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (i == 1 && arg.Value is ulong ulongVal1)
                    {
                        candidate.order = ulongVal1;
                    }
                    else if (i == 2 && arg.Value is string stringVal1)
                    {
                        candidate.displayName = stringVal1;
                    }
                    else if (i == 3 && arg.Value is bool boolVal1)
                    {
                        candidate.signed = boolVal1;
                    }
                    else if (i == 4 && arg.Value is byte byteVal1)
                    {
                        candidate.toStringMethods = (ToStringMethods)byteVal1;
                    }
                    else if (i == 5 && arg.Value is byte byteVal2)
                    {
                        candidate.tryParseSpan = (TryParseMethodType)byteVal2;
                    }
                }

                output.Add(candidate);
            }
        }

        private static string RemoveTypeKindSuffix(string name)
        {
            while (name.Length > 4 && name.EndsWith("Type") || name.EndsWith("Kind"))
            {
                name = name.Remove(name.Length - 4, 4);
            }

            return name;
        }

        private static bool CheckToFixedString(INamedTypeSymbol symbol, string name, out int byteCount)
        {
            var members = symbol.GetMembers(name);

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic == true
                    || method.Parameters.Length != 0
                    || method.ReturnsVoid
                )
                {
                    continue;
                }

                var returnTypeName = method.ReturnType.ToFullName();

                if (returnTypeName.StartsWith("global::Unity.Collections.FixedString", StringComparison.Ordinal) == false)
                {
                    continue;
                }

                var indexOfBytes = returnTypeName.IndexOf("Bytes", 36);

                if (indexOfBytes < 39)
                {
                    continue;
                }

                if (int.TryParse(returnTypeName.Substring(37, indexOfBytes - 37), out byteCount) == false)
                {
                    continue;
                }

                return true;
            }

            byteCount = 0;
            return false;
        }

        private static bool CheckToDisplayFixedString(INamedTypeSymbol symbol)
        {
            var members = symbol.GetMembers("ToDisplayString");

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic == true
                    || method.Parameters.Length != 0
                    || method.ReturnType.SpecialType != SpecialType.System_String
                )
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static string ToUnsignedTypeName(int value)
            => value switch {
                <= 0 => string.Empty,
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                <= 8 => "ulong",
                <= 12 => "global::EncosyTower.UnionIds.Types.UnionId_UInt3",
                <= 16 => "global::EncosyTower.UnionIds.Types.UnionId_ULong2",
                _ => "ulong",
            };

        private static string ToSignedTypeName(int value)
            => value switch {
                <= 0 => string.Empty,
                <= 1 => "sbyte",
                <= 2 => "short",
                <= 4 => "int",
                <= 8 => "ulong",
                <= 12 => "global::EncosyTower.UnionIds.Types.UnionId_UInt3",
                <= 16 => "global::EncosyTower.UnionIds.Types.UnionId_ULong2",
                _ => "long",
            };

        private static int NormalizeSize(int value)
            => value switch {
                <= 1 => 1,
                <= 2 => 2,
                <= 4 => 4,
                <= 8 => 8,
                <= 12 => 12,
                <= 16 => 16,
                _ => 8,
            };

        private static int ItemCountToSize(ulong itemCount)
            => itemCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                _ => 4,
            };

        private static int SizeToIntCount(int size)
            => size switch {
                <= 1 => byte.MaxValue,
                <= 2 => ushort.MaxValue,
                _ => int.MaxValue,
            };

        private static int ToSize(UnionIdSize value)
            => value switch {
                UnionIdSize.UShort => 2,
                UnionIdSize.UInt => 4,
                UnionIdSize.ULong => 8,
                UnionIdSize.UInt3 => 12,
                UnionIdSize.ULong2 => 16,
                _ => 4,
            };

        private static int ToSize(UnionIdSize value, int candidate)
            => value switch {
                UnionIdSize.UShort => 2,
                UnionIdSize.UInt => 4,
                UnionIdSize.ULong => 8,
                UnionIdSize.UInt3 => 12,
                UnionIdSize.ULong2 => 16,
                _ => NormalizeSize(candidate),
            };

        private static int GetTypeSize(int candidateSize, UnionIdSize inputSize)
        {
            var sizeCandidate = NormalizeSize(candidateSize);
            var sizeInput = ToSize(inputSize);
            return sizeCandidate > sizeInput ? sizeCandidate : sizeInput;
        }

        private static int Compare(KindRef a, KindRef b)
        {
            var comp = a.order.CompareTo(b.order);
            return comp != 0 ? comp : string.CompareOrdinal(a.name, b.name);
        }

        public static readonly DiagnosticDescriptor SameKindIsIgnored = new(
              id: "UNION_ID_0001"
            , title: "A kind of the same name will be ignored"
            , messageFormat: "Kind \"{0}\" has already been defined by another type \"{1}\""
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A kind of the same name will be ignored"
        );

        public static readonly DiagnosticDescriptor MustBeUnmanagedType = new(
              id: "UNION_ID_0002"
            , title: "First parameter must be an unmanaged type"
            , messageFormat: "First parameter must be an unmanaged type"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "First parameter must be an unmanaged type"
        );

        public static readonly DiagnosticDescriptor KindTypeCannotBeIdType = new(
              id: "UNION_ID_0003"
            , title: "Cannot use the id type as a kind of itself"
            , messageFormat: "Cannot use \"{0}\" as a kind of itself"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Cannot use the id type as a kind of itself"
        );

        public static readonly DiagnosticDescriptor TypeAlreadyDeclared = new(
              id: "UNION_ID_0004"
            , title: "Type has already be declared"
            , messageFormat: "Type \"{0}\" has already be declared"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type has already be declared"
        );

        public static readonly DiagnosticDescriptor KindSizeMustBeSmallerThan7Bytes = new(
              id: "UNION_ID_0005"
            , title: "The size of a type is larger than 7 bytes"
            , messageFormat: "The size of \"{0}\" is larger than 7 bytes"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The size of a type must be smaller than 8 bytes"
        );

        public struct KindRef
        {
            public string name;
            public string fullName;
            public string fullNameFromNullable;
            public string displayName;
            public string enumExtensionsName;
            public ulong order;
            public int size;
            public bool isEnum;
            public bool signed;
            public ToStringMethods toStringMethods;
            public MemberExistence tryParseSpan;
            public Equality equality;
            public bool nestedEnumExtensions;
        }
    }
}
