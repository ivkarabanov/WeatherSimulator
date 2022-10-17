using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WeatherSimulator.Client.Exceptions;

public class StatusCodeExceptionAttribute: ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentException ex:
                HandleEx(context, ex.Message, HttpStatusCode.BadRequest);
                return;
            case RateLimiterException ex:
                HandleEx(context, ex.Message, HttpStatusCode.TooManyRequests);
                return;
            case { } ex:
                HandleEx(context, "Возникла ошибка. Обратитесь к разработчикам", HttpStatusCode.InternalServerError);
                return;

        }
        base.OnException(context);
    }

    private void HandleEx(ExceptionContext context, string message, HttpStatusCode code)
    {
        context.Result = new ObjectResult(new
        {
            context.Exception.Message
        })
        {
            StatusCode = (int)code
        };
    }
}