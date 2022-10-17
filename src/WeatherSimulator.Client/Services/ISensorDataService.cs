using WeatherSimulator.Core.Models;

namespace WeatherSimulator.Client.Services;

public interface ISensorDataService
{
    Task<IEnumerable<SensorMeasure>> GetHistoryAsync(int sensorNumber);
    Task<SensorMeasure> GetCurrentAsync(int sensorNumber);
}