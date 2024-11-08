namespace EncosyTower.Modules.Unions.Converters
{
    public interface IUnionConverter
    {
        string ToString(in Union union);
    }

    public interface IUnionConverter<T> : IUnionConverter
    {
        Union ToUnion(T value);

        Union<T> ToUnionT(T value);

        T GetValue(in Union union);

        bool TryGetValue(in Union union, out T result);

        bool TrySetValueTo(in Union union, ref T dest);
    }
}
