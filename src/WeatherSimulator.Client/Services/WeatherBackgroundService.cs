using Grpc.Core;
using Microsoft.Extensions.Options;
using Polly;
using WeatherSimulator.Client.Configurations;
using WeatherSimulator.Client.Mappers;
using WeatherSimulator.Core.Abstractions.Repositories;
using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Client.Services;

public class WeatherBackgroundService:BackgroundService
{
    private readonly WeatherSimulatorService.WeatherSimulatorServiceClient _weatherClientService;
    private readonly ISensorListManager _sensorListManager;
    private readonly IRepository<SensorMeasure> _measureRepository;
    private readonly ILogger<WeatherBackgroundService> _logger;
    private readonly ISensorMeasureMapper _sensorMeasureMapper;
    private int[] _sensorNumbers;
    private int _secondsToRenew;
    private const int ProgressiveRetryDelayMs = 100, MaxDelayMs=60000;

    public WeatherBackgroundService(WeatherSimulatorService.WeatherSimulatorServiceClient weatherClientService,
         ISensorListManager sensorListManager,
         IRepository<SensorMeasure> measureRepository,
         ILogger<WeatherBackgroundService> logger,
         IOptionsMonitor<SubscriptionConfiguration> optionsMonitor,
         ISensorMeasureMapper sensorMeasureMapper)
    {
        _weatherClientService = weatherClientService;
        _sensorListManager = sensorListManager;
        _measureRepository = measureRepository;
        _logger = logger;
        _sensorMeasureMapper = sensorMeasureMapper;
        optionsMonitor.OnChange(TrackOptionsChange);
        _sensorNumbers = optionsMonitor.CurrentValue.SensorNumbers ?? Array.Empty<int>();
        _secondsToRenew = optionsMonitor.CurrentValue.RenewSeconds;
    }

    private void TrackOptionsChange(SubscriptionConfiguration options, string listener)
    {
        _sensorNumbers = options.SensorNumbers ?? Array.Empty<int>();
        _secondsToRenew = options.RenewSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource? tokenSource = null;
        var policy = Policy.Handle<Exception>()
            .WaitAndRetryForeverAsync(retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Min(retryAttempt * ProgressiveRetryDelayMs, MaxDelayMs)),
                (exception, attemptCount, timeSpan) =>
                {
                    _logger.LogInformation($"Попытка повторного подключения {attemptCount}");
                }
            );

            await policy.ExecuteAsync(async () =>
            {                
                tokenSource?.Cancel();
                tokenSource = new CancellationTokenSource();
                var subscribeCancellationToken = tokenSource.Token;
                var stream = _weatherClientService.GetSensorsStream();
                var responseTask = ReadResponse(subscribeCancellationToken, stream);
                var subscribeTask = Subscribe(subscribeCancellationToken, stream);

                await Task.WhenAny(subscribeTask, responseTask);
                throw new Exception();
            });
    }

    private async Task Subscribe(CancellationToken cancellationToken, AsyncDuplexStreamingCall<ToServerMessage, SensorDataResponse> stream)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = new ToServerMessage()
                {
                    SubscribeSensorsIds = {_sensorListManager.SubscribeList(_sensorNumbers!)},
                    UnsubscribeSensorsIds = {_sensorListManager.UnsubscribeList(_sensorNumbers!)}
                };
                await stream.RequestStream.WriteAsync(message, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Возникла ошибка при отправке запроса {Message}", e.Message);
                throw;
            }

            await Task.Delay(TimeSpan.FromSeconds(_secondsToRenew), cancellationToken);
        }
    }

    private async Task ReadResponse(CancellationToken cancellationToken, AsyncDuplexStreamingCall<ToServerMessage, SensorDataResponse> stream)
    {
        await foreach (var request in stream.ResponseStream.ReadAllAsync(cancellationToken: cancellationToken))
        {
            try
            {
                var measure = _sensorMeasureMapper.Map(request);
                await _measureRepository.InsertAsync(measure);
                _logger.LogInformation($"Пришел ответ от { request.SensorData.SensorId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Возникла ошибка при обработке ответа {Message}", e.Message);
                throw;
            }
        }
    }
}
