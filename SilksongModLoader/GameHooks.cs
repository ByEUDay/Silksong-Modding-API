using System;
using System.Reflection;

namespace SilksongModLoader
{
    internal static class GameHooks
    {
        private const string GameManagerTypeName = "GameManager";
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
