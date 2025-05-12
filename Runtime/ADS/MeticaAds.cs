#nullable enable

using UnityEngine;
using System;
using System.Threading.Tasks;
using Metica.SDK;

namespace Metica.ADS 
{
    public static class MeticaAds
    {   
        public const string TAG = "MeticaUnityPlugin";
        private static readonly PlatformDelegate platformDelegate;

        static MeticaAds()
        {
#if UNITY_ANDROID
            platformDelegate = new AndroidDelegate();
#endif

#if UNITY_IOS
            // TODO
            platformDelegate = new IOSDelegate();
#endif
            platformDelegate.InterstitialAdLoadSuccess += MeticaAdsCallbacks.Interstitial.OnAdLoadSuccessInternal;
            platformDelegate.InterstitialAdLoadFailed += MeticaAdsCallbacks.Interstitial.OnAdLoadFailedInternal;
            platformDelegate.InterstitialAdShowSuccess += MeticaAdsCallbacks.Interstitial.OnAdShowSuccessInternal;
            platformDelegate.InterstitialAdShowFailed += MeticaAdsCallbacks.Interstitial.OnAdShowFailedInternal;
            platformDelegate.InterstitialAdHidden += MeticaAdsCallbacks.Interstitial.OnAdHiddenInternal;
            platformDelegate.InterstitialAdClicked += MeticaAdsCallbacks.Interstitial.OnAdClickedInternal;
        }

        public static void Initialize()
        {
            platformDelegate.Initialize(MeticaSdk.ApiKey, MeticaSdk.AppId, MeticaSdk.CurrentUserId, MeticaSdk.BaseEndpoint);
        }
        public static void SetLogEnabled(bool logEnabled) 
        {
            platformDelegate.SetLogEnabled(logEnabled);
        }
        
        public static async Task<bool> LoadInterstitialAsync()
        {
            return await platformDelegate.LoadInterstitialAsync();
        }
        public static void ShowInterstitial()
        {
            platformDelegate.ShowInterstitial();
        }

        public static bool IsInterstitialReady()
        {
            return platformDelegate.IsInterstitialReady();
        }
    }

    internal interface PlatformDelegate {
        // Events for interstitial ad lifecycle callbacks
        public event Action<string> InterstitialAdLoadSuccess;
        public event Action<string, string> InterstitialAdLoadFailed;
        public event Action<string> InterstitialAdShowSuccess;
        public event Action<string, string> InterstitialAdShowFailed;
        public event Action<string> InterstitialAdHidden;
        public event Action<string> InterstitialAdClicked;

        public void Initialize(string apiKey, string appId, string userId, string baseEndpoint);
        void SetLogEnabled(bool logEnabled);
        Task<bool> LoadInterstitialAsync();
        void ShowInterstitial();
        bool IsInterstitialReady();
    }
}
