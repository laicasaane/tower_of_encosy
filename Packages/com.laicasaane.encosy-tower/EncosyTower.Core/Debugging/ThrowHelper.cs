// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Debugging
{
    public static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
        {
            throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
        {
            throw new InvalidOperationException("Enumeration has either not started or has already finished.");
        }
    }
}
