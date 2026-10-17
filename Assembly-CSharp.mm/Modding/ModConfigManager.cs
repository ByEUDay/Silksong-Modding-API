using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModConfigManager
    {
        private static readonly Dictionary<string, Dictionary<string, object>> Configs =
            new Dictionary<string, Dictionary<string, object>>();

        private static readonly string ConfigPath = System.IO.Path.Combine(
            Application.persistentDataPath, "ModConfig.json");




        public static void SetConfig(string key, object value, string section = "general")
        {
            if (!Configs.ContainsKey(section))
            {
                Configs[section] = new Dictionary<string, object>();
            }
            Configs[section][key] = value;
        }




        public static T GetConfig<T>(string key, T defaultValue = default, string section = "general")
        {
            if (Configs.ContainsKey(section) && Configs[section].ContainsKey(key))
            {
                try
                {
                    return (T)Convert.ChangeType(Configs[section][key], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }




        public static bool HasConfig(string key, string section = "general")
        {
            return Configs.ContainsKey(section) && Configs[section].ContainsKey(key);
        }




        public static void DeleteConfig(string key, string section = "general")
        {
            if (Configs.ContainsKey(section))
            {
                Configs[section].Remove(key);
            }
        }




        public static void DeleteSection(string section)
        {
            Configs.Remove(section);
        }




        public static void ClearAll()
        {
            Configs.Clear();
        }




        public static void Save()
        {
            try
            {
                var saveData = new Dictionary<string, Dictionary<string, string>>();
                foreach (var section in Configs)
                {
                    saveData[section.Key] = new Dictionary<string, string>();
                    foreach (var kvp in section.Value)
                    {
                        saveData[section.Key][kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }
                string json = JsonUtility.ToJson(new ConfigWrapper { data = saveData });
                System.IO.File.WriteAllText(ConfigPath, json);
                Logger.Log($"[ModConfigManager] 配置已保存到 {ConfigPath}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModConfigManager] 保存失败: {e}");
            }
        }




        public static void Load()
        {
            try
            {
                if (System.IO.File.Exists(ConfigPath))
                {
                    string json = System.IO.File.ReadAllText(ConfigPath);
                    var wrapper = JsonUtility.FromJson<ConfigWrapper>(json);
                    if (wrapper != null && wrapper.data != null)
                    {
                        Configs.Clear();
                        foreach (var section in wrapper.data)
                        {
                            Configs[section.Key] = new Dictionary<string, object>();
                            foreach (var kvp in section.Value)
                            {
                                Configs[section.Key][kvp.Key] = kvp.Value;
                            }
                        }
                        Logger.Log($"[ModConfigManager] 配置已从 {ConfigPath} 加载");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModConfigManager] 加载失败: {e}");
            }
        }

        [Serializable]
        private class ConfigWrapper
        {
            public Dictionary<string, Dictionary<string, string>> data;
        }
    }




    public static class ModProfiler
    {
        private static readonly Dictionary<string, ProfilerData> Profiles =
            new Dictionary<string, ProfilerData>();




        public static void Begin(string name)
        {
            if (!Profiles.ContainsKey(name))
            {
                Profiles[name] = new ProfilerData();
            }
            Profiles[name].Begin();
        }




        public static void End(string name)
        {
            if (Profiles.ContainsKey(name))
            {
                Profiles[name].End();
            }
        }




        public static ProfilerData GetProfile(string name)
        {
            return Profiles.TryGetValue(name, out var data) ? data : null;
        }




        public static void ClearAll()
        {
            Profiles.Clear();
        }




        public static void LogAll()
        {
            Logger.Log("[ModProfiler] 性能分析结果:");
            foreach (var kvp in Profiles)
            {
                var data = kvp.Value;
                Logger.Log($"  {kvp.Key}:");
                Logger.Log($"    平均: {data.AverageTime:F4}ms");
                Logger.Log($"    最小: {data.MinTime:F4}ms");
                Logger.Log($"    最大: {data.MaxTime:F4}ms");
                Logger.Log($"    调用次数: {data.CallCount}");
            }
        }




        public class ProfilerData
        {
            private float _startTime;
            private float _totalTime;
            private float _minTime = float.MaxValue;
            private float _maxTime = float.MinValue;
            private int _callCount;

            public float AverageTime => _callCount > 0 ? _totalTime / _callCount * 1000f : 0f;
            public float MinTime => _minTime * 1000f;
            public float MaxTime => _maxTime * 1000f;
            public int CallCount => _callCount;

            public void Begin()
            {
                _startTime = Time.realtimeSinceStartup;
            }

            public void End()
            {
                float elapsed = Time.realtimeSinceStartup - _startTime;
                _totalTime += elapsed;
                _minTime = Mathf.Min(_minTime, elapsed);
                _maxTime = Mathf.Max(_maxTime, elapsed);
                _callCount++;
            }
        }
    }
}
