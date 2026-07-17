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

            // 收集所有候选 dll 路径:
            // 1. Mods 目录根下的 dll(兼容老的"直接把 dll 扔进 Mods"用法)
            // 2. Mods 目录下每个第一层子文件夹里的 dll(推荐用法,一个 mod 一个文件夹,
            //    方便把依赖 dll、贴图等资源跟 mod 主 dll 放在一起)
            var dllPaths = new List<string>(Directory.GetFiles(modsDir, "*.dll"));
            var modFolders = new List<string>();

            foreach (var subDir in Directory.GetDirectories(modsDir))
            {
                var subDlls = Directory.GetFiles(subDir, "*.dll");
                if (subDlls.Length == 0)
                {
                    ModLog.Warn($"子文件夹 {Path.GetFileName(subDir)} 里没有找到任何 .dll,已跳过。");
                    continue;
                }
                modFolders.Add(subDir);
                dllPaths.AddRange(subDlls);
            }

            // 让同一个 mod 文件夹内的依赖 dll(不是 IMod 本体,只是被主 dll 引用的库)
            // 在运行时也能被找到 —— 否则 Assembly.LoadFrom 加载主 dll 后,CLR 默认只会去
            // Managed 目录找依赖,不会自动去 Mods\子文件夹 里找。
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) => ResolveFromModFolders(args, modFolders);

            var candidates = new List<IMod>();

            foreach (var dllPath in dllPaths)
            {
                try
                {
                    var asm = Assembly.LoadFrom(dllPath);
                    var folderLabel = Path.GetDirectoryName(dllPath) == modsDir
                        ? "(根目录)"
                        : Path.GetFileName(Path.GetDirectoryName(dllPath));

                    foreach (var type in asm.GetTypes())
                    {
                        if (typeof(IMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        {
                            if (Activator.CreateInstance(type) is IMod mod)
                            {
                                candidates.Add(mod);
                                ModLog.Info($"发现 mod: {mod.Name} v{mod.Version} (来自 {folderLabel}/{Path.GetFileName(dllPath)})");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ModLog.Error($"加载 {Path.GetFileName(dllPath)} 失败: {e}");
                }
            }

            if (candidates.Count == 0)
            {
                ModLog.Info($"Mods 目录(含子文件夹)里没有发现任何实现 IMod 的类型: {modsDir}");
            }

            return TopoSort(DeduplicateByName(candidates));
        }

        private static List<IMod> DeduplicateByName(List<IMod> mods)
        {
            var result = new List<IMod>();
            var seen = new HashSet<string>();
            foreach (var mod in mods)
            {
                if (!seen.Add(mod.Name))
                {
                    ModLog.Warn($"发现重名 mod \"{mod.Name}\",已忽略后出现的那个(保留先发现的)。");
                    continue;
                }
                result.Add(mod);
            }
            return result;
        }

        private static Assembly? ResolveFromModFolders(ResolveEventArgs args, List<string> modFolders)
        {
            var asmName = new AssemblyName(args.Name).Name;
            foreach (var folder in modFolders)
            {
                var candidate = Path.Combine(folder, asmName + ".dll");
                if (File.Exists(candidate))
                {
                    try { return Assembly.LoadFrom(candidate); }
                    catch (Exception e)
                    {
                        ModLog.Error($"从 {candidate} 解析依赖程序集 {asmName} 失败: {e}");
                    }
                }
            }
            return null;
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
