using UnityEngine;

namespace Metica
{
public class MeticaInitResponse
{
    public MeticaSmartFloors SmartFloors { get; private set; }

    public MeticaInitResponse(MeticaSmartFloors smartFloors)
    {
        SmartFloors = smartFloors;
    }

    public static MeticaInitResponse FromJson(string json)
    {
        return JsonUtility.FromJson<MeticaInitResponse>(json);
    }
}
}
