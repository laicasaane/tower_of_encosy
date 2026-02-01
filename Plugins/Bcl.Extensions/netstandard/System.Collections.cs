#pragma warning disable

namespace System.Collections;

public interface IEnumerator
{
    object Current { get; }

    bool MoveNext();

    void Reset();
}

public interface IEnumerable
{
    IEnumerator GetEnumerator();
}

public static partial class HashHelpers
{
    public const uint HashCollisionThreshold = 100;
}
