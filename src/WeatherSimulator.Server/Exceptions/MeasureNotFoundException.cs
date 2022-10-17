using System;

namespace WeatherSimulator.Server.Exceptions;

public class MeasureNotFoundException:ApplicationException
{
    public MeasureNotFoundException(string message):base(message)
    {
    }
}