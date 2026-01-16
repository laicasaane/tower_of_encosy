// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Debugging
{
    public static class ThrowHelper
    {
        public static void ThrowIfIndexOutOfRangeException([DoesNotReturnIf(false)] bool condition)
        {
            if (condition == false)
            {
                throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the buffer.");
            }
        }

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

        public static InvalidOperationException CreateInvalidOperationException_CollectionNotCreated()
            => new("Collection was not created.");

        public static ArgumentException CreateArgumentException_CollectionNotCreated(string paramName)
            => new("Collection was not created.", paramName);

        public static ArgumentNullException CreateArgumentNullException(string paramName)
            => new(paramName);

        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_LengthNegative()
            => new("length", "The value must be non-negative.");

        public static ArgumentException CreateArgumentException_SourceStartIndex_Length()
            => new("The number of elements from 'sourceStartIndex' to the end of the collection is lesser than 'length'.");

        public static ArgumentException CreateArgumentException_DestinationTooShort()
            => new("The destination span is too short to copy the requested number of elements.");
    }
}
