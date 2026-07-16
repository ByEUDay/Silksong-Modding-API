using System.Collections.Generic;

namespace SilksongModLoader
{
    internal enum ModLoadState { Pending, Loaded, Failed }

    internal class ModEntry
    {
        public IMod Mod = null!;
        public ModLoadState State = ModLoadState.Pending;
    }

    /// <summary>
    /// 记录当前 mod 加载进度和结果,UI(进度条/左上角列表)从这里读数据。
    /// </summary>
    internal static class ModStatus
    {
        public static readonly List<ModEntry> Mods = new();
        public static int LoadedCount;
        public static bool IsLoading => LoadedCount < Mods.Count;
    }
}
