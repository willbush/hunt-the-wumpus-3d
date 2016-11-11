using System;

namespace HuntTheWumpus3d.Infrastructure
{
    public static class Rand
    {
        private static readonly Random Random = new Random();
        private static readonly object SyncLock = new object();

        public static int Next(int min, int max)
        {
            lock (SyncLock)
                return Random.Next(min, max);
        }

        internal static int Next(int max)
        {
            lock (SyncLock)
                return Random.Next(max);
        }
    }
}