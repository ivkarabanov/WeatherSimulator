using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherSimulator.Server.Models;
using WeatherSimulator.Server.Services.Abstractions;

namespace WeatherSimulator.Server.Services.Background;

public class SensorPoolingService : IHostedService, IDisposable
{
    private readonly ILogger<SensorPoolingService> _logger;
    private readonly ConcurrentBag<Timer> _timers = new();
    private readonly IMeasureService _measureService;
    private readonly IMeasureSourceService _measureSourceService;

    public SensorPoolingService(IMeasureService measureService,
        IMeasureSourceService measureSourceService,
        ILogger<SensorPoolingService> logger)
    {
        _logger = logger;
        _measureSourceService = measureSourceService;
        _measureService = measureService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var registeredSensors = _measureService.GetAvailableSensors().ToArray();

        _logger.LogInformation("Старт сервиса, опрашивающего {sensorsCount} сенсоров.", registeredSensors.Length);

        for(var i = 0; i < registeredSensors.Length; i++)
        {
            var sensorItem = registeredSensors[i];
            _timers.Add(new Timer(PoolSensor, 
                sensorItem, 
                TimeSpan.Zero, 
                TimeSpan.FromMilliseconds(sensorItem.PollingFrequency)));
        }

        return Task.CompletedTask;
    }

    private void PoolSensor(object? state)
    {
        var sensorInfo = state as Sensor;
        if (sensorInfo == null)
            return;

        var measureInfo =_measureSourceService.MakeMeasure(sensorInfo);

        _measureService.OnNewMeasure(measureInfo);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        foreach(var timer in _timers)
        {
            timer?.Change(Timeout.Infinite, 0);
        }  

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach(var timer in _timers)
        {
            timer?.Dispose();
        }
    }
}
