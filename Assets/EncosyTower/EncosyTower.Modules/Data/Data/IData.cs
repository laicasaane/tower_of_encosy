namespace EncosyTower.Modules.Data
{
    /// <summary>
    /// Any class or struct explicitly implements this interface will have additional APIs generated
    /// to simplify the specification of a custom data type.
    /// <br/>
    /// There are 2 possible approaches to specify data members: property and field.
    /// <br/>
    /// By using properties, each should be annotated with a <see cref="DataPropertyAttribute"/>.
    /// <br/>
    /// By using fields, each should be annotated with one of these attributes:
    /// <list type="bullet">
    /// <item><see cref="UnityEngine.SerializeField"/></item>
    /// <item><see cref="Newtonsoft.Json.JsonPropertyAttribute"/></item>
    /// <item><see href="https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonincludeattribute">JsonIncludeAttribute</see></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    ///     <see cref="IData"/> type declaration must contains the <c>partial</c> keyword.
    /// </item>
    /// <item>
    ///     Any type with a field named <c>_id</c> or a property named <c>Id</c>
    ///     will have <see cref="IDataWithId{TDataId}"/> implementation generated.
    /// </item>
    /// <item>
    ///     By default, <see cref="IData"/> types are immutable, that means they do not expose any API
    ///     which allows modifying their internal data.
    /// </item>
    /// <item>
    ///     To make a <see cref="IData"/> type mutable,
    ///     <see cref="DataMutableAttribute"/> should be put upon the type declaration.
    /// </item>
    /// <item>
    ///     It is adviced to use only types that are compatible to
    ///     <see href="https://docs.unity3d.com/Manual/script-serialization-rules.html">Unity serialization rules</see>.
    /// </item>
    /// <item>
    ///     These collection types can also be used when a property is manually declared:
    ///     <see cref="System.ReadOnlyMemory{T}"/>,
    ///     <see cref="System.Memory{T}"/>,
    ///     <see cref="System.ReadOnlySpan{T}"/>,
    ///     <see cref="System.Span{T}"/>,
    ///     <see cref="System.Collections.Generic.IReadOnlyList{T}"/>,
    ///     <see cref="System.Collections.Generic.IList{T}"/>,
    ///     <see cref="System.Collections.Generic.ISet{T}"/>,
    ///     <see cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}"/>,
    ///     <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>.
    /// </item>
    /// <item>
    ///     To explicitly specify the type of the property generated from a serializable field,
    ///     simply annotates that field with <see cref="PropertyTypeAttribute"/>
    ///     and pass a <c>typeof</c> expression into the attribute constructor.
    ///     For example: <c>[PropertyType(typeof(HashSet&lt;int&gt;)]</c>
    /// </item>
    /// <item>
    ///     To explicitly specify the type of the field generated from
    ///     a property annotated with <see cref="DataPropertyAttribute"/>,
    ///     simply pass a <c>typeof</c> expression into the attribute constructor.
    ///     For example: <c>[DataProperty(typeof(HashSet&lt;int&gt;)]</c>.
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public partial class RewardData : IData
    /// {
    ///     [SerializeField] private int _id;
    ///
    ///     [DataProperty] public RewardType { get => Get_RewardType(); }
    ///
    ///     [DataProperty] public float Amount { get => Get_Amount(); init => Set_Amount(value); }
    /// }
    /// </code>
    /// </example>
    public interface IData { }
}
