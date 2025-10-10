using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class InitializeCallbackProxy : AndroidJavaProxy
    {
        private const string TAG = MeticaAds.TAG;
        private readonly TaskCompletionSource<MeticaAdsInitializationResult> _tcs;

        public InitializeCallbackProxy(TaskCompletionSource<MeticaAdsInitializationResult> tcs)
            : base("com.metica.MeticaInitCallback")
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
            _tcs = tcs;
        }

        // Called from Android when initialization succeeds
        public void onInitialized(bool adsEnabled)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onInitialized: {adsEnabled}");
            if (adsEnabled)
            {
                _tcs.SetResult(
                    new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Normal)
                );
            }
            else
            {
                _tcs.SetResult(
                    new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Holdout)
                );
            }
        }

        // Called from Android when initialization fails
        public void onFailed(string reason)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onFailed: {reason}");
            _tcs.SetResult(
                new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError)
            );
        }
    }
}
