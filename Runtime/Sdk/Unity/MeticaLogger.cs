using System;
using UnityEngine;
using Metica.Core;

namespace Metica.Unity
{
    internal class MeticaLogger : ILog
    {
        public LogLevel CurrentLogLevel { get; set; } = LogLevel.Error;

        public MeticaLogger(LogLevel initialLogLevel)
        {
            CurrentLogLevel = initialLogLevel;
        }

        public void LogInfo(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Info)
            {
                Debug.Log(messageSupplier());
            }
        }

        public void LogError(Func<string> messageSupplier, Exception error = null)
        {
            if (CurrentLogLevel >= LogLevel.Error)
            {
                Debug.LogError(messageSupplier());
                if (error != null) Debug.LogException(error);
            }
        }

        public void LogWarning(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Warning)
            {
                Debug.LogWarning(messageSupplier());
            }
        }

        public void LogDebug(Func<string> messageSupplier)
        {
            if (CurrentLogLevel >= LogLevel.Debug)
            {
                Debug.Log(messageSupplier());
            }
        }
    }
}
