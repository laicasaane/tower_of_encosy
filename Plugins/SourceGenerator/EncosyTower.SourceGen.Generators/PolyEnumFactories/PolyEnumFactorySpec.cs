using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyEnumFactories
{
    internal partial struct PolyEnumFactorySpec : IEquatable<PolyEnumFactorySpec>
    {
        public LocationInfo location;
        public string wrapperTypeName;
        public string wrapperTypeNamespace;
        public string wrapperKindKeyword;
        public string wrapperPreModifiers;
        public string wrapperAccessibility;
        public string enumStructTypeName;
        public string enumStructNamespace;
        public string fieldName;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public EquatableArray<CaseSpec> cases;
        public int enumStructSize;
        public bool enumStructIsReadOnly;
        public bool emitBackingField;
        public bool emitExplicitUndefinedMethod;
        public bool parentIsNamespace;
        public bool isStruct;

        public readonly bool IsValid
            => string.IsNullOrEmpty(wrapperTypeName) == false
            && string.IsNullOrEmpty(wrapperKindKeyword) == false
            && string.IsNullOrEmpty(enumStructTypeName) == false
            && string.IsNullOrEmpty(fieldName) == false
            ;

        public readonly bool Equals(PolyEnumFactorySpec other)
            => string.Equals(wrapperTypeName, other.wrapperTypeName, StringComparison.Ordinal)
            && string.Equals(wrapperTypeNamespace, other.wrapperTypeNamespace, StringComparison.Ordinal)
            && string.Equals(wrapperKindKeyword, other.wrapperKindKeyword, StringComparison.Ordinal)
            && string.Equals(wrapperPreModifiers, other.wrapperPreModifiers, StringComparison.Ordinal)
            && string.Equals(wrapperAccessibility, other.wrapperAccessibility, StringComparison.Ordinal)
            && string.Equals(enumStructTypeName, other.enumStructTypeName, StringComparison.Ordinal)
            && string.Equals(enumStructNamespace, other.enumStructNamespace, StringComparison.Ordinal)
            && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
            && cases.Equals(other.cases)
            && enumStructSize == other.enumStructSize
            && enumStructIsReadOnly == other.enumStructIsReadOnly
            && emitBackingField == other.emitBackingField
            && emitExplicitUndefinedMethod == other.emitExplicitUndefinedMethod
            && parentIsNamespace == other.parentIsNamespace
            && isStruct == other.isStruct
            ;

        public readonly override bool Equals(object obj)
            => obj is PolyEnumFactorySpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(wrapperTypeName);
            hash.Add(wrapperTypeNamespace);
            hash.Add(wrapperKindKeyword);
            hash.Add(wrapperPreModifiers);
            hash.Add(wrapperAccessibility);
            hash.Add(enumStructTypeName);
            hash.Add(enumStructNamespace);
            hash.Add(fieldName);
            hash.Add(cases);
            hash.Add(enumStructSize);
            hash.Add(enumStructIsReadOnly);
            hash.Add(emitBackingField);
            hash.Add(emitExplicitUndefinedMethod);
            hash.Add(parentIsNamespace);
            hash.Add(isStruct);
            return hash.ToHashCode();
        }

        internal enum ConstructionStrategy
        {
            Default,
            Ctors,
            MemberInit,
        }

        internal struct CaseSpec : IEquatable<CaseSpec>
        {
            public string name;
            public string identifier;
            public string qualifiedName;
            public EquatableArray<CtorSpec> ctors;
            public EquatableArray<MemberSpec> initMembers;
            public ConstructionStrategy strategy;
            public int size;
            public bool isUndefined;
            public bool isReadOnly;
            public bool emitMemberInitOverload;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && string.IsNullOrEmpty(qualifiedName) == false
                ;

            public readonly bool Equals(CaseSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && string.Equals(identifier, other.identifier, StringComparison.Ordinal)
                && string.Equals(qualifiedName, other.qualifiedName, StringComparison.Ordinal)
                && ctors.Equals(other.ctors)
                && initMembers.Equals(other.initMembers)
                && strategy == other.strategy
                && size == other.size
                && isUndefined == other.isUndefined
                && isReadOnly == other.isReadOnly
                && emitMemberInitOverload == other.emitMemberInitOverload
                ;

            public readonly override bool Equals(object obj)
                => obj is CaseSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(name);
                hash.Add(identifier);
                hash.Add(qualifiedName);
                hash.Add(ctors);
                hash.Add(initMembers);
                hash.Add(strategy);
                hash.Add(size);
                hash.Add(isUndefined);
                hash.Add(isReadOnly);
                hash.Add(emitMemberInitOverload);
                return hash.ToHashCode();
            }
        }

        internal struct CtorSpec : IEquatable<CtorSpec>
        {
            public EquatableArray<ParamSpec> parameters;
            public bool isParameterless;

            public readonly bool Equals(CtorSpec other)
                => parameters.Equals(other.parameters)
                && isParameterless == other.isParameterless
                ;

            public readonly override bool Equals(object obj)
                => obj is CtorSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(parameters);
                hash.Add(isParameterless);
                return hash.ToHashCode();
            }
        }

        internal struct ParamSpec : IEquatable<ParamSpec>
        {
            public string name;
            public string typeFullyQualifiedName;
            public string defaultValueLiteral;
            public RefKind refKind;
            public bool isParams;
            public bool hasExplicitDefaultValue;

            public readonly bool Equals(ParamSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && string.Equals(typeFullyQualifiedName, other.typeFullyQualifiedName, StringComparison.Ordinal)
                && string.Equals(defaultValueLiteral, other.defaultValueLiteral, StringComparison.Ordinal)
                && refKind == other.refKind
                && isParams == other.isParams
                && hasExplicitDefaultValue == other.hasExplicitDefaultValue
                ;

            public readonly override bool Equals(object obj)
                => obj is ParamSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(name);
                hash.Add(typeFullyQualifiedName);
                hash.Add(defaultValueLiteral);
                hash.Add(refKind);
                hash.Add(isParams);
                hash.Add(hasExplicitDefaultValue);
                return hash.ToHashCode();
            }
        }

        internal struct MemberSpec : IEquatable<MemberSpec>
        {
            public string name;
            public string parameterName;
            public string typeFullyQualifiedName;
            public bool isProperty;

            public readonly bool Equals(MemberSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && string.Equals(parameterName, other.parameterName, StringComparison.Ordinal)
                && string.Equals(typeFullyQualifiedName, other.typeFullyQualifiedName, StringComparison.Ordinal)
                && isProperty == other.isProperty
                ;

            public readonly override bool Equals(object obj)
                => obj is MemberSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(name);
                hash.Add(parameterName);
                hash.Add(typeFullyQualifiedName);
                hash.Add(isProperty);
                return hash.ToHashCode();
            }
        }
    }
}
