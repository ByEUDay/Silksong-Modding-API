using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modding
{

    public static class ModLoader
    {
        public const string ApiVersion = "1.0.0";

        private static bool _loaded;
        private static readonly List<IMod> Mods = new List<IMod>();
        private static readonly HashSet<string> Disabled = new HashSet<string>();

        public static IReadOnlyList<IMod> LoadedMods => Mods;


        public static string ModsDirectory
        {
            get
            {


                string managedDir = Path.GetDirectoryName(typeof(ModLoader).Assembly.Location);
                return Path.Combine(managedDir, "Mods");
            }
        }

        private static string DisabledFile => Path.Combine(Application.persistentDataPath, "ModsDisabled.txt");

        internal static void LoadMods()
        {
            if (_loaded) return;
            _loaded = true;

            Logger.Reset();
            Logger.Log($"Silksong Modding API {ApiVersion} 启动");

            Directory.CreateDirectory(ModsDirectory);
            LoadDisabledList();

            ModRunner.Ensure();
            NativeEventBridge.Attach();
#if !NO_PLAYMAKERHOOKS && !NO_FSMUTIL
            PlayMakerSupport.PlayMakerBridge.Attach();
#endif

            SceneManager.activeSceneChanged += (from, to) => ModHooks.OnActiveSceneChanged(from.name, to.name);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromModsDir;

            var assemblies = new List<Assembly>();
            foreach (string dll in Directory.GetFiles(ModsDirectory, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Logger.Log($"发现 Mod DLL: {dll}");
                    assemblies.Add(Assembly.LoadFrom(dll));
                }
                catch (Exception e) { Logger.LogError($"无法加载 {dll}: {e}"); }
            }

            foreach (Assembly asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                catch (Exception e) { Logger.LogError($"读取 {asm.GetName().Name} 的类型失败: {e.Message}"); continue; }

                foreach (Type type in types)
                {
                    if (type.IsAbstract || !typeof(IMod).IsAssignableFrom(type)) continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null) continue;

                    try { Mods.Add((IMod) Activator.CreateInstance(type)); }
                    catch (Exception e) { Logger.LogError($"实例化 {type.FullName} 失败: {e}"); }
                }
            }

            foreach (IMod mod in Mods.OrderBy(m => m.LoadPriority()))
            {
                if (Disabled.Contains(mod.GetName()))
                {
                    Logger.Log($"跳过已禁用的 {mod.GetName()}");
                    continue;
                }

                try
                {
                    Logger.Log($"初始化 {mod.GetName()} v{mod.GetVersion()}");
                    ModSettingsManager.LoadGlobal(mod);
                    mod.Initialize();
                }
                catch (Exception e)
                {
                    Logger.LogError($"{mod.GetName()} 初始化失败: {e}");
                }
            }


            ModHooks.ApplicationQuitHook += () => ModSettingsManager.SaveAllGlobal(Mods);
            ModHooks.NewGameHook += () => ModSettingsManager.ResetLocal(Mods);
            ModHooks.SavegameLoadHook += slot => ModSettingsManager.LoadLocal(Mods, slot);
            ModHooks.SavegameSaveHook += slot => ModSettingsManager.SaveLocal(Mods, slot);
            ModHooks.AfterSavegameClearHook += slot => ModSettingsManager.ClearLocal(Mods, slot);

#if !NO_PRELOADER
            List<IPreloadMod> preloadMods = Mods.OfType<IPreloadMod>().Where(m => !Disabled.Contains(m.GetName())).ToList();
            if (preloadMods.Count > 0)
                ModRunner.Start(Preloader.Run(preloadMods));
#endif

            Logger.Log($"共加载 {Mods.Count} 个 Mod");
        }

        public static bool IsModLoaded(string name) => Mods.Any(m => m.GetName() == name);

        public static IMod GetMod(string name) => Mods.FirstOrDefault(m => m.GetName() == name);

        public static T GetMod<T>() where T : class, IMod => Mods.OfType<T>().FirstOrDefault();

        public static bool IsEnabled(IMod mod) => !Disabled.Contains(mod.GetName());




        public static void SetEnabled(IMod mod, bool enabled)
        {
            if (IsEnabled(mod) == enabled) return;

            if (enabled)
            {
                Disabled.Remove(mod.GetName());
                try { mod.Initialize(); }
                catch (Exception e) { Logger.LogError($"{mod.GetName()} 启用失败: {e}"); }
            }
            else
            {
                Disabled.Add(mod.GetName());
                if (mod is ITogglableMod togglable)
                {
                    try { togglable.Unload(); }
                    catch (Exception e) { Logger.LogError($"{mod.GetName()} 卸载失败: {e}"); }
                }
                else
                {
                    Logger.LogWarn($"{mod.GetName()} 不支持运行时关闭，下次启动生效");
                }
            }

            SaveDisabledList();
        }

        private static void LoadDisabledList()
        {
            try
            {
                if (File.Exists(DisabledFile))
                    foreach (string line in File.ReadAllLines(DisabledFile))
                        if (!string.IsNullOrWhiteSpace(line)) Disabled.Add(line.Trim());
            }
            catch (Exception e) { Logger.LogWarn($"读取 ModsDisabled.txt 失败: {e.Message}"); }
        }

        private static void SaveDisabledList()
        {
            try { File.WriteAllLines(DisabledFile, Disabled); }
            catch (Exception e) { Logger.LogWarn($"写入 ModsDisabled.txt 失败: {e.Message}"); }
        }

        private static Assembly ResolveFromModsDir(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name + ".dll";
            try
            {
                string found = Directory.GetFiles(ModsDirectory, name, SearchOption.AllDirectories).FirstOrDefault();
                if (found != null) return Assembly.LoadFrom(found);


                string managedDir = Path.GetDirectoryName(typeof(ModLoader).Assembly.Location);
                found = Directory.GetFiles(managedDir, name).FirstOrDefault();
                if (found != null) return Assembly.LoadFrom(found);
            }
            catch { }
            return null;
        }
    }
}
