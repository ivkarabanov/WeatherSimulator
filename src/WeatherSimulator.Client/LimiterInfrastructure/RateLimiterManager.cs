using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiterCore;

namespace WeatherSimulator.Client.LimiterInfrastructure;

public class RateLimiterManager:IRateLimiterManager
{
    private const int RequestsPerInterval = 5;
    private const int IntervalSeconds = 120;
    private readonly object _locker = new object();
    private Dictionary<LimiterKey, RateLimiter<ActionExecutedContext>> _actionLimiters = new Dictionary<LimiterKey, RateLimiter<ActionExecutedContext>>();

    public RateLimiter<ActionExecutedContext> GetLimiter(string userName, string methodName)
    {
        var key = new LimiterKey(userName, methodName);
        lock (_locker)
        {
            if (_actionLimiters.ContainsKey(key))
            {
                return _actionLimiters[key];
            }

            var rateLimiter =
                new RateLimiter<ActionExecutedContext>(new RateLimiterPreferences(RequestsPerInterval, IntervalSeconds));
            _actionLimiters[key] = rateLimiter;        
            return rateLimiter;
        }
    }

    public void RecreateActionLimiterStorage()
    {
        lock (_locker)
        {
            _actionLimiters = new Dictionary<LimiterKey, RateLimiter<ActionExecutedContext>>();
        }
    }
}