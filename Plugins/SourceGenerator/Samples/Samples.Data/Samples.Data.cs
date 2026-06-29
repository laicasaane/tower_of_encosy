[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Samples.DatabaseAuthoring")]

namespace Samples.Data.Databases
{
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Naming;
    using Samples.Data.Heroes;
    using Samples.Data.Enemies;
    using Samples.Data.DataConverters;
    using EncosyTower.Databases;

    [Database(NameCasing.SnakeLower, typeof(IntWrapperConverter), WithInstanceAPI = true)]
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
namespace EncosyTower.Tests.Databases.Authoring
{
    using EncosyTower.Data;
    using EncosyTower.Databases.Authoring;
    using Samples.Data.Databases;
    using Unity.Mathematics;

    [AuthorDatabase(typeof(SampleDatabase), typeof(Int2DataSetConverter))]
    public partial struct SampleDatabaseAuthoring
    {

    }

    public readonly struct Int2DataSetConverter
    {
        public static int2[] Convert(Int2Data[] data)
        {
            return default;
        }
    }

    [Data]
    public partial struct Int2Data
    {
        [DataProperty] public readonly int X => Get_X();

        [DataProperty] public readonly int Y => Get_Y();
    }
}
#endif

namespace Samples.Data.Data
{
    using System;
    using EncosyTower.Data;
    using Samples.Data.DataConverters;
    using Newtonsoft.Json;
    using UnityEngine;
    using EncosyTower.Data.Authoring;

    public enum EntityKind
    {
        Hero,
        Enemy,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FieldAttribute : Attribute { }

    [Data]
    public partial class IdData
    {
        [DataProperty]
        [field: Field]
        public EntityKind Kind => Get_Kind();

        [DataProperty]
        public int Id => Get_Id();
    }

    [Data]
    public partial class StatData
    {
        [DataAuthoringConverter(typeof(WrapperConverter<float, FloatWrapper>))]
        [DataProperty] public FloatWrapper Hp => Get_Hp();

        [DataAuthoringConverter(typeof(FloatWrapperConverter))]
        [JsonProperty] private FloatWrapper _atk;
    }

    [Data]
    [DataMutable(DataMutableOptions.WithReadOnlyView)]
    public partial class GenericData<T>
    {
        [DataProperty]
        public int Id { get => Get_Id(); init => Set_Id(value); }

        public int X { get; init; }

        public bool Equals(GenericData<T> other)
        {
            return false;
        }
    }

    [Data]
    [DataMutable(DataMutableOptions.WithReadOnlyView)]
    public partial struct StatMultiplierData
    {
        [SerializeField] private IntWrapper _level;
        [SerializeField] private FloatWrapper _hp;
        [SerializeField] private float _atk;
    }

    public enum StatKind
    {
        Hp,
        Atk,
    }
}

namespace Samples.Data.Heroes
{
    using System;
    using System.Collections.Generic;
    using EncosyTower.Data;
    using EncosyTower.Databases;
    using Samples.Data.Data;
    using Unity.Properties;
    using UnityEngine;

    [Data]
    [GeneratePropertyBag]
    [DataMutable(DataMutableOptions.WithoutPropertySetters | DataMutableOptions.WithReadOnlyView)]
    public partial class MutableData
    {
        [SerializeField, DontCreateProperty]
        private int _intValue;

        [SerializeField]
        private int[] _arrayValue;

        [DataProperty, CreateProperty]
        public ReadOnlyMemory<float> Multipliers => Get_Multipliers();
    }

    [Data]
    public partial class HeroData
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

    [DataTableAsset]
    public partial class HeroDataTableAsset : DataTableAssetBase<IdData, HeroData>
    {
    }

    [Data]
    public partial class NewHeroData : HeroData
    {
        [DataProperty]
        [field: SerializeField]
        public ReadOnlyMemory<int> NewValues => Get_NewValues();

        [DataProperty(typeof(HashSet<int>))]
        public IReadOnlyCollection<int> ValueSet => Get_ValueSet();
    }

    [DataTableAsset]
    public partial class NewHeroDataTableAsset : DataTableAssetBase<IdData, NewHeroData>
    {
    }
}

namespace Samples.Data.Enemies
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using EncosyTower.Data;
    using EncosyTower.Data.Authoring;
    using EncosyTower.Databases;
    using EncosyTower.Initialization;
    using Samples.Data.Data;
    using Unity.Mathematics;
    using UnityEngine;

    [Data]
    public partial class EnemyData : IInitializable
    {
        [SerializeField] private IdData _id;
        [SerializeField] private string _name;
        [DataAuthoringConverter(typeof(EnemyTypeExConverter))]
        [SerializeField] private EnemyTypeEx _type;
        [SerializeField] private StatData _stat;
        [SerializeField] private HashSet<int> _intSet;
        [SerializeField] private Queue<float> _floatQueue;
        [SerializeField] private Stack<float> _floatStack;
        [SerializeField] private Modifier _modifier;
        [SerializeField] private int2[] _int2Array;

        public void Initialize()
        {
        }
    }

    [Data]
    public partial struct Modifier
    {
        [SerializeField, DataManualAuthoring(typeof(string))] private float _hp;
    }

    public struct EnemyTypeExConverter
    {
        public readonly EnemyTypeEx Convert(EnemyType value)
        {
            return (EnemyTypeEx)value;
        }
    }

    public enum EnemyType : byte
    {
        [Description("N")] Normal = 0,
        [Description("E")] Elite = 1,
        [Description("B")] Boss = 2,
    }

    public enum EnemyTypeEx : byte
    {
        [Description("N")] Normal = 0,
        [Description("E")] Elite = 1,
        [Description("B")] Boss = 2,
    }

    public abstract class EnemyDataTableAsset<T> : DataTableAssetBase<IdData, T> where T : IDataWithId<IdData>
    {
    }

    [DataTableAsset]
    public partial class EnemyDataTableAsset : EnemyDataTableAsset<EnemyData>
    {
    }

    [DataTableAsset]
    public partial class NewEnemyDataTableAsset : EnemyDataTableAsset<EnemyData>
    {
    }

    public abstract class GenericDataTableAsset<T> : DataTableAssetBase<int, GenericData<T>>
    {
    }

    [DataTableAsset]
    public partial class GenericDataTableAsset : GenericDataTableAsset<int> { }
}

namespace Samples.Data.DataConverters
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

namespace Samples.Data.ConvertibleIds
{
    using EncosyTower.Collections;
    using EncosyTower.Data;
    using EncosyTower.Databases;

    [Data]
    public partial struct IdData
    {
        [DataProperty] public readonly int Value => Get_Value();

        [DataProperty] public readonly ListFast<int>.ReadOnly Values => Get_Values();

        public static implicit operator int(IdData id)
            => id.Value;

        private static int Convert(int a) => a;

        public static bool Equals(int a, int b) => a == b;

        public static bool Equals<T, U>(T a, T b) => false;
    }

    [Data]
    [DataMutable(DataMutableOptions.WithReadOnlyView)]
    public partial struct ItemData
    {
        [DataProperty] public readonly IdData Id => Get_Id();

        [DataProperty] public readonly float Amount => Get_Amount();
    }

    [DataTableAsset]
    public sealed partial class ItemTableAsset : DataTableAssetBase<IdData, ItemData, int>
    {
    }
}

namespace Samples.Data.Databases.Settings
{
    using EncosyTower.Data;
    using EncosyTower.Data.Authoring;
    using EncosyTower.Databases;
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Ids;
    using EncosyTower.Naming;
    using EncosyTower.StringIds;
    using UnityEngine;

    [Database(NameCasing.SnakeLower)]
    internal readonly partial struct FileDatabase
    {
        [Table] public readonly FileTableAsset FileList => Get_FileList();
    }

    [HideInInspector]
    [AuthorDatabase(typeof(FileDatabase))]
    internal readonly partial struct FileDatabaseAuthoring
    {
    }

    [DataTableAsset]
    internal sealed partial class FileTableAsset : DataTableAssetBase<StringId, FileData> { }

    [Data(Converters = new[] { typeof(Converters) }, Comparers = new[] { typeof(Comparers) })]
    internal partial struct FileData
    {
        [DataProperty(typeof(uint), typeof(StringIdValueConverter))]
        [DataManualAuthoring(typeof(string))]
        public readonly StringId Id => Get_Id();

        [DataProperty, DataComparer(typeof(StringComparer))]
        public readonly string FileName => Get_FileName();

        [DataProperty(typeof(string), typeof(StringConverter))]
        public readonly string FileId => Get_FileId();

        [DataProperty]
        public readonly string MimeType => Get_MimeType();

        [DataProperty]
        public readonly string Url => Get_Url();
    }
    public readonly struct StringIdValueConverter
    {
        public static StringId Convert(uint value)
        {
            return (StringId)((Id)value);
        }

        public static uint Convert(StringId value)
        {
            return (uint)((Id)value);
        }
    }

    public readonly struct Converters
    {
        public static int Convert(uint value) => (int)value;

        public static uint Convert(int value) => (uint)value;
    }

    public readonly struct Comparers
    {
        public static bool Equals(int a, int b) => a == b;
    }

    public readonly struct StringConverter
    {
        public static string Convert(string value) => value;
    }

    public readonly struct StringComparer
    {
        public static bool Equals(string a, string b) => string.Equals(a, b, System.StringComparison.Ordinal);
    }
}

namespace Samples.Data.XDatabases
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Data;
    using EncosyTower.Databases;

    [DataTableAsset]
    public sealed partial class PlacementTableAsset : DataTableAssetBase<int, PlacementData>
    {
    }

    [Serializable]
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Data]
    public partial struct PlacementData : IData
    {
        [DataProperty]
        public readonly int Id => Get_Id();

        [DataProperty]
        public readonly Vector2Int Size => Get_Size();

        [DataProperty(typeof(Vector2Int[]))]
        public readonly ReadOnlyMemory<Vector2Int> Locations => Get_Locations();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2Int[] Convert(ReadOnlyMemory<Vector2Int> value)
            => value.ToArray();
    }
}

namespace Samples.Data.XDatabases
{
    using EncosyTower.Databases;
    using EncosyTower.Naming;

    [Database(NameCasing.SnakeLower, AssetName = $"{nameof(GameDatabase)}Asset")]
    public readonly partial struct GameDatabase
    {
        [Table] public readonly PlacementTableAsset Placements => Get_Placements();
    }
}
