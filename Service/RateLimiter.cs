public class RateLimitService
{
    private readonly Dictionary<string, (int Count, DateTime ResetTime)> _limits = new();
    private readonly int _limit = 5;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

    public bool IsRateLimited(string key)
    {
        var now = DateTime.Now;

        if (_limits.TryGetValue(key, out var entry))
        {
            if (now < entry.ResetTime)
            {
                if (entry.Count >= _limit)
                    return true;

                _limits[key] = (entry.Count + 1, entry.ResetTime);
                return false;
            }
        }

        _limits[key] = (1, now.Add(_window));
        return false;
    }
}
