#pragma warning disable

using System;

namespace Samples.Formatters
{
    internal class Base { }

    internal interface IA { }

    internal interface IB { }

    internal interface IC { }

    internal static class Demo
    {
        #region ParameterList

        internal static void RunCombined(int a, int b, int c, int d, int e, int f)
        {
        }

        internal static void RunSplit(
              int a
            , int b
            , int c
            , int d
            , int e
            , int f
        )
        {
        }

        internal static void Invoke()
        {
            RunCombined(1, 2, 3, 4, 5, 6);

            RunSplit(
                  1
                , 2
                , 3
                , 4
                , 5
                , 6
            );
        }

        #endregion

        #region BooleanChain

        internal static bool TestAndCombined(int a, int b, int c, int d)
            => a > 0 && b > 0 && c > 0 && d > 0;

        internal static bool TestAndSplit(int a, int b, int c, int d)
            => a > 0
            && b > 0
            && c > 0
            && d > 0;

        internal static bool TestOrCombined(int a, int b, int c, int d)
            => a > 0 || b > 0 || c > 0 || d > 0;

        internal static bool TestOrSplit(int a, int b, int c, int d)
            => a > 0
            || b > 0
            || c > 0
            || d > 0;

        #endregion

        #region PatternChain

        internal static bool MatchCombined(int x)
            => x is > 0 and < 100 or 999;

        internal static bool MatchSplit(int x)
            => x is > 0
            and < 100
            or 999;

        #endregion

        #region Conditional

        internal static int PickCombined(bool a, bool b)
            => a ? 1 : b ? 2 : 3;

        internal static int PickSplit(bool a, bool b)
            => a
            ? 1
            : b
                ? 2
                : 3;

        #endregion
    }

    #region InheritanceList

    internal sealed class FooCombined : Base, IA, IB, IC { }

    internal sealed class FooSplit
        : Base
        , IA
        , IB
        , IC
    { }


    #endregion

    #region Constraints

    internal sealed class BagCombined<T>
        where T : class, IDisposable, IEquatable<T>, new()
    {
    }

    internal sealed class BagSplit<T>
        where T : class
            , IDisposable
            , IEquatable<T>
            , new()
    {
    }

    #endregion
}
