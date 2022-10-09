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

using System.Collections.Generic;

namespace Infohazard.Core {
    /// <summary>
    /// Contains various static methods for working with sequences, extending the functionality of LINQ.
    /// </summary>
    public static class EnumerableUtility {
        /// <summary>
        /// Delegate for functions that perform both a select/map and where/filter operation.
        /// </summary>
        /// <typeparam name="T1">Input type of the function.</typeparam>
        /// <typeparam name="T2">Output type of the select operation.</typeparam>
        public delegate bool SelectWhereDelegate<in T1, T2>(T1 input, out T2 output);

        /// <summary>
        /// Perform select/map and where/filter operations on a sequence with a single function.
        /// </summary>
        /// <param name="input">The input sequence.</param>
        /// <param name="selectionDelegate">The delegate to use.</param>
        /// <typeparam name="T1">Input type of the delegate.</typeparam>
        /// <typeparam name="T2">Output type of the select operation.</typeparam>
        /// <returns>The resulting sequence.</returns>
        public static IEnumerable<T2> SelectWhere<T1, T2>(this IEnumerable<T1> input,
            SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) yield return value;
            }
        }
        
        /// <summary>
        /// Perform select/map and where/filter operations on a sequence with a single function,
        /// and returns the result of the select operation for the first passing element.
        /// </summary>
        /// <param name="input">The input sequence.</param>
        /// <param name="selectionDelegate">The delegate to use.</param>
        /// <typeparam name="T1">Input type of the delegate.</typeparam>
        /// <typeparam name="T2">Output type of the select operation.</typeparam>
        /// <returns>The result of the select operation for the first passing element.</returns>
        public static T2 FirstOrDefaultWhere<T1, T2>(this IEnumerable<T1> input,
            SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) return value;
            }

            return default;
        }
    }
}