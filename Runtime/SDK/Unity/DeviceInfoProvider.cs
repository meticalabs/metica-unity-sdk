using System;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

using Metica.Model;

namespace Metica.Unity
{
    /// <summary>
    /// Unity implementation of <see cref="IDeviceInfoProvider"/>.
    /// The obtained information is cached so it is collected <b>once</b> and remains unchanged for the whole application lifetime.
    /// </summary>
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        private readonly Lazy<DeviceInfo> _cachedDeviceInfo = new Lazy<DeviceInfo>(() => CreateDeviceInfo());
        private readonly Lazy<string> _cachedHashedDeviceId = new Lazy<string>(() =>
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        });

        /// <inheritdoc/>
        public string deviceType => SystemInfo.deviceType.ToString();
        /// <inheritdoc/>
        public string deviceUniqueId => _cachedHashedDeviceId.Value;
        /// <inheritdoc/>
        public string operatingSystem => SystemInfo.operatingSystem;
        /// <inheritdoc/>
        public string deviceModel => SystemInfo.deviceModel;

        /// <inheritdoc/>
        public DeviceInfo GetDeviceInfo() => _cachedDeviceInfo.Value;

        private static DeviceInfo CreateDeviceInfo()
        {
            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = ((systemTz >= TimeSpan.Zero) ? "+" : "-") + systemTz.ToString(@"hh\:mm");
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            var appVersion = Application.version;

            return new DeviceInfo()
            {
                store = MapRuntimePlatformToStoreType(Application.platform)?.ToString(),
                timezone = timezone,
                locale = locale,
                appVersion = appVersion,
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
