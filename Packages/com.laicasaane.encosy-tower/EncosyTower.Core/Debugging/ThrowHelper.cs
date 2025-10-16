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
        public static void ThrowInvalidOperationException_EnumFailedVersion()
        {
            throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_EnumOpCantHappen()
        {
            throw new InvalidOperationException("Enumeration has either not started or has already finished.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KeyPresent()
        {
            throw new InvalidOperationException("Key already present");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ModifyWhileBeingIterated_Map()
        {
            throw new InvalidOperationException("Cannot modify a map while it is being iterated");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SetCountGreaterThanStartingOne()
        {
            throw new InvalidOperationException("Cannot set a count greater than the starting one");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TrySetValueOnNotExistingKey()
        {
            throw new InvalidOperationException("Trying to set a value on a not existing key");
        }

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException_KeyNotFound()
        {
            throw new KeyNotFoundException("Key not found");
        }
    }
}
