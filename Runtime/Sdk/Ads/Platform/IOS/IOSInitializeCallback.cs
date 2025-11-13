using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using Metica;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    public class IOSInitializeCallback
    {
        private const string TAG = MeticaAds.TAG;
        private static TaskCompletionSource<MeticaInitResponse> _currentTcs;

        public IOSInitializeCallback(TaskCompletionSource<MeticaInitResponse> tcs)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
            _currentTcs = tcs;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnInitializeSuccessDelegate(string meticaAdsInitializationResponseJson);

        [DllImport("__Internal")]
        private static extern void ios_sdkInitialize(string apiKey, string appId, string userId, string mediationInfoKey, OnInitializeSuccessDelegate result);

        public void InitializeSDK(string apiKey, string appId, string userId, string mediationInfoKey)
        {
            ios_sdkInitialize(
                apiKey,
                appId,
                userId,
                mediationInfoKey,
                OnInitialized
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnInitializeSuccessDelegate))]
        private static void OnInitialized(string meticaAdsInitializationResponseJson)
        {
            var tcs = _currentTcs;
            if (tcs == null)
            {
                MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback OnInitialized called but _currentTcs is null");
                return;
            }

            if (string.IsNullOrEmpty(meticaAdsInitializationResponseJson))
            {
                MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback OnInitialized received null or empty JSON");
                _currentTcs = null;
                tcs.SetException(new Exception("iOS initialization returned null or empty response"));
                return;
            }

            try
            {
                MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback OnInitialized received JSON: {meticaAdsInitializationResponseJson}");
                
                var meticaInitResponse = MeticaInitResponse.FromJson(meticaAdsInitializationResponseJson);
                if (meticaInitResponse == null)
                {
                    MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback OnInitialized failed to parse JSON: {meticaAdsInitializationResponseJson}");
                    _currentTcs = null;
                    tcs.SetException(new Exception($"Failed to parse initialization response JSON: {meticaAdsInitializationResponseJson}"));
                    return;
                }

                MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback onInit");
                var smartFloors = meticaInitResponse.SmartFloors;
                
                if (smartFloors == null)
                {
                    MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback OnInitialized SmartFloors is null after parsing, attempting manual parse");

                    try
                    {
                        // Parse the root structure to get smartFloors
                        // JSON structure: {"smartFloors":{"isSuccess":true,"userGroup":{"holdout":{}}}}
                        var rootWrapper = JsonUtility.FromJson<MeticaInitResponseJsonWrapper>(meticaAdsInitializationResponseJson);
                        bool isSuccess = rootWrapper.smartFloors.isSuccess;
                        MeticaUserGroup userGroup;
                        string userGroupKey = null;
                        
                        if (meticaAdsInitializationResponseJson.Contains("\"holdout\""))
                        {
                            userGroupKey = "HOLDOUT";
                        }
                        else if (meticaAdsInitializationResponseJson.Contains("\"trial\""))
                        {
                            userGroupKey = "TRIAL";
                        }
                        
                        if (string.IsNullOrEmpty(userGroupKey))
                        {
                            MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback could not find userGroup type in object");
                            _currentTcs = null;
                            tcs.SetException(new Exception($"Unknown userGroup structure in JSON. Expected type, got: {meticaAdsInitializationResponseJson}"));
                            return;
                        }
                        
                        userGroup = (MeticaUserGroup)System.Enum.Parse(typeof(MeticaUserGroup), userGroupKey, true);
                        smartFloors = new MeticaSmartFloors(userGroup, isSuccess);
                        MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback manually parsed SmartFloors: userGroup={userGroup}, isSuccess={isSuccess}");
                    }
                    catch (Exception parseEx)
                    {
                        MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback failed to manually parse SmartFloors: {parseEx.Message}\nStack: {parseEx.StackTrace}");
                        _currentTcs = null;
                        tcs.SetException(new Exception($"Failed to parse SmartFloors from JSON: {meticaAdsInitializationResponseJson}", parseEx));
                        return;
                    }
                }
                
                MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback smartFloorsObj = {smartFloors}");
                _currentTcs = null;
                tcs.SetResult(new MeticaInitResponse(smartFloors));
            }
            catch (Exception ex)
            {
                MeticaAds.Log.LogError(() => $"{TAG} IOSInitializeCallback OnInitialized exception: {ex.Message}\nStack: {ex.StackTrace}");
                _currentTcs = null;
                tcs.SetException(ex);
            }
        }
        
        [System.Serializable]
        private class MeticaInitResponseJsonWrapper
        {
            public SmartFloorsJsonWrapper smartFloors;
        }
        
        [System.Serializable]
        private class SmartFloorsJsonWrapper
        {
            public bool isSuccess;
            // Note: userGroup is not included here because JsonUtility can't handle empty objects {}
            // We parse it manually from the JSON string instead
        }
    }
}