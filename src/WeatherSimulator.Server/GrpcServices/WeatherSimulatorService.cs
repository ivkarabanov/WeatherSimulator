using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;
using WeatherSimulator.Server.Exceptions;
using WeatherSimulator.Server.Mappers;
using WeatherSimulator.Server.Services.Abstractions;
using static WeatherSimulator.Proto.WeatherSimulatorService;

namespace WeatherSimulator.Server.GrpcServices;

public class WeatherSimulatorService : WeatherSimulatorServiceBase
{
    private readonly IMeasureService _measureService;
    private readonly ISensorMeasureMapper _measureMapper;
    private readonly ILogger<WeatherSimulatorService> _logger;

    public WeatherSimulatorService(
        IMeasureService measureService,
        ISensorMeasureMapper measureMapper,
        ILogger<WeatherSimulatorService> logger)
    {
        _measureService = measureService;
        _measureMapper = measureMapper;
        _logger = logger;
    }

    public override async Task GetSensorsStream(IAsyncStreamReader<ToServerMessage> requestStream, IServerStreamWriter<SensorDataResponse> responseStream, ServerCallContext context)
    {
        await ProceedMessage(requestStream, responseStream, context.CancellationToken);
    }

    public override async Task<SensorDataResponse> GetSensorData(SensorDataRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SensorId, out Guid sensorId))
        {
            throw new ArgumentException($"Wrong SensorId {request.SensorId}");
        }
        
        var sensors = _measureService.GetAvailableSensors();
        var sensor = sensors.FirstOrDefault(x => x.Id == sensorId);
        if (sensor == null)
        {
            throw new ArgumentOutOfRangeException(nameof(request),$"Unavailable SensorId {sensorId}");
        }

        _logger.LogInformation($"Data requested from sensor {sensor.Id}");
        
        var measureInfo = _measureService.GetLastMeasure(sensor.Id);
        if (measureInfo == null)
        {
            throw new MeasureNotFoundException($"Нет последних данных от датчика {sensor.Id}");
        }
        
        var sensorData = _measureMapper.Map(measureInfo);
        return sensorData;
    }

    private async Task ProceedMessage(IAsyncStreamReader<ToServerMessage> requestStream,
        IServerStreamWriter<SensorDataResponse> responseStream,
        CancellationToken cancellationToken)
    {
        ConcurrentDictionary<Guid, Guid> sensorSubscriptionIds = new();
        while (await requestStream.MoveNext() && !cancellationToken.IsCancellationRequested)
        {
            var current = requestStream.Current;
            if(current.SubscribeSensorsIds is not null) 
                Subscribe(responseStream, sensorSubscriptionIds, cancellationToken, current);

            if(current.UnsubscribeSensorsIds is not null) 
                Unsubscribe(sensorSubscriptionIds, current);
        }
    }

    private void Subscribe(IServerStreamWriter<SensorDataResponse> responseStream, ConcurrentDictionary<Guid, Guid> sensorSubscriptionIds,
        CancellationToken cancellationToken, ToServerMessage current)
    {
        foreach (var id in current.SubscribeSensorsIds)
        {
            if (Guid.TryParse(id, out var tempId) && !sensorSubscriptionIds.TryGetValue(tempId, out Guid _))
            {
                var containsSub = sensorSubscriptionIds.TryGetValue(tempId, out Guid subscriptionId);
                if (!containsSub)
                {
                    sensorSubscriptionIds[tempId] = _measureService.SubscribeToMeasures(tempId,
                        async measure => await OnNewMeasure(responseStream, measure, cancellationToken), cancellationToken);
                    _logger.LogDebug("Subscribed!");
                }
            }
        }
    }

    private void Unsubscribe(ConcurrentDictionary<Guid, Guid> sensorSubscriptionIds, ToServerMessage current)
    {
        foreach (var id in current.UnsubscribeSensorsIds)
        {
            if (!Guid.TryParse(id, out var tempId) ||
                !sensorSubscriptionIds.TryGetValue(tempId, out Guid subscriptionId)) 
                continue;
            
            _measureService.UnsubscribeFromMeasures(tempId, subscriptionId);
            sensorSubscriptionIds.Remove(tempId, out _);
            _logger.LogDebug("Unsubscribed!");
        }
    }

    private async Task OnNewMeasure(IAsyncStreamWriter<SensorDataResponse> responseStream, SensorMeasure measure, CancellationToken cancellationToken)
    {
        var measureForResponse = _measureMapper.Map(measure);
        await responseStream.WriteAsync(measureForResponse, cancellationToken);
    }
}