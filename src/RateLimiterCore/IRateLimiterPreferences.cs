using Microsoft.Extensions.Logging;

namespace RateLimiterCore;

public interface IRateLimiterPreferences
{
    int RequestsPerInterval { get; init; }
    int IntervalSeconds{ get;init; }
    bool DoNotLaunchTasks { get; set; }
    bool SerialMode { get; set; }
}