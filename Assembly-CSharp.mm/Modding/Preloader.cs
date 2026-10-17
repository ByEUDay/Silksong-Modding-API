#if !NO_PRELOADER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Modding
{







    internal static class Preloader
    {
        internal static IEnumerator Run(List<IPreloadMod> mods)
        {

            float waited = 0f;
            while (waited < 60f && !IsAtMainMenu())
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            var requests = new Dictionary<IPreloadMod, List<(string, string)>>();
            var perScene = new Dictionary<string, HashSet<string>>();
            foreach (IPreloadMod mod in mods)
            {
                List<(string, string)> names;
                try { names = mod.GetPreloadNames() ?? new List<(string, string)>(); }
                catch (Exception e) { Logger.LogError($"{mod.GetName()}.GetPreloadNames 失败: {e}"); continue; }

                requests[mod] = names;
                foreach ((string scene, string path) in names)
                {
                    if (!perScene.TryGetValue(scene, out HashSet<string> set))
                        perScene[scene] = set = new HashSet<string>();
                    set.Add(path);
                }
            }

            var loaded = new Dictionary<string, Dictionary<string, GameObject>>();

            foreach (KeyValuePair<string, HashSet<string>> kv in perScene)
            {
                string sceneName = kv.Key;
                AsyncOperationHandle<SceneInstance> handle;
                try { handle = Addressables.LoadSceneAsync("Scenes/" + sceneName, LoadSceneMode.Additive, true); }
                catch (Exception e) { Logger.LogError($"预加载场景 {sceneName} 失败: {e.Message}"); continue; }

                yield return handle;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Logger.LogError($"预加载场景 {sceneName} 失败: {handle.OperationException?.Message}");
                    continue;
                }

                Scene scene = handle.Result.Scene;
                var objects = new Dictionary<string, GameObject>();
                foreach (string path in kv.Value)
                {
                    GameObject source = FindByPath(scene, path);
                    if (source == null)
                    {
                        Logger.LogWarn($"预加载：在 {sceneName} 里找不到 {path}");
                        continue;
                    }


                    bool wasActive = source.activeSelf;
                    source.SetActive(false);
                    GameObject copy = UnityEngine.Object.Instantiate(source);
                    source.SetActive(wasActive);
                    copy.name = source.name;
                    copy.SetActive(false);
                    UnityEngine.Object.DontDestroyOnLoad(copy);
                    objects[path] = copy;
                }
                loaded[sceneName] = objects;

                foreach (GameObject root in scene.GetRootGameObjects())
                    root.SetActive(false);

                AsyncOperationHandle<SceneInstance> unload = Addressables.UnloadSceneAsync(handle);
                yield return unload;
            }

            foreach (KeyValuePair<IPreloadMod, List<(string, string)>> kv in requests)
            {
                var subset = new Dictionary<string, Dictionary<string, GameObject>>();
                foreach ((string scene, string path) in kv.Value)
                {
                    if (!loaded.TryGetValue(scene, out Dictionary<string, GameObject> all)) continue;
                    if (!all.TryGetValue(path, out GameObject go)) continue;
                    if (!subset.TryGetValue(scene, out Dictionary<string, GameObject> dict))
                        subset[scene] = dict = new Dictionary<string, GameObject>();
                    dict[path] = go;
                }

                try { kv.Key.OnPreloaded(subset); }
                catch (Exception e) { Logger.LogError($"{kv.Key.GetName()}.OnPreloaded 失败: {e}"); }
            }

            Logger.Log($"预加载完成：{perScene.Count} 个场景");
        }

        private static bool IsAtMainMenu()
        {
            try { return GameManager.instance != null && GameManager.instance.GameState == GlobalEnums.GameState.MAIN_MENU; }
            catch { return false; }
        }

        internal static GameObject FindByPath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0]) continue;
                if (parts.Length == 1) return root;
                Transform t = root.transform.Find(string.Join("/", parts.Skip(1)));
                if (t != null) return t.gameObject;
            }
            return null;
        }
    }
}
#endif
