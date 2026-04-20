#if !NET11_0_OR_GREATER

using UnityEngine.Scripting;

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This is needed due to NativeAOT which doesn't enable nullable globally yet
#nullable enable

// Allow use of union types on downlevel frameworks.
namespace System.Runtime.CompilerServices
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class UnionAttribute : Attribute { }
}

#endif
