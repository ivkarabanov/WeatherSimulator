namespace WeatherSimulator.Client.Exceptions;

public class RateLimiterException:InvalidOperationException
{
    public RateLimiterException(string message):base(message)
    {
    }
}