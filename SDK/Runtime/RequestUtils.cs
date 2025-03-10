using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    [Serializable]
    internal class RequestWithUserDataAndDeviceInfo
    {
        public string userId;
        public Dictionary<string, object> userData;
        public DeviceInfo deviceInfo;
    }

    internal static class RequestUtils
    {
        public static RequestWithUserDataAndDeviceInfo CreateRequestWithUserDataAndDeviceInfo(
            Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            if (string.IsNullOrEmpty(locale))
            {
                locale = Constants.DefaultLocale;
            }

            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = ((systemTz >= TimeSpan.Zero) ? "+" : "-") + systemTz.ToString(@"hh\:mm");

            var deviceInfo = overrideDeviceInfo ?? new DeviceInfo();
            deviceInfo.locale = string.IsNullOrEmpty(overrideDeviceInfo?.locale) ? locale : overrideDeviceInfo.locale;
            deviceInfo.store = overrideDeviceInfo?.store ??
                               MapRuntimePlatformToStoreType(Application.platform)?.ToString();
            deviceInfo.timezone = overrideDeviceInfo?.timezone ?? timezone;
            deviceInfo.appVersion =
                overrideDeviceInfo?.appVersion ?? Application.version ?? Constants.DefaultAppVersion;

            var request = new RequestWithUserDataAndDeviceInfo
            {
                userId = MeticaAPI.UserId,
                userData = userData,
                deviceInfo = deviceInfo
            };

            // Prefer excluding fields with null or empty values.
            var serializationSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            MeticaLogger.LogInfo(() => $"The request is {JsonConvert.SerializeObject(userData, serializationSettings)}");

            return request;
        }


        private static StoreTypeEnum? MapRuntimePlatformToStoreType(RuntimePlatform runtimePlatform)
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
                    // Fallback to null
                    return null;
            }
        }
    }
}