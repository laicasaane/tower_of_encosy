#if UNITY_LOCALIZATION

/// <auto-generated>
///*****************************************************************///
///                                                                 ///
/// This file is auto-generated by L10nKeyToStringAdaptersGenerator ///
///                   DO NOT manually modify it!                    ///
///                                                                 ///
///*****************************************************************///
/// </auto-generated>

#pragma warning disable

using System;
using EncosyTower.Modules.Localization;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Localization
{
    [Serializable]
    [Label("L10nKey ⇒ Entry Key", "Default")]
    [Adapter(sourceType: typeof(L10nKey), destType: typeof(string), order: 0)]
    public class L10nKeyToEntryKeyAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey key) && key.IsValid)
            {
                return key.Entry.Key;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey ⇒ Table Collection Name", "Default")]
    [Adapter(sourceType: typeof(L10nKey), destType: typeof(string), order: 1)]
    public class L10nKeyToTableCollectionNameAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey key) && key.IsValid)
            {
                return key.Table.TableCollectionName;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey ⇒ Table & Key Format: \"table,key\"", "Default")]
    [Adapter(sourceType: typeof(L10nKey), destType: typeof(string), order: 2)]
    public class L10nKeyToTableAndKeyFormatAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey key) && key.IsValid)
            {
                return key.ToString();
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey<string> ⇒ Entry Key", "Default")]
    [Adapter(sourceType: typeof(L10nKey<string>), destType: typeof(string), order: 0)]
    public class L10nKeyStringToEntryKeyAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey<string> key) && key.IsValid)
            {
                return key.Entry.Key;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey<string> ⇒ Table Collection Name", "Default")]
    [Adapter(sourceType: typeof(L10nKey<string>), destType: typeof(string), order: 1)]
    public class L10nKeyStringToTableCollectionNameAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey<string> key) && key.IsValid)
            {
                return key.Table.TableCollectionName;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey<string> ⇒ Table & Key Format: \"table,key\"", "Default")]
    [Adapter(sourceType: typeof(L10nKey<string>), destType: typeof(string), order: 2)]
    public class L10nKeyStringToTableAndKeyFormatAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey<string> key) && key.IsValid)
            {
                return key.ToString();
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable ⇒ Entry Key", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable), destType: typeof(string), order: 0)]
    public class L10nKeySerializableToEntryKeyAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable key) && key.IsValid)
            {
                return key.Entry.Key;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable ⇒ Table Collection Name", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable), destType: typeof(string), order: 1)]
    public class L10nKeySerializableToTableCollectionNameAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable key) && key.IsValid)
            {
                return key.Table.TableCollectionName;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable ⇒ Table & Key Format: \"table,key\"", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable), destType: typeof(string), order: 2)]
    public class L10nKeySerializableToTableAndKeyFormatAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable key) && key.IsValid)
            {
                return key.ToString();
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable<string> ⇒ Entry Key", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable<string>), destType: typeof(string), order: 0)]
    public class L10nKeyStringSerializableToEntryKeyAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable<string> key) && key.IsValid)
            {
                return key.Entry.Key;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable<string> ⇒ Table Collection Name", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable<string>), destType: typeof(string), order: 1)]
    public class L10nKeyStringSerializableToTableCollectionNameAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable<string> key) && key.IsValid)
            {
                return key.Table.TableCollectionName;
            }

            return union;
        }
    }

    [Serializable]
    [Label("L10nKey.Serializable<string> ⇒ Table & Key Format: \"table,key\"", "Default")]
    [Adapter(sourceType: typeof(L10nKey.Serializable<string>), destType: typeof(string), order: 2)]
    public class L10nKeyStringSerializableToTableAndKeyFormatAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            var converter = Union<L10nKey.Serializable<string>>.GetConverter();

            if (converter.TryGetValue(union, out L10nKey.Serializable<string> key) && key.IsValid)
            {
                return key.ToString();
            }

            return union;
        }
    }

}

#endif
