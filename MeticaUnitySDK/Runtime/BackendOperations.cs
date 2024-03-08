using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Metica.Unity
{
    [Serializable]
    internal class ODSRequest
    {
        public Dictionary<string, object> userData;
        public DeviceInfo deviceInfo;
    }

    [Serializable]
    internal class ODSResponse
    {
        public Dictionary<string, List<Offer>> placements;
    }

    internal abstract class PostRequestOperation
    {
        public static IEnumerator PostRequest<T>(
            string url,
            [CanBeNull] string queryString,
            string apiKey,
            object body,
            MeticaSdkDelegate<T> callback)
        {
            // if there is no internet connection, return the cached offers
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                callback(SdkResultImpl<T>.WithError("No internet connection"));
                yield break;
            }

            if (apiKey == null)
            {
                callback(SdkResultImpl<T>.WithError("API Key is not set"));
                yield break;
            }
            
            if (body == null)
            {
                callback(SdkResultImpl<T>.WithError("Body is null"));
                yield break;
            }
            
            string jsonBody = null;
            try
            {
                jsonBody = JsonConvert.SerializeObject(body);
                Debug.Log($"json body: {jsonBody}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while fetching offers: {e.Message}");
                Debug.LogException(e);
            }

            if (jsonBody == null)
            {
                yield return SdkResultImpl<T>.WithError("Failed to serialize body");
            }
            else
            {
                var fullUrl = queryString != null ? $"{url}?{UnityWebRequest.EscapeURL(queryString)}" : url;
                using (var www = new UnityWebRequest(fullUrl, "POST"))
                {
                    var jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
                    www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.SetRequestHeader("X-API-KEY", apiKey);

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        var error = $"Error: {www.error}, status: {www.responseCode}";
                        Debug.LogError(error);
                        callback(SdkResultImpl<T>.WithError($"API Error: {error}"));
                    }
                    else
                    {
                        Debug.Log($"Response: {www.downloadHandler.text}");
                        var result = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
                        callback(SdkResultImpl<T>.WithResult(result));
                    }
                }
            }
        }
    }

    internal abstract class BackendOperations
    {
        public static void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
            var op = ScriptingObjects.AddComponent<GetOffersOperation>();
            op.Placements = placements;
            op.OffersCallback = offersCallback;
        }

        public static void CallSubmitEventsAPI(List<Dictionary<string, object>> events,
            MeticaSdkDelegate<String> callback)
        {
            var op = ScriptingObjects.AddComponent<SubmitEventsOperation>();
            op.Events = events;
            op.EventsSubmitCallback = callback;
        }

        private static StoreTypeEnum MapRuntimePlatformToStoreType(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.Android:
                    return StoreTypeEnum.GooglePlayStore;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return StoreTypeEnum.AppStore;
                default:
                    throw new Exception($"Got an unsupported application platform: {runtimePlatform}");
            }
        }

        internal static string CreateIngestionRequestBody(List<Dictionary<string, object>> events)
        {
            return $"{{ \"events\": {{{JsonConvert.SerializeObject(events)} }}";
        }

        // ReSharper disable once InconsistentNaming
        internal static string CreateODSRequestBody(Dictionary<string, object> userData)
        {
            var locale = Thread.CurrentThread.CurrentCulture.Name;

            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            Debug.Log(TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            var deviceInfo = new DeviceInfo
            {
                locale = locale,
                store = MapRuntimePlatformToStoreType(Application.platform).ToString(),
                timezone = systemTz.TotalMinutes == 0
                    ? "+00:00"
                    : $"{systemTz.Hours:D2}:{systemTz.Minutes:D2}",
                appVersion = Application.version
            };

            var request = new ODSRequest
            {
                userData = userData,
                deviceInfo = deviceInfo
            };

            return JsonConvert.SerializeObject(request);
        }
    }
}