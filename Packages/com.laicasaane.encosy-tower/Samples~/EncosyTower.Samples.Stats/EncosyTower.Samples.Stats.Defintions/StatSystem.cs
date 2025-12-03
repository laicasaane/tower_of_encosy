using System;
using System.Runtime.CompilerServices;
using EncosyTower.Entities.Stats;
using EncosyTower.EnumExtensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    [EnumExtensions]
    public enum StatOp : byte
    {
        Undefined = 0,
        Set,
        Add,
        SetMultiplier,
        AddMultiplier,
        MultiplyMultiplier,
        Clamp,
        And,
        Or,
    }

    [EnumExtensionsFor(typeof(StatSystem.StackFlag))]
    internal static partial class StatStackFlagExtensions { }

    public readonly struct StatModifierSource
    {
        public readonly StatHandle StatHandle;
        public readonly StatSystem.ValuePair ValuePair;
        public readonly bool UseBaseValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifierSource(StatHandle statHandle, bool useBaseValue = false)
        {
            StatHandle = statHandle;
            ValuePair = default;
            UseBaseValue = useBaseValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifierSource(StatSystem.ValuePair valuePair, bool useBaseValue = false)
        {
            StatHandle = StatHandle.Null;
            ValuePair = valuePair;
            UseBaseValue = useBaseValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(
              out StatHandle statHandle
            , out StatSystem.ValuePair valuePair
            , out bool useBaseValue
        )
        {
            statHandle = StatHandle;
            valuePair = ValuePair;
            useBaseValue = UseBaseValue;
        }
    }

    [StatSystem(StatDataSize.Size8)]
    public static partial class StatSystem
    {
        [Flags]
        public enum StackFlag : byte
        {
            None  = 0,
            Set   = 1 << 0,
            Clamp = 1 << 1,
            And   = 1 << 2,
            Or    = 1 << 3,
        }

        // NOTICE: This struct has been fully implemented via source generator.
        // We only need to set the attribute [InternalBufferCapacity(0)] to our liking.
        [InternalBufferCapacity(0)]
        partial struct Stat { }

        // NOTICE: This struct has been fully implemented via source generator.
        // We only need to set the attribute [InternalBufferCapacity(0)] to our liking.
        [InternalBufferCapacity(0)]
        partial struct StatObserver { }

        // TODO: This struct is ONLY partially implemented via source generator.
        // We have to manually implement the missing parts.
        [InternalBufferCapacity(0)]
        partial struct StatModifier
        {
            [SerializeField] private Entity _entityA;
            [SerializeField] private Entity _entityB;
            [SerializeField] private int _statIndexA;
            [SerializeField] private int _statIndexB;
            [SerializeField] private uint _id;
            [SerializeField] private ValuePair _valuePairA;
            [SerializeField] private ValuePair _valuePairB;
            [SerializeField] private bool _useBaseValueA;
            [SerializeField] private bool _useBaseValueB;
            [SerializeField] private StatOp _op;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatModifier(StatOp op, in StatModifierSource statA) : this()
            {
                Op = op;
                StatA = statA;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatModifier(StatOp op, in StatModifierSource statA, in StatModifierSource statB) : this()
            {
                Op = op;
                StatA = statA;
                StatB = statB;
            }

            public StatModifierSource StatA
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => StatHandleA.IsValid ? new(StatHandleA, _useBaseValueA) : new(_valuePairA, _useBaseValueA);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => (StatHandleA, _valuePairA, _useBaseValueA) = value;
            }

            public StatModifierSource StatB
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => StatHandleB.IsValid ? new(StatHandleB, _useBaseValueB) : new(_valuePairB, _useBaseValueB);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => (StatHandleB, _valuePairB, _useBaseValueB) = value;
            }

            public StatOp Op
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => _op;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _op = value;
            }

            private StatHandle StatHandleA
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => new(_entityA, _statIndexA);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => (_entityA, _statIndexA) = value;
            }

            private StatHandle StatHandleB
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => new(_entityB, _statIndexB);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => (_entityB, _statIndexB) = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly partial void GetIdInternal(ref uint id)
                => id = _id;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void SetIdInternal(uint value)
                => _id = value;

            readonly partial void AddObservedStatsToListInternal(NativeList<StatHandle> observedStatHandles)
            {
                switch (_op)
                {
                    case StatOp.Set:
                    case StatOp.Add:
                    case StatOp.SetMultiplier:
                    case StatOp.AddMultiplier:
                    case StatOp.MultiplyMultiplier:
                    case StatOp.And:
                    case StatOp.Or:
                    {
                        if (StatHandleA.IsValid)
                        {
                            observedStatHandles.Add(StatHandleA);
                        }
                        break;
                    }

                    case StatOp.Clamp:
                    {
                        if (StatHandleA.IsValid)
                        {
                            observedStatHandles.Add(StatHandleA);
                        }

                        if (StatHandleB.IsValid)
                        {
                            observedStatHandles.Add(StatHandleB);
                        }
                        break;
                    }
                }
            }

            partial void ApplyInternal(Reader reader, ref Stack stack, ref bool shouldProduceModifierTriggerEvent)
            {
                shouldProduceModifierTriggerEvent = false;

                ref var stackFlags = ref stack.flags;

                switch (_op)
                {
                    // For the sake of determinism, the "Set" modifier takes the min value
                    // between the existing one and the new one (if a "Set" value was already present)
                    case StatOp.Set:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            var hadSet = stackFlags.Contains(StackFlag.Set);
                            stackFlags = stackFlags.Set(StackFlag.Set);

                            if (hadSet)
                            {
                                stack.setValue = StatVariant.Min(stack.setValue, value);
                            }
                            else
                            {
                                stack.setValue = value;
                            }
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.Add:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            stack.addValue += value;
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.SetMultiplier:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            stack.multiplierValue = value;
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.AddMultiplier:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            stack.multiplierValue += value;
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.MultiplyMultiplier:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            stack.multiplierValue *= value;
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    // For the sake of determinism, the "Clamp" modifier takes the min/max values
                    // between the existing ones and the new ones (if a "Clamp" value was already present)
                    case StatOp.Clamp:
                    {
                        if (TryGetValueA(reader, out var valueA)
                            && TryGetValueB(reader, out var valueB)
                        )
                        {
                            stackFlags = stackFlags.Set(StackFlag.Clamp);
                            stack.clampMinValue = valueA;
                            stack.clampMaxValue = valueB;
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.And:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            var hadAnd = stackFlags.Contains(StackFlag.And);
                            stackFlags = stackFlags.Set(StackFlag.And);

                            if (hadAnd)
                            {
                                stack.andValue &= value;
                            }
                            else
                            {
                                stack.andValue = value;
                            }
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case StatOp.Or:
                    {
                        if (TryGetValueA(reader, out var value))
                        {
                            var hadOr = stackFlags.Contains(StackFlag.Or);
                            stackFlags = stackFlags.Set(StackFlag.Or);

                            if (hadOr)
                            {
                                stack.orValue |= value;
                            }
                            else
                            {
                                stack.orValue = value;
                            }
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    default:
                    {
                        shouldProduceModifierTriggerEvent = true;
                        break;
                    }
                }
            }

            private readonly bool TryGetValueA(Reader reader, out StatVariant value)
            {
                var statHandle = StatHandleA;
                var useBaseValue = _useBaseValueA;

                if (statHandle.IsValid == false)
                {
                    value = useBaseValue
                        ? _valuePairA.GetBaseValueOrDefault()
                        : _valuePairA.GetCurrentValueOrDefault();
                    return true;
                }

                if (reader.TryGetStatValue(statHandle, out var valuePair))
                {
                    value = useBaseValue
                        ? valuePair.GetBaseValueOrDefault()
                        : valuePair.GetCurrentValueOrDefault();
                    return true;
                }

                value = default;
                return false;
            }

            private readonly bool TryGetValueB(Reader reader, out StatVariant value)
            {
                var statHandle = StatHandleB;
                var useBaseValue = _useBaseValueB;

                if (statHandle.IsValid == false)
                {
                    value = useBaseValue
                        ? _valuePairB.GetBaseValueOrDefault()
                        : _valuePairB.GetCurrentValueOrDefault();
                    return true;
                }

                if (reader.TryGetStatValue(statHandle, out var valuePair))
                {
                    value = useBaseValue
                        ? valuePair.GetBaseValueOrDefault()
                        : valuePair.GetCurrentValueOrDefault();
                    return true;
                }

                value = default;
                return false;
            }

            partial struct Stack
            {
                public StatVariant setValue;

                public StatVariant addValue;
                public StatVariant multiplierValue;

                public StatVariant clampMinValue;
                public StatVariant clampMaxValue;

                public StatVariant andValue;
                public StatVariant orValue;

                public StatVariantType valueType;
                public StackFlag flags;

                partial void ResetInternal(in Stat stat)
                {
                    valueType = stat.ValuePair.Type;

                    flags = flags.Unset(StackFlag.Set);
                    setValue = valueType.ZeroVariant();

                    addValue = valueType.ZeroVariant();
                    multiplierValue = valueType.OneVariant();

                    flags = flags.Unset(StackFlag.Clamp);
                    clampMinValue = valueType.ZeroVariant();
                    clampMaxValue = valueType.ZeroVariant();

                    flags = flags.Unset(StackFlag.And);
                    andValue = valueType.OneVariant();

                    flags = flags.Unset(StackFlag.Or);
                    orValue = valueType.ZeroVariant();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                partial void ApplyInternal(in StatVariant baseValue, ref StatVariant currentValue)
                {
                    StatVariant value = baseValue;

                    if (flags.Contains(StackFlag.Set))
                    {
                        value = setValue;
                    }

                    value += addValue;
                    value *= multiplierValue;

                    if (flags.Contains(StackFlag.Clamp))
                    {
                        value = StatVariant.Clamp(value, clampMinValue, clampMaxValue);
                    }

                    if (flags.Contains(StackFlag.And))
                    {
                        value &= andValue;
                    }

                    if (flags.Contains(StackFlag.Or))
                    {
                        value |= orValue;
                    }

                    currentValue = value;
                }
            }
        }
    }
}
