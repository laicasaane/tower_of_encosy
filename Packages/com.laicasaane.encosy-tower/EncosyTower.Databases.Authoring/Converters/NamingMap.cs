using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Authoring
{
    public class NamingMap
    {
        private readonly Dictionary<string, string> _serializedNameToProperName = new();
        private readonly Dictionary<string, string> _properNameToSerializedName = new();
        private readonly NameCasing _strategy;

        public NamingMap(NameCasing strategy)
        {
            _strategy = strategy;
        }

        public void AddProperName(string properName)
        {
            if (properName.IsEmptyOrWhiteSpace())
                return;

            var serializedName = _strategy.ConvertName(properName);
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
            => _properNameToSerializedName.GetValueOrDefault(properName, properName);

        public string GetProperName(string serializedName)
            => _serializedNameToProperName.GetValueOrDefault(serializedName, serializedName);
    }
}
