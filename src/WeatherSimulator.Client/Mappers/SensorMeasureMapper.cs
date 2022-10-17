using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Client.Mappers;

public class SensorMeasureMapper:ISensorMeasureMapper
{
    public SensorMeasure Map(SensorDataResponse response)
    {
        var measure = response.SensorData;
        return new SensorMeasure(Guid.Parse(measure.SensorId), measure.Temperature, measure.Humidity, measure.Co2);
    }
}