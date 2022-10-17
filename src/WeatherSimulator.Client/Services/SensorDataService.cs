using Grpc.Core;
using Polly;
using WeatherSimulator.Client.Mappers;
using WeatherSimulator.Core.Abstractions.Repositories;
using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Client.Services;

public class SensorDataService:ISensorDataService
{
    private readonly IRepository<SensorMeasure> _measureRepository;
    private readonly ISensorListManager _sensorListManager;
    private readonly ISensorMeasureMapper _sensorMeasureMapper;
    private readonly ILogger<SensorDataService> _logger;
    private readonly WeatherSimulatorService.WeatherSimulatorServiceClient _weatherClientService;
    
    private const int LastValuesCount = 50;
    private const int RetryAttemptMs = 1000;
    private const int RetryAttemptCount = 3;
    
    private const string DefaultErrorMessage = "Произошла неизвестная ошибка";
    private const string LogGetCurrentErrorMessage = "Ошибка при получении последних измерений от rpc сервера";

    public SensorDataService(IRepository<SensorMeasure> measureRepository,
        ISensorListManager sensorListManager,
        ISensorMeasureMapper sensorMeasureMapper,
        WeatherSimulatorService.WeatherSimulatorServiceClient weatherClientService,
        ILogger<SensorDataService> logger)
    {
        _measureRepository = measureRepository;
        _sensorListManager = sensorListManager;
        _sensorMeasureMapper = sensorMeasureMapper;
        _weatherClientService = weatherClientService;
        _logger = logger;
    }
    public async Task<IEnumerable<SensorMeasure>> GetHistoryAsync(int sensorNumber)
    {
        Guid sensorId = GetSensorId(sensorNumber);
        try
        {
            var allHistory = await _measureRepository.GetAllAsync();
            var sensorMeasureData = allHistory.Where(x => x.SensorId == sensorId).OrderByDescending(x => x.LastUpdate)
                .Take(LastValuesCount);
            return sensorMeasureData;
        }
        catch (Exception ex)
        {
            var message = "Не удалось получить историю измерений";
            _logger.LogError(ex, message);
            throw new Exception(message, ex);
        }
    }
    
    public async Task<SensorMeasure> GetCurrentAsync(int sensorNumber)
    {
        Guid sensorId = GetSensorId(sensorNumber);
        SensorDataResponse sensorData = await GetSensorDataFromRpc(sensorId);

        var measure = _sensorMeasureMapper.Map(sensorData);
        return measure;
    }
    
    private async Task<SensorDataResponse> GetSensorDataFromRpc(Guid sensorId)
    {
        try
        {
            var policy = Policy.Handle<RpcException>(ex => 
                    ex.StatusCode != StatusCode.InvalidArgument &&
                    ex.StatusCode != StatusCode.Internal)
                .WaitAndRetryAsync(RetryAttemptCount ,retryAttempt =>
                        TimeSpan.FromMilliseconds(RetryAttemptMs),
                    (exception, attemptCount, timeSpan) =>
                    {
                        _logger.LogInformation($"Попытка повторного подключения {attemptCount}");
                    }
                );
            var response = 
                await policy.ExecuteAsync(async () => await _weatherClientService.GetSensorDataAsync(new SensorDataRequest()
            {
                SensorId = sensorId.ToString()
            }));
            return response;
        }
        catch(RpcException rpcException)
        {
            _logger.LogError(rpcException, LogGetCurrentErrorMessage);
            var errorMessage = FormErrorMessageByRpcStatusCode(rpcException);
            throw new Exception(errorMessage, rpcException);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogGetCurrentErrorMessage);
            throw new Exception(DefaultErrorMessage,ex);
        }
    }

    private string FormErrorMessageByRpcStatusCode(RpcException rpcException)
    {
        string message;

        switch (rpcException.StatusCode )
        {
            case StatusCode.Unavailable:
                message = "Сервер сбора информации недоступен";
                break;
            case StatusCode.Internal:
                message = "Сервер сбора информации не смог обработать запрос";
                break;
            case StatusCode.NotFound:
            case StatusCode.InvalidArgument:
                message = rpcException.Message;
                break;
            default:
                message = DefaultErrorMessage;
                break;
        }

        return message;
    }

    private Guid GetSensorId(int sensorNumber)
    {
        Guid sensorId;
        try
        {
            sensorId = _sensorListManager.GetSensorId(sensorNumber);
        }
        catch (Exception e)
        {
            var errorMessage = $"Не удалось получить id сенсора по номеру: {sensorNumber}";
            _logger.LogError(e, errorMessage);
            throw new ArgumentException(errorMessage, nameof(sensorNumber));
        }

        return sensorId;
    }
}