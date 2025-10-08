using Metica.ADS;

namespace Metica.SDK
{
    public interface ISdkConfigProvider
    {
        public MeticaConfiguration SdkConfig { get; }
    }
}
