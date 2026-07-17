using System;
using System.IO;

namespace SilksongModLoader
{
    public static class ModLog
    {
        private static readonly object _lock = new();
        private static string LogPath = null!;

        public static void Init(string baseDir)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            LogPath = Path.Combine(home, "SilksongModLoader.log");
            try { File.WriteAllText(LogPath, $"=== Log started {DateTime.Now:u} (baseDir was: {baseDir}) ===\n"); }
            catch { /* 目录不可写时静默忽略,不应影响游戏启动 */ }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message) => Write("ERROR", message);

        private static void Write(string level, string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            lock (_lock)
            {
                try { File.AppendAllText(LogPath, line + "\n"); } catch { }
            }
            Console.WriteLine(line);
        }
    }
}
