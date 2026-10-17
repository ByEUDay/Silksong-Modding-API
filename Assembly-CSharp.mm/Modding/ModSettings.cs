using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Modding
{




    public interface IGlobalSettings<T> where T : class, new()
    {
        void OnLoadGlobal(T settings);
        T OnSaveGlobal();
    }





    public interface ILocalSettings<T> where T : class, new()
    {
        void OnLoadLocal(T settings);
        T OnSaveLocal();
    }

    internal static class ModSettingsManager
    {
        private static string Dir => Application.persistentDataPath;

        private static string SafeName(IMod m)
        {
            string n = m.GetName();
            foreach (char c in Path.GetInvalidFileNameChars()) n = n.Replace(c, '_');
            return n;
        }

        private static string GlobalPath(IMod m) => Path.Combine(Dir, SafeName(m) + ".GlobalSettings.json");
        private static string LocalPath(IMod m, int slot) => Path.Combine(Dir, $"user{slot}.{SafeName(m)}.json");

        private static Type FindInterface(IMod mod, Type openGeneric) =>
            mod.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric);

        private static object ReadOrNew(string path, Type t)
        {
            try
            {
                if (File.Exists(path))
                    return JsonUtility.FromJson(File.ReadAllText(path), t);
            }
            catch (Exception e)
            {
                Logger.LogError($"读取设置失败 {path}: {e.Message}");
            }
            return Activator.CreateInstance(t);
        }

        private static void Write(string path, object data)
        {
            if (data == null) return;
            try { File.WriteAllText(path, JsonUtility.ToJson(data, true)); }
            catch (Exception e) { Logger.LogError($"写入设置失败 {path}: {e.Message}"); }
        }

        internal static void LoadGlobal(IMod mod)
        {
            Type iface = FindInterface(mod, typeof(IGlobalSettings<>));
            if (iface == null) return;
            try
            {
                Type t = iface.GetGenericArguments()[0];
                iface.GetMethod("OnLoadGlobal").Invoke(mod, new[] { ReadOrNew(GlobalPath(mod), t) });
            }
            catch (Exception e) { Logger.LogError($"{mod.GetName()} 读取全局设置失败: {e}"); }
        }

        internal static void SaveAllGlobal(IEnumerable<IMod> mods)
        {
            foreach (IMod mod in mods)
            {
                Type iface = FindInterface(mod, typeof(IGlobalSettings<>));
                if (iface == null) continue;
                try { Write(GlobalPath(mod), iface.GetMethod("OnSaveGlobal").Invoke(mod, null)); }
                catch (Exception e) { Logger.LogError($"{mod.GetName()} 保存全局设置失败: {e}"); }
            }
        }

        internal static void LoadLocal(IEnumerable<IMod> mods, int slot)
        {
            foreach (IMod mod in mods)
            {
                Type iface = FindInterface(mod, typeof(ILocalSettings<>));
                if (iface == null) continue;
                try
                {
                    Type t = iface.GetGenericArguments()[0];
                    iface.GetMethod("OnLoadLocal").Invoke(mod, new[] { ReadOrNew(LocalPath(mod, slot), t) });
                }
                catch (Exception e) { Logger.LogError($"{mod.GetName()} 读取存档设置失败: {e}"); }
            }
        }

        internal static void ResetLocal(IEnumerable<IMod> mods)
        {
            foreach (IMod mod in mods)
            {
                Type iface = FindInterface(mod, typeof(ILocalSettings<>));
                if (iface == null) continue;
                try
                {
                    Type t = iface.GetGenericArguments()[0];
                    iface.GetMethod("OnLoadLocal").Invoke(mod, new[] { Activator.CreateInstance(t) });
                }
                catch (Exception e) { Logger.LogError($"{mod.GetName()} 重置存档设置失败: {e}"); }
            }
        }

        internal static void SaveLocal(IEnumerable<IMod> mods, int slot)
        {
            foreach (IMod mod in mods)
            {
                Type iface = FindInterface(mod, typeof(ILocalSettings<>));
                if (iface == null) continue;
                try { Write(LocalPath(mod, slot), iface.GetMethod("OnSaveLocal").Invoke(mod, null)); }
                catch (Exception e) { Logger.LogError($"{mod.GetName()} 保存存档设置失败: {e}"); }
            }
        }

        internal static void ClearLocal(IEnumerable<IMod> mods, int slot)
        {
            foreach (IMod mod in mods)
            {
                try { File.Delete(LocalPath(mod, slot)); } catch {   }
            }
        }
    }
}
