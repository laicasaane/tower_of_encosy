#if UNITY_NEWTONSOFT_JSON

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EncosyTower.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace EncosyTower.Serialization
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

        public static bool TrySerialize(object data, out string json)
        {
            try
            {
                json = JsonConvert.SerializeObject(data, Settings);
            }
            catch (Exception ex)
            {
                StaticDevLogger.LogException(ex);
                json = string.Empty;
            }

            if (json == "null")
            {
                ErrorIfSerializeToNull();
                return false;
            }

            return string.IsNullOrWhiteSpace(json) == false;
        }

        public static bool TryDeserialize<T>([NotNull] string json, out T data)
        {
            try
            {
                data = JsonConvert.DeserializeObject<T>(json, Settings);
                return data != null;
            }
            catch (Exception ex)
            {
                StaticDevLogger.LogException(ex);
                data = default;
                return false;
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfSerializeToNull()
        {
            StaticDevLogger.LogError("Serialize save data to `null`.");
        }
    }
}

#endif
