namespace WeatherSimulator.Client.LimiterInfrastructure;

public struct LimiterKey
{
    public LimiterKey(string userName, string methodName)
    {
        UserName = userName;
        MethodName = methodName;
    }

    private string UserName { get; set; }
    private string MethodName { get; set; }
    
    public override int GetHashCode()
    {
        return UserName.GetHashCode() ^ MethodName.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is LimiterKey)
        {
            LimiterKey compositeKey = (LimiterKey)obj;

            return UserName == compositeKey.UserName &&
                    MethodName == compositeKey.MethodName;
        }

        return false;
    }
}