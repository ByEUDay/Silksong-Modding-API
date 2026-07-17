using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace SilksongModLoader
{
    public static class HookHelper
    {
        private static readonly List<Hook> ActiveHooks = new();
        public static Hook? Attach(MethodBase? original, Delegate detour)
        {
            if (original == null)
            {
                ModLog.Error("HookHelper.Attach: 目标方法为 null,无法挂载 hook(可能是上一步反射没找到方法)。");
                return null;
            }
            try
            {
                var hook = new Hook(original, detour);
                ActiveHooks.Add(hook);
                ModLog.Info($"Hook 已挂载: {original.DeclaringType?.FullName}.{original.Name}");
                return hook;
            }
            catch (Exception e)
            {
                ModLog.Error($"挂载 hook 失败 ({original.DeclaringType?.FullName}.{original.Name}): {e}");
                return null;
            }
        }
        public static Hook? Attach(
            Type? type,
            string methodName,
            Delegate detour,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type == null)
            {
                ModLog.Error($"HookHelper.Attach: 类型为 null(目标方法名 {methodName}),无法挂载。");
                return null;
            }
            MethodInfo? method;
            try
            {
                method = type.GetMethod(methodName, flags);
            }
            catch (AmbiguousMatchException)
            {
                ModLog.Error($"HookHelper.Attach: {type.FullName}.{methodName} 有多个重载,无法自动选择,请自己用 GetMethod 指定参数类型后调用 Attach(MethodBase, Delegate)。");
                return null;
            }
            if (method == null)
            {
                ModLog.Error($"HookHelper.Attach: 在 {type.FullName} 里没找到方法 {methodName},可能是游戏更新后名字/签名变了,请用 dnSpy 核对。");
                return null;
            }
            return Attach(method, detour);
        }
        public static Hook? Attach(
            string typeName,
            string methodName,
            Delegate detour,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            var type = FindType(typeName);
            if (type == null)
            {
                ModLog.Error($"HookHelper.Attach: 找不到类型 {typeName},存档钩子/其他 hook 未挂载。");
                return null;
            }
            return Attach(type, methodName, detour, flags);
        }
        public static void Remove(Hook? hook)
        {
            if (hook == null) return;
            try
            {
                hook.Undo();
                hook.Dispose();
            }
            catch (Exception e)
            {
                ModLog.Error($"卸载 hook 失败: {e}");
            }
            finally
            {
                ActiveHooks.Remove(hook);
            }
        }
        public static void RemoveAll()
        {
            foreach (var hook in ActiveHooks)
            {
                try
                {
                    hook.Undo();
                    hook.Dispose();
                }
                catch (Exception e)
                {
                    ModLog.Error($"卸载 hook 失败: {e}");
                }
            }
            ActiveHooks.Clear();
        }
        public static Type? FindType(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type;
                try
                {
                    type = asm.GetType(typeName);
                    if (type == null)
                    {
                        foreach (var t in asm.GetTypes())
                        {
                            if (t.Name == typeName)
                            {
                                type = t;
                                break;
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                if (type != null) return type;
            }
            return null;
        }
        public static void DumpMembers(Type type)
        {
            ModLog.Info($"=== 成员列表: {type.FullName} ===");
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                        BindingFlags.Instance | BindingFlags.Static |
                                        BindingFlags.DeclaredOnly;
            foreach (var m in type.GetMethods(flags))
            {
                var ps = string.Join(", ", Array.ConvertAll(m.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                ModLog.Info($"  {m.ReturnType.Name} {m.Name}({ps})");
            }
        }

        public static void DumpMembers(string typeName)
        {
            var type = FindType(typeName);
            if (type == null)
            {
                ModLog.Error($"DumpMembers: 找不到类型 {typeName}。");
                return;
            }
            DumpMembers(type);
        }
    }
}
