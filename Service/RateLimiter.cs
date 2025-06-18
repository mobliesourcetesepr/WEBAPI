// public class RateLimitService
// {
//     private readonly Dictionary<string, (int Count, DateTime ResetTime)> _limits = new();
//     private readonly int _limit = 5;
//     private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

//     public bool IsRateLimited(string key)
//     {
//         var now = DateTime.Now;

//         if (_limits.TryGetValue(key, out var entry))
//         {
//             if (now < entry.ResetTime)
//             {
//                 if (entry.Count >= _limit)
//                     return true;

//                 _limits[key] = (entry.Count + 1, entry.ResetTime);
//                 return false;
//             }
//         }

//         _limits[key] = (1, now.Add(_window));
//         return false;
//     }
// }
public class RateLimitService
{
    private readonly Dictionary<string, (int Count, DateTime ResetTime)> _limits = new();
    private readonly Dictionary<string, (int Limit, TimeSpan Window)> _rules = new();

    public RateLimitService()
    {
        // Define per-endpoint rate limits
        _rules["/auth/token"] = (5, TimeSpan.FromMinutes(1));       // e.g., token
        _rules["/auth/dashboard"] = (3, TimeSpan.FromMinutes(1));     // e.g., dashboard

    }

    public bool IsRateLimited(string key, string endpoint)
    {
        var now = DateTime.Now;
        var rule = _rules.ContainsKey(endpoint) ? _rules[endpoint] : (Limit: 10, Window: TimeSpan.FromMinutes(1));

        var fullKey = $"{key}:{endpoint}";

        if (_limits.TryGetValue(fullKey, out var entry))
        {
            if (now < entry.ResetTime)
            {
                if (entry.Count >= rule.Limit)
                    return true;

                _limits[fullKey] = (entry.Count + 1, entry.ResetTime);
                return false;
            }
        }

        _limits[fullKey] = (1, now.Add(rule.Window));
        return false;
    }
}
