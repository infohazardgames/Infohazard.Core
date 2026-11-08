// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;

namespace Infohazard.Core {
    /// <summary>
    /// Contains extensions to builtin randomization functionality.
    /// </summary>
    public static class RandomUtility {
        private static byte[] _randBuf = new byte[8];

        /// <summary>
        /// Generate a random long greater than or equal to min and less than max.
        /// </summary>
        /// <remarks>
        /// If min equals max, min is returned.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">If max is less than min.</exception>
        public static long NextLong(this System.Random random, long min, long max) {
            if (max < min) {
                throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
            }

            // Match System.Random.Next(min, max), which returns min for an empty range.
            if (max == min) return min;

            ulong uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                random.NextBytes(_randBuf);
                ulongRand = (ulong)BitConverter.ToInt64(_randBuf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        /// <summary>
        /// Generate a random ulong.
        /// </summary>
        public static ulong NextUlong(this System.Random random) {
            random.NextBytes(_randBuf);
            return BitConverter.ToUInt64(_randBuf, 0);
        }
    }
}
