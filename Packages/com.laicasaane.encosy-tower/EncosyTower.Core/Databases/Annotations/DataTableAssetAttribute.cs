using System;

namespace EncosyTower.Databases
{
    /// <summary>
    /// Applying <see cref="DataTableAssetAttribute"/> to a <c>partial</c> class triggers a Roslyn source
    /// generator that provides the boilerplate implementations required by the data-table asset system.
    /// </summary>
    /// <remarks>
    /// The annotated class must be declared as <c>partial</c>, must not be abstract,
    /// and must not be an open generic type. It must directly or indirectly inherit from either
    /// <see cref="DataTableAsset{TDataId, TData}"/> or
    /// <see cref="DataTableAsset{TDataId, TData, TConvertedId}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataTableAssetAttribute : Attribute
    {
    }
}
