using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiterCore;

namespace WeatherSimulator.Client.LimiterInfrastructure;

public interface IRateLimiterManager
{
    RateLimiter<ActionExecutedContext> GetLimiter(string userName, string methodName);
    void RecreateActionLimiterStorage();
}