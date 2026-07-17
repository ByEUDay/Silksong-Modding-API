using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SilksongModLoader
{
    public static class Entry
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            var loaderDir = Path.GetDirectoryName(typeof(Entry).Assembly.Location) ?? AppContext.BaseDirectory;
            ModLog.Init(loaderDir);
            ModLog.Info($"SilksongModLoader 启动中... (loaderDir = {loaderDir})");

            try
            {
                InternalHooks.Apply(loaderDir);
            }
            catch (Exception e)
            {
                ModLog.Error($"内部 Hook 挂载失败,mod 功能可能不完整: {e}");
            }
        }
        internal static List<IMod> DiscoverMods(string modsDir)
        {
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
                ModLog.Info($"Mods 目录不存在,已创建: {modsDir}");
                return new List<IMod>();
            }

            var candidates = new List<IMod>();

            foreach (var dllPath in Directory.GetFiles(modsDir, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dllPath);
                    foreach (var type in asm.GetTypes())
                    {
                        if (typeof(IMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        {
                            if (Activator.CreateInstance(type) is IMod mod)
                                candidates.Add(mod);
                        }
                    }
                }
                catch (Exception e)
                {
                    ModLog.Error($"加载 {Path.GetFileName(dllPath)} 失败: {e}");
                }
            }

            return TopoSort(candidates);
        }

        private static List<IMod> TopoSort(List<IMod> mods)
        {
            var byName = mods.ToDictionary(m => m.Name, m => m);
            var result = new List<IMod>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(string name)
            {
                if (visited.Contains(name) || !byName.ContainsKey(name)) return;
                if (visiting.Contains(name))
                {
                    ModLog.Warn($"检测到循环依赖: {name},按发现顺序处理。");
                    return;
                }

                visiting.Add(name);
                foreach (var dep in byName[name].Dependencies)
                {
                    if (!byName.ContainsKey(dep))
                        ModLog.Warn($"{name} 依赖的 {dep} 未找到,可能导致该 mod 功能异常。");
                    else
                        Visit(dep);
                }
                visiting.Remove(name);
                visited.Add(name);
                result.Add(byName[name]);
            }

            foreach (var mod in mods) Visit(mod.Name);
            return result;
        }
    }
}
