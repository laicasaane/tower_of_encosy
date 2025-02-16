using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EncosyTower.Modules.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace EncosyTower.Modules.Tests.Data
{
    public enum EntityKind
    {
        Hero,
        Enemy,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FieldAttribute : Attribute { }

    public partial class IdData : IData
    {
        [DataProperty]
        [field: Field]
        public EntityKind Kind => Get_Kind();

        [DataProperty]
        public int Id => Get_Id();
    }

    public interface IConvert<in TFrom, out TTo>
    {
        TTo Convert(TFrom value);
    }

    [Serializable]
    public struct IntWrapper : IConvert<int, IntWrapper>
    {
        public int value;

        public IntWrapper(int value)
        {
            this.value = value;
        }

        public readonly IntWrapper Convert(int value) => new(value);
    }

    [Serializable]
    public struct FloatWrapper : IConvert<float, FloatWrapper>
    {
        public float value;

        public FloatWrapper(float value)
        {
            this.value = value;
        }

        public readonly FloatWrapper Convert(float value) => new(value);
    }

    public struct IntWrapperConverter
    {
        public static IntWrapper Convert(int value) => new(value);
    }

    public struct FloatWrapperConverter
    {
        public readonly FloatWrapper Convert(float value) => new(value);
    }

    public struct WrapperConverter<TFrom, TTo> where TTo : struct, IConvert<TFrom, TTo>
    {
        public readonly TTo Convert(TFrom value) => default(TTo).Convert(value);
    }

    public partial class StatData : IData
    {
        [DataProperty, DataConverter(typeof(WrapperConverter<float, FloatWrapper>))]
        public FloatWrapper Hp => Get_Hp();

        [JsonProperty, DataConverter(typeof(FloatWrapperConverter))]
        private FloatWrapper _atk;
    }

    public partial class GenericData<T> : IData
    {
        [DataProperty]
        public int Id { get => Get_Id(); init => Set_Id(value); }

        public int X { get; init; }

        public bool Equals(GenericData<T> other)
        {
            return false;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public partial struct StatMultiplierData : IData
    {
        [SerializeField]
        private IntWrapper _level;

        [SerializeField]
        private FloatWrapper _hp;

        [DataProperty]
        public readonly float Atk => Get_Atk();
    }

    public enum StatKind
    {
        Hp,
        Atk,
    }
}

namespace EncosyTower.Modules.Tests.Data.Heroes
{
    [DataMutable(true)]
    public partial class MutableData : IData
    {
        [SerializeField]
        private int _intValue;

        [SerializeField]
        private int[] _arrayValue;

        [DataProperty]
        public ReadOnlyMemory<float> Multipliers => Get_Multipliers();
    }

    public partial class HeroData : IData
    {
        [SerializeField]
        private IdData _id;

        [SerializeField]
        private string _name;

        [SerializeField]
        private StatData _stat;

        [SerializeField]
        private int[] _values;

        [SerializeField]
        private List<float> _floats;

        [DataProperty(typeof(Dictionary<int, string>))]
        public ReadDictionary<int, string> StringMap { get => Get_StringMap(); init => Set_StringMap(value); }

        [DataProperty]
        public ReadOnlyMemory<StatMultiplierData> Multipliers => Get_Multipliers();

        [DataProperty]
        public ReadOnlyMemory<StatMultiplierData> MultipliersX => Get_MultipliersX();

        [SerializeField]
        private List<StatMultiplierData> _abc;

        [SerializeField]
        private Dictionary<StatKind, StatMultiplierData> _statMap;
    }

    public readonly struct ReadDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ReadDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public static implicit operator ReadDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
            => new(dictionary);

        public static explicit operator Dictionary<TKey, TValue>(ReadDictionary<TKey, TValue> readDictionary)
            => readDictionary._dictionary;
    }

    public partial class HeroDataTableAsset : DataTableAsset<IdData, HeroData>, IDataTableAsset
    {
    }

    public partial class NewHeroData : HeroData, IData
    {
        [DataProperty]
        [field: SerializeField]
        public ReadOnlyMemory<int> NewValues => Get_NewValues();

        [DataProperty(typeof(HashSet<int>))]
        public IReadOnlyCollection<int> ValueSet => Get_ValueSet();
    }

    public partial class NewHeroDataTableAsset : DataTableAsset<IdData, NewHeroData>, IDataTableAsset
    {
    }
}

namespace EncosyTower.Modules.Tests.Data.Enemies
{
    public partial class EnemyData : IData, IInitializable
    {
        [SerializeField]
        private IdData _id;

        [SerializeField]
        private string _name;

        [SerializeField]
        private StatData _stat;

        [SerializeField]
        private HashSet<int> _intSet;

        [SerializeField]
        private Queue<float> _floatQueue;

        [SerializeField]
        private Stack<float> _floatStack;

        public void Initialize()
        {
        }
    }

    public abstract class EnemyDataTableAsset<T> : DataTableAsset<IdData, T>, IDataTableAsset where T : IDataWithId<IdData>
    {
    }

    public partial class EnemyDataTableAsset : EnemyDataTableAsset<EnemyData>, IDataTableAsset
    {
    }

    public partial class NewEnemyDataTableAsset : EnemyDataTableAsset<EnemyData>, IDataTableAsset
    {
    }

    public abstract class GenericDataTableAsset<T> : DataTableAsset<int, GenericData<T>>, IDataTableAsset
    {
    }

    public partial class GenericDataTableAsset : GenericDataTableAsset<int>, IDataTableAsset { }
}

namespace EncosyTower.Modules.Tests.Data.ConvertibleIds
{
    public partial struct IdData : IData
    {
        [DataProperty]
        public readonly int Value => Get_Value();

        public static implicit operator int(IdData id)
            => id.Value;
    }

    public partial struct ItemData : IData
    {
        [DataProperty]
        public readonly IdData Id => Get_Id();

        [DataProperty]
        public readonly float Amount => Get_Amount();
    }

    public sealed partial class ItemTableAsset : DataTableAsset<IdData, ItemData, int>, IDataTableAsset
    {
    }
}

#if UNITY_EDITOR
namespace EncosyTower.Modules.Tests.Data.Authoring
{
    using EncosyTower.Modules.Data.Authoring;
    using EncosyTower.Modules.Tests.Data.Heroes;
    using EncosyTower.Modules.Tests.Data.Enemies;

    [Database(NamingStrategy.SnakeCase, typeof(IntWrapperConverter))]
    public partial class Database
    {
        partial class SheetContainer
        {
        }
    }

    partial class Database
    {
        [Table] public HeroDataTableAsset heroes;
    }

    partial class Database
    {
        [Horizontal(typeof(NewHeroData), nameof(NewHeroData.Multipliers))]
        [Table] public NewHeroDataTableAsset newHeroes;
    }

    partial class Database
    {
        [Table] public EnemyDataTableAsset enemies;
        [Table] public NewEnemyDataTableAsset newEnemies;
    }
}
#endif
