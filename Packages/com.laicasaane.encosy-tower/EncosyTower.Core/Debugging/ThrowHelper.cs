// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Debugging
{
    public static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowArrayLengthMismatchException(int arrayLength, int length)
        {
            throw new ArgumentException($"array.Length ({arrayLength}) does not match the Length of this instance ({length}).", "array");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceWithStrideOffsetAndSizeExceededException()
        {
            throw new ArgumentException("SliceWithStride sizeof(U) + offset must be <= sizeof(T)", "offset");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceWithStrideOffsetOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("offset", "SliceWithStride offset must be >= 0");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceConvertSizeMismatchException()
        {
            throw new InvalidOperationException("SliceConvert requires that Length * sizeof(T) is a multiple of sizeof(U).");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceConvertOnRestrictedRangeException()
        {
            throw new InvalidOperationException("SliceConvert may not be used on a restricted range array");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceConvertStrideMismatchException()
        {
            throw new InvalidOperationException("SliceConvert requires that stride matches the size of the source type");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceOnRestrictedRangeException(string paramName)
            => throw new ArgumentException($"Slice may not be used on a restricted range {paramName}", paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceRangeExceedsLengthException(int sourceLength, int start, int length, string paramName)
            => throw new ArgumentException($"Slice start + length ({start + length}) range must be <= {paramName}.Length ({sourceLength})");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceNegativeLengthException(int length)
            => throw new ArgumentOutOfRangeException("length", $"Slice length {length} < 0.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceNegativeStartException(int start)
            => throw new ArgumentOutOfRangeException("start", $"Slice start {start} < 0.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowSliceIntegerOverflowException()
            => throw new ArgumentException("Slice start + length ({start + length}) causes an integer overflow");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNotUnmanagedType<T>([DoesNotReturnIf(false)] bool isUnmanaged)
        {
            if (isUnmanaged == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"{typeof(T)} is not an unmanaged type. Only unmanaged type is supported.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack]
        public static void ThrowIfIndexOutOfRangeException([DoesNotReturnIf(false)] bool withinRange)
        {
            if (withinRange == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static IndexOutOfRangeException CreateException()
                => new("Index must be non-negative and less than the size of the buffer.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack]
        public static void ThrowArgumentOutOfRangeException_IfNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName, "The value must be non-negative.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack]
        public static void ThrowArgumentOutOfRangeException_IfNegativeZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName, "The value must be positive and non-zero.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowArgumentException_ArrayPlusOffTooSmall()
            => throw CreateArgumentException_ArrayPlusOffTooSmall();

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack]
        public static void ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(
            [DoesNotReturnIf(false)] bool isCreated
        )
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The read-only collection is not created.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowKeyNotFoundException<TKey>(TKey key)
            => throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumFailedVersion()
            => throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumOpCantHappen()
            => throw new InvalidOperationException("Enumeration has either not started or has already finished.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumeratorNotValid()
            => throw new InvalidOperationException("Enumerator is not retrieved via a valid method.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_KeyPresent()
            => throw new InvalidOperationException("Key already present");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_ModifyWhileBeingIterated_Map()
            => throw new InvalidOperationException("Cannot modify a map while it is being iterated");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_ModifyWhileBeingIterated_Set()
            => throw new InvalidOperationException("Cannot modify a set while it is being iterated");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowInvalidOperationException_SetCountGreaterThanStartingOne()
            => throw new InvalidOperationException("Cannot set a count greater than the starting one");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        public static void ThrowKeyNotFoundException_KeyNotFound()
            => throw new KeyNotFoundException("Key not found");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IndexOutOfRangeException CreateIndexOutOfRangeException_Collection()
            => new("Index was out of range. Must be non-negative and less than the size of the collection.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateInvalidOperationException_TypeNotCreatedCorrectly(string name)
            => new($"Type '{name}' was not created correctly.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateInvalidOperationException_CollectionNotCreated()
            => new("Collection was not created.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_CollectionNotCreated(string paramName)
            => new("Collection was not created.", paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentNullException CreateArgumentNullException(string paramName)
            => new(paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_LengthNegative()
            => new("length", "The value must be non-negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_IndexNegative()
            => new("index", "The value must be non-negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_SourceStartIndex_Length()
            => new("The number of elements from 'sourceStartIndex' to the end of the collection is lesser than 'length'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_DestinationTooShort()
            => new("The destination span is too short to copy the requested number of elements.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_ArrayPlusOffTooSmall()
            => new("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
    }
}
