using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Pool;

namespace Metica.Unity
{
    internal class RequestResponse<T>
    {
        public T Data;
        public Dictionary<string, string> Headers;
        public long ResponseCode;

        public RequestResponse(T data, Dictionary<string, string> headers, long responseCode)
        {
            Data = data;
            Headers = headers;
            ResponseCode = responseCode;
        }
    }
    
    internal abstract class PostRequestOperation
    {
        public static IEnumerator PostRequest<T>(
            string url,
            Dictionary<string, object>? queryParams,
            string apiKey,
            object body,
            MeticaSdkDelegate<RequestResponse<T>> callback) where T : class
        {
            // if there is no internet connection, return the cached offers
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                callback(SdkResultImpl<RequestResponse<T>>.WithError("No internet connection"));
                yield break;
            }

            if (apiKey == null)
            {
                callback(SdkResultImpl<RequestResponse<T>>.WithError("API Key is not set"));
                yield break;
            }

            if (body == null)
            {
                callback(SdkResultImpl<RequestResponse<T>>.WithError("Body is null"));
                yield break;
            }

            string jsonBody = null;
            try
            {
                jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            catch (Exception e)
            {
                MeticaLogger.LogError(() => $"Error while fetching offers: {e.Message}", e);
            }

            if (jsonBody == null)
            {
                yield return SdkResultImpl<RequestResponse<T>>.WithError("Failed to serialize body");
            }
            else
            {
                var fullUrl = queryParams is { Count: > 0 } ? $"{url}?{BuildUrlWithParameters(queryParams)}" : url;

                using (var www = new UnityWebRequest(fullUrl, "PUT"))
                {
                    var jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
                    www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.SetRequestHeader("X-API-KEY", apiKey);
                    www.method = "POST";
                    www.timeout = MeticaAPI.Config.networkTimeout;

                    MeticaLogger.LogDebug(() =>
                        $@"Sending request using {www}:
                        --------
                        Headers:
                            {"Content-Type"} : {www.GetRequestHeader("Content-Type")}
                            {"X-API-KEY"} : {www.GetRequestHeader("X-API-KEY")}
                        Method: {www.method}
                        Timeout: {www.timeout}
                        Url: {www.url}
                        Body: {jsonBody}");

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        var error = $"Error: {www.error}, status: {www.responseCode}, endpoint: {www.url}";
                        MeticaLogger.LogError(() => error);
                        callback(SdkResultImpl<RequestResponse<T>>.WithError($"API Error: {error}"));
                    }
                    else
                    {
                        var responseText = www.downloadHandler.text;
                        MeticaLogger.LogDebug(() => $"Response raw text:\n{responseText}, endpoint: {www.url}");

                        if (string.IsNullOrEmpty(responseText) && (www.responseCode >= 200 || www.responseCode <= 204))
                        {
                            callback(SdkResultImpl<RequestResponse<T>>.WithResult(null));
                        }
                        else
                        {
                            RequestResponse<T> response = null;
                            try
                            {
                                var result = JsonConvert.DeserializeObject<T>(responseText);
                                response = new RequestResponse<T>(data: result, headers: www.GetResponseHeaders(), responseCode: www.responseCode);
                            }
                            catch (Exception e)
                            {
                                MeticaLogger.LogError(() => $"Error while decoding the ODS response: {e.Message}", e);
                            }

                            callback(response != null
                                ? SdkResultImpl<RequestResponse<T>>.WithResult(response)
                                : SdkResultImpl<RequestResponse<T>>.WithError("Failed to decode the server response"));
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

    internal interface IBackendOperations
    {
        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null);

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<string> callback);
        
        public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null);
    }

    internal class BackendOperationsImpl : IBackendOperations
    {
        #region OFFER REFACTOR
        //private static readonly LinkedPool<GetOffersOperation> GetOffersPool = new(
        //    createFunc: () =>
        //    {
        //        var item = ScriptingObjects.AddComponent<GetOffersOperation>();
        //        return item;
        //    },
        //    actionOnGet: item => item.gameObject.SetActive(true),
        //    actionOnRelease: item => item.gameObject.SetActive(false),
        //    actionOnDestroy: item => { item.OnDestroyPoolObject(); },
        //    maxSize: 100
        //);

        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(GetOffersOperationStart(placements, userProperties, deviceInfo, offersCallback));
            //var op = GetOffersPool.Get();
            //op.pool = GetOffersPool;
            //op.Placements = placements;
            //op.OffersCallback = offersCallback;
            //op.UserProperties = userProperties;
            //op.DeviceInfo = deviceInfo;
        }

        [Serializable]
        internal class ODSRequest : RequestWithUserDataAndDeviceInfo
        {
        }

        [Serializable]
        internal class ODSResponse
        {
            public Dictionary<string, List<Offer>> placements;
        }
        
        private static ODSRequest CreateODSRequestBody(Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            var requestWithUserDataAndDeviceInfo = RequestUtils.CreateRequestWithUserDataAndDeviceInfo(userData, overrideDeviceInfo);
            var request = new ODSRequest
            {
                userId = MeticaAPI.UserId,
                userData = requestWithUserDataAndDeviceInfo.userData,
                deviceInfo = requestWithUserDataAndDeviceInfo.deviceInfo
            };

            return request;
        }

        private IEnumerator GetOffersOperationStart(string[] Placements, Dictionary<string, object> UserProperties, DeviceInfo DeviceInfo, MeticaSdkDelegate<OffersByPlacement> OffersCallback)
        {
            yield return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.Config.offersEndpoint}/offers/v1/apps/{MeticaAPI.AppId}",
                new Dictionary<string, object>
                {
                    { "placements", Placements },
                },
                MeticaAPI.ApiKey,
                CreateODSRequestBody(UserProperties, DeviceInfo),
                result =>
                {
                    if (result.Error != null)
                    {
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithError(result.Error));
                    }
                    else
                    {
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement
                        {
                            placements = result.Result.Data.placements
                        }));
                    }
                });
        }

        #endregion OFFER REFACTOR


        #region EVENT REFACTORING
        //private static readonly LinkedPool<CallEventsIngestionOperation> CallIngestionPool = new(
        //    createFunc: () =>
        //    {
        //        var item = ScriptingObjects.AddComponent<CallEventsIngestionOperation>();
        //        return item;
        //    },
        //    actionOnGet: item => item.gameObject.SetActive(true),
        //    actionOnRelease: item => item.gameObject.SetActive(false),
        //    actionOnDestroy: item => { item.OnDestroyPoolObject(); },
        //    maxSize: 100
        //);

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<string> callback)
        {
            //var op = CallIngestionPool.Get();
            //op.pool = CallIngestionPool;
            //op.Events = events;
            //op.EventsSubmitCallback = callback;
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(CallEventsIngestionOperationStart(events, callback));
        }

        internal IEnumerator CallEventsIngestionOperationStart(ICollection<Dictionary<string, object>> Events, MeticaSdkDelegate<String> EventsSubmitCallback)
        {
            return PostRequestOperation.PostRequest<String>($"{MeticaAPI.Config.ingestionEndpoint}/ingest/v1/events",
                null,
                MeticaAPI.ApiKey,
                CreateIngestionRequestBody(Events),
                result =>
                {
                    EventsSubmitCallback(result.Error != null
                        ? SdkResultImpl<string>.WithError(result.Error)
                        : SdkResultImpl<string>.WithResult(result.Result?.Data ?? string.Empty));
                });
        }

        [Serializable]
        internal class RequestWithUserDataAndDeviceInfo
        {
            public string userId;
            public Dictionary<string, object> userData;
            public DeviceInfo deviceInfo;
        }

        private static object CreateIngestionRequestBody(ICollection<Dictionary<string, object>> events)
        {
            return new Dictionary<string, object> { { "events", events } };
        }


        #endregion EVENT REFACTORING

        #region CONFIG REFACTOR
        //private static readonly LinkedPool<CallRemoteConfigOperation> CallRemoteConfigPool = new(
        //    createFunc: () =>
        //    {
        //        var item = ScriptingObjects.AddComponent<CallRemoteConfigOperation>();
        //        return item;
        //    },
        //    actionOnGet: item => item.gameObject.SetActive(true),
        //    actionOnRelease: item => item.gameObject.SetActive(false),
        //    actionOnDestroy: item => { item.OnDestroyPoolObject(); },
        //    maxSize: 100
        //);

        //public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null,
        //    DeviceInfo deviceInfo = null)
        //{
        //    var op = CallRemoteConfigPool.Get();
        //    op.pool = CallRemoteConfigPool;
        //    op.ConfigKeys = configKeys;
        //    op.UserProperties = userProperties;
        //    op.DeviceInfo = deviceInfo;
        //    op.ResponseCallback = responseCallback;
        //}

        public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(RemoteConfigOperationStart(configKeys, userProperties, deviceInfo, responseCallback));
        }

        public IEnumerator RemoteConfigOperationStart(
            ICollection<string> ConfigKeys,
            Dictionary<string, object> UserProperties,
            DeviceInfo DeviceInfo,
            MeticaSdkDelegate<RemoteConfig> ResponseCallback)
        {
            yield return PostRequestOperation.PostRequest<Dictionary<string, object>>(
                $"{MeticaAPI.Config.remoteConfigEndpoint}/config/v1/apps/{MeticaAPI.AppId}",
                ConfigKeys != null && ConfigKeys.Any() ?
                new Dictionary<string, object>
                {
                    { "keys", ConfigKeys },
                } : null,
                MeticaAPI.ApiKey,
                RequestUtils.CreateRequestWithUserDataAndDeviceInfo(UserProperties, DeviceInfo),
                result =>
                {
                    if (result.Error != null)
                    {
                        ResponseCallback(SdkResultImpl<RemoteConfig>.WithError(result.Error));
                    }
                    else
                    {
                        var remoteConfig = new RemoteConfig(
                            config: result.Result.Data,
                            cacheDurationSecs: ParseCacheExpirationFromHeaders(result.Result.Headers));
                        ResponseCallback(SdkResultImpl<RemoteConfig>.WithResult(remoteConfig));
                    }
                });
        }

        private static readonly char[] ChEquals = { '=' };
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(3);
        private static long ParseCacheExpirationFromHeaders(Dictionary<string, string> headers)
        {
            var seconds = DefaultCacheDuration.Seconds;
            var cacheControl = headers["Cache-Control"];
            if (cacheControl == null) return seconds;
            try
            {
                seconds = int.Parse(cacheControl.Split(ChEquals)[1]);
            }
            catch (Exception e)
            {
                MeticaLogger.LogError(() => $"Failed to parse the cache control directive from the header value {cacheControl}");
            }

            return seconds;
        }
    }
        #endregion CONFIG REFACTOR
}