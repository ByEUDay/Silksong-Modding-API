using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModNetworkUtils
    {



        public static void Get(string url, Action<string> onSuccess, Action<string> onError = null)
        {
            try
            {
                var request = new UnityEngine.Networking.UnityWebRequest(url, "GET");
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                var operation = request.SendWebRequest();
                operation.completed += (op) =>
                {
                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        onSuccess?.Invoke(request.downloadHandler.text);
                    }
                    else
                    {
                        onError?.Invoke(request.error);
                    }
                    request.Dispose();
                };
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }
        }




        public static void Post(string url, string data, Action<string> onSuccess, Action<string> onError = null)
        {
            try
            {
                var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                var operation = request.SendWebRequest();
                operation.completed += (op) =>
                {
                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        onSuccess?.Invoke(request.downloadHandler.text);
                    }
                    else
                    {
                        onError?.Invoke(request.error);
                    }
                    request.Dispose();
                };
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }
        }




        public static void DownloadFile(string url, string savePath, Action onSuccess, Action<string> onError = null)
        {
            try
            {
                var request = UnityEngine.Networking.UnityWebRequest.Get(url);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerFile(savePath);
                var operation = request.SendWebRequest();
                operation.completed += (op) =>
                {
                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        onSuccess?.Invoke();
                    }
                    else
                    {
                        onError?.Invoke(request.error);
                    }
                    request.Dispose();
                };
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }
        }




        public static void CheckConnection(Action<bool> callback)
        {
            Get("https://www.google.com",
                (response) => callback?.Invoke(true),
                (error) => callback?.Invoke(false));
        }
    }




    public static class ModLocalizationUtils
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>();

        private static string _currentLanguage = "en";




        public static void SetLanguage(string language)
        {
            _currentLanguage = language;
            Logger.Log($"[ModLocalizationUtils] 语言已设置为: {language}");
        }




        public static string GetLanguage()
        {
            return _currentLanguage;
        }




        public static void AddTranslation(string language, string key, string value)
        {
            if (!Translations.ContainsKey(language))
            {
                Translations[language] = new Dictionary<string, string>();
            }
            Translations[language][key] = value;
        }




        public static void AddTranslations(string language, Dictionary<string, string> translations)
        {
            if (!Translations.ContainsKey(language))
            {
                Translations[language] = new Dictionary<string, string>();
            }
            foreach (var kvp in translations)
            {
                Translations[language][kvp.Key] = kvp.Value;
            }
        }




        public static string GetText(string key, string defaultValue = null)
        {
            if (Translations.ContainsKey(_currentLanguage) &&
                Translations[_currentLanguage].ContainsKey(key))
            {
                return Translations[_currentLanguage][key];
            }
            return defaultValue ?? key;
        }




        public static string GetText(string key, params object[] args)
        {
            string text = GetText(key);
            try
            {
                return string.Format(text, args);
            }
            catch
            {
                return text;
            }
        }




        public static bool HasTranslation(string key, string language = null)
        {
            language = language ?? _currentLanguage;
            return Translations.ContainsKey(language) && Translations[language].ContainsKey(key);
        }




        public static IEnumerable<string> GetSupportedLanguages()
        {
            return Translations.Keys;
        }




        public static void ClearAll()
        {
            Translations.Clear();
        }




        public static void LoadFromJson(string language, string json)
        {
            try
            {
                var translations = JsonUtility.FromJson<TranslationWrapper>(json);
                if (translations != null && translations.data != null)
                {
                    AddTranslations(language, translations.data);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModLocalizationUtils] 加载翻译失败: {e}");
            }
        }

        [Serializable]
        private class TranslationWrapper
        {
            public Dictionary<string, string> data;
        }
    }




    public static class ModVersionUtils
    {



        public static int CompareVersions(string v1, string v2)
        {
            var parts1 = v1.Split('.');
            var parts2 = v2.Split('.');

            int maxLength = Mathf.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                int num1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                int num2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;

                if (num1 < num2) return -1;
                if (num1 > num2) return 1;
            }
            return 0;
        }




        public static bool HasNewVersion(string currentVersion, string latestVersion)
        {
            return CompareVersions(currentVersion, latestVersion) < 0;
        }




        public static Version ParseVersion(string versionString)
        {
            try
            {
                var parts = versionString.Split('.');
                int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
                int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
                int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
                return new Version(major, minor, patch);
            }
            catch
            {
                return new Version(0, 0, 0);
            }
        }




        public class Version
        {
            public int Major { get; }
            public int Minor { get; }
            public int Patch { get; }
            public string Build { get; }

            public Version(int major, int minor, int patch, string build = "")
            {
                Major = major;
                Minor = minor;
                Patch = patch;
                Build = build;
            }

            public override string ToString()
            {
                string version = $"{Major}.{Minor}.{Patch}";
                if (!string.IsNullOrEmpty(Build))
                {
                    version += $"-{Build}";
                }
                return version;
            }
        }
    }
}
