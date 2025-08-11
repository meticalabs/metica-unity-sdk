using Metica.SDK.Model;

namespace Metica.SDK
{
    public interface IDeviceInfoProvider
    {
        DeviceInfo GetDeviceInfo();

        public string deviceType { get; }
        public string operatingSystem { get; }
        public string deviceModel { get; }
    }
}
