using System.Collections.Generic;

namespace EncosyTower.Collections
{
    /// <summary>
    /// Any type implements this interface can be seen as a proxy of <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IListProxy<T> : IResizable
    {
        T[] Items { get; set; }

        ref int Size { get; }

        ref int Version { get; }
    }
}
