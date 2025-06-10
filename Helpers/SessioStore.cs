using System;
using System.Collections.Concurrent;

namespace WEBAPI.Helpers
{
    public static class SessionStore
    {
        private static readonly ConcurrentDictionary<string, DateTime> LastAccessedMap = new();

        public static void UpdateLastAccess(string token)
        {
            LastAccessedMap[token] = DateTime.UtcNow;
        }

        public static bool IsTokenAlive(string token)
        {
            return LastAccessedMap.TryGetValue(token, out var lastAccessed) &&
                   (DateTime.UtcNow - lastAccessed <= TimeSpan.FromMinutes(10));
        }
    }
}
