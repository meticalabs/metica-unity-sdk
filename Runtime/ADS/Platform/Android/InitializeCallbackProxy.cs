using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS.Android
{
    internal class InitializeCallbackProxy : AndroidJavaProxy
    {
        private readonly TaskCompletionSource<MeticaAdsInitializationResult> _taskCompletionSource;

        public InitializeCallbackProxy(TaskCompletionSource<MeticaAdsInitializationResult> taskCompletionSource)
            : base("com.metica.ads.unity.InitializeCallback")
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public void onInitializeSuccess(bool isEnabled, string status)
        {
            Debug.Log($"[Metica] Initialize success: isEnabled={isEnabled}, status={status}");

            if (isEnabled)
            {
                var result = new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Normal);
                _taskCompletionSource.SetResult(result);
            }
            else
            {
                var result = new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Holdout);
                _taskCompletionSource.SetResult(result);
            }
        }

        public void onInitializeFailed(string error)
        {
            Debug.LogError($"[Metica] Initialize failed: {error}");

            // On failure, we could set a specific status or create a failed result
            // For now, let's use a hypothetical "Failed" status or handle as exception
            var result = new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError);
            _taskCompletionSource.SetResult(result);
        }
    }
}
