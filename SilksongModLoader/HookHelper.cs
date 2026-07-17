using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace SilksongModLoader
{
    /// <summary>
    /// 面向 mod 作者的运行时 hook 封装。目的是让 mod 不用自己直接摸 MonoMod.RuntimeDetour 的
    /// API,而是用更简单的方式挂载/卸载 hook,并且挂载失败时只记录日志,不会把游戏崩掉。
    ///
    /// 用法示例(在你的 mod 的 Initialize() 里):
    ///
    ///   // 1. 拿到目标方法(反射)
    ///   var method = typeof(HeroController).GetMethod("Attack",
    ///       BindingFlags.Public | BindingFlags.Instance);
    ///
    ///   // 2. 定义 detour。第一个参数是"原方法"的委托(orig),后面的参数要跟原方法签名一致
    ///   //    (实例方法的话,第一个业务参数其实是 this,类型写 HeroController)。
    ///   //    MonoMod 会在运行时按参数类型自动识别并把 orig 传进来。
    ///   On_Attack_orig origDelegate = null!; // 仅用于说明委托形状,可省略
    ///   Action<Action<HeroController, AttackDirection>, HeroController, AttackDirection> detour =
    ///       (orig, self, dir) =>
    ///       {
    ///           // 在原方法执行前插入逻辑
    ///           orig(self, dir); // 调用原方法
    ///           // 在原方法执行后插入逻辑
    ///       };
    ///
    ///   // 3. 挂载,拿到 Hook 对象存起来,卸载/mod 重载时调用 hook.Undo()
    ///   var hook = HookHelper.Attach(method, detour);
    ///
    /// 如果不确定目标方法的确切签名,先用 <see cref="DumpMembers"/> 把类型的所有成员打到日志里,
    /// 或者用 dnSpy 反编译 Assembly-CSharp.dll 核对。
    /// </summary>
    public static class HookHelper
    {
        private static readonly List<Hook> ActiveHooks = new();

        /// <summary>
        /// 直接用 MethodBase + Delegate 挂 hook。失败(比如签名对不上)只记录日志,不抛异常炸游戏。
        /// </summary>
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

        /// <summary>
        /// 用类型 + 方法名挂 hook,省去自己反射 GetMethod 的步骤。
        /// 同名重载多于 1 个时会挂载失败并报日志,请改用 <see cref="Attach(MethodBase?, Delegate)"/>
        /// 自己反射拿到唯一的 MethodInfo。
        /// </summary>
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

        /// <summary>
        /// 按类型名(不用 using 引入命名空间)查找类型,再挂 hook。
        /// 方便在不确定类型具体命名空间时使用(游戏内部很多类是 global 命名空间)。
        /// </summary>
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

        /// <summary>
        /// 卸载单个 hook 并从跟踪列表移除。
        /// </summary>
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

        /// <summary>
        /// 卸载所有当前挂载的 hook(比如游戏退出前清理,避免残留 detour)。
        /// </summary>
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

        /// <summary>
        /// 在所有已加载的程序集里按类型名查找类型(不需要知道具体命名空间/所在程序集)。
        /// </summary>
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
                    continue; // 有些程序集反射加载会部分失败,跳过即可
                }

                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// 调试用:把一个类型的所有方法签名打到日志里,方便在不方便用 dnSpy 时
        /// 快速核对方法名/参数/返回值,以便正确编写 hook。
        /// </summary>
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
