namespace WeatherSimulator.Client.Services;

public interface ISensorListManager
{
    List<string> SubscribeList(int[] sensorNumbers);
    List<string> UnsubscribeList(int[] sensorNumbers);
    Guid GetSensorId(int sensorNumber);
}