namespace Metica
{
public class MeticaInitResponse
{
    public MeticaSmartFloors SmartFloors { get; private set; }

    public MeticaInitResponse(MeticaSmartFloors smartFloors)
    {
        SmartFloors = smartFloors;
    }
}
}
