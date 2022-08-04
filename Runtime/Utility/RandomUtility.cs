using System;

namespace Infohazard.Core.Runtime {
    public static class RandomUtility {
        private static byte[] _randBuf = new byte[8];

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

        public static ulong NextUlong(this System.Random random) {
            random.NextBytes(_randBuf);
            return BitConverter.ToUInt64(_randBuf, 0);
        }
    }
}