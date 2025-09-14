#if !NET5_0_OR_GREATER

using UnityEngine.Scripting;

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This is needed due to NativeAOT which doesn't enable nullable globally yet
#nullable enable

// Allow use of init setters on downlevel frameworks.
namespace System.Runtime.CompilerServices
{
    [Preserve]
    public sealed class IsExternalInit
    {
        [Preserve]
        public IsExternalInit() { }
    }
}

#endif
