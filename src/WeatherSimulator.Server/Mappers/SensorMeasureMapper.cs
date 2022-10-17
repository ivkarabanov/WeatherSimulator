using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Server.Mappers;

public class SensorMeasureMapper:ISensorMeasureMapper
{
    private readonly ISensorLocationTypeMapper _sensorLocationTypeMapper;

    public SensorMeasureMapper(ISensorLocationTypeMapper sensorLocationTypeMapper)
    {
        _sensorLocationTypeMapper = sensorLocationTypeMapper;
    }
    public SensorDataResponse Map(SensorMeasure measure)
    {
        return new SensorDataResponse()
        {
            SensorData = new SensorData()
            {
                SensorId = measure.SensorId.ToString(),
                Temperature = measure.Temperature,
                Humidity = measure.Humidity,
                Co2 = measure.CO2,
                LocationType = _sensorLocationTypeMapper.FromEntityToProto(measure.LocationType)
            }

        };
    }
}