using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    internal partial struct PolyEnumStructSpec : IEquatable<PolyEnumStructSpec>
    {
        public LocationInfo location;
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public InterfaceSpec interfaceDef;
        public EquatableArray<StructSpec> structs;
        public DefinedUndefinedStruct definedUndefinedStruct;
        public bool parentIsNamespace;
        public bool sortFieldsBySize;
        public bool autoEquatable;
        public bool withEnumExtensions;
        public bool isReadOnly;
        public bool isExplicitLayout;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            ;

        public readonly bool Equals(PolyEnumStructSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && interfaceDef.Equals(other.interfaceDef)
            && structs.Equals(other.structs)
            && definedUndefinedStruct == other.definedUndefinedStruct
            && parentIsNamespace == other.parentIsNamespace
            && sortFieldsBySize == other.sortFieldsBySize
            && autoEquatable == other.autoEquatable
            && withEnumExtensions == other.withEnumExtensions
            && isReadOnly == other.isReadOnly
            && isExplicitLayout == other.isExplicitLayout
            ;

        public readonly override bool Equals(object obj)
            => obj is PolyEnumStructSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(typeName);
            hash.Add(typeNamespace);
            hash.Add(interfaceDef);
            hash.Add(structs);
            hash.Add(definedUndefinedStruct);
            hash.Add(parentIsNamespace);
            hash.Add(sortFieldsBySize);
            hash.Add(autoEquatable);
            hash.Add(withEnumExtensions);
            hash.Add(isReadOnly);
            hash.Add(isExplicitLayout);
            return hash.ToHashCode();
        }

        internal enum DefinedUndefinedStruct
        {
            None,
            Default,
            Verbose,
        }

        internal interface IHasDim
        {
            bool IsDim { get; }
        }

        internal interface ICloneWithDim<T> : IHasDim
        {
            T Clone(bool dim);
        }

        internal interface ICast<TOut>
        {
            TOut Cast();
        }

        internal struct TypeSpec : IEquatable<TypeSpec>
        {
            public string name;
            public string identifier;
            public bool isEnum;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && string.IsNullOrEmpty(identifier) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is TypeSpec other && Equals(other);

            public readonly bool Equals(TypeSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && isEnum == other.isEnum
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(name, isEnum);
        }

        internal struct SlimMethodSpec : IEquatable<SlimMethodSpec>
        {
            public string name;
            public EquatableArray<SlimParameterSpec> parameters;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is SlimMethodSpec other && Equals(other);

            public readonly bool Equals(SlimMethodSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && parameters.Equals(other.parameters)
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(name, parameters);
        }

        internal struct InterfaceSpec : IEquatable<InterfaceSpec>
        {
            public string name;
            public EquatableArray<PropertyDeclaration> properties;
            public EquatableArray<IndexerDeclaration> indexers;
            public EquatableArray<MethodDeclaration> methods;
            public bool definedInterface;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is InterfaceSpec other && Equals(other);

            public readonly bool Equals(InterfaceSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && properties.Equals(other.properties)
                && indexers.Equals(other.indexers)
                && methods.Equals(other.methods)
                && definedInterface == other.definedInterface
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , properties
                    , indexers
                    , methods
                    , definedInterface
                );
        }

        internal struct StructId : IEquatable<StructId>
        {
            public string name;
            public string identifier;

            public readonly bool Equals(StructId other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && string.Equals(identifier, other.identifier, StringComparison.Ordinal);

            public readonly override bool Equals(object obj)
                => obj is StructId other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(name, identifier);
        }

        internal struct StructSpec : IEquatable<StructSpec>
        {
            public string name;
            public string displayName;
            public string identifier;
            public EquatableArray<ConstructionSpec> constructions;
            public EquatableArray<FieldSpec> fields;
            public EquatableArray<PropertyDeclaration> properties;
            public EquatableArray<IndexerDeclaration> indexers;
            public EquatableArray<MethodDeclaration> methods;
            public EquatableArray<ParameterSpec> parameters;
            public int size;
            public bool isUndefined;
            public bool implicitlyDeclared;
            public bool isReadOnly;
            public bool isRecord;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is StructSpec other && Equals(other);

            public readonly bool Equals(StructSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
                && constructions.Equals(other.constructions)
                && fields.Equals(other.fields)
                && properties.Equals(other.properties)
                && indexers.Equals(other.indexers)
                && methods.Equals(other.methods)
                && parameters.Equals(other.parameters)
                && size == other.size
                && isUndefined == other.isUndefined
                && implicitlyDeclared == other.implicitlyDeclared
                && isReadOnly == other.isReadOnly
                && isRecord == other.isRecord
                ;

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(name);
                hash.Add(displayName);
                hash.Add(constructions);
                hash.Add(fields);
                hash.Add(properties);
                hash.Add(indexers);
                hash.Add(methods);
                hash.Add(parameters);
                hash.Add(size);
                hash.Add(isUndefined);
                hash.Add(implicitlyDeclared);
                hash.Add(isReadOnly);
                hash.Add(isRecord);
                return hash.ToHashCode();
            }
        }

        internal struct FieldSpec : IEquatable<FieldSpec>
        {
            public string name;
            public TypeSpec returnType;
            public int size;
            public bool implicityDeclared;
            public bool isReadOnly;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && returnType.IsValid
                ;

            public readonly override bool Equals(object obj)
                => obj is FieldSpec other && Equals(other);

            public readonly bool Equals(FieldSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && returnType.Equals(other.returnType)
                && size == other.size
                && implicityDeclared == other.implicityDeclared
                && isReadOnly == other.isReadOnly
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , returnType
                    , size
                    , implicityDeclared
                    , isReadOnly
                );
        }

        internal struct PropertyDeclaration
            : IEquatable<PropertyDeclaration>
            , ICloneWithDim<PropertyDeclaration>
            , ICast<PropertySignature>
        {
            public string name;
            public TypeSpec returnType;
            public RefKind refKind;
            public PropertyMethodDeclaration getter;
            public PropertyMethodDeclaration setter;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && returnType.IsValid
                ;

            public readonly bool IsDim
                => getter.IsDim || setter.IsDim;

            public readonly bool IsReadOnly
                => getter.IsValid && setter.IsValid == false && getter.isReadOnly;

            public readonly bool IsWriteOnly
                => setter.IsValid && getter.IsValid == false;

            public readonly bool CanHaveSetter
                => IsReadOnly == false && refKind is not (RefKind.Ref or RefKind.RefReadOnly);

            public readonly PropertySignature Cast()
                => this;

            public readonly PropertyDeclaration Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is PropertyDeclaration other && Equals(other);

            public readonly bool Equals(PropertyDeclaration other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && returnType.Equals(other.returnType)
                && refKind == other.refKind
                && getter.Equals(other.getter)
                && setter.Equals(other.setter)
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , returnType
                    , refKind
                    , getter
                    , setter
                );
        }

        internal struct PropertyMethodDeclaration
            : IEquatable<PropertyMethodDeclaration>
            , ICloneWithDim<PropertyMethodDeclaration>
        {
            public RefKind refKind;
            public bool isValid;
            public bool isGetter;
            public bool isReadOnly;
            public bool isDim;

            public readonly bool IsValid => isValid;

            public readonly bool IsDim => isDim;

            public readonly PropertyMethodDeclaration Clone(bool dim)
                => this with { isDim = dim };

            public readonly override bool Equals(object obj)
                => obj is PropertyMethodDeclaration other && Equals(other);

            public readonly bool Equals(PropertyMethodDeclaration other)
                => refKind == other.refKind
                && isValid == other.isValid
                && isGetter == other.isGetter
                && isReadOnly == other.isReadOnly
                && isDim == other.isDim
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      refKind
                    , isValid
                    , isGetter
                    , isReadOnly
                    , isDim
                );
        }

        internal struct IndexerDeclaration
            : IEquatable<IndexerDeclaration>
            , ICloneWithDim<IndexerDeclaration>
            , ICast<IndexerSignature>
        {
            public TypeSpec returnType;
            public EquatableArray<SlimParameterSpec> parameters;
            public PropertyMethodDeclaration getter;
            public PropertyMethodDeclaration setter;
            public RefKind refKind;

            public readonly bool IsValid
                => returnType.IsValid
                && parameters.IsEmpty == false
                ;

            public readonly bool IsDim
                => getter.IsDim || setter.IsDim;

            public readonly bool IsReadOnly
                => getter.IsValid && setter.IsValid == false && getter.isReadOnly;

            public readonly bool IsWriteOnly
                => setter.IsValid && getter.IsValid == false;

            public readonly bool CanHaveSetter
                => IsReadOnly == false && refKind is not (RefKind.Ref or RefKind.RefReadOnly);

            public readonly IndexerSignature Cast()
                => this;

            public readonly IndexerDeclaration Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is IndexerDeclaration other && Equals(other);

            public readonly bool Equals(IndexerDeclaration other)
                => returnType.Equals(other.returnType)
                && parameters.Equals(other.parameters)
                && getter.Equals(other.getter)
                && setter.Equals(other.setter)
                && refKind == other.refKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      returnType
                    , parameters
                    , getter
                    , setter
                    , refKind
                );
        }

        internal struct MethodDeclaration
            : IEquatable<MethodDeclaration>
            , ICloneWithDim<MethodDeclaration>
            , ICast<MethodSignature>
        {
            public string name;
            public TypeSpec returnType;
            public EquatableArray<SlimParameterSpec> parameters;
            public RefKind refKind;
            public bool returnsVoid;
            public bool isReadOnly;
            public bool isDim;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && returnType.IsValid
                ;

            public readonly bool IsDim => isDim;

            public readonly MethodSignature Cast()
                => this;

            public readonly MethodDeclaration Clone(bool dim)
                => this with { isDim = dim };

            public readonly override bool Equals(object obj)
                => obj is MethodDeclaration other && Equals(other);

            public readonly bool Equals(MethodDeclaration other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && returnType.Equals(other.returnType)
                && parameters.Equals(other.parameters)
                && refKind == other.refKind
                && returnsVoid == other.returnsVoid
                && isReadOnly == other.isReadOnly
                && isDim == other.isDim
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , returnType
                    , parameters
                    , refKind
                    , returnsVoid
                    , isReadOnly
                    , isDim
                );
        }

        internal struct ParameterSpec : IEquatable<ParameterSpec>
        {
            public FieldSpec field;
            public RefKind refKind;

            public readonly bool IsValid
                => field.IsValid;

            public readonly override bool Equals(object obj)
                => obj is ParameterSpec other && Equals(other);

            public readonly bool Equals(ParameterSpec other)
                => field.Equals(other.field)
                && refKind == other.refKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(field, refKind);
        }

        internal struct SlimParameterSpec : IEquatable<SlimParameterSpec>
        {
            public string name;
            public TypeSpec type;
            public RefKind refKind;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && type.IsValid
                ;

            public readonly override bool Equals(object obj)
                => obj is SlimParameterSpec other && Equals(other);

            public readonly bool Equals(SlimParameterSpec other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && type.Equals(other.type)
                && refKind == other.refKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , type
                    , refKind
                );
        }

        internal struct ConstructionSpec : IEquatable<ConstructionSpec>
        {
            public TypeSpec type;
            public string value;

            public readonly bool IsValid
                => type.IsValid
                && string.IsNullOrEmpty(value) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is ConstructionSpec other && Equals(other);

            public readonly bool Equals(ConstructionSpec other)
                => type.Equals(other.type)
                && string.Equals(value, other.value, StringComparison.Ordinal)
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      type
                    , value
                );
        }

        internal struct ConstructionValue : IEquatable<ConstructionValue>
        {
            public string value;

            public static implicit operator ConstructionValue(string value)
                => new() { value = value };

            public static implicit operator string(ConstructionValue value)
                => value.value;

            public readonly bool Equals(ConstructionValue other)
                => string.Equals(value, other.value, StringComparison.Ordinal);

            public readonly override bool Equals(object obj)
                => obj is ConstructionValue other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(value);

            public readonly override string ToString()
                => value;
        }

        internal struct PropertySignature
            : IEquatable<PropertySignature>
            , ICloneWithDim<PropertySignature>
        {
            public string name;
            public RefKind refKind;
            public PropertyMethodDeclaration getter;
            public PropertyMethodDeclaration setter;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false;

            public readonly bool IsDim
                => getter.IsDim || setter.IsDim;

            public readonly bool IsReadOnly
                => getter.IsValid && setter.IsValid == false && getter.isReadOnly;

            public readonly bool IsWriteOnly
                => setter.IsValid && getter.IsValid == false;

            public readonly bool CanHaveSetter
                => IsReadOnly == false && refKind is not (RefKind.Ref or RefKind.RefReadOnly);

            public static implicit operator PropertySignature(PropertyDeclaration def)
                => new() {
                    name = def.name,
                    refKind = def.refKind,
                    getter = def.getter,
                    setter = def.setter,
                };

            public readonly PropertySignature Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is PropertySignature other && Equals(other);

            public readonly bool Equals(PropertySignature other)
                => string.Equals(name, other.name, StringComparison.Ordinal);

            public readonly override int GetHashCode()
                => HashValue.Combine(name);
        }

        internal struct IndexerSignature
            : IEquatable<IndexerSignature>
            , ICloneWithDim<IndexerSignature>
        {
            public EquatableArray<SlimParameterSpec> parameters;
            public PropertyMethodDeclaration getter;
            public PropertyMethodDeclaration setter;
            public RefKind refKind;

            public readonly bool IsValid
                => parameters.IsEmpty == false;

            public readonly bool IsDim
                => getter.IsDim || setter.IsDim;

            public readonly bool IsReadOnly
                => getter.IsValid && setter.IsValid == false && getter.isReadOnly;

            public readonly bool IsWriteOnly
                => setter.IsValid && getter.IsValid == false;

            public readonly bool CanHaveSetter
                => IsReadOnly == false && refKind is not (RefKind.Ref or RefKind.RefReadOnly);

            public static implicit operator IndexerSignature(IndexerDeclaration def)
                => new() {
                    parameters = def.parameters,
                    getter = def.getter,
                    setter = def.setter,
                    refKind = def.refKind,
                };

            public readonly IndexerSignature Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is IndexerSignature other && Equals(other);

            public readonly bool Equals(IndexerSignature other)
                => parameters.Equals(other.parameters);

            public readonly override int GetHashCode()
                => HashValue.Combine(parameters);
        }

        internal struct MethodSignature
            : IEquatable<MethodSignature>
            , ICloneWithDim<MethodSignature>
        {
            public string name;
            public EquatableArray<SlimParameterSpec> parameters;
            public RefKind refKind;
            public bool returnsVoid;
            public bool isReadOnly;
            public bool isDim;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false;

            public readonly bool IsDim => isDim;

            public static implicit operator MethodSignature(MethodDeclaration def)
                => new() {
                    name = def.name,
                    parameters = def.parameters,
                    refKind = def.refKind,
                    returnsVoid = def.returnsVoid,
                    isReadOnly = def.isReadOnly,
                    isDim = def.isDim,
                };

            public readonly MethodSignature Clone(bool dim)
                => this with { isDim = dim };

            public readonly override bool Equals(object obj)
                => obj is MethodSignature other && Equals(other);

            public readonly bool Equals(MethodSignature other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && parameters.Equals(other.parameters)
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(name, parameters);
        }
    }
}
