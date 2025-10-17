namespace Metica.ADS
{
[System.Obsolete(
    "Use MeticaUserGroup instead. This enum has been superseded by MeticaUserGroup which provides clearer trial/holdout assignment status.")]
public enum MeticaAdsAssignmentStatus
{
    Normal,
    Holdout,
    HoldoutDueToError,
}
}
