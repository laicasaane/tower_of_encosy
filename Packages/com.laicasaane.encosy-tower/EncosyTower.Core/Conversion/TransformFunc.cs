namespace EncosyTower.Conversion
{
    public delegate bool TransformFunc<TInput, TOutput>(TInput input, out TOutput output);

    public delegate bool TransformFuncIn<TInput, TOutput>(in TInput input, out TOutput output);
}
