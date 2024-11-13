using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace EncosyTower.Modules.Data.Authoring
{
    public class NamingMap
    {
        private static readonly CamelCaseNamingStrategy s_camelNaming = new();
        private static readonly SnakeCaseNamingStrategy s_snakeNaming = new();
        private static readonly KebabCaseNamingStrategy s_kebabNaming = new();
        private static readonly DefaultNamingStrategy s_defaultNaming = new();

        private readonly Dictionary<string, string> _serializedNameToProperName = new();
        private readonly Dictionary<string, string> _properNameToSerializedName = new();
        private readonly NamingStrategy _strategy;

        public NamingMap(NamingStrategy strategy)
        {
            _strategy = strategy;
        }

        public void AddProperName(string properName)
        {
            if (string.IsNullOrWhiteSpace(properName))
                return;

            var serializedName = ConvertName(properName, _strategy);
            _serializedNameToProperName[serializedName] = properName;
            _properNameToSerializedName[properName] = serializedName;
        }

        public bool Validate(string properName, string serializedName)
        {
            if (_properNameToSerializedName.TryGetValue(properName, out var value))
            {
                return value == serializedName;
            }

            return false;
        }

        public string GetSerializedName(string properName)
        {
            return _properNameToSerializedName.GetValueOrDefault(properName, properName);
        }

        public string GetProperName(string serializedName)
        {
            return _serializedNameToProperName.GetValueOrDefault(serializedName, serializedName);
        }

        public static string ConvertName(string name, NamingStrategy namingStrategy)
        {
            return namingStrategy switch {
                NamingStrategy.CamelCase => s_camelNaming.GetPropertyName(name, false),
                NamingStrategy.SnakeCase => s_snakeNaming.GetPropertyName(name, false),
                NamingStrategy.KebabCase => s_kebabNaming.GetPropertyName(name, false),
                _ => s_defaultNaming.GetPropertyName(name, false),
            };
        }
    }
}
