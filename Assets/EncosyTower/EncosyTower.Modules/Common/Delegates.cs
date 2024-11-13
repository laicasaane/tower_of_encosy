namespace EncosyTower.Modules
{
    public delegate void ActionIn<T>(in T arg);

    public delegate void ActionRef<T>(ref T arg);

    public delegate void ActionRef<T1, T2>(ref T1 arg1, ref T2 arg2);

    public delegate TResult FuncIn<T, out TResult>(in T arg);

    public delegate TResult FuncRef<T, out TResult>(ref T arg);

    public delegate bool PredicateIn<T>(in T arg);

    public delegate bool PredicateRef<T>(ref T arg);
}