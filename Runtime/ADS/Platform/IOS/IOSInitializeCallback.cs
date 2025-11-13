using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Metica.ADS.IOS
{
    public class IOSInitializeCallback
    {
        private const string TAG = MeticaAds.TAG;
        private readonly TaskCompletionSource<MeticaAdsInitializationResult> _tcs;

        public IOSInitializeCallback(TaskCompletionSource<MeticaAdsInitializationResult> tcs)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
            _tcs = tcs;
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
                onInitialized,
                onFailed
            );
        }

        public void

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

        public void onFailed(string reason)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onFailed: {reason}");
            _tcs.SetResult(
                new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError)
            );
        }
    }
}