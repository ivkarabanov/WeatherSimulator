using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Server.Mappers;

public interface ISensorMeasureMapper
{
    SensorDataResponse Map(SensorMeasure measure);
}