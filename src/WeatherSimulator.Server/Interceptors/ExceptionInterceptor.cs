using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using WeatherSimulator.Server.Exceptions;

namespace WeatherSimulator.Server.Interceptors;

public class ExceptionInterceptor:Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ArgumentOutOfRangeException e)
        {
            _logger.LogError(e, $"An error occured when calling {context.Method}");
            Status status = new Status(StatusCode.NotFound, e.Message);
            throw new RpcException(status);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"An error occured when calling {context.Method}");
            Status status = new Status(StatusCode.InvalidArgument, e.Message);
            throw new RpcException(status);
        }
        catch (MeasureNotFoundException e)
        {
            _logger.LogError(e, $"An error occured when calling {context.Method}");
            Status status = new Status(StatusCode.NotFound, e.Message);
            throw new RpcException(status);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"An error occured when calling {context.Method}");
            Status status = new Status(StatusCode.Internal, e.Message);
            throw new RpcException(status, e.Message);
        }
    }
}