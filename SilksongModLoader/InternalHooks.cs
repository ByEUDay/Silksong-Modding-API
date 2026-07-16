using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongModLoader
{
    /// <summary>
    /// 真正把 ModHooks 的事件和游戏内部方法连起来的地方。
    /// 这里的类名/方法名(GameManager、HeroController 等)是 Hollow Knight 系列
    /// 常见的类名,Silksong 大概率沿用,但请务必用 dnSpy 核对后再上线,
    /// 类名/方法签名一旦不对,Hook 会在游戏启动时直接抛异常。
    /// </summary>
    internal static class InternalHooks
    {
        public static void Apply(string loaderDir)
        {
            // 场景切换:Unity 自带事件,不需要 MonoMod,最稳妥
            SceneManager.activeSceneChanged += (prev, next) =>
            {
                try { ModHooks.RaiseSceneChanged(next.name); }
                catch (Exception e) { ModLog.Error($"OnSceneChanged 订阅者抛出异常: {e}"); }
            };

            // 常驻的 Ticker + UI 组件:
            //   - 用 Unity 原生 MonoBehaviour.Update() 驱动 OnHeroUpdate,不依赖 MonoMod,
            //     避免了 MonoMod.RuntimeDetour 内部 Mono.Cecil 版本冲突的问题。
            //   - 用协程逐帧加载 mod,这样加载进度条才能实时刷新。
            //   - 用 OnGUI 画进度条 + 左上角常驻 mod 列表。
            var tickerObject = new GameObject("SilksongModLoaderTicker");
            UnityEngine.Object.DontDestroyOnLoad(tickerObject);
            var behaviour = tickerObject.AddComponent<ModTickerBehaviour>();
            behaviour.ModsDir = Path.Combine(loaderDir, "Mods");

            ModLog.Info("已挂载 Unity 原生 Ticker,OnHeroUpdate 将每帧触发,mod 将逐帧加载。");

            // 存档相关的 Hook(OnSaveGameLoaded/OnSaveGameSaved)通常需要 hook
            // GameManager 的具体存档方法,这类"只在特定时机触发一次"的 hook
            // 没有 Unity 原生事件可以直接替代。如果之后要做这部分且遇到
            // MonoMod 版本报错,思路是:确认 MonoMod.RuntimeDetour/MonoMod.Utils
            // 和随它们一起复制过去的 Mono.Cecil.dll 版本是否匹配(都来自同一次
            // dotnet build 的输出目录,理论上应该一致;如果还报错,考虑换用
            // 更简单的反射轮询代替方法级 hook)。
        }
    }

    /// <summary>
    /// 挂在一个常驻 GameObject 上的组件:
    ///   - Update() 驱动 ModHooks.OnHeroUpdate
    ///   - 协程逐帧加载 mod 并更新 ModStatus
    ///   - OnGUI() 画加载进度条 + 左上角 mod 列表
    /// </summary>
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
        }

        private IEnumerator LoadModsRoutine()
        {
            var discovered = Entry.DiscoverMods(ModsDir);
            foreach (var mod in discovered)
            {
                ModStatus.Mods.Add(new ModEntry { Mod = mod });
            }

            // 让一帧先过去,这样进度条至少能画出"0 / N"的初始状态
            yield return null;

            foreach (var entry in ModStatus.Mods)
            {
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
                yield return null; // 每加载一个 mod 让一帧,进度条才能实时刷新
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
                    _ => $"{entry.Mod.Name} (等待中...)"
                };
                GUI.Label(new Rect(8, y, 400, 20), text, style);
                y += 18f;
            }
        }
    }
}
