using System;
using EncosyTower.Modules.TypeWrap;

namespace EncosyTower.Modules.Tests.TypeWraps
{
    [WrapRecord]
    public readonly partial record struct HeroId(int _);

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ArrayAttribute : Attribute
    {
        public ArrayAttribute(int[] values) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ValueAttribute : Attribute
    {
        public ValueAttribute(string strValue, AttributeTargets enumValue, Type type) { }
    }

    public struct MyPrinter
    {
        public void PrintArray([Array(new[] { 1, 2, 3 })] int[] values) { }

        public void Print([Value("a", AttributeTargets.Method, typeof(int))] object value) { }
    }

    [WrapRecord]
    public partial record struct MyPrinterNext(MyPrinter _)
    {
        void Do()
        {
            PrintArray(default);
            Print(default);
        }
    }
}
