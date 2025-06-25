using AgentCreation.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class ConnectionMapping
    {
        private static readonly Dictionary<string, string> _connections = new();

      

        public static void Add(string username, string connectionId)
        {
            lock (_connections)
            {
                
                _connections[username] = connectionId;
                Console.WriteLine(_connections[username] = connectionId);
            }
        }

       public static void Remove(string username)
    {
        lock (_connections)
        {
            _connections.Remove(username, out _); // ✅ Corrected line
        }
    }

        public static bool TryGetConnection(string username, out string connId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(username, out connId);
            }
        }

    public static List<string> GetConnectionIdsByUsernames(IEnumerable<string> usernames)
    {
        lock (_connections)
        {
            var Found = usernames
                .Select(u => _connections.TryGetValue(u, out var cid) ? cid : null)
                .Where(cid => cid != null)
                .ToList()!;

            Console.WriteLine("🔍 Matching connection IDs:");
            foreach (var id in Found)
            {
                Console.WriteLine($"➡️ {id}");
            }
            return Found;
        }
        
        }

        public static string? GetUsernameByConnectionId(string connectionId)
        {
            lock (_connections)
            {
                return _connections.FirstOrDefault(x => x.Value == connectionId).Key;
            }
        }
    }