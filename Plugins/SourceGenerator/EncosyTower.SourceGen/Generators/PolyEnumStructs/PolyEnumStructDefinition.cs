using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    internal partial struct PolyEnumStructDefinition : IEquatable<PolyEnumStructDefinition>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public Location location;
        public InterfaceDefinition interfaceDef;
        public EquatableArray<StructDefinition> structs;
        public DefinedUndefinedStruct definedUndefinedStruct;
        public bool sortFieldsBySize;
        public bool autoEquatable;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && location != null
            ;

        public readonly bool Equals(PolyEnumStructDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && interfaceDef.Equals(other.interfaceDef)
            && structs.Equals(other.structs)
            && definedUndefinedStruct == other.definedUndefinedStruct
            && sortFieldsBySize == other.sortFieldsBySize
            && autoEquatable == other.autoEquatable
            ;

        public readonly override bool Equals(object obj)
            => obj is PolyEnumStructDefinition other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , interfaceDef
                , structs
                , definedUndefinedStruct
                , sortFieldsBySize
                , autoEquatable
            );

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

        internal struct SlimTypeDefinition : IEquatable<SlimTypeDefinition>
        {
            public string name;
            public bool isEnum;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly bool HasFullName
                => string.IsNullOrEmpty(name) == false && name.IndexOf('.', 0) > 0;

            public readonly override bool Equals(object obj)
                => obj is SlimTypeDefinition other && Equals(other);

            public readonly bool Equals(SlimTypeDefinition other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && isEnum == other.isEnum
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , isEnum
                );
        }

        internal struct InterfaceDefinition : IEquatable<InterfaceDefinition>
        {
            public string name;
            public EquatableArray<PropertyDefinition> properties;
            public EquatableArray<IndexerDefinition> indexers;
            public EquatableArray<MethodDefinition> methods;
            public bool definedInterface;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is InterfaceDefinition other && Equals(other);

            public readonly bool Equals(InterfaceDefinition other)
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

        internal struct StructDefinition : IEquatable<StructDefinition>
        {
            public string name;
            public string identifier;
            public EquatableArray<ConstructionDefinition> constructions;
            public EquatableArray<FieldDefinition> fields;
            public EquatableArray<PropertyDefinition> properties;
            public EquatableArray<IndexerDefinition> indexers;
            public EquatableArray<MethodDefinition> methods;
            public int size;
            public bool isUndefined;
            public bool implicitlyDeclared;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is StructDefinition other && Equals(other);

            public readonly bool Equals(StructDefinition other)
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && constructions.Equals(other.constructions)
                && fields.Equals(other.fields)
                && properties.Equals(other.properties)
                && indexers.Equals(other.indexers)
                && methods.Equals(other.methods)
                && size == other.size
                && isUndefined == other.isUndefined
                && implicitlyDeclared == other.implicitlyDeclared
                ;

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(name);
                hash.Add(constructions);
                hash.Add(fields);
                hash.Add(properties);
                hash.Add(indexers);
                hash.Add(methods);
                hash.Add(size);
                hash.Add(isUndefined);
                hash.Add(implicitlyDeclared);
                return hash.ToHashCode();
            }
        }

        internal struct FieldDefinition : IEquatable<FieldDefinition>
        {
            public string name;
            public SlimTypeDefinition returnType;
            public int size;
            public bool implicityDeclared;
            public bool isReadOnly;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && returnType.IsValid
                ;

            public readonly override bool Equals(object obj)
                => obj is FieldDefinition other && Equals(other);

            public readonly bool Equals(FieldDefinition other)
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

        internal struct PropertyDefinition
            : IEquatable<PropertyDefinition>
            , ICloneWithDim<PropertyDefinition>
            , ICast<PropertySignature>
        {
            public string name;
            public SlimTypeDefinition returnType;
            public RefKind refKind;
            public PropertyMethodDefinition getter;
            public PropertyMethodDefinition setter;

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

            public readonly PropertyDefinition Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is PropertyDefinition other && Equals(other);

            public readonly bool Equals(PropertyDefinition other)
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

        internal struct PropertyMethodDefinition
            : IEquatable<PropertyMethodDefinition>
            , ICloneWithDim<PropertyMethodDefinition>
        {
            public RefKind refKind;
            public bool isValid;
            public bool isGetter;
            public bool isReadOnly;
            public bool isDim;

            public readonly bool IsValid => isValid;

            public readonly bool IsDim => isDim;

            public readonly PropertyMethodDefinition Clone(bool dim)
                => this with { isDim = dim };

            public readonly override bool Equals(object obj)
                => obj is PropertyMethodDefinition other && Equals(other);

            public readonly bool Equals(PropertyMethodDefinition other)
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

        internal struct IndexerDefinition
            : IEquatable<IndexerDefinition>
            , ICloneWithDim<IndexerDefinition>
            , ICast<IndexerSignature>
        {
            public SlimTypeDefinition returnType;
            public EquatableArray<ParameterDefinition> parameters;
            public PropertyMethodDefinition getter;
            public PropertyMethodDefinition setter;
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

            public readonly IndexerDefinition Clone(bool dim)
                => this with { getter = this.getter.Clone(dim), setter = this.setter.Clone(dim), };

            public readonly override bool Equals(object obj)
                => obj is IndexerDefinition other && Equals(other);

            public readonly bool Equals(IndexerDefinition other)
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

        internal struct MethodDefinition
            : IEquatable<MethodDefinition>
            , ICloneWithDim<MethodDefinition>
            , ICast<MethodSignature>
        {
            public string name;
            public SlimTypeDefinition returnType;
            public EquatableArray<ParameterDefinition> parameters;
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

            public readonly MethodDefinition Clone(bool dim)
                => this with { isDim = dim };

            public readonly override bool Equals(object obj)
                => obj is MethodDefinition other && Equals(other);

            public readonly bool Equals(MethodDefinition other)
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

        internal struct ParameterDefinition : IEquatable<ParameterDefinition>
        {
            public string name;
            public SlimTypeDefinition type;
            public RefKind refKind;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false
                && type.IsValid
                ;

            public readonly override bool Equals(object obj)
                => obj is ParameterDefinition other && Equals(other);

            public readonly bool Equals(ParameterDefinition other)
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

        internal struct ConstructionDefinition : IEquatable<ConstructionDefinition>
        {
            public SlimTypeDefinition type;
            public string value;

            public readonly bool IsValid
                => type.IsValid
                && string.IsNullOrEmpty(value) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is ConstructionDefinition other && Equals(other);

            public readonly bool Equals(ConstructionDefinition other)
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
            public PropertyMethodDefinition getter;
            public PropertyMethodDefinition setter;

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

            public static implicit operator PropertySignature(PropertyDefinition def)
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
                => string.Equals(name, other.name, StringComparison.Ordinal)
                && refKind == other.refKind
                && getter.Equals(other.getter)
                && setter.Equals(other.setter)
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , refKind
                    , getter
                    , setter
                );
        }

        internal struct IndexerSignature
            : IEquatable<IndexerSignature>
            , ICloneWithDim<IndexerSignature>
        {
            public EquatableArray<ParameterDefinition> parameters;
            public PropertyMethodDefinition getter;
            public PropertyMethodDefinition setter;
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

            public static implicit operator IndexerSignature(IndexerDefinition def)
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
                => parameters.Equals(other.parameters)
                && getter.Equals(other.getter)
                && setter.Equals(other.setter)
                && refKind == other.refKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      parameters
                    , getter
                    , setter
                    , refKind
                );
        }

        internal struct MethodSignature
            : IEquatable<MethodSignature>
            , ICloneWithDim<MethodSignature>
        {
            public string name;
            public EquatableArray<ParameterDefinition> parameters;
            public RefKind refKind;
            public bool returnsVoid;
            public bool isReadOnly;
            public bool isDim;

            public readonly bool IsValid
                => string.IsNullOrEmpty(name) == false;

            public readonly bool IsDim => isDim;

            public static implicit operator MethodSignature(MethodDefinition def)
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
                && refKind == other.refKind
                && returnsVoid == other.returnsVoid
                && isReadOnly == other.isReadOnly
                && isDim == other.isDim
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      name
                    , parameters
                    , refKind
                    , returnsVoid
                    , isReadOnly
                    , isDim
                );
        }
    }
}
