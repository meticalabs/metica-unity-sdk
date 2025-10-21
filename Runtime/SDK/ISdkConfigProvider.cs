using Metica.ADS;

namespace Metica
{
    public interface IMeticaConfigProvider
    {
        public MeticaInitConfig Config { get; }
    }
}
