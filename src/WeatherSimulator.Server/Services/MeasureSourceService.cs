using System;
using WeatherSimulator.Core.Models;
using WeatherSimulator.Core.Models.Enums;
using WeatherSimulator.Server.Models;
using WeatherSimulator.Server.Services.Abstractions;

namespace WeatherSimulator.Server.Services;

public class MeasureSourceService:IMeasureSourceService
{
    public SensorMeasure MakeMeasure(Sensor sensorInfo)
    {
        var randGen = new Random();
        var measureInfo = new SensorMeasure(sensorInfo.Id, 
            temperature: randGen.Next(200, 320) / 10,
            humidity: randGen.Next(40, 60),
            co2: sensorInfo.LocationType == SensorLocationType.External ? randGen.Next(350, 360) : randGen.Next(400, 600));
        return measureInfo;
    }
}