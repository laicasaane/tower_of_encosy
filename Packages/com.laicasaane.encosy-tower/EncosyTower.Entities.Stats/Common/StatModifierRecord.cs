// MIT License
// 
// Copyright (c) 2023 Philippe St-Amand
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.CompilerServices;

namespace EncosyTower.Entities.Stats
{
    public struct StatModifierRecord<TValuePair, TStat, TStatModifier, TStatModifierStack>
        where TValuePair : unmanaged, IStatValuePair
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
    {
        public StatModifierHandle handle;
        public TStatModifier modifier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifierRecord(StatModifierHandle handle, TStatModifier modifier)
        {
            this.handle = handle;
            this.modifier = modifier;
        }

        public readonly void Deconstruct(out StatModifierHandle handle, out TStatModifier modifier)
        {
            handle = this.handle;
            modifier = this.modifier;
        }
    }
}
