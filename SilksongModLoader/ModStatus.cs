using System.Collections.Generic;

namespace SilksongModLoader
{
    internal enum ModLoadState { Pending, Loaded, Failed, Disabled }
    internal class ModEntry
    {
        public IMod Mod = null!;
        public ModLoadState State = ModLoadState.Pending;
        public bool PendingRestart;
    }
    internal static class ModStatus
    {
        public static readonly List<ModEntry> Mods = new();
        public static int LoadedCount;
        public static bool IsLoading => LoadedCount < Mods.Count;
    }
}
