namespace EncosyTower.Common
{
    public delegate void ActionIn<T>(in T arg);

    public delegate void ActionRef<T>(ref T arg);

    public delegate void ActionRef<T1, T2>(ref T1 arg1, ref T2 arg2);

    public delegate TResult FuncIn<T, out TResult>(in T arg);

    public delegate TResult FuncRef<T, out TResult>(ref T arg);

    public delegate bool PredicateIn<T>(in T arg);

    public delegate bool PredicateRef<T>(ref T arg);

    public interface IFunc<TResult>
    {
        TResult Invoke();
    }

    public interface IFuncRef<T, TResult>
    {
        TResult Invoke(ref T p);
    }

    public interface IActionRef<T>
    {
        void Invoke(ref T p);
    }

    public interface IActionRef<T1, T2>
    {
        void Invoke(ref T1 p1, ref T2 p2);
    }

    public interface IPredicateRef<T>
    {
        bool Invoke(ref T p);
    }
}
