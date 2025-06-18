using System;
using System.Collections.Concurrent;
using MultiTenantAPI.Helpers;
namespace MultiTenantAPI.Services
{
    public class SessionService
    {
        // Simple in-memory session store: token => username and tenantId
        private ConcurrentDictionary<string, (string Username, string TenantId, DateTime Expiry)> _sessions = new();

        // public string CreateSession(string username, string tenantId)
        // {
        //     var sessionInfo = $"{username}|{tenantId}|{DateTime.UtcNow.AddHours(1):O}";

        //     var token = AesEncryptionHelper.Encrypt(sessionInfo);

        //     _sessions[token] = (username, tenantId, DateTime.UtcNow.AddHours(1));

        //     return token;
        // }

        public bool ValidateSession(string token, out string username, out string tenantId)
        {
            username = null;
            tenantId = null;

            if (_sessions.TryGetValue(token, out var session))
            {
                if (session.Expiry > DateTime.UtcNow)
                {
                    username = session.Username;
                    tenantId = session.TenantId;
                    return true;
                }
                else
                {
                    _sessions.TryRemove(token, out _);
                }
            }
            return false;
        }

        public void RemoveSession(string token)
        {
            _sessions.TryRemove(token, out _);
        }
    }
}
