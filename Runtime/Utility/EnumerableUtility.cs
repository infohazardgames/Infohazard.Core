using System.Collections.Generic;

namespace Infohazard.Core.Runtime {
    public static class EnumerableUtility {
        public delegate bool SelectWhereDelegate<in T1, T2>(T1 input, out T2 output);

        public static IEnumerable<T2> SelectWhere<T1, T2>(this IEnumerable<T1> input,
            SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) yield return value;
            }
        }

        public static T2 FirstOrDefaultWhere<T1, T2>(this IEnumerable<T1> input,
            SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) return value;
            }

            return default;
        }
    }
}