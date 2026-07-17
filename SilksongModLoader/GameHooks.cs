using System;
using System.Reflection;

namespace SilksongModLoader
{
    /// <summary>
    /// 负责把 ModHooks.OnSaveGameLoaded / OnSaveGameSaved 接到游戏真实的存档读写方法上。
    ///
    /// !!! 重要说明,请先读完再改代码 !!!
    ///
    /// 我(写这份代码的人)手上没有真实的丝之歌 Assembly-CSharp.dll,没法确认 GameManager 里
    /// 存档/读档方法的真实名字、参数个数和类型 —— 瞎猜签名写一个"看起来能编译"的 detour
    /// 反而会误导你,所以这里不去猜,而是提供一套"帮你自己一步步找到真实签名"的流程:
    ///
    /// 【推荐做法:用 MonoMod HookGen 生成 On.* 命名空间(Hollow Knight 一代 modding 生态就是这么干的)】
    ///   1. 用 MonoMod.RuntimeDetour.HookGen 工具,对着游戏的 Assembly-CSharp.dll 跑一遍,
    ///      会生成一个 MMHOOK_Assembly-CSharp.dll,里面按每个方法自动生成了强类型的
    ///      `On.类型名.方法名` 事件,比如(以一代举例):
    ///          On.GameManager.SaveGame += (orig, self, saveSlot) =>
    ///          {
    ///              orig(self, saveSlot);           // 调用原方法
    ///              ModHooks.RaiseSaveGameSaved(saveSlot); // 触发我们自己的事件
    ///          };
    ///      这种写法不需要你手写委托类型、不用担心参数个数对不对,HookGen 已经按真实签名生成好了。
    ///   2. 把生成的 MMHOOK_Assembly-CSharp.dll 也一起丢进 Managed 目录,
    ///      在 Loader 里(或者具体某个 mod 里)引用它,就能用上面的 On.* 写法了。
    ///
    /// 【如果暂时不想引入 HookGen,退而求其次的手动做法】
    ///   1. 编译好 Loader,启动一次游戏,查看用户主目录下的 SilksongModLoader.log。
    ///      下面 ApplySaveLoadHooks() 会调用 HookHelper.DumpMembers("GameManager"),
    ///      把 GameManager 的所有方法签名打到日志里。
    ///   2. 在日志里找存档/读档相关的方法(通常名字带 Save / Load / Continue),
    ///      记下真实的方法名、参数类型、参数个数、是否是 static。
    ///   3. 参照 HookHelper 顶部的用法示例,写一个跟真实签名完全匹配的
    ///      强类型 delegate,再调用 HookHelper.Attach(method, detour) 挂上去。
    ///   4. 挂载失败(比如参数个数不对)只会在日志里报错,不会导致游戏崩溃/无法启动,
    ///      可以放心反复试。
    /// </summary>
    internal static class GameHooks
    {
        private const string GameManagerTypeName = "GameManager";

        /// <summary>
        /// 目前只做一件事:把 GameManager 的成员列表打到日志里,方便你核对真实的存档方法签名。
        /// 找到真实签名后,按照本文件顶部的说明,在这里补上真正的 HookHelper.Attach(...) 调用。
        /// </summary>
        public static void ApplySaveLoadHooks()
        {
            var gameManagerType = HookHelper.FindType(GameManagerTypeName);
            if (gameManagerType == null)
            {
                ModLog.Error($"找不到类型 {GameManagerTypeName},存档钩子未挂载。请确认游戏里这个类是否叫别的名字," +
                             "可以尝试 HookHelper.DumpMembers 别的候选类型名。");
                return;
            }

            ModLog.Info("正在打印 GameManager 的成员列表,用于核对存档相关方法的真实签名(见 GameHooks.cs 顶部说明)...");
            HookHelper.DumpMembers(gameManagerType);

            ModLog.Warn("存档保存/读取钩子尚未接入真实方法(需要你核对上面打印的签名后手动补上)," +
                        "ModHooks.OnSaveGameSaved / OnSaveGameLoaded 暂时不会被触发。");
        }
    }
}
