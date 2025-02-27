using Metica.Unity;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    public class DummyTimeSource : ITimeSource
    {
        private int _value;
        public void SetValue(int value)
        {
            _value = value;
        }
        public long EpochSeconds()
        {
            return _value;
        }
    }
}
