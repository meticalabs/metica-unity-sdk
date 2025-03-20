using System;

namespace Metica.Experimental.SDK
{
    public enum LogLevel
    {
        Off,
        Error,
        Warning,
        Info,
        Debug,
    }

    public interface ILog
    {
        LogLevel CurrentLogLevel { get; set; }
        void LogDebug(Func<string> messageSupplier);
        void LogError(Func<string> messageSupplier, Exception error = null);
        void LogInfo(Func<string> messageSupplier);
        void LogWarning(Func<string> messageSupplier);
    }
}
