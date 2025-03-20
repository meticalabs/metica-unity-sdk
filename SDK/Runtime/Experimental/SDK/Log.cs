using Metica.Experimental.Core;
using Metica.Experimental.SDK;
using System;

/// <summary>
/// Facade class for logging.
/// This requires a registered implementation of <see cref="ILog"/>
/// Example: Registry.Register<ILog>(new MyLogger());
/// </summary>
public static class Log
{
    private static readonly Lazy<ILog> _logger = new Lazy<ILog>(() => Registry.Resolve<ILog>());
    private static ILog Logger => _logger.Value;

    public static LogLevel CurrentLogLevel
    {
        get => Logger.CurrentLogLevel;
        set => Logger.CurrentLogLevel = value;
    }

    public static void Info(Func<string> messageSupplier) => Logger.LogInfo(messageSupplier);
    public static void Error(Func<string> messageSupplier, Exception error = null) => Logger.LogError(messageSupplier, error);
    public static void Warn(Func<string> messageSupplier) => Logger.LogWarning(messageSupplier);
    public static void Debug(Func<string> messageSupplier) => Logger.LogDebug(messageSupplier);
}