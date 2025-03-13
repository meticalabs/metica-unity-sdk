using Metica.Experimental.Core;
using System;

namespace Metica.Experimental
{
    internal class SystemDateTimeSource : ITimeSource
    {
        public long EpochSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
