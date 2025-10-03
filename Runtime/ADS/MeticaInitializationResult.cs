namespace Metica.ADS
{
    public class MeticaInitializationResult
    {
        public MeticaAdsAssignmentStatus Status { get; }
     
        public MeticaInitializationResult(MeticaAdsAssignmentStatus status)
        {
            Status = status;
        }

        public bool IsMeticaAdsEnabled
        {
            get
            {
                // Check if the status indicates that Metica Ads should be enabled
                if (Status == MeticaAdsAssignmentStatus.Normal)
                {
                    return true;
                }

                // For all other status values (i.e. Holdout and HoldoutDueToError), Metica Ads should be disabled
                return false;
            }
        }
    }
}
