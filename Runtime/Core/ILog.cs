using System;

namespace Metica.Core
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
        /// <summary>
        /// The current log level can be changed at runtime.
        /// </summary>
        LogLevel CurrentLogLevel { get; set; }
        void LogDebug(Func<string> messageSupplier);
        void LogError(Func<string> messageSupplier, Exception error = null);
        void LogInfo(Func<string> messageSupplier);
        void LogWarning(Func<string> messageSupplier);
    }
}
