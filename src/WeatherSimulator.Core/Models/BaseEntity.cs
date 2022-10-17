using System;

namespace WeatherSimulator.Core.Models;

public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime LastUpdate { get; protected set; }
}