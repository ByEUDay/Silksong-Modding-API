using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SilksongModLoader
{
    /// <summary>
    /// 唯一的公开入口。Prepatcher 会往游戏 dll 里插入对
    /// Entry.Initialize() 的调用,这个方法只会真正执行一次。
    /// </summary>
    public static class Entry
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return; // 防止被插桩的方法多次触发(比如场景重进)
            _initialized = true;

            // 不用 AppContext.BaseDirectory(在游戏进程里不一定指向 Managed 目录),
            // 改用"这个 dll 自己所在的物理路径"——既然是你手动放进 Managed 目录的,
            // 这个路径一定可靠。
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

        /// <summary>
        /// 扫描 Mods 目录,实例化所有 IMod,并按依赖关系排好序。
        /// 只做"发现"和"排序",不调用 IMod.Initialize()——
        /// 真正的 Initialize() 由 ModTickerBehaviour 的协程逐帧调用,
        /// 这样才能在加载过程中把进度条实时画出来。
        /// </summary>
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
