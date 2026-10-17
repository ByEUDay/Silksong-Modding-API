using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modding
{



    public static class ModSceneLoader
    {
        private static readonly Dictionary<string, List<Action<string>>> SceneLoadCallbacks = new Dictionary<string, List<Action<string>>>();
        private static readonly Dictionary<string, List<Action<string>>> SceneUnloadCallbacks = new Dictionary<string, List<Action<string>>>();
        private static readonly HashSet<string> PreloadedScenes = new HashSet<string>();

        public static void OnSceneLoaded(string sceneName, Action<string> callback)
        {
            if (!SceneLoadCallbacks.ContainsKey(sceneName))
                SceneLoadCallbacks[sceneName] = new List<Action<string>>();
            SceneLoadCallbacks[sceneName].Add(callback);
        }

        public static void OnSceneUnloaded(string sceneName, Action<string> callback)
        {
            if (!SceneUnloadCallbacks.ContainsKey(sceneName))
                SceneUnloadCallbacks[sceneName] = new List<Action<string>>();
            SceneUnloadCallbacks[sceneName].Add(callback);
        }

        public static void RemoveSceneLoadCallback(string sceneName, Action<string> callback)
        {
            if (SceneLoadCallbacks.ContainsKey(sceneName))
                SceneLoadCallbacks[sceneName].Remove(callback);
        }

        public static void RemoveSceneUnloadCallback(string sceneName, Action<string> callback)
        {
            if (SceneUnloadCallbacks.ContainsKey(sceneName))
                SceneUnloadCallbacks[sceneName].Remove(callback);
        }

        internal static void NotifySceneLoaded(string sceneName)
        {
            if (SceneLoadCallbacks.TryGetValue(sceneName, out var callbacks))
            {
                foreach (var callback in callbacks.ToList())
                {
                    try { callback(sceneName); }
                    catch (Exception e) { Logger.LogError($"[SceneLoader] 场景加载回调失败: {e.Message}"); }
                }
            }
        }

        internal static void NotifySceneUnloaded(string sceneName)
        {
            if (SceneUnloadCallbacks.TryGetValue(sceneName, out var callbacks))
            {
                foreach (var callback in callbacks.ToList())
                {
                    try { callback(sceneName); }
                    catch (Exception e) { Logger.LogError($"[SceneLoader] 场景卸载回调失败: {e.Message}"); }
                }
            }
        }

        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                    return true;
            }
            return false;
        }

        public static string GetCurrentScene()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return activeScene.name ?? "";
        }

        public static string[] GetLoadedScenes()
        {
            var scenes = new List<string>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    scenes.Add(scene.name);
            }
            return scenes.ToArray();
        }

        public static void PreloadScene(string sceneName)
        {
            if (PreloadedScenes.Contains(sceneName)) return;
            PreloadedScenes.Add(sceneName);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            Logger.Log($"[SceneLoader] 预加载场景: {sceneName}");
        }

        public static void UnloadPreloadedScene(string sceneName)
        {
            if (!PreloadedScenes.Contains(sceneName)) return;
            PreloadedScenes.Remove(sceneName);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            Logger.Log($"[SceneLoader] 卸载预加载场景: {sceneName}");
        }

        public static void LoadScene(string sceneName, Action onComplete = null)
        {
            ModRunner.Start(LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single, onComplete));
        }

        public static void LoadSceneAdditive(string sceneName, Action onComplete = null)
        {
            ModRunner.Start(LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive, onComplete));
        }

        private static System.Collections.IEnumerator LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, Action onComplete)
        {
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            while (!operation.isDone)
                yield return null;
            onComplete?.Invoke();
        }

        public static void UnloadScene(string sceneName, Action onComplete = null)
        {
            ModRunner.Start(UnloadSceneAsync(sceneName, onComplete));
        }

        private static System.Collections.IEnumerator UnloadSceneAsync(string sceneName, Action onComplete)
        {
            var operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            if (operation != null)
            {
                while (!operation.isDone)
                    yield return null;
            }
            onComplete?.Invoke();
        }

        public static GameObject[] FindObjectsInScene(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return new GameObject[0];
            return scene.GetRootGameObjects();
        }

        public static T[] FindComponentsInScene<T>(string sceneName) where T : Component
        {
            var roots = FindObjectsInScene(sceneName);
            var components = new List<T>();
            foreach (var root in roots)
                components.AddRange(root.GetComponentsInChildren<T>(true));
            return components.ToArray();
        }
    }




    public static class ModObjectFinder
    {
        public static GameObject Find(string name)
        {
            return GameObject.Find(name);
        }

        public static GameObject FindWithTag(string tag)
        {
            return GameObject.FindWithTag(tag);
        }

        public static GameObject[] FindAllWithTag(string tag)
        {
            return GameObject.FindGameObjectsWithTag(tag);
        }

        public static T FindObjectOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectOfType<T>();
        }

        public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsOfType<T>();
        }

        public static GameObject FindByPath(string path)
        {
            var parts = path.Split('/');
            GameObject current = GameObject.Find(parts[0]);
            for (int i = 1; i < parts.Length && current != null; i++)
            {
                var child = current.transform.Find(parts[i]);
                current = child?.gameObject;
            }
            return current;
        }

        public static T FindComponentInParents<T>(Component component) where T : Component
        {
            if (component == null) return null;
            var current = component.transform;
            while (current != null)
            {
                var comp = current.GetComponent<T>();
                if (comp != null) return comp;
                current = current.parent;
            }
            return null;
        }

        public static T FindComponentInChildren<T>(Component component) where T : Component
        {
            if (component == null) return null;
            return component.GetComponentInChildren<T>();
        }

        public static T[] FindComponentsInChildren<T>(Component component) where T : Component
        {
            if (component == null) return new T[0];
            return component.GetComponentsInChildren<T>(true);
        }

        public static GameObject FindChildByName(GameObject parent, string childName)
        {
            if (parent == null) return null;
            var child = parent.transform.Find(childName);
            return child?.gameObject;
        }

        public static GameObject[] FindAllChildren(GameObject parent)
        {
            if (parent == null) return new GameObject[0];
            var children = new GameObject[parent.transform.childCount];
            for (int i = 0; i < parent.transform.childCount; i++)
                children[i] = parent.transform.GetChild(i).gameObject;
            return children;
        }

        public static bool HasComponent<T>(GameObject go) where T : Component
        {
            return go != null && go.GetComponent<T>() != null;
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            var comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }
    }
}
