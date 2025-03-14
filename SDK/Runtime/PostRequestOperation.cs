using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;


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

}
