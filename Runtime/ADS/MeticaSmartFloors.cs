using UnityEngine;

namespace Metica.ADS
{
public class MeticaSmartFloors
{
    public MeticaUserGroup userGroup { get; private set; }
    public bool isSuccess { get; private set; }
    
    public MeticaSmartFloors(MeticaUserGroup userGroup, bool isSuccess)
    {
        this.userGroup = userGroup;
        this.isSuccess = isSuccess;
    }
}
}
