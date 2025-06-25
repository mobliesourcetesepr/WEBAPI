using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace AgentCreation.Hubs
{
        public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
            if (!string.IsNullOrEmpty(username))
            {
                _userConnections[username] = Context.ConnectionId;
                 ConnectionMapping.Add(username, Context.ConnectionId);
                Console.WriteLine($"✅ Connected: {username}, ConnID: {Context.ConnectionId}");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var item = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                _userConnections.TryRemove(item.Key, out _);
                Console.WriteLine($"❌ Disconnected: {item.Key}");
            }
            return base.OnDisconnectedAsync(exception);
        }

        // 🔹 Get all connection IDs for a given list of usernames
        public static List<string> GetConnectionIdsByUsernames(IEnumerable<string> usernames)
        {
            return usernames
                .Select(u => _userConnections.TryGetValue(u, out var connId) ? connId : null)
                .Where(cid => cid != null)
                .ToList()!;
        }

        // 🔹 Optional: single user lookup
        public static string? GetConnectionId(string username)
        {
            _userConnections.TryGetValue(username, out var connId);
            return connId;
        }
    }    
}
