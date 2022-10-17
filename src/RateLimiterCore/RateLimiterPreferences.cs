using Microsoft.Extensions.Logging;

namespace RateLimiterCore;

public class RateLimiterPreferences:IRateLimiterPreferences
{
    private const int MinRequestsPerInterval = 1;
    private const int MaxRequestsPerInterval = 1000;
    public RateLimiterPreferences(int requestsPerInterval = 5, int intervalSeconds = 1)
    {
        if (requestsPerInterval is < MinRequestsPerInterval or > MaxRequestsPerInterval)
        {
            throw new ArgumentException( 
                $"Параметр должен быть задан положительным числом от {MinRequestsPerInterval} до {MaxRequestsPerInterval}",
                nameof(requestsPerInterval));
        }
        if (intervalSeconds <= 0)
        {
            throw new ArgumentException( "Параметр должен быть задан положительным числом",nameof(intervalSeconds));
        }
        RequestsPerInterval = requestsPerInterval;
        IntervalSeconds = intervalSeconds;
    }
    public int RequestsPerInterval { get; init; }
    public int IntervalSeconds { get; init; }
    public bool DoNotLaunchTasks { get; set; }
    public bool SerialMode { get; set; }
}