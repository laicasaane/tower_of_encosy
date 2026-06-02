//

// The MIT License (MIT)
// https://github.com/dotnet/dotnet/blob/e0aa94285c859b6ab5061903e91dafffeee8094c/src/runtime/src/libraries/System.Text.Json/Common/JsonSeparatorNamingPolicy.cs#L11

// Copyright (c) .NET Foundation and Contributors
//
// All rights reserved.
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

#nullable enable

using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace EncosyTower.SourceGen.Internals
{
    internal abstract class JsonSeparatorNamingPolicy : NamingPolicy
    {
        private readonly char? _separator;
        private readonly WordCasing _wordCasing;

        internal JsonSeparatorNamingPolicy(bool lowercase, char separator)
        {
            Debug.Assert(char.IsPunctuation(separator));

            _separator = separator;
            _wordCasing = lowercase ? WordCasing.LowerCase : WordCasing.UpperCase;
        }

        internal JsonSeparatorNamingPolicy(WordCasing wordCasing)
        {
            Debug.Assert(wordCasing is WordCasing.PascalCase);

            _separator = null;
            _wordCasing = wordCasing;
        }

        public sealed override string ConvertName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return ConvertNameCore(_separator, _wordCasing, name.AsSpan());
        }

        private static string ConvertNameCore(char? separator, WordCasing wordCasing, ReadOnlySpan<char> chars)
        {
            char[]? rentedBuffer = null;

            // While we can't predict the expansion factor of the resultant string,
            // start with a buffer that is at least 20% larger than the input.
            int initialBufferLength = (int)(1.2 * chars.Length);
            Span<char> destination = initialBufferLength <= NamingConstants.STACKALLOC_CHAR_THRESHOLD
                ? stackalloc char[NamingConstants.STACKALLOC_CHAR_THRESHOLD]
                : (rentedBuffer = ArrayPool<char>.Shared.Rent(initialBufferLength));

            SeparatorState state = SeparatorState.NotStarted;
            int charsWritten = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                // NB this implementation does not handle surrogate pair letters
                // cf. https://github.com/dotnet/runtime/issues/90352

                char current = chars[i];
                UnicodeCategory category = char.GetUnicodeCategory(current);

                switch (category)
                {
                    case UnicodeCategory.UppercaseLetter:

                        bool isWordBoundary = false;

                        switch (state)
                        {
                            case SeparatorState.NotStarted:
                                isWordBoundary = true;
                                break;

                            case SeparatorState.LowercaseLetterOrDigit:
                            case SeparatorState.SpaceSeparator:
                                // An uppercase letter following a sequence of lowercase letters or spaces
                                // denotes the start of a new grouping: emit a separator character.
                                isWordBoundary = true;
                                if (separator.HasValue)
                                {
                                    WriteChar(separator.Value, ref destination);
                                }
                                break;

                            case SeparatorState.UppercaseLetter:
                                // We are reading through a sequence of two or more uppercase letters.
                                // Uppercase letters are grouped together with the exception of the
                                // final letter, assuming it is followed by lowercase letters.
                                // For example, the value 'XMLReader' should render as 'xml_reader',
                                // however 'SHA512Hash' should render as 'sha512-hash'.
                                if (i + 1 < chars.Length && char.IsLower(chars[i + 1]))
                                {
                                    isWordBoundary = true;
                                    if (separator.HasValue)
                                    {
                                        WriteChar(separator.Value, ref destination);
                                    }
                                }
                                break;

                            default:
                                Debug.Fail($"Unexpected state {state}");
                                break;
                        }

                        current = wordCasing switch {
                            WordCasing.LowerCase => char.ToLowerInvariant(current),
                            WordCasing.PascalCase => isWordBoundary ? current : char.ToLowerInvariant(current),
                            _ => current,
                        };

                        WriteChar(current, ref destination);
                        state = SeparatorState.UppercaseLetter;
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:

                        bool isWordStart = state is SeparatorState.SpaceSeparator or SeparatorState.NotStarted;

                        if (state is SeparatorState.SpaceSeparator)
                        {
                            // Normalize preceding spaces to one separator.
                            if (separator.HasValue)
                            {
                                WriteChar(separator.Value, ref destination);
                            }
                        }

                        if (category is UnicodeCategory.LowercaseLetter)
                        {
                            current = wordCasing switch {
                                WordCasing.UpperCase => char.ToUpperInvariant(current),
                                WordCasing.PascalCase => isWordStart ? char.ToUpperInvariant(current) : current,
                                _ => current,
                            };
                        }

                        WriteChar(current, ref destination);
                        state = SeparatorState.LowercaseLetterOrDigit;
                        break;

                    case UnicodeCategory.SpaceSeparator:
                        // Space characters are trimmed from the start and end of the input string
                        // but are normalized to separator characters if between letters.
                        if (state != SeparatorState.NotStarted)
                        {
                            state = SeparatorState.SpaceSeparator;
                        }
                        break;

                    default:
                        // Non-alphanumeric characters (including the separator character and surrogates)
                        // are written as-is to the output and reset the separator state.
                        // E.g. 'ABC???def' maps to 'abc???def' in snake_case.

                        WriteChar(current, ref destination);
                        state = SeparatorState.NotStarted;
                        break;
                }
            }

            string result = destination.Slice(0, charsWritten).ToString();

            if (rentedBuffer is not null)
            {
                destination.Slice(0, charsWritten).Clear();
                ArrayPool<char>.Shared.Return(rentedBuffer);
            }

            return result;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void WriteChar(char value, ref Span<char> destination)
            {
                if (charsWritten == destination.Length)
                {
                    ExpandBuffer(ref destination);
                }

                destination[charsWritten++] = value;
            }

            void ExpandBuffer(ref Span<char> destination)
            {
                int newSize = checked(destination.Length * 2);
                char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
                destination.CopyTo(newBuffer);

                if (rentedBuffer is not null)
                {
                    destination.Slice(0, charsWritten).Clear();
                    ArrayPool<char>.Shared.Return(rentedBuffer);
                }

                rentedBuffer = newBuffer;
                destination = rentedBuffer;
            }
        }

        private enum SeparatorState
        {
            NotStarted,
            UppercaseLetter,
            LowercaseLetterOrDigit,
            SpaceSeparator,
        }

        internal enum WordCasing
        {
            LowerCase,
            UpperCase,
            PascalCase,
        }
    }
}
