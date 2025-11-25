using System.Runtime.CompilerServices;
using EncosyTower.Entities.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace EncosyTower.Samples.Stats
{
    [StatSystem(StatDataSize.Size4)]
    public static partial class StatSystem
    {
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
            public StatHandle statHandleA;
            public StatHandle statHandleB;
            public StatVariant valueA;
            public StatVariant valueB;
            public uint id;
            public Operation operation;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly partial void GetIdInternal(ref uint id)
                => id = this.id;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void SetIdInternal(uint value)
                => id = value;

            readonly partial void AddObservedStatsToListInternal(NativeList<StatHandle> observedStatHandles)
            {
                switch (operation)
                {
                    case Operation.SetFromStat:
                    case Operation.AddFromStat:
                    case Operation.SetMultiplierFromStat:
                    case Operation.AddMultiplierFromStat:
                    case Operation.MultiplyMultiplierFromStat:
                    case Operation.AndFromStat:
                    case Operation.OrFromStat:
                        observedStatHandles.Add(statHandleA);
                        break;

                    case Operation.ClampFromStat:
                        observedStatHandles.Add(statHandleA);
                        observedStatHandles.Add(statHandleB);
                        break;
                }
            }

            partial void ApplyInternal(Reader reader, ref Stack stack, ref bool shouldProduceModifierTriggerEvent)
            {
                shouldProduceModifierTriggerEvent = false;

                switch (operation)
                {
                    // For the sake of determinism, the "Set" modifier takes the min value
                    // between the existing one and the new one (if a "Set" value was already present)
                    case Operation.Set:
                    {
                        var hadSet = stack.hasSet;
                        stack.hasSet = true;

                        if (hadSet)
                        {
                            stack.setValue = StatVariant.Min(stack.setValue, valueA);
                        }
                        else
                        {
                            stack.setValue = valueA;
                        }

                        break;
                    }

                    // For the sake of determinism, the "Set" modifier takes the min value
                    // between the existing one and the new one (if a "Set" value was already present)
                    case Operation.SetFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair valuePair))
                        {
                            var hadSet = stack.hasSet;
                            stack.hasSet = true;

                            if (hadSet)
                            {
                                stack.setValue = StatVariant.Min(stack.setValue, valuePair.GetCurrentValueOrDefault());
                            }
                            else
                            {
                                stack.setValue = valuePair.GetCurrentValueOrDefault();
                            }
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.Add:
                    {
                        stack.addValue += valueA;
                        break;
                    }

                    case Operation.AddFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair valuePair))
                        {
                            stack.addValue += valuePair.GetCurrentValueOrDefault();
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.SetMultiplier:
                    {
                        stack.multiplierValue = valueA;
                        break;
                    }

                    case Operation.SetMultiplierFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair statValue))
                        {
                            stack.multiplierValue = statValue.GetCurrentValueOrDefault();
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.AddMultiplier:
                    {
                        stack.multiplierValue += valueA;
                        break;
                    }

                    case Operation.AddMultiplierFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair statValue))
                        {
                            stack.multiplierValue += statValue.GetCurrentValueOrDefault();
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.MultiplyMultiplier:
                    {
                        stack.multiplierValue *= valueA;
                        break;
                    }

                    case Operation.MultiplyMultiplierFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair statValue))
                        {
                            stack.multiplierValue *= statValue.GetCurrentValueOrDefault();
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    // For the sake of determinism, the "Clamp" modifier takes the min/max values
                    // between the existing ones and the new ones (if a "Clamp" value was already present)
                    case Operation.Clamp:
                    {
                        var hadClamp = stack.hasClamp;
                        stack.hasClamp = true;

                        if (hadClamp)
                        {
                            stack.clampMinValue = StatVariant.Min(stack.clampMinValue, valueA);
                            stack.clampMaxValue = StatVariant.Max(stack.clampMinValue, valueB);
                        }
                        else
                        {
                            stack.clampMinValue = valueA;
                            stack.clampMaxValue = valueB;
                        }
                        break;
                    }

                    // For the sake of determinism, the "Clamp" modifier takes the min/max values
                    // between the existing ones and the new ones (if a "Clamp" value was already present)
                    case Operation.ClampFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair valuePairA)
                            && reader.TryGetStatValue(statHandleB, out ValuePair valuePairB)
                        )
                        {
                            stack.hasClamp = true;
                            stack.clampMinValue = valuePairA.GetCurrentValueOrDefault();
                            stack.clampMaxValue = valuePairB.GetCurrentValueOrDefault();
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.And:
                    {
                        var hadAnd = stack.hasAnd;
                        stack.hasAnd = true;

                        if (hadAnd)
                        {
                            stack.andValue &= valueA;
                        }
                        else
                        {
                            stack.andValue = valueA;
                        }
                        break;
                    }

                    case Operation.AndFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair valuePair))
                        {
                            var hadAnd = stack.hasAnd;
                            stack.hasAnd = true;

                            if (hadAnd)
                            {
                                stack.andValue &= valuePair.GetCurrentValueOrDefault();
                            }
                            else
                            {
                                stack.andValue = valuePair.GetCurrentValueOrDefault();
                            }
                        }
                        else
                        {
                            shouldProduceModifierTriggerEvent = true;
                        }
                        break;
                    }

                    case Operation.Or:
                    {
                        var hadOr = stack.hasOr;
                        stack.hasOr = true;

                        if (hadOr)
                        {
                            stack.orValue |= valueA;
                        }
                        else
                        {
                            stack.orValue = valueA;
                        }
                        break;
                    }

                    case Operation.OrFromStat:
                    {
                        if (reader.TryGetStatValue(statHandleA, out ValuePair valuePair))
                        {
                            var hadOr = stack.hasOr;
                            stack.hasOr = true;

                            if (hadOr)
                            {
                                stack.orValue |= valuePair.GetCurrentValueOrDefault();
                            }
                            else
                            {
                                stack.orValue = valuePair.GetCurrentValueOrDefault();
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

            public enum Operation : byte
            {
                Undefined = 0,

                Set,
                SetFromStat,

                Add,
                AddFromStat,

                SetMultiplier,
                SetMultiplierFromStat,

                AddMultiplier,
                AddMultiplierFromStat,

                MultiplyMultiplier,
                MultiplyMultiplierFromStat,

                Clamp,
                ClampFromStat,

                And,
                AndFromStat,

                Or,
                OrFromStat,
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

                public StatVariantType type;
                public bool isPair;
                public bool hasSet;
                public bool hasClamp;
                public bool hasAnd;
                public bool hasOr;

                partial void ResetInternal(in Stat stat)
                {
                    type = stat.ValuePair.Type;
                    isPair = stat.ValuePair.IsPair;

                    hasSet = false;
                    setValue = type.ZeroVariant();

                    addValue = type.ZeroVariant();
                    multiplierValue = type.OneVariant();

                    hasClamp = false;
                    clampMinValue = type.ZeroVariant();
                    clampMaxValue = type.ZeroVariant();

                    hasAnd = false;
                    andValue = type.OneVariant();

                    hasOr = false;
                    orValue = type.ZeroVariant();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                partial void ApplyInternal(in StatVariant baseValue, ref StatVariant currentValue)
                {
                    StatVariant value = baseValue;

                    if (hasSet)
                    {
                        value = setValue;
                    }

                    value += addValue;
                    value *= multiplierValue;

                    if (hasClamp)
                    {
                        value = StatVariant.Clamp(value, clampMinValue, clampMaxValue);
                    }

                    if (hasAnd)
                    {
                        value &= andValue;
                    }

                    if (hasOr)
                    {
                        value |= orValue;
                    }

                    currentValue = value;
                }
            }
        }
    }
}
