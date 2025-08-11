using Metica.SDK.Model;

namespace Metica.SDK
{
    /// <summary>
    /// Provider of <see cref="DeviceInfo"/> structure as defined in Metica API schema
    /// and other information like <see cref="deviceType"/> and <see cref="operatingSystem"/>.
    /// </summary>
    public interface IDeviceInfoProvider
    {
        /// <summary>
        /// Gets the <see cref="DeviceInfo"/> structure as defined in the Metica API schema.
        /// </summary>
        /// <returns><see cref="DeviceInfo"/></returns>
        /// <remarks>An instance of <see cref="DeviceInfo"/> is created lazily (when requested the first time)
        /// and cached for the whole application lifetime.</remarks>
        DeviceInfo GetDeviceInfo();

        /// <summary>
        /// Gets the device type.
        /// </summary>
        public string deviceType { get; }
        /// <summary>
        /// Gets the device's operating system.
        /// </summary>
        public string operatingSystem { get; }
        /// <summary>
        /// Gets the device model.
        /// </summary>
        public string deviceModel { get; }
    }
}
