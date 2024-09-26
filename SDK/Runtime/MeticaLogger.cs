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

    public class MeticaLogger
    {
        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Error;

        public static void LogInfo(object message)
        {
            if (CurrentLogLevel >= LogLevel.Info)
            {
                Debug.Log(message);
            }
        }

        public static void LogError(object message, Exception error = null)
        {
            if (CurrentLogLevel >= LogLevel.Error)
            {
                Debug.LogError(message);
                if (error != null) Debug.LogException(error);
            }
        }

        public static void LogWarning(object message)
        {
            if (CurrentLogLevel >= LogLevel.Warning)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogDebug(object message)
        {
            if (CurrentLogLevel >= LogLevel.Debug)
            {
                Debug.Log(message);
            }
        }
    }
}