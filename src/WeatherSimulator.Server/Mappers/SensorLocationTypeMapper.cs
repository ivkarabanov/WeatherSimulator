using WeatherSimulator.Core.Models.Enums;
using System;
using System.Linq;
using Enum = System.Enum;

namespace WeatherSimulator.Server.Mappers;

public class SensorLocationTypeMapper:ISensorLocationTypeMapper
{
    public Proto.SensorLocationType FromEntityToProto(SensorLocationType locationType)
    {
        var destinationNames = System.Enum.GetNames<Proto.SensorLocationType>();
        var destinationName = destinationNames.FirstOrDefault(locationType.ToString());
        if (!Enum.TryParse(destinationName, out Proto.SensorLocationType value))
        {
            throw new ArgumentException(
                "Не удалось конвертировать enum значение SensorLocationType в Proto.SensorLocationType",
                nameof(locationType));
        }

        return value;
    }
}