using System;

namespace SilksongModLoader
{
    public static class ModHooks
    {
        public static event Action<int>? OnSaveGameLoaded;
        public static event Action<int>? OnSaveGameSaved;
        public static event Action<string>? OnSceneChanged;
        public static event Action? OnHeroUpdate;
        internal static void RaiseSaveGameLoaded(int slot) => OnSaveGameLoaded?.Invoke(slot);
        internal static void RaiseSaveGameSaved(int slot) => OnSaveGameSaved?.Invoke(slot);
        internal static void RaiseSceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);
        internal static void RaiseHeroUpdate() => OnHeroUpdate?.Invoke();
    }
}
