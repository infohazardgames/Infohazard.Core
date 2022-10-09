// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using System.Text.RegularExpressions;

namespace Infohazard.Core {
    /// <summary>
    /// Contains string processing utilities.
    /// </summary>
    public static class StringUtility {
        /// <summary>
        /// Splits a camel-case string into words separated by spaces.
        /// </summary>
        /// <remarks>
        /// Multiple consecutive capitals are considered the same word.
        /// </remarks>
        /// <param name="str">The string to split.</param>
        /// <param name="capitalizeFirst">Whether to capitalize the first letter.</param>
        /// <returns>The split string.</returns>
        public static string SplitCamelCase(this string str, bool capitalizeFirst = false) {
            // https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token/5796793
            string result = Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );

            if (result.Length > 0 && capitalizeFirst) {
                result = result[0].ToString().ToUpper() + result.Substring(1);
            }
            return result;
        }
    }
}