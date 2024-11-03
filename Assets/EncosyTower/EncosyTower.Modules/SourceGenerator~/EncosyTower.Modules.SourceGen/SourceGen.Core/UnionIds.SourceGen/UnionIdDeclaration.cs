using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.EnumExtensions.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UnionIds.SourceGen
{
    internal partial class UnionIdDeclaration
    {
        private const string UNION_ID_KIND_ATTRIBUTE = "global::EncosyTower.Modules.UnionIds.UnionIdKindAttribute";

        public StructDeclarationSyntax Syntax { get; }

        public bool IsInvalid { get; }

        public UnionIdSize Size { get; }

        public bool PreserveIdKindOrder { get; }

        public string DisplayNameForId { get; }

        public string DisplayNameForKind { get; }

        public string RawTypeName { get; }

        public string IdRawTypeName { get; }

        public bool KindEnumIsEmpty { get; }

        public string KindRawTypeName { get; }

        public int KindFieldOffset { get; }

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
            PreserveIdKindOrder = idCandidate.kindSettings.HasFlag(UnionIdKindSettings.PreserveOrder);
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
                bool hasToFixedString;

                if (isEnum)
                {
                    var extensions = new EnumExtensionsDeclaration(
                          kindSymbol
                        , parentIsNamespace: false
                        , extensionsName: $"{kindName}Extensions"
                        , accessibility: Accessibility.Private
                        , references.unityCollections
                    ) {
                        GeneratedCode = GENERATED_CODE,
                        OnlyNames = true,
                        NoDocumentation = true,
                        OnlyClass = true,
                    };

                    idFixedStringBytes = Math.Max(idFixedStringBytes, extensions.FixedStringBytes);
                    idExtensionsRefs.Add(extensions);
                    hasToFixedString = true;
                }
                else
                {
                    hasToFixedString = CheckToFixedString(kindSymbol, out var byteCount);
                    idFixedStringBytes = Math.Max(idFixedStringBytes, byteCount);
                }

                kindRefs.Add(new KindRef {
                    name = kindName,
                    fullName = kindSymbol.ToFullName(),
                    displayName = candidate.displayName,
                    order = order,
                    size = size,
                    isEnum = isEnum,
                    //hasTryParse = CheckTryParse(kindSymbol),
                    hasTryParseSpan = CheckTryParseSpan(kindSymbol),
                    hasToFixedString = hasToFixedString,
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
                var typeSize = ToSize(Size, idSize);
                RawTypeName = ToRawTypeName(typeSize);
                IdRawTypeName = kindRefs.Count > 0 ? kindRefs[0].fullName : ToRawTypeName(idSize);
                KindRawTypeName = RawTypeName;
                KindFieldOffset = 0;
            }
            else
            {
                var kindSizeCandidate = ItemCountToSize(Math.Max((ulong)kindRefs.Count, maxKeyOrder));
                var typeSize = Math.Max(GetTypeSize(idSize + kindSizeCandidate, Size), 2);
                var kindSizeCapacity = typeSize - idSize;
                var kindSize = kindSizeCandidate <= kindSizeCapacity
                    ? kindSizeCandidate
                    : NormalizeSize(kindSizeCapacity);

                var removeCount = kindRefs.Count - SizeToIntCount(kindSize);

                for (; removeCount >= 0; removeCount--)
                {
                    kindRefs.RemoveAt(kindRefs.Count - 1);
                }

                RawTypeName = ToRawTypeName(typeSize);
                IdRawTypeName = ToRawTypeName(idSize);
                KindRawTypeName = ToRawTypeName(kindSize);
                KindFieldOffset = typeSize - kindSize;
            }

            var kindEnumName = $"{idSymbol.ToSimpleValidIdentifier()}_IdKind";

            KindExtensionsRef = new EnumExtensionsDeclaration(references.unityCollections, kindFixedStringBytes) {
                GeneratedCode = GENERATED_CODE,
                Name = kindEnumName,
                ExtensionsName = $"{kindEnumName}Extensions",
                ExtensionsWrapperName = $"{kindEnumName}ExtensionsWrapper",
                ParentIsNamespace = idCandidate.syntax.Parent is NamespaceDeclarationSyntax,
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

                    if (arg.Kind != TypedConstantKind.Primitive)
                    {
                        continue;
                    }

                    if (arg.Value is ulong ulongVal)
                    {
                        candidate.order = ulongVal;
                    }
                    else if (arg.Value is string stringVal)
                    {
                        candidate.displayName = stringVal;
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

        private static bool CheckTryParse(INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind == TypeKind.Enum)
            {
                return true;
            }

            var members = symbol.GetMembers("TryParse");
            var result = false;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic == false
                    || method.ReturnsVoid
                    || method.ReturnType.SpecialType != SpecialType.System_Boolean
                )
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (parameters.Length != 2)
                {
                    continue;
                }

                if (parameters[0].Type.SpecialType != SpecialType.System_String)
                {
                    continue;
                }

                var secondParam = parameters[1];

                if (secondParam.RefKind != RefKind.Out
                    || SymbolEqualityComparer.Default.Equals(secondParam.Type, symbol) == false
                )
                {
                    continue;
                }

                return true;
            }

            return result;
        }

        private static bool CheckTryParseSpan(INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind == TypeKind.Enum)
            {
                return true;
            }

            var members = symbol.GetMembers("TryParse");
            var result = false;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic == false
                    || method.ReturnsVoid
                    || method.ReturnType.SpecialType != SpecialType.System_Boolean
                )
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (parameters.Length != 2)
                {
                    continue;
                }

                if (parameters[0].Type.Is("global::System.ReadOnlySpan<char>", false) == false)
                {
                    continue;
                }

                var secondParam = parameters[1];

                if (secondParam.RefKind != RefKind.Out
                    || SymbolEqualityComparer.Default.Equals(secondParam.Type, symbol) == false
                )
                {
                    continue;
                }

                return true;
            }

            return result;
        }

        private static bool CheckToFixedString(INamedTypeSymbol symbol, out int byteCount)
        {
            var members = symbol.GetMembers("ToFixedString");

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

        private static string ToRawTypeName(int value)
            => value switch {
                <= 0 => string.Empty,
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                _ => "ulong",
            };

        private static int NormalizeSize(int value)
            => value switch {
                <= 1 => 1,
                <= 2 => 2,
                <= 4 => 4,
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
                _ => 4,
            };

        private static int ToSize(UnionIdSize value, int candidate)
            => value switch {
                UnionIdSize.UShort => 2,
                UnionIdSize.UInt => 4,
                UnionIdSize.ULong => 8,
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
            public string displayName;
            public ulong order;
            public int size;
            public bool isEnum;
            //public bool hasTryParse;
            public bool hasTryParseSpan;
            public bool hasToFixedString;
        }
    }
}
