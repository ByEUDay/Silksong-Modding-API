using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModDataPersistence
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Data =
            new Dictionary<string, Dictionary<string, string>>();

        private static readonly string SavePath = System.IO.Path.Combine(
            Application.persistentDataPath, "ModData.json");




        public static void SetString(string key, string value, string category = "default")
        {
            if (!Data.ContainsKey(category))
            {
                Data[category] = new Dictionary<string, string>();
            }
            Data[category][key] = value;
        }




        public static string GetString(string key, string defaultValue = "", string category = "default")
        {
            if (Data.ContainsKey(category) && Data[category].ContainsKey(key))
            {
                return Data[category][key];
            }
            return defaultValue;
        }




        public static void SetInt(string key, int value, string category = "default")
        {
            SetString(key, value.ToString(), category);
        }




        public static int GetInt(string key, int defaultValue = 0, string category = "default")
        {
            string str = GetString(key, "", category);
            return int.TryParse(str, out int result) ? result : defaultValue;
        }




        public static void SetFloat(string key, float value, string category = "default")
        {
            SetString(key, value.ToString("R"), category);
        }




        public static float GetFloat(string key, float defaultValue = 0f, string category = "default")
        {
            string str = GetString(key, "", category);
            return float.TryParse(str, out float result) ? result : defaultValue;
        }




        public static void SetBool(string key, bool value, string category = "default")
        {
            SetString(key, value.ToString(), category);
        }




        public static bool GetBool(string key, bool defaultValue = false, string category = "default")
        {
            string str = GetString(key, "", category);
            return bool.TryParse(str, out bool result) ? result : defaultValue;
        }




        public static void SetVector3(string key, Vector3 value, string category = "default")
        {
            SetString(key, $"{value.x},{value.y},{value.z}", category);
        }




        public static Vector3 GetVector3(string key, Vector3 defaultValue = default, string category = "default")
        {
            string str = GetString(key, "", category);
            if (string.IsNullOrEmpty(str)) return defaultValue;

            string[] parts = str.Split(',');
            if (parts.Length != 3) return defaultValue;

            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return defaultValue;
        }




        public static bool HasKey(string key, string category = "default")
        {
            return Data.ContainsKey(category) && Data[category].ContainsKey(key);
        }




        public static void DeleteKey(string key, string category = "default")
        {
            if (Data.ContainsKey(category))
            {
                Data[category].Remove(key);
            }
        }




        public static void DeleteCategory(string category)
        {
            Data.Remove(category);
        }




        public static void ClearAll()
        {
            Data.Clear();
        }




        public static void Save()
        {
            try
            {
                var saveData = new Dictionary<string, Dictionary<string, string>>(Data);
                string json = JsonUtility.ToJson(new SaveDataWrapper { data = saveData });
                System.IO.File.WriteAllText(SavePath, json);
                Logger.Log($"[ModDataPersistence] 数据已保存到 {SavePath}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModDataPersistence] 保存失败: {e}");
            }
        }




        public static void Load()
        {
            try
            {
                if (System.IO.File.Exists(SavePath))
                {
                    string json = System.IO.File.ReadAllText(SavePath);
                    var wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
                    if (wrapper != null && wrapper.data != null)
                    {
                        Data.Clear();
                        foreach (var kvp in wrapper.data)
                        {
                            Data[kvp.Key] = new Dictionary<string, string>(kvp.Value);
                        }
                        Logger.Log($"[ModDataPersistence] 数据已从 {SavePath} 加载");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModDataPersistence] 加载失败: {e}");
            }
        }

        [Serializable]
        private class SaveDataWrapper
        {
            public Dictionary<string, Dictionary<string, string>> data;
        }
    }




    public static class ModEventSystem
    {
        private static readonly Dictionary<string, List<Action<object>>> Listeners =
            new Dictionary<string, List<Action<object>>>();




        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (!Listeners.ContainsKey(eventName))
            {
                Listeners[eventName] = new List<Action<object>>();
            }
            Listeners[eventName].Add(listener);
        }




        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (Listeners.ContainsKey(eventName))
            {
                Listeners[eventName].Remove(listener);
            }
        }




        public static void Publish(string eventName, object data = null)
        {
            if (Listeners.ContainsKey(eventName))
            {
                foreach (var listener in Listeners[eventName])
                {
                    try
                    {
                        listener(data);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"[ModEventSystem] 事件 {eventName} 监听器异常: {e}");
                    }
                }
            }
        }




        public static void ClearAll()
        {
            Listeners.Clear();
        }




        public static void Clear(string eventName)
        {
            Listeners.Remove(eventName);
        }




        public static bool HasListeners(string eventName)
        {
            return Listeners.ContainsKey(eventName) && Listeners[eventName].Count > 0;
        }




        public static int GetListenerCount(string eventName)
        {
            return Listeners.ContainsKey(eventName) ? Listeners[eventName].Count : 0;
        }
    }
}
