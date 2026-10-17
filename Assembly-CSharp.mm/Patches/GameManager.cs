using System;
using System.Collections;
using GlobalEnums;
using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::GameManager")]
    public class patch_GameManager : global::GameManager
    {

        public extern void orig_Awake();

        private void Awake()
        {
            orig_Awake();
            if (this == _instance)
                ModLoader.LoadMods();
        }

        public extern void orig_OnApplicationQuit();

        private void OnApplicationQuit()
        {
            orig_OnApplicationQuit();
            ModHooks.OnApplicationQuit();
        }

        public extern void orig_LoadScene(string destScene);

        public void LoadScene(string destScene)
        {
            destScene = ModHooks.OnBeforeSceneLoad(destScene);
            orig_LoadScene(destScene);
        }

        public extern void orig_BeginSceneTransition(SceneLoadInfo info);

        public void BeginSceneTransition(SceneLoadInfo info)
        {
            info.SceneName = ModHooks.OnBeforeSceneLoad(info.SceneName);
            orig_BeginSceneTransition(info);
        }

        public extern void orig_StartNewGame(bool permadeathMode, bool bossRushMode);

        public void StartNewGame(bool permadeathMode = false, bool bossRushMode = false)
        {
            orig_StartNewGame(permadeathMode, bossRushMode);
            ModHooks.OnNewGame();
        }

        public extern void orig_SetLoadedGameData(SaveGameData saveGameData, int saveSlot);

        private void SetLoadedGameData(SaveGameData saveGameData, int saveSlot)
        {
            orig_SetLoadedGameData(saveGameData, saveSlot);
            ModHooks.OnSavegameLoad(saveSlot);
        }

        public extern void orig_SaveGame(int saveSlot, Action<bool> ogCallback, bool withAutoSave, AutoSaveName autoSaveName);

        private void SaveGame(int saveSlot, Action<bool> ogCallback, bool withAutoSave = false, AutoSaveName autoSaveName = AutoSaveName.NONE)
        {
            if (saveSlot >= 0)
                ModHooks.OnSavegameSave(saveSlot);
            orig_SaveGame(saveSlot, ogCallback, withAutoSave, autoSaveName);
        }

        public extern void orig_ClearSaveFile(int saveSlot, Action<bool> callback);

        public void ClearSaveFile(int saveSlot, Action<bool> callback)
        {
            ModHooks.OnSavegameClear(saveSlot);
            orig_ClearSaveFile(saveSlot, delegate (bool ok)
            {
                callback?.Invoke(ok);
                ModHooks.OnAfterSavegameClear(saveSlot);
            });
        }

        public extern IEnumerator orig_PlayerDead(float waitTime);

        public IEnumerator PlayerDead(float waitTime)
        {
            ModHooks.OnBeforePlayerDead();
            yield return orig_PlayerDead(waitTime);
            ModHooks.OnAfterPlayerDead();
        }

        public extern void orig_SetState(GameState newState);

        public void SetState(GameState newState)
        {
            orig_SetState(newState);
            ModHooks.OnGameStateChanged(newState);
        }

        public extern void orig_FinishedEnteringScene();

        public void FinishedEnteringScene()
        {
            orig_FinishedEnteringScene();
            ModHooks.OnSceneEntered(GetSceneNameString());
        }

        public extern IEnumerator orig_ReturnToMainMenu(bool willSave, Action<bool> callback, bool isEndGame, bool forceMainMenu);

        public IEnumerator ReturnToMainMenu(bool willSave, Action<bool> callback = null, bool isEndGame = false, bool forceMainMenu = false)
        {
            ModHooks.OnReturnToMainMenu(willSave);
            return orig_ReturnToMainMenu(willSave, callback, isEndGame, forceMainMenu);
        }
    }
}
