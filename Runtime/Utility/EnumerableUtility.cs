// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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

        /// <summary>
        /// IndexOf operation for an IReadOnlyList, which is not included in .NET.
        /// </summary>
        /// <remarks>
        /// This cannot take into account custom equality comparers.
        /// </remarks>
        /// <param name="list">List to search.</param>
        /// <param name="item">Item to search for.</param>
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <returns>The index of the item, or -1 if not found.</returns>
        public static int IndexOf<T>(this IReadOnlyList<T> list, in T item) {
            for (int i = 0; i < list.Count; i++) {
                if (EqualityComparer<T>.Default.Equals(list[i], item)) return i;
            }

            return -1;
        }
    }
}
