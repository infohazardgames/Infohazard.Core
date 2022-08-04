using System.Text.RegularExpressions;

namespace Infohazard.Core.Runtime {
    public static class StringUtility {
        /// <summary>
        /// Splits a camel-case string into words separated by spaces.
        /// Multiple consecutive capitals are considered the same word.
        /// </summary>
        /// <remarks>
        /// From stackoverflow:
        /// https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token/5796793
        /// </remarks>
        /// <param name="str">The string to split.</param>
        /// <param name="capitalizeFirst">Whether to capitalize the first letter.</param>
        /// <returns>The split string.</returns>
        public static string SplitCamelCase(this string str, bool capitalizeFirst = false) {
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