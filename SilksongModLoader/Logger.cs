using System;
using System.IO;

namespace SilksongModLoader
{
    /// <summary>
    /// 极简日志:写文件 + 控制台(如果游戏以 -batchmode 之外的方式启动,
    /// stdout 通常不可见,所以文件日志是主要排查手段)。
    /// </summary>
    public static class ModLog
    {
        private static readonly object _lock = new();
        private static string LogPath = null!;

        public static void Init(string baseDir)
        {
            // 注意:不用传入的 baseDir(即 AppContext.BaseDirectory),
            // 因为在游戏进程里这个值不一定指向 Managed 目录,尤其是 macOS 的 .app 包,
            // 可能指向一个你根本看不到、或者没有写权限的沙盒路径。
            // 改成写死到用户主目录下,保证一定能找到、一定有写权限。
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
