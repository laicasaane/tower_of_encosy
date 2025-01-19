using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Types
{
    public static class EncosyTypeExtensions
    {
        public const string VOID_TYPE_NAME = "void";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TypeId> FindId([NotNull] this Type type)
            => TypeIdVault.TryGetId(type, out var id) ? id : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeId GetOrRegisterId([NotNull] this Type type)
            => TypeIdVault.TryGetId(type, out var id) ? id : RuntimeTypeCache.GetInfo(type).Id;

        public static string GetName([NotNull] this Type type)
        {
            if (type.IsEnum)
            {
                return type.Name;
            }

            return Type.GetTypeCode(type) switch {
                TypeCode.Boolean => "bool",
                TypeCode.Byte => "byte",
                TypeCode.Char => "char",
                TypeCode.Decimal => "decimal",
                TypeCode.Double => "double",
                TypeCode.Int16 => "short",
                TypeCode.Int32 => "int",
                TypeCode.Int64 => "long",
                TypeCode.SByte => "sbyte",
                TypeCode.Single => "float",
                TypeCode.String => "string",
                TypeCode.UInt16 => "ushort",
                TypeCode.UInt32 => "uint",
                TypeCode.UInt64 => "ulong",
                _ => type.Name
            };
        }

        public static string GetFullName([NotNull] this Type type)
        {
            if (type.IsEnum)
            {
                return type.FullName;
            }

            return Type.GetTypeCode(type) switch {
                TypeCode.Boolean => "bool",
                TypeCode.Byte => "byte",
                TypeCode.Char => "char",
                TypeCode.Decimal => "decimal",
                TypeCode.Double => "double",
                TypeCode.Int16 => "short",
                TypeCode.Int32 => "int",
                TypeCode.Int64 => "long",
                TypeCode.SByte => "sbyte",
                TypeCode.Single => "float",
                TypeCode.String => "string",
                TypeCode.UInt16 => "ushort",
                TypeCode.UInt32 => "uint",
                TypeCode.UInt64 => "ulong",
                _ => type.FullName
            };
        }

        public static string GetFriendlyName([NotNull] this Type type, bool fullName = false)
        {
            if (type == typeof(void))
            {
                return VOID_TYPE_NAME;
            }

            var friendlyName = fullName ? type.GetName() : type.GetFullName();

            if (type.IsGenericType)
            {
                var iBacktick = friendlyName.IndexOf('`');

                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }

                friendlyName += "<";

                var typeParameters = type.GetGenericArguments();

                for (var i = 0; i < typeParameters.Length; ++i)
                {
                    var typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }

                friendlyName += ">";
            }

            return friendlyName;
        }

        public static bool IsReferenceOrContainsReferences([NotNull] this Type type)
        {
            if (type.IsValueType == false)
            {
                return true;
            }

            if (type.IsEnum)
            {
                return false;
            }

            var typeCode = Type.GetTypeCode(type);

            if (typeCode is >= TypeCode.Boolean and <= TypeCode.DateTime)
            {
                return false;
            }

            foreach (var field in type.GetFields())
            {
                if (field.FieldType.IsReferenceOrContainsReferences())
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetNameWithoutSuffix([NotNull] this Type type, [NotNull] string suffix)
        {
            var name = type.Name;

            if (string.IsNullOrEmpty(suffix))
            {
                return name;
            }

            var lastIndex = name.Length - 1;
            var index = name.LastIndexOf(suffix, lastIndex, StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                name = name[..index];
            }

            return name;
        }
    }
}
