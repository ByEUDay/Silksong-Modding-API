using System;
using System.IO;
using UnityEngine;

namespace Modding
{



    public static class Logger
    {
        private static readonly object Lock = new object();
        private static string _path;

        private static string LogPath
        {
            get
            {
                if (_path != null) return _path;
                try { _path = Path.Combine(Application.persistentDataPath, "ModLog.txt"); }
                catch { _path = "ModLog.txt"; }
                return _path;
            }
        }

        internal static void Reset()
        {
            try { lock (Lock) File.WriteAllText(LogPath, string.Empty); } catch {   }
        }

        public static void Log(string tag, object message) => Write("INFO", tag, message);
        public static void LogWarn(string tag, object message) => Write("WARN", tag, message);
        public static void LogError(string tag, object message) => Write("ERROR", tag, message);

        public static void Log(object message) => Write("INFO", "API", message);
        public static void LogWarn(object message) => Write("WARN", "API", message);
        public static void LogError(object message) => Write("ERROR", "API", message);

        private static void Write(string level, string tag, object message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] [{tag}] {message}";
            try { lock (Lock) File.AppendAllText(LogPath, line + Environment.NewLine); } catch {   }

            switch (level)
            {
                case "ERROR": Debug.LogError(line); break;
                case "WARN": Debug.LogWarning(line); break;
                default: Debug.Log(line); break;
            }
        }
    }
}
