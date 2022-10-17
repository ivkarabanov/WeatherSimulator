using WeatherSimulator.Core.Models.Enums;

namespace WeatherSimulator.Server.Mappers;

public interface ISensorLocationTypeMapper
{
    Proto.SensorLocationType FromEntityToProto(SensorLocationType locationType);
}