namespace Metica.ADS
{
public class MeticaInitResponse
{
    public MeticaSmartFloors SmartFloors { get; }

    public MeticaInitResponse(MeticaSmartFloors smartFloors)
    {
        SmartFloors = smartFloors;
    }
}
}
