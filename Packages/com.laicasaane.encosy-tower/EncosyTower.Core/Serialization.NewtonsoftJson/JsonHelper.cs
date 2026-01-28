#if UNITY_NEWTONSOFT_JSON

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    public static class JsonHelper
    {
        private static JsonSerializerSettings s_settings;
        private static JsonSerializer s_serializer;

        public static JsonSerializerSettings Settings
        {
            get => s_settings ??= new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                ContractResolver = new ContractResolver(),
            };
        }

        public static JsonSerializer Serializer
        {
            get
            {
                if (s_serializer == null)
                {
                    s_serializer = JsonSerializer.CreateDefault(Settings);
                    s_serializer.CheckAdditionalContent = true;
                }

                return s_serializer;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_settings = null;
            s_serializer = null;
        }
#endif

        private sealed class ContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(
                  Type type
                , MemberSerialization memberSerialization
            )
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerialize(object data, out string json)
            => TrySerialize(data, out json, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDeserialize<T>([NotNull] string json, out T data)
            => TryDeserialize(json, out data, null);

        public static bool TrySerialize(object data, out string json, Logging.ILogger logger)
        {
            try
            {
                json = JsonConvert.SerializeObject(data, Settings);

                if (string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
                {
                    LogErrorSerializeToNull(logger);
                    return false;
                }

                return string.IsNullOrWhiteSpace(json) == false;
            }
            catch (Exception ex)
            {
                LogException(ex, logger);
                json = string.Empty;
                return false;
            }
        }

        public static bool TryDeserialize<T>([NotNull] string json, out T data, Logging.ILogger logger)
        {
            try
            {
                data = JsonConvert.DeserializeObject<T>(json, Settings);
                return data != null;
            }
            catch (Exception ex)
            {
                LogException(ex, logger);
                data = default;
                return false;
            }
        }

        public static Result<string> Serialize(object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Settings);

                if (string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<string>.Err(new Error(GetSerializedStringIsNullMessage()));
                }

                return string.IsNullOrWhiteSpace(json) == false
                    ? Result<string>.Succeed(json)
                    : Result<string>.Err(new Error(GetSerializedStringIsEmptyMessage()));
            }
            catch (Exception ex)
            {
                return Result<string>.Err(ex);
            }
        }

        public static Result<T> Deserialize<T>([NotNull] string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<T>(json, Settings);

                return data != null
                    ? Result<T>.Succeed(data)
                    : Result<T>.Err(new Error(GetDeserializedObjectIsNullMessage()));
            }
            catch (Exception ex)
            {
                return Result<T>.Err(ex);
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogException(Exception ex, Logging.ILogger logger)
        {
            (logger ?? DevLogger.Default).LogException(ex);
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorSerializeToNull(Logging.ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(GetSerializedStringIsNullMessage());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetSerializedStringIsNullMessage()
        {
            return "Serialized string is `null`.";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetSerializedStringIsEmptyMessage()
        {
            return "Serialized string is empty.";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetDeserializedObjectIsNullMessage()
        {
            return "Deserialized object is null.";
        }
    }
}

#endif
