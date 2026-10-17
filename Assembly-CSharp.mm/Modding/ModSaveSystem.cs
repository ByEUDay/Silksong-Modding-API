using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Modding
{



    public static class ModSaveSystem
    {
        private static readonly Dictionary<string, ISaveHandler> Handlers = new Dictionary<string, ISaveHandler>();
        private static bool _autoSaveEnabled = true;
        private static int _maxBackups = 5;

        public static void RegisterHandler(string key, ISaveHandler handler)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Handlers[key] = handler;
            Logger.Log($"[SaveSystem] 注册处理器: {key}");
        }

        public static void UnregisterHandler(string key)
        {
            if (Handlers.Remove(key))
                Logger.Log($"[SaveSystem] 移除处理器: {key}");
        }

        public static void SetAutoSaveEnabled(bool enabled)
        {
            _autoSaveEnabled = enabled;
            Logger.Log($"[SaveSystem] 自动保存: {(enabled ? "启用" : "禁用")}");
        }

        public static bool IsAutoSaveEnabled() => _autoSaveEnabled;

        public static void SetMaxBackups(int max)
        {
            _maxBackups = Mathf.Max(1, max);
            Logger.Log($"[SaveSystem] 最大备份数: {_maxBackups}");
        }

        public static int GetMaxBackups() => _maxBackups;

        internal static void SaveAll(int slot)
        {
            if (!_autoSaveEnabled) return;

            string dir = Path.Combine(Application.persistentDataPath, $"slot{slot}");
            Directory.CreateDirectory(dir);

            CreateBackup(dir);

            foreach (var kvp in Handlers)
            {
                try
                {
                    string path = Path.Combine(dir, kvp.Key + ".json");
                    object data = kvp.Value.OnSave();
                    if (data != null)
                    {
                        string json = JsonUtility.ToJson(data, true);
                        File.WriteAllText(path, json);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"[SaveSystem] 保存 {kvp.Key} 失败: {e.Message}");
                }
            }

            Logger.Log($"[SaveSystem] 存档 {slot} 保存完成");
        }

        internal static void LoadAll(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, $"slot{slot}");
            if (!Directory.Exists(dir))
            {
                Logger.Log($"[SaveSystem] 存档 {slot} 不存在");
                return;
            }

            foreach (var kvp in Handlers)
            {
                try
                {
                    string path = Path.Combine(dir, kvp.Key + ".json");
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        object data = kvp.Value.CreateNew();
                        JsonUtility.FromJsonOverwrite(json, data);
                        kvp.Value.OnLoad(data);
                    }
                    else
                    {
                        kvp.Value.OnLoad(kvp.Value.CreateNew());
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"[SaveSystem] 加载 {kvp.Key} 失败: {e.Message}");
                }
            }

            Logger.Log($"[SaveSystem] 存档 {slot} 加载完成");
        }

        internal static void ClearAll(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, $"slot{slot}");
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    Logger.Log($"[SaveSystem] 存档 {slot} 已清除");
                }
                catch (Exception e)
                {
                    Logger.LogError($"[SaveSystem] 清除存档 {slot} 失败: {e.Message}");
                }
            }

            foreach (var kvp in Handlers)
            {
                try { kvp.Value.OnLoad(kvp.Value.CreateNew()); }
                catch (Exception e) { Logger.LogError($"[SaveSystem] 重置 {kvp.Key} 失败: {e.Message}"); }
            }
        }

        private static void CreateBackup(string dir)
        {
            try
            {
                string backupDir = dir + "_backup";
                if (Directory.Exists(backupDir))
                    Directory.Delete(backupDir, true);

                CopyDirectory(dir, backupDir);

                CleanupOldBackups(Path.GetDirectoryName(dir));
            }
            catch (Exception e)
            {
                Logger.LogWarn($"[SaveSystem] 创建备份失败: {e.Message}");
            }
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
            foreach (string subDir in Directory.GetDirectories(source))
                CopyDirectory(subDir, Path.Combine(dest, Path.GetFileName(subDir)));
        }

        private static void CleanupOldBackups(string parentDir)
        {
            try
            {
                var backups = Directory.GetDirectories(parentDir, "*_backup")
                    .Select(d => new DirectoryInfo(d))
                    .OrderByDescending(d => d.LastWriteTime)
                    .Skip(_maxBackups)
                    .ToList();

                foreach (var backup in backups)
                {
                    try { backup.Delete(true); }
                    catch {   }
                }
            }
            catch {   }
        }

        public static bool ExportSave(int slot, string exportPath)
        {
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, $"slot{slot}");
                if (!Directory.Exists(dir)) return false;

                CopyDirectory(dir, exportPath);
                Logger.Log($"[SaveSystem] 导出存档 {slot} -> {exportPath}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SaveSystem] 导出存档失败: {e.Message}");
                return false;
            }
        }

        public static bool ImportSave(int slot, string importPath)
        {
            try
            {
                if (!Directory.Exists(importPath)) return false;

                string dir = Path.Combine(Application.persistentDataPath, $"slot{slot}");
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);

                CopyDirectory(importPath, dir);
                Logger.Log($"[SaveSystem] 导入存档 {slot} <- {importPath}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SaveSystem] 导入存档失败: {e.Message}");
                return false;
            }
        }
    }

    public interface ISaveHandler
    {
        object CreateNew();
        void OnLoad(object data);
        object OnSave();
    }

    public abstract class SaveHandler<T> : ISaveHandler where T : class, new()
    {
        object ISaveHandler.CreateNew() => new T();
        void ISaveHandler.OnLoad(object data) => OnLoad((T)data);
        object ISaveHandler.OnSave() => OnSave();

        protected abstract void OnLoad(T data);
        protected abstract T OnSave();
    }
}
