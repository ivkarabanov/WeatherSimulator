using Microsoft.Extensions.Options;
using WeatherSimulator.Client.Configurations;

namespace WeatherSimulator.Client.Services;

public class SensorListManager:ISensorListManager
{

    private readonly Dictionary<int, Guid> _sensorIds = new();
    public SensorListManager(IOptionsMonitor<WeatherClientConfiguration> configuration, ILogger<SensorListManager> logger)
    {
        foreach (var sensor in configuration.CurrentValue.Sensors)
        {
            _sensorIds.Add(sensor.SensorNumber, sensor.Id);
        }

        configuration.OnChange(RenewSensorDictionary);
    }

    private void RenewSensorDictionary(WeatherClientConfiguration configuration)
    {
        _sensorIds.Clear();        
        foreach (var sensor in configuration.Sensors)
        {
            _sensorIds.Add(sensor.SensorNumber, sensor.Id);
        }
    }
    
    public List<string> SubscribeList(int[] sensorNumbers)
    {
        return _sensorIds.Where(x => sensorNumbers.Contains(x.Key)).Select(x => x.Value.ToString()).ToList();
    }
    
    public List<string> UnsubscribeList(int[] sensorNumbers)
    {
        return _sensorIds.Where(x => !sensorNumbers.Contains(x.Key)).Select(x => x.Value.ToString()).ToList();
    }

    public Guid GetSensorId(int sensorNumber)
    {
        return _sensorIds[sensorNumber];
    }
}