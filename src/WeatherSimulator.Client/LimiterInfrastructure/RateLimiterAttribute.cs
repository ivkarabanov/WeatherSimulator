using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WeatherSimulator.Client.Exceptions;

namespace WeatherSimulator.Client.LimiterInfrastructure;

public class RateLimiterAttribute: Attribute, IAsyncActionFilter
{
    private readonly IRateLimiterManager _limiterManager;

    public RateLimiterAttribute(IRateLimiterManager limiterManager)
    {
        _limiterManager = limiterManager;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var actionName = context.HttpContext.GetRouteValue("action") as string ?? string.Empty;
        var limiter = _limiterManager.GetLimiter(ipAddress, actionName);
        var result = await limiter.Invoke(async() =>await next(),context.HttpContext.RequestAborted);
        if (result.IsLimited)
        {
            throw new RateLimiterException("Слишком много запросов");
        }
    }
}