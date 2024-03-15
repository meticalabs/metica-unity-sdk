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
            [CanBeNull] Dictionary<string, object> queryParams,
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
                var fullUrl = queryParams is { Count: > 0 } ? $"{url}?{BuildUrlWithParameters(queryParams)}" : url;
                Debug.Log($"sending request to {fullUrl} with body: {jsonBody}");

                using (var www = new UnityWebRequest(fullUrl, "PUT"))
                {
                    var jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
                    www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.SetRequestHeader("X-API-KEY", apiKey);
                    www.method = "POST";

                    yield return www.SendWebRequest();

                    Debug.Log("result: " + www.result);
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


        private static string BuildUrlWithParameters(Dictionary<string, object> parameters)
        {
            StringBuilder parameterString = new StringBuilder();
            foreach (var param in parameters)
            {
                // check if value is a string array
                if (param.Value is string[] values)
                {
                    // handle multiple values joined by comma
                    var escapedValues = Array.ConvertAll(values, Uri.EscapeDataString);
                    var valuesString = string.Join(",", escapedValues);

                    AppendParameterToUrl(parameterString, param.Key, valuesString);
                }
                else
                {
                    // handle single value
                    string valueString = Convert.ToString(param.Value);
                    AppendParameterToUrl(parameterString, param.Key, Uri.EscapeDataString(valueString));
                }
            }

            return parameterString.ToString();
        }

        private static void AppendParameterToUrl(StringBuilder parameterString, string key, string value)
        {
            if (parameterString.Length > 0)
            {
                parameterString.Append("&");
            }

            parameterString.AppendFormat("{0}={1}", Uri.EscapeDataString(key), value);
        }
    }

    internal abstract class BackendOperations
    {
        public static void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            var op = ScriptingObjects.AddComponent<GetOffersOperation>();
            op.Placements = placements;
            op.OffersCallback = offersCallback;
            op.UserProperties = userProperties;
            op.DeviceInfo = deviceInfo;
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

        internal static object CreateIngestionRequestBody(List<Dictionary<string, object>> events)
        {
            return new Dictionary<string, object>() { { "events", events } };
        }

        // ReSharper disable once InconsistentNaming
        internal static ODSRequest CreateODSRequestBody(Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            var locale = Thread.CurrentThread.CurrentCulture.Name;

            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = systemTz.TotalMinutes == 0
                ? "+00:00"
                : $"{systemTz.Hours:D2}:{systemTz.Minutes:D2}";
            Debug.Log(TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            var deviceInfo = overrideDeviceInfo ?? new DeviceInfo();
            deviceInfo.locale = overrideDeviceInfo?.locale ?? locale;
            deviceInfo.store = overrideDeviceInfo?.store ??
                               MapRuntimePlatformToStoreType(Application.platform).ToString();
            deviceInfo.timezone = overrideDeviceInfo?.timezone ?? timezone;
            deviceInfo.appVersion = overrideDeviceInfo?.appVersion ?? Application.version;

            var request = new ODSRequest
            {
                userData = userData,
                deviceInfo = deviceInfo
            };

            return request;
        }
    }
}