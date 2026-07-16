using System;

namespace SilksongModLoader
{
    /// <summary>
    /// 对外暴露的事件总线。Mod 只需要订阅这里的 event,不需要知道
    /// 底层是用 MonoMod.RuntimeDetour 挂在哪个游戏方法上。
    ///
    /// 下面列的事件名/签名是"占位设计",具体要 hook 游戏里的哪个类型/方法,
    /// 需要你用 dnSpy 打开 Assembly-CSharp.dll 确认实际类名后,
    /// 在 InternalHooks.cs 里补上真实的 Hook(...) 调用。
    /// </summary>
    public static class ModHooks
    {
        /// <summary>每次读档完成后触发,可用于恢复 mod 自己的存档数据。</summary>
        public static event Action<int>? OnSaveGameLoaded;

        /// <summary>每次存档时触发,可用于把 mod 自己的数据写进存档。</summary>
        public static event Action<int>? OnSaveGameSaved;

        /// <summary>场景切换完成后触发,参数为场景名。</summary>
        public static event Action<string>? OnSceneChanged;

        /// <summary>每帧调用一次(对应玩家角色的 Update),高频事件,谨慎订阅重逻辑。</summary>
        public static event Action? OnHeroUpdate;

        // ---- 以下方法仅供 SilksongModLoader 内部的 InternalHooks 调用,mod 不应直接调用 ----
        internal static void RaiseSaveGameLoaded(int slot) => OnSaveGameLoaded?.Invoke(slot);
        internal static void RaiseSaveGameSaved(int slot) => OnSaveGameSaved?.Invoke(slot);
        internal static void RaiseSceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);
        internal static void RaiseHeroUpdate() => OnHeroUpdate?.Invoke();
    }
}
