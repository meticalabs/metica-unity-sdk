using System;
using System.Threading;
using UnityEngine;

using Metica.SDK;
using Metica.SDK.Model;

namespace Metica.Unity
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
            var osVersion = Environment.OSVersion.VersionString;
            // var machineName = Environment.MachineName;

            return new DeviceInfo()
            {
                store = MapRuntimePlatformToStoreType(Application.platform)?.ToString(),
                timezone = timezone,
                locale = locale,
                appVersion = appVersion,
                osVersion = osVersion,
                // machineName = machineName
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
