using System;

namespace EncosyTower.Data
{
    /// <summary>
    /// Applying <see cref="DataAttribute"/> to a class or struct enables source generation
    /// that adds APIs for defining custom data types.
    /// <br/>
    /// Data members can be declared using either properties or fields.
    /// <br/>
    /// When using properties, each property should be annotated with <see cref="DataPropertyAttribute"/>.
    /// <br/>
    /// When using fields, each field should be annotated with one of the following attributes:
    /// <list type="bullet">
    /// <item><see cref="UnityEngine.SerializeField"/></item>
    /// <item><see cref="Newtonsoft.Json.JsonPropertyAttribute"/></item>
    /// <item><see href="https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonincludeattribute">JsonIncludeAttribute</see></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    ///     A type annotated with <see cref="DataAttribute"/> must be declared as <c>partial</c>.
    /// </item>
    /// <item>
    ///     Generated code also implements <see cref="IData"/> for the annotated type.
    /// </item>
    /// <item>
    ///     If the type contains a field named <c>_id</c> or a property named <c>Id</c>,
    ///     an implementation of <see cref="IDataWithId{TDataId}"/> is generated.
    /// </item>
    /// <item>
    ///     By default, generated data types are immutable, meaning they do not expose any API
    ///     that allows modifying their internal data.
    /// </item>
    /// <item>
    ///     To make an annotated type mutable, apply <see cref="DataMutableAttribute"/>
    ///     to the type declaration.
    /// </item>
    /// <item>
    ///     It is advised to use only types that are compatible with
    ///     <see href="https://docs.unity3d.com/Manual/script-serialization-rules.html">Unity serialization rules</see>.
    /// </item>
    /// <item>
    ///     The following collection types can also be used when a property is manually declared:
    ///     <see cref="System.ReadOnlyMemory{T}"/>,
    ///     <see cref="System.Memory{T}"/>,
    ///     <see cref="System.ReadOnlySpan{T}"/>,
    ///     <see cref="System.Span{T}"/>,
    ///     <see cref="System.Collections.Generic.IReadOnlyList{T}"/>,
    ///     <see cref="System.Collections.Generic.IList{T}"/>,
    ///     <see cref="System.Collections.Generic.ISet{T}"/>,
    ///     <see cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}"/>,
    ///     <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>,
    ///     <see cref="System.Collections.Generic.List{T}"/>,
    ///     <see cref="System.Collections.Generic.HashSet{T}"/>,
    ///     <see cref="System.Collections.Generic.Queue{T}"/>,
    ///     <see cref="System.Collections.Generic.Stack{T}"/>,
    ///     <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>,
    ///     <see cref="EncosyTower.Collections.ListFast{T}"/>.
    /// </item>
    /// <item>
    ///     To explicitly specify the type of a generated property for a serializable field,
    ///     annotate the field with <see cref="PropertyTypeAttribute"/>
    ///     and pass a <c>typeof</c> expression to the attribute constructor.
    ///     For example: <c>[PropertyType(typeof(HashSet&lt;int&gt;)]</c>
    /// </item>
    /// <item>
    ///     To explicitly specify the type of the generated field from
    ///     a property annotated with <see cref="DataPropertyAttribute"/>,
    ///     pass a <c>typeof</c> expression to the attribute constructor.
    ///     For example: <c>[DataProperty(typeof(HashSet&lt;int&gt;)]</c>.
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Data]
    /// public partial class RewardData
    /// {
    ///     [SerializeField] private int _id;
    ///
    ///     [DataProperty] public RewardType { get => Get_RewardType(); }
    ///
    ///     [DataProperty] public float Amount { get => Get_Amount(); init => Set_Amount(value); }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class DataAttribute : Attribute { }
}
