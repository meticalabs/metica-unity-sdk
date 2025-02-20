using System;
using UnityEngine;

namespace Metica.Unity
{
    public enum LogLevel
    {
        Off,
        Error,
        Warning,
        Info,
        Debug,
    }

    public static class MeticaLogger
    {
        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Error;

        public static void LogInfo(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Info)
            {
                Debug.Log(messageSupplier());
            }
        }

        public static void LogError(Func<string> messageSupplier, Exception error = null)
        {
            if (CurrentLogLevel >= LogLevel.Error)
            {
                Debug.LogError(messageSupplier());
                if (error != null) Debug.LogException(error);
            }
        }

        public static void LogWarning(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Warning)
            {
                Debug.LogWarning(messageSupplier());
            }
        }

        public static void LogDebug(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Debug)
            {
                Debug.Log(messageSupplier());
            }
        }
    }
}