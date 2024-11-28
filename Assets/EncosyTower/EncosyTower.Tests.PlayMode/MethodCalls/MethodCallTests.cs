using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace EncosyTower.Tests.MethodCalls
{
    public class MethodCallTests
    {
        private const int TYPE = 6;
        private const int COUNT = 100;

        private IHandler<int>[] _handlers;
        private Action<int>[] _actions;
        private MethodA<int>[] _directsA;
        private MethodB<int>[] _directsB;
        private MethodC<int>[] _directsC;
        private MethodD<int>[] _directsD;
        private MethodE<int>[] _directsE;
        private MethodF<int>[] _directsF;

        [SetUp]
        public void SetUp()
        {
            var handlers = _handlers = new IHandler<int>[TYPE * COUNT];
            var actions = _actions = new Action<int>[TYPE * COUNT];

            Make(ref _directsA, handlers, actions, COUNT, 0, TYPE);
            Make(ref _directsB, handlers, actions, COUNT, 1, TYPE);
            Make(ref _directsC, handlers, actions, COUNT, 2, TYPE);
            Make(ref _directsD, handlers, actions, COUNT, 3, TYPE);
            Make(ref _directsE, handlers, actions, COUNT, 4, TYPE);
            Make(ref _directsF, handlers, actions, COUNT, 5, TYPE);

            static void Make<T>(
                  ref T[] directs
                , IHandler<int>[] handlers
                , Action<int>[] actions
                , int count
                , int type
                , int maxType
            )
                where T : IHandler<int>, new()
            {
                directs = new T[count];

                for (var i = 0; i < count; i++)
                {
                    var index = (i * maxType) + type;
                    var handler = new T();
                    handlers[index] = handler;
                    actions[index] = handler.Method;
                    directs[i] = handler;
                }
            }
        }

        [Test, Performance]
        public void Direct_Method_Calls_Linear()
        {
            Measure.Method(() => {
                {
                    var directs = _directsA.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }

                {
                    var directs = _directsB.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }

                {
                    var directs = _directsC.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }

                {
                    var directs = _directsD.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }

                {
                    var directs = _directsE.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }

                {
                    var directs = _directsF.AsSpan();
                    var length = directs.Length;

                    for (var i = 0; i < length; i++)
                    {
                        directs[i].Method(i);
                    }
                }
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void Direct_Method_Calls_Alternate()
        {
            Measure.Method(() => {
                var directsA = _directsA.AsSpan();
                var directsB = _directsB.AsSpan();
                var directsC = _directsC.AsSpan();
                var directsD = _directsD.AsSpan();
                var directsE = _directsE.AsSpan();
                var directsF = _directsF.AsSpan();

                var length = TYPE * COUNT;

                for (var i = 0; i < length; i++)
                {
                    var selector = i % TYPE;

                    switch (selector)
                    {
                        case 0:
                            directsA[i / TYPE].Method(i);
                            break;

                        case 1:
                            directsB[i / TYPE].Method(i);
                            break;

                        case 2:
                            directsC[i / TYPE].Method(i);
                            break;

                        case 3:
                            directsD[i / TYPE].Method(i);
                            break;

                        case 4:
                            directsE[i / TYPE].Method(i);
                            break;

                        case 5:
                            directsF[i / TYPE].Method(i);
                            break;
                    }
                }
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void Virtual_Method_Calls()
        {
            Measure.Method(() => {
                var handlers = _handlers.AsSpan();
                var length = handlers.Length;

                for (var i = 0; i < length; i++)
                {
                    handlers[i].Method(i);
                }
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void Interface_Switch_To_Concrete_Type_Direct_Method_Calls()
        {
            Measure.Method(() => {
                var handlers = _handlers.AsSpan();
                var length = handlers.Length;

                for (var i = 0; i < length; i++)
                {
                    switch (handlers[i])
                    {
                        case MethodA<int> methodA:
                            methodA.Method(i);
                            break;

                        case MethodB<int> methodB:
                            methodB.Method(i);
                            break;

                        case MethodC<int> methodC:
                            methodC.Method(i);
                            break;

                        case MethodD<int> methodD:
                            methodD.Method(i);
                            break;

                        case MethodE<int> methodE:
                            methodE.Method(i);
                            break;

                        case MethodF<int> methodF:
                            methodF.Method(i);
                            break;
                    }
                }
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void Delegate_Calls()
        {
            Measure.Method(() => {
                var actions = _actions.AsSpan();
                var length = actions.Length;

                for (var i = 0; i < length; i++)
                {
                    actions[i](i);
                }
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        public interface IHandler<T>
        {
            void Method(T value);
        }

        public class MethodA<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }

        public class MethodB<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }

        public class MethodC<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }

        public class MethodD<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }

        public class MethodE<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }

        public class MethodF<T> : IHandler<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void Method(T value)
            {
            }
        }
    }
}
