// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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