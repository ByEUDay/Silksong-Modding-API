using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Modding
{



    public static class ModAssetManager
    {
        private static readonly Dictionary<string, AssetBundle> LoadedBundles =
            new Dictionary<string, AssetBundle>();

        private static readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> CachedAssets =
            new Dictionary<string, Dictionary<string, UnityEngine.Object>>();




        public static AssetBundle LoadBundle(string path)
        {
            if (LoadedBundles.ContainsKey(path))
            {
                Logger.LogWarn($"[ModAssetManager] Bundle {path} 已加载");
                return LoadedBundles[path];
            }

            try
            {
                var bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                {
                    LoadedBundles[path] = bundle;
                    Logger.Log($"[ModAssetManager] 已加载 Bundle: {path}");
                }
                return bundle;
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModAssetManager] 加载 Bundle 失败: {e}");
                return null;
            }
        }




        public static AssetBundleCreateRequest LoadBundleAsync(string path)
        {
            try
            {
                var request = AssetBundle.LoadFromFileAsync(path);
                request.completed += (op) =>
                {
                    var bundle = (op as AssetBundleCreateRequest)?.assetBundle;
                    if (bundle != null)
                    {
                        LoadedBundles[path] = bundle;
                        Logger.Log($"[ModAssetManager] 异步加载 Bundle 完成: {path}");
                    }
                };
                return request;
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModAssetManager] 异步加载 Bundle 失败: {e}");
                return null;
            }
        }




        public static T LoadAsset<T>(string bundlePath, string assetName) where T : UnityEngine.Object
        {
            if (!LoadedBundles.ContainsKey(bundlePath))
            {
                Logger.LogError($"[ModAssetManager] Bundle 未加载: {bundlePath}");
                return null;
            }

            var bundle = LoadedBundles[bundlePath];
            try
            {
                var asset = bundle.LoadAsset<T>(assetName);
                if (asset != null)
                {
                    CacheAsset(bundlePath, assetName, asset);
                }
                return asset;
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModAssetManager] 加载资源失败: {e}");
                return null;
            }
        }




        public static AssetBundleRequest LoadAssetAsync<T>(string bundlePath, string assetName, Action<T> callback)
            where T : UnityEngine.Object
        {
            if (!LoadedBundles.ContainsKey(bundlePath))
            {
                Logger.LogError($"[ModAssetManager] Bundle 未加载: {bundlePath}");
                callback?.Invoke(null);
                return null;
            }

            var bundle = LoadedBundles[bundlePath];
            var request = bundle.LoadAssetAsync<T>(assetName);
            request.completed += (op) =>
            {
                var asset = (op as AssetBundleRequest)?.asset as T;
                if (asset != null)
                {
                    CacheAsset(bundlePath, assetName, asset);
                }
                callback?.Invoke(asset);
            };
            return request;
        }




        public static T[] LoadAllAssets<T>(string bundlePath) where T : UnityEngine.Object
        {
            if (!LoadedBundles.ContainsKey(bundlePath))
            {
                Logger.LogError($"[ModAssetManager] Bundle 未加载: {bundlePath}");
                return null;
            }

            var bundle = LoadedBundles[bundlePath];
            try
            {
                return bundle.LoadAllAssets<T>();
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModAssetManager] 加载所有资源失败: {e}");
                return null;
            }
        }




        public static void UnloadBundle(string bundlePath, bool unloadAllLoadedObjects = true)
        {
            if (LoadedBundles.ContainsKey(bundlePath))
            {
                var bundle = LoadedBundles[bundlePath];
                bundle.Unload(unloadAllLoadedObjects);
                LoadedBundles.Remove(bundlePath);
                CachedAssets.Remove(bundlePath);
                Logger.Log($"[ModAssetManager] 已卸载 Bundle: {bundlePath}");
            }
        }




        public static void UnloadAllBundles(bool unloadAllLoadedObjects = true)
        {
            foreach (var kvp in LoadedBundles)
            {
                kvp.Value.Unload(unloadAllLoadedObjects);
            }
            LoadedBundles.Clear();
            CachedAssets.Clear();
            Logger.Log("[ModAssetManager] 已卸载所有 Bundle");
        }




        private static void CacheAsset(string bundlePath, string assetName, UnityEngine.Object asset)
        {
            if (!CachedAssets.ContainsKey(bundlePath))
            {
                CachedAssets[bundlePath] = new Dictionary<string, UnityEngine.Object>();
            }
            CachedAssets[bundlePath][assetName] = asset;
        }




        public static T GetCachedAsset<T>(string bundlePath, string assetName) where T : UnityEngine.Object
        {
            if (CachedAssets.ContainsKey(bundlePath) && CachedAssets[bundlePath].ContainsKey(assetName))
            {
                return CachedAssets[bundlePath][assetName] as T;
            }
            return null;
        }




        public static bool IsBundleLoaded(string bundlePath)
        {
            return LoadedBundles.ContainsKey(bundlePath);
        }




        public static IEnumerable<string> GetLoadedBundles()
        {
            return LoadedBundles.Keys;
        }
    }




    public static class ModSceneUtils
    {



        public static void LoadScene(string sceneName)
        {
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                Logger.Log($"[ModSceneUtils] 已加载场景: {sceneName}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSceneUtils] 加载场景失败: {e}");
            }
        }




        public static AsyncOperation LoadSceneAsync(string sceneName, Action callback = null)
        {
            try
            {
                var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                op.completed += (o) =>
                {
                    Logger.Log($"[ModSceneUtils] 异步加载场景完成: {sceneName}");
                    callback?.Invoke();
                };
                return op;
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSceneUtils] 异步加载场景失败: {e}");
                return null;
            }
        }




        public static UnityEngine.SceneManagement.Scene GetActiveScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }




        public static string GetSceneName()
        {
            return GetActiveScene().name;
        }




        public static UnityEngine.SceneManagement.Scene[] GetAllScenes()
        {
            var scenes = new UnityEngine.SceneManagement.Scene[UnityEngine.SceneManagement.SceneManager.sceneCount];
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                scenes[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            }
            return scenes;
        }




        public static bool IsSceneLoaded(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }
    }
}
