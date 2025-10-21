namespace Metica
{
public class MeticaSmartFloors
{
    public MeticaUserGroup UserGroup { get; private set; }
    public bool IsSuccess { get; private set; }
    
    public MeticaSmartFloors(MeticaUserGroup userGroup, bool isSuccess)
    {
        UserGroup = userGroup;
        IsSuccess = isSuccess;
    }
}
}
