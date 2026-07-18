using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongModLoader
{
    internal static class InternalHooks
    {
        public static void Apply(string loaderDir)
        {
            SceneManager.activeSceneChanged += (prev, next) =>
            {
                try { ModHooks.RaiseSceneChanged(next.name); }
                catch (Exception e) { ModLog.Error($"OnSceneChanged 订阅者抛出异常: {e}"); }
            };
            var tickerObject = new GameObject("SilksongModLoaderTicker");
            UnityEngine.Object.DontDestroyOnLoad(tickerObject);
            var behaviour = tickerObject.AddComponent<ModTickerBehaviour>();
            behaviour.ModsDir = Path.Combine(loaderDir, "Mods");

            ModLog.Info("已挂载 Unity 原生 Ticker,OnHeroUpdate 将每帧触发,mod 将逐帧加载。");

            try
            {
                GameHooks.ApplySaveLoadHooks();
            }
            catch (Exception e)
            {
                ModLog.Error($"GameHooks.ApplySaveLoadHooks 执行失败,存档相关事件可能无法触发: {e}");
            }
        }
    }
    internal class ModTickerBehaviour : MonoBehaviour
    {
        public string ModsDir = "";

        private GUIStyle? _labelStyle;
        private GUIStyle? _failedLabelStyle;

        private void Start()
        {
            StartCoroutine(LoadModsRoutine());
        }

        private void Update()
        {
            try { ModHooks.RaiseHeroUpdate(); }
            catch (Exception e) { ModLog.Error($"OnHeroUpdate 订阅者抛出异常: {e}"); }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                ModMenuUI.Toggle();
            }

            ModMenuUI.EnforceInputBlock();
        }

        private void OnApplicationQuit()
        {
            try { HookHelper.RemoveAll(); }
            catch (Exception e) { ModLog.Error($"退出时清理 hook 失败: {e}"); }
        }

        private IEnumerator LoadModsRoutine()
        {
            ModConfig.Load(ModsDir);

            var discovered = Entry.DiscoverMods(ModsDir);
            foreach (var mod in discovered)
            {
                ModStatus.Mods.Add(new ModEntry { Mod = mod });
            }
            yield return null;

            foreach (var entry in ModStatus.Mods)
            {
                if (ModConfig.IsDisabled(entry.Mod.Name))
                {
                    entry.State = ModLoadState.Disabled;
                    ModLog.Info($"{entry.Mod.Name} 已被禁用,跳过初始化。");
                    ModStatus.LoadedCount++;
                    yield return null;
                    continue;
                }

                try
                {
                    entry.Mod.Initialize();
                    entry.State = ModLoadState.Loaded;
                    ModLog.Info($"已加载: {entry.Mod.Name} v{entry.Mod.Version}");
                }
                catch (Exception e)
                {
                    entry.State = ModLoadState.Failed;
                    ModLog.Error($"{entry.Mod.Name} 初始化失败,已跳过: {e}");
                }

                ModStatus.LoadedCount++;
                yield return null;
            }

            ModLog.Info($"共加载 {ModStatus.Mods.Count} 个 mod。");
        }

        private void OnGUI()
        {
            _labelStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            _failedLabelStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.red } };

            if (ModStatus.IsLoading)
            {
                DrawLoadingBar();
            }

            DrawModList();
            ModMenuUI.Draw();
        }

        private void DrawLoadingBar()
        {
            int total = ModStatus.Mods.Count;
            int loaded = ModStatus.LoadedCount;
            float pct = total == 0 ? 1f : (float)loaded / total;

            const float barWidth = 320f;
            const float barHeight = 22f;
            float x = (Screen.width - barWidth) / 2f;
            float y = 60f;

            GUI.Box(new Rect(x, y, barWidth, barHeight), "");
            GUI.Box(new Rect(x, y, barWidth * pct, barHeight), "");
            GUI.Label(new Rect(x, y + barHeight + 4, barWidth, 20),
                $"正在加载 Mod... {loaded}/{total}", _labelStyle);
        }

        private void DrawModList()
        {
            float y = 8f;
            foreach (var entry in ModStatus.Mods)
            {
                var style = entry.State == ModLoadState.Failed ? _failedLabelStyle : _labelStyle;
                var text = entry.State switch
                {
                    ModLoadState.Loaded => $"{entry.Mod.Name} v{entry.Mod.Version}",
                    ModLoadState.Failed => $"{entry.Mod.Name} (加载失败)",
                    ModLoadState.Disabled => $"{entry.Mod.Name} (已禁用)",
                    _ => $"{entry.Mod.Name} (等待中...)"
                };
                GUI.Label(new Rect(8, y, 400, 20), text, style);
                y += 18f;
            }
        }
    }
}
