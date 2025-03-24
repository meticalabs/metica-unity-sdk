using System;

using Metica.Experimental.Core;

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
