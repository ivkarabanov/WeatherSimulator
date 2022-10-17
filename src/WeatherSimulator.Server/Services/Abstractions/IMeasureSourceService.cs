using WeatherSimulator.Core.Models;
using WeatherSimulator.Server.Models;

namespace WeatherSimulator.Server.Services.Abstractions;

public interface IMeasureSourceService
{
    SensorMeasure MakeMeasure(Sensor sensorInfo);
}