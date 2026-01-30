// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Debugging
{
    public static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArrayLengthMismatchException(int arrayLength, int length)
        {
            throw new ArgumentException($"array.Length ({arrayLength}) does not match the Length of this instance ({length}).", "array");
        }

        [DoesNotReturn]
        public static void ThrowSliceWithStrideOffsetAndSizeExceededException()
        {
            throw new ArgumentException("SliceWithStride sizeof(U) + offset must be <= sizeof(T)", "offset");
        }

        [DoesNotReturn]
        public static void ThrowSliceWithStrideOffsetOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("offset", "SliceWithStride offset must be >= 0");
        }

        [DoesNotReturn]
        public static void ThrowSliceConvertSizeMismatchException()
        {
            throw new InvalidOperationException("SliceConvert requires that Length * sizeof(T) is a multiple of sizeof(U).");
        }

        [DoesNotReturn]
        public static void ThrowSliceConvertOnRestrictedRangeException()
        {
            throw new InvalidOperationException("SliceConvert may not be used on a restricted range array");
        }

        [DoesNotReturn]
        public static void ThrowSliceConvertStrideMismatchException()
        {
            throw new InvalidOperationException("SliceConvert requires that stride matches the size of the source type");
        }

        [DoesNotReturn]
        public static void ThrowSliceOnRestrictedRangeException(string paramName)
            => throw new ArgumentException($"Slice may not be used on a restricted range {paramName}", paramName);

        [DoesNotReturn]
        public static void ThrowSliceRangeExceedsLengthException(int sourceLength, int start, int length, string paramName)
            => throw new ArgumentException($"Slice start + length ({start + length}) range must be <= {paramName}.Length ({sourceLength})");

        [DoesNotReturn]
        public static void ThrowSliceNegativeLengthException(int length)
            => throw new ArgumentOutOfRangeException("length", $"Slice length {length} < 0.");

        [DoesNotReturn]
        public static void ThrowSliceNegativeStartException(int start)
            => throw new ArgumentOutOfRangeException("start", $"Slice start {start} < 0.");

        [DoesNotReturn]
        public static void ThrowSliceIntegerOverflowException()
            => throw new ArgumentException("Slice start + length ({start + length}) causes an integer overflow");

        public static void ThrowIfIndexOutOfRangeException([DoesNotReturnIf(false)] bool condition)
        {
            if (condition == false)
            {
                throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the buffer.");
            }
        }

        public static void ThrowArgumentOutOfRangeException_IfNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, "The value must be non-negative.");
            }
        }

        public static void ThrowArgumentOutOfRangeException_IfNegativeZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, "The value must be positive and non-zero.");
            }
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_ArrayPlusOffTooSmall()
            => throw CreateArgumentException_ArrayPlusOffTooSmall();

        public static void ThrowInvalidOperationException_ReadOnlyCollectionNotCreated([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw new InvalidOperationException("The read-only collection is not created.");
            }

        }

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException<TKey>(TKey key)
            => throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumFailedVersion()
            => throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumOpCantHappen()
            => throw new InvalidOperationException("Enumeration has either not started or has already finished.");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumeratorNotValid()
            => throw new InvalidOperationException("Enumerator is not retrieved via a valid method.");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KeyPresent()
            => throw new InvalidOperationException("Key already present");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ModifyWhileBeingIterated_Map()
            => throw new InvalidOperationException("Cannot modify a map while it is being iterated");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ModifyWhileBeingIterated_Set()
            => throw new InvalidOperationException("Cannot modify a set while it is being iterated");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SetCountGreaterThanStartingOne()
            => throw new InvalidOperationException("Cannot set a count greater than the starting one");

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TrySetValueOnNotExistingKey()
            => throw new InvalidOperationException("Trying to set a value on a not existing key");

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException_KeyNotFound()
            => throw new KeyNotFoundException("Key not found");

        public static IndexOutOfRangeException CreateIndexOutOfRangeException_Collection()
            => new("Index was out of range. Must be non-negative and less than the size of the collection.");

        public static InvalidOperationException CreateInvalidOperationException_TypeNotCreatedCorrectly(string name)
            => new($"Type '{name}' was not created correctly.");

        public static InvalidOperationException CreateInvalidOperationException_CollectionNotCreated()
            => new("Collection was not created.");

        public static ArgumentException CreateArgumentException_CollectionNotCreated(string paramName)
            => new("Collection was not created.", paramName);

        public static ArgumentNullException CreateArgumentNullException(string paramName)
            => new(paramName);

        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_LengthNegative()
            => new("length", "The value must be non-negative.");

        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_IndexNegative()
            => new("index", "The value must be non-negative.");

        public static ArgumentException CreateArgumentException_SourceStartIndex_Length()
            => new("The number of elements from 'sourceStartIndex' to the end of the collection is lesser than 'length'.");

        public static ArgumentException CreateArgumentException_DestinationTooShort()
            => new("The destination span is too short to copy the requested number of elements.");

        public static ArgumentException CreateArgumentException_ArrayPlusOffTooSmall()
            => new("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
    }
}
