using System;
using System.Collections.Generic;
using System.IO;

namespace SilksongModLoader
{
    internal static class ModConfig
    {
        private static string _filePath = "";
        private static readonly HashSet<string> Disabled = new(StringComparer.OrdinalIgnoreCase);
        private static bool _loaded;

        public static void Load(string modsDir)
        {
            _filePath = Path.Combine(modsDir, "disabled.txt");
            Disabled.Clear();
            try
            {
                if (File.Exists(_filePath))
                {
                    foreach (var line in File.ReadAllLines(_filePath))
                    {
                        var name = line.Trim();
                        if (name.Length > 0) Disabled.Add(name);
                    }
                }
            }
            catch (Exception e)
            {
                ModLog.Error($"读取 disabled.txt 失败,本次将视为没有禁用任何 mod: {e}");
            }
            _loaded = true;
        }

        public static bool IsDisabled(string modName) => Disabled.Contains(modName);

        public static void SetDisabled(string modName, bool disabled)
        {
            if (!_loaded)
            {
                ModLog.Warn("ModConfig 尚未加载就被写入,忽略这次修改。");
                return;
            }

            if (disabled) Disabled.Add(modName);
            else Disabled.Remove(modName);
            Save();
        }

        private static void Save()
        {
            try
            {
                File.WriteAllLines(_filePath, Disabled);
            }
            catch (Exception e)
            {
                ModLog.Error($"保存 disabled.txt 失败,禁用状态可能不会在下次启动时生效: {e}");
            }
        }
    }
}
