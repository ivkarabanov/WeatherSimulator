namespace WeatherSimulator.Client.LimiterInfrastructure;

public class LimitersRenewBackgroundService:BackgroundService
{
    private readonly IRateLimiterManager _rateLimiterManager;

    public LimitersRenewBackgroundService(IRateLimiterManager rateLimiterManager)
    {
        _rateLimiterManager = rateLimiterManager;
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var delayMilliseconds = 2* 60 * 60 * 1000; //2 часа
            await Task.Delay(delayMilliseconds);
            _rateLimiterManager.RecreateActionLimiterStorage();
        }
    }
}