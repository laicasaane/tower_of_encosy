namespace Module.Core.Unions.Converters
{
    public interface IUnionConverter<T>
    {
        Union ToUnion(T value);

        Union<T> ToUnionT(T value);

        T GetValue(in Union union);

        bool TryGetValue(in Union union, out T result);

        bool TrySetValueTo(in Union union, ref T dest);

        string ToString(in Union union);
    }
}
