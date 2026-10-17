using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modding
{



    public static class ModLifecycleManager
    {
        private static readonly Dictionary<string, ModLifecycle> Lifecycles = new Dictionary<string, ModLifecycle>();

        public static void Register(IMod mod)
        {
            if (mod == null) return;
            string name = mod.GetName();
            if (!Lifecycles.ContainsKey(name))
            {
                Lifecycles[name] = new ModLifecycle(mod);
                Logger.Log($"[Lifecycle] 注册: {name}");
            }
        }

        public static void Unregister(string modName)
        {
            if (Lifecycles.Remove(modName))
                Logger.Log($"[Lifecycle] 移除: {modName}");
        }

        public static ModLifecycle GetLifecycle(string modName)
        {
            return Lifecycles.TryGetValue(modName, out var lifecycle) ? lifecycle : null;
        }

        public static IEnumerable<ModLifecycle> GetAllLifecycles() => Lifecycles.Values;

        public static bool IsInitialized(string modName)
        {
            return Lifecycles.TryGetValue(modName, out var lifecycle) && lifecycle.IsInitialized;
        }

        public static bool IsEnabled(string modName)
        {
            return Lifecycles.TryGetValue(modName, out var lifecycle) && lifecycle.IsEnabled;
        }
    }

    public class ModLifecycle
    {
        public IMod Mod { get; }
        public bool IsInitialized { get; private set; }
        public bool IsEnabled { get; private set; }
        public DateTime InitializedAt { get; private set; }
        public DateTime? DisabledAt { get; private set; }

        private readonly List<Action> _onInitializeCallbacks = new List<Action>();
        private readonly List<Action> _onEnableCallbacks = new List<Action>();
        private readonly List<Action> _onDisableCallbacks = new List<Action>();
        private readonly List<Action> _onDestroyCallbacks = new List<Action>();

        public ModLifecycle(IMod mod)
        {
            Mod = mod;
            IsEnabled = true;
        }

        public void OnInitialize(Action callback) => _onInitializeCallbacks.Add(callback);
        public void OnEnable(Action callback) => _onEnableCallbacks.Add(callback);
        public void OnDisable(Action callback) => _onDisableCallbacks.Add(callback);
        public void OnDestroy(Action callback) => _onDestroyCallbacks.Add(callback);

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            InitializedAt = DateTime.Now;

            foreach (var callback in _onInitializeCallbacks)
            {
                try { callback(); }
                catch (Exception e) { Logger.LogError($"[Lifecycle:{Mod.GetName()}] 初始化回调失败: {e.Message}"); }
            }
        }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            DisabledAt = null;

            foreach (var callback in _onEnableCallbacks)
            {
                try { callback(); }
                catch (Exception e) { Logger.LogError($"[Lifecycle:{Mod.GetName()}] 启用回调失败: {e.Message}"); }
            }
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            DisabledAt = DateTime.Now;

            foreach (var callback in _onDisableCallbacks)
            {
                try { callback(); }
                catch (Exception e) { Logger.LogError($"[Lifecycle:{Mod.GetName()}] 禁用回调失败: {e.Message}"); }
            }
        }

        public void Destroy()
        {
            foreach (var callback in _onDestroyCallbacks)
            {
                try { callback(); }
                catch (Exception e) { Logger.LogError($"[Lifecycle:{Mod.GetName()}] 销毁回调失败: {e.Message}"); }
            }

            _onInitializeCallbacks.Clear();
            _onEnableCallbacks.Clear();
            _onDisableCallbacks.Clear();
            _onDestroyCallbacks.Clear();
        }
    }




    public static class ModDependencyManager
    {
        private static readonly Dictionary<string, List<string>> Dependencies = new Dictionary<string, List<string>>();

        public static void AddDependency(string modName, string dependsOn)
        {
            if (!Dependencies.ContainsKey(modName))
                Dependencies[modName] = new List<string>();
            if (!Dependencies[modName].Contains(dependsOn))
                Dependencies[modName].Add(dependsOn);
        }

        public static void RemoveDependency(string modName, string dependsOn)
        {
            if (Dependencies.ContainsKey(modName))
                Dependencies[modName].Remove(dependsOn);
        }

        public static IEnumerable<string> GetDependencies(string modName)
        {
            return Dependencies.TryGetValue(modName, out var deps) ? deps : Enumerable.Empty<string>();
        }

        public static bool HasDependency(string modName, string dependsOn)
        {
            return Dependencies.TryGetValue(modName, out var deps) && deps.Contains(dependsOn);
        }

        public static bool AreDependenciesSatisfied(string modName)
        {
            if (!Dependencies.ContainsKey(modName)) return true;
            return Dependencies[modName].All(ModLoader.IsModLoaded);
        }

        public static IEnumerable<string> GetMissingDependencies(string modName)
        {
            if (!Dependencies.ContainsKey(modName)) return Enumerable.Empty<string>();
            return Dependencies[modName].Where(d => !ModLoader.IsModLoaded(d));
        }

        public static IEnumerable<string> GetDependents(string modName)
        {
            return Dependencies.Where(kvp => kvp.Value.Contains(modName)).Select(kvp => kvp.Key);
        }

        public static bool CanDisable(string modName)
        {
            return !GetDependents(modName).Any(d => {
                var mod = ModLoader.GetMod(d);
                return mod != null && ModLoader.IsEnabled(mod);
            });
        }

        public static IEnumerable<string> GetLoadOrder()
        {
            var sorted = new List<string>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            foreach (var modName in Dependencies.Keys)
                Visit(modName, sorted, visited, visiting);

            return sorted;
        }

        private static void Visit(string modName, List<string> sorted, HashSet<string> visited, HashSet<string> visiting)
        {
            if (visited.Contains(modName)) return;
            if (visiting.Contains(modName))
            {
                Logger.LogWarn($"[Dependency] 循环依赖: {modName}");
                return;
            }

            visiting.Add(modName);

            if (Dependencies.TryGetValue(modName, out var deps))
            {
                foreach (var dep in deps)
                    Visit(dep, sorted, visited, visiting);
            }

            visiting.Remove(modName);
            visited.Add(modName);
            sorted.Add(modName);
        }
    }
}
