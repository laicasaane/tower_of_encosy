using System;

namespace Module.Core
{
    public static class TypeExtensions
    {
        public const string VOID_TYPE_NAME = "void";

        public static string GetName(this Type type)
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

        public static string GetFullName(this Type type)
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

        public static string GetFriendlyName(this Type type)
        {
            if (type == typeof(void))
            {
                return VOID_TYPE_NAME;
            }

            var friendlyName = type.GetName();

            if (type.IsGenericType)
            {
                var iBacktick = friendlyName.IndexOf('`');

                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }

                friendlyName += "<";

                var typeParameters = type.GetGenericArguments();

                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }

                friendlyName += ">";
            }

            return friendlyName;
        }
    }
}
