#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Processing
{
    public interface IRequest { }

    public interface IRequest<TResult> { }

    public interface IAsyncRequest { }

    public interface IAsyncRequest<TResult> { }
}

#endif
