// https://github.com/dotnet/dotnet/blob/4370ea16341331f045fa9b89cc46e03aed27195c/src/runtime/src/libraries/System.Text.Json/Common/JsonNamingPolicy.cs

// The MIT License (MIT)
//
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

using EncosyTower.SourceGen.Internals;

namespace EncosyTower.SourceGen
{
    /// <summary>
    /// Determines the naming policy used to convert a string-based name to another format, such as a camel-casing format.
    /// </summary>
    public abstract class NamingPolicy
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NamingPolicy"/>.
        /// </summary>
        protected NamingPolicy() { }

        /// <summary>
        /// Returns the naming policy for <c>PascalCasing</c>.
        /// </summary>
        /// <seealso cref="NameCasing.Pascal"/>
        public static NamingPolicy PascalCase { get; } = new PascalCaseNamingPolicy();

        /// <summary>
        /// Returns the naming policy for <c>camelCasing</c>.
        /// </summary>
        /// <seealso cref="NameCasing.Camel"/>
        public static NamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();

        /// <summary>
        /// Returns the naming policy for lower <c>snake-casing</c>.
        /// </summary>
        /// <seealso cref="NameCasing.SnakeLower"/>
        public static NamingPolicy SnakeCaseLower { get; } = new SnakeCaseLowerNamingPolicy();

        /// <summary>
        /// Returns the naming policy for upper <c>SNAKE-CASING</c>.
        /// </summary>
        /// <seealso cref="NameCasing.SnakeUpper"/>
        public static NamingPolicy SnakeCaseUpper { get; } = new SnakeCaseUpperNamingPolicy();

        /// <summary>
        /// Returns the naming policy for lower <c>kebab-casing</c>.
        /// </summary>
        /// <seealso cref="NameCasing.KebabLower"/>
        public static NamingPolicy KebabCaseLower { get; } = new KebabCaseLowerNamingPolicy();

        /// <summary>
        /// Returns the naming policy for upper <c>KEBAB-CASING</c>.
        /// </summary>
        /// <seealso cref="NameCasing.KebabUpper"/>
        public static NamingPolicy KebabCaseUpper { get; } = new KebabCaseUpperNamingPolicy();

        /// <summary>
        /// When overridden in a derived class, converts the specified name according to the policy.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public abstract string ConvertName(string name);
    }
}
