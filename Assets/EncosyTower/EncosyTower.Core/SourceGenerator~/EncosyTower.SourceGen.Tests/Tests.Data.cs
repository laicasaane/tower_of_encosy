namespace EncosyTower.Tests.Databases
{
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Naming;
    using EncosyTower.Tests.Data.Heroes;
    using EncosyTower.Tests.Data.Enemies;
    using EncosyTower.Tests.DataConverters;
    using EncosyTower.Databases;

    [Database(NamingStrategy.SnakeCase, typeof(IntWrapperConverter), WithInstanceAPI = true)]
    public partial struct SampleDatabase
    {
        [Table] public readonly HeroDataTableAsset Heroes => Get_Heroes();

        [Table] public readonly HeroDataTableAsset HeroesX => Get_HeroesX();

        [Horizontal(typeof(NewHeroData), nameof(NewHeroData.Multipliers))]
        [Table] public readonly NewHeroDataTableAsset NewHeroes => Get_NewHeroes();

        [Table] public readonly EnemyDataTableAsset Enemies => Get_Enemies();

        [Table] public readonly NewEnemyDataTableAsset NewEnemies => Get_NewEnemies();
    }
}

#if UNITY_EDITOR
namespace EncosyTower.Tests.DatabaseAuthoring
{
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Tests.Databases;

    [AuthorDatabase(typeof(SampleDatabase))]
    public partial struct SampleDatabaseAuthoring
    {

    }
}
#endif

namespace EncosyTower.Tests.Data
{
    using System;
    using System.Runtime.InteropServices;
    using EncosyTower.Data;
    using EncosyTower.Tests.DataConverters;
    using Newtonsoft.Json;
    using UnityEngine;

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

    public partial class StatData : IData
    {
        [DataConverter(typeof(WrapperConverter<float, FloatWrapper>))]
        [DataProperty] public FloatWrapper Hp => Get_Hp();

        [DataConverter(typeof(FloatWrapperConverter))]
        [JsonProperty] private FloatWrapper _atk;
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
        [SerializeField] private IntWrapper _level;
        [SerializeField] private FloatWrapper _hp;

        [DataProperty]
        public readonly float Atk => Get_Atk();
    }

    public enum StatKind
    {
        Hp,
        Atk,
    }
}

namespace EncosyTower.Tests.Data.Heroes
{
    using System;
    using System.Collections.Generic;
    using EncosyTower.Data;
    using EncosyTower.Databases;
    using UnityEngine;

    [DataMutable(DataMutableOptions.WithoutPropertySetters | DataMutableOptions.WithReadOnlyView)]
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
        [SerializeField] private IdData _id;
        [SerializeField] private string _name;
        [SerializeField] private StatData _stat;
        [SerializeField] private int[] _values;
        [SerializeField] private List<float> _floats;

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

namespace EncosyTower.Tests.Data.Enemies
{
    using System.Collections.Generic;
    using EncosyTower.Common;
    using EncosyTower.Data;
    using EncosyTower.Databases;
    using UnityEngine;

    public partial class EnemyData : IData, IInitializable
    {
        [SerializeField] private IdData _id;
        [SerializeField] private string _name;
        [SerializeField] private StatData _stat;
        [SerializeField] private HashSet<int> _intSet;
        [SerializeField] private Queue<float> _floatQueue;
        [SerializeField] private Stack<float> _floatStack;

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

namespace EncosyTower.Tests.DataConverters
{
    using System;

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
}

namespace EncosyTower.Tests.Data.ConvertibleIds
{
    using EncosyTower.Data;
    using EncosyTower.Databases;

    public partial struct IdData : IData
    {
        [DataProperty] public readonly int Value => Get_Value();

        public static implicit operator int(IdData id)
            => id.Value;
    }

    public partial struct ItemData : IData
    {
        [DataProperty] public readonly IdData Id => Get_Id();

        [DataProperty] public readonly float Amount => Get_Amount();
    }

    public sealed partial class ItemTableAsset : DataTableAsset<IdData, ItemData, int>, IDataTableAsset
    {
    }
}
