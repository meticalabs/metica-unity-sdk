using Metica.Experimental.Core;
using Metica.Experimental.SDK.Model;
using System;
using System.Threading;
using UnityEngine;

namespace Metica.Experimental.Unity
{
    /// <summary>
    /// Unity implementation of <see cref="IDeviceInfoProvider"/>.
    /// </summary>
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        public DeviceInfo GetDeviceInfo()
        {
            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = ((systemTz >= TimeSpan.Zero) ? "+" : "-") + systemTz.ToString(@"hh\:mm");
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            var appVersion = Application.version;

            return new() {
                store = MapRuntimePlatformToStoreType(Application.platform)?.ToString(),
                timezone = timezone,
                locale = locale,
                appVersion = appVersion
            };
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
