﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PropUnlimiter.Utils
{
    public class LoggerUtils
    {
        private static readonly string TAG = "Prop Unlimiter: ";

        public static void Log(System.Object message)
        {
            Debug.Log(TAG + message);
        }

        public static void LogWarning(System.Object message)
        {
            Debug.LogWarning(TAG + message);
        }

        public static void LogError(System.Object message)
        {
            Debug.LogError(TAG + message);
        }

        public static void LogException(Exception e)
        {
            Debug.LogException(e);
        }

        public static void LogToConsole(string message)
        {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, message);
        }

        public static void LogWarningToConsole(string message)
        {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, message);
        }

        public static void LogErrorToConsole(string message)
        {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, message);
        }

    }
}
