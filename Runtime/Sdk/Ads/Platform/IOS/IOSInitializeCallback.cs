using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Metica.ADS.IOS
{
    public class IOSInitializeCallback
    {
        private const string TAG = MeticaAds.TAG;
        private static TaskCompletionSource<MeticaAdsInitializationResult> _currentTcs;

        public IOSInitializeCallback(TaskCompletionSource<MeticaAdsInitializationResult> tcs)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
            _currentTcs = tcs;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnInitializeSuccessDelegate(bool enabled);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnInitializeFailedDelegate(string error);

        [DllImport("__Internal")]
        private static extern void ios_sdkInitialize(string apiKey, string appId, string userId, string version, string baseEndpoint, OnInitializeSuccessDelegate onSuccess, OnInitializeFailedDelegate onFail);

        public void InitializeSDK(string apiKey, string appId, string userId, string version, string baseEndpoint)
        {
            ios_sdkInitialize(
                apiKey,
                appId,
                userId,
                version,
                baseEndpoint,
                OnInitialized,
                OnFailed
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnInitializeSuccessDelegate))]
        private static void OnInitialized(bool adsEnabled)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onInitialized: {adsEnabled}");
            var tcs = _currentTcs;
            if (tcs != null)
            {
                if (adsEnabled)
                {
                    tcs.SetResult(
                        new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Normal)
                    );
                }
                else
                {
                    tcs.SetResult(
                        new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.Holdout)
                    );
                }
                _currentTcs = null;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnInitializeFailedDelegate))]
        private static void OnFailed(string reason)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onFailed: {reason}");
            var tcs = _currentTcs;
            if (tcs != null)
            {
                tcs.SetResult(
                    new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError)
                );
                _currentTcs = null;
            }
        }
    }
}