using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Pool;

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
            MeticaSdkDelegate<T> callback) where T : class
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
                jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                MeticaLogger.LogDebug($"json body: {jsonBody}");
            }
            catch (Exception e)
            {
                MeticaLogger.LogError($"Error while fetching offers: {e.Message}", e);
            }

            if (jsonBody == null)
            {
                yield return SdkResultImpl<T>.WithError("Failed to serialize body");
            }
            else
            {
                var fullUrl = queryParams is { Count: > 0 } ? $"{url}?{BuildUrlWithParameters(queryParams)}" : url;
                MeticaLogger.LogDebug($"sending request to {fullUrl} with body: {jsonBody}");

                using (var www = new UnityWebRequest(fullUrl, "PUT"))
                {
                    var jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
                    www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.SetRequestHeader("X-API-KEY", apiKey);
                    www.method = "POST";
                    www.timeout = MeticaAPI.Config.networkTimeout;

                    yield return www.SendWebRequest();

                    MeticaLogger.LogDebug("result: " + www.result);

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        var error = $"Error: {www.error}, status: {www.responseCode}";
                        MeticaLogger.LogError(error);
                        callback(SdkResultImpl<T>.WithError($"API Error: {error}"));
                    }
                    else
                    {
                        var responseText = www.downloadHandler.text;
                        MeticaLogger.LogDebug($"Response: {responseText}");

                        if (string.IsNullOrEmpty(responseText) && (www.responseCode >= 200 || www.responseCode <= 204))
                        {
                            callback(SdkResultImpl<T>.WithResult(null));
                        }
                        else
                        {
                            T result = null;
                            try
                            {
                                result = JsonConvert.DeserializeObject<T>(responseText);
                            }
                            catch (Exception e)
                            {
                                MeticaLogger.LogError($"Error while decoding the ODS response: {e.Message}", e);
                            }

                            callback(result != null
                                ? SdkResultImpl<T>.WithResult(result)
                                : SdkResultImpl<T>.WithError("Failed to decode the server response"));
                        }
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

    public interface IBackendOperations
    {
        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null);

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<String> callback);
    }

    internal class BackendOperationsImpl : IBackendOperations
    {
        private static readonly LinkedPool<GetOffersOperation> GetOffersPool = new(
            createFunc: () =>
            {
                var item = ScriptingObjects.AddComponent<GetOffersOperation>();
                return item;
            },
            actionOnGet: item => item.gameObject.SetActive(true),
            actionOnRelease: item => item.gameObject.SetActive(false),
            actionOnDestroy: item => { item.OnDestroyPoolObject(); },
            maxSize: 100
        );

        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            var op = GetOffersPool.Get();
            op.pool = GetOffersPool;
            op.Placements = placements;
            op.OffersCallback = offersCallback;
            op.UserProperties = userProperties;
            op.DeviceInfo = deviceInfo;
        }

        private static readonly LinkedPool<CallEventsIngestionOperation> CallIngestionPool = new(
            createFunc: () =>
            {
                var item = ScriptingObjects.AddComponent<CallEventsIngestionOperation>();
                return item;
            },
            actionOnGet: item => item.gameObject.SetActive(true),
            actionOnRelease: item => item.gameObject.SetActive(false),
            actionOnDestroy: item => { item.OnDestroyPoolObject(); },
            maxSize: 100
        );

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<String> callback)
        {
            var op = CallIngestionPool.Get();
            op.pool = CallIngestionPool;
            op.Events = events;
            op.EventsSubmitCallback = callback;
        }
    }
}