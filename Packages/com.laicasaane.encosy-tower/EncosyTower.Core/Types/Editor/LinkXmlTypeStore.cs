#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Core;

namespace EncosyTower.Types.Editor
{
    [ApiForEditor]
    internal class IndexedType : IEquatable<IndexedType>
    {
        public int index;

        public Type Type { get; init; }

        [ApiForEditor]
        public IndexedType(Type type)
        {
            Type = type;
        }

        public override int GetHashCode()
            => Type.GetHashCode();

        public override bool Equals(object obj)
            => obj is IndexedType other && Type == other.Type;

        public bool Equals(IndexedType other)
            => ReferenceEquals(Type, other?.Type);
    }

    [ApiForEditor]
    internal class LinkXmlTypeStore
    {
        public readonly Dictionary<Assembly, Dictionary<IndexedType, HashSet<MemberInfo>>> Store = new();

        [ApiForEditor]
        public LinkXmlTypeStore() { }

        public void Add(IndexedType type)
        {
            var assembly = type.Type.Assembly;

            if (Store.TryGetValue(assembly, out var typeToMemberMap) == false)
            {
                Store[assembly] = typeToMemberMap = new();
            }

            if (typeToMemberMap.TryGetValue(type, out _) == false)
            {
                typeToMemberMap[type] = new();
            }
        }

        public void Add(IndexedType type, MemberInfo member)
        {
            var assembly = type.Type.Assembly;

            if (Store.TryGetValue(assembly, out var typeToMemberMap) == false)
            {
                Store[assembly] = typeToMemberMap = new();
            }

            if (typeToMemberMap.TryGetValue(type, out var members) == false)
            {
                typeToMemberMap[type] = members = new();
            }

            members.Add(member);
        }
    }

}

#endif
