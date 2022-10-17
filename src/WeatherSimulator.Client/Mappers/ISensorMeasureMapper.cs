using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Client.Mappers;

public interface ISensorMeasureMapper
{
    SensorMeasure Map(SensorDataResponse measure);
}