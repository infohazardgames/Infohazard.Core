// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;

namespace Infohazard.Core {
    /// <summary>
    /// Contains extensions to builtin randomization functionality.
    /// </summary>
    public static class RandomUtility {
        private static byte[] _randBuf = new byte[8];

        /// <summary>
        /// Generate a random long between min and max.
        /// </summary>
        public static long NextLong(this System.Random random, long min, long max) {
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