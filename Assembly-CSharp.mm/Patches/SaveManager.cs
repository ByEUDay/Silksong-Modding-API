using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::SaveManager")]
    public class patch_SaveManager
    {
        public static extern void orig_SaveGame(int slot);

        public static void SaveGame(int slot)
        {
            ModHooks.OnBeforeSavegameSave(slot);
            orig_SaveGame(slot);
            ModHooks.OnAfterSavegameSave(slot);
        }

        public static extern void orig_LoadGame(int slot);

        public static void LoadGame(int slot)
        {
            ModHooks.OnBeforeSavegameLoad(slot);
            orig_LoadGame(slot);
            ModHooks.OnAfterSavegameLoad(slot);
        }

        public static extern void orig_ClearSave(int slot);

        public static void ClearSave(int slot)
        {
            ModHooks.OnBeforeSavegameClear(slot);
            orig_ClearSave(slot);
        }

        public static extern void orig_NewGame();

        public static void NewGame()
        {
            ModHooks.OnBeforeNewGame();
            orig_NewGame();
            ModHooks.OnAfterNewGame();
        }

        public static extern void orig_AutoSave();

        public static void AutoSave()
        {
            int slot = GetCurrentSlot();
            ModHooks.OnAutoSaveTriggered(slot);
            orig_AutoSave();
        }

        public static extern void orig_ManualSave();

        public static void ManualSave()
        {
            int slot = GetCurrentSlot();
            ModHooks.OnManualSaveTriggered(slot);
            orig_ManualSave();
        }

        public static extern void orig_QuickSave();

        public static void QuickSave()
        {
            int slot = GetCurrentSlot();
            ModHooks.OnQuickSaveTriggered(slot);
            orig_QuickSave();
        }

        public static extern void orig_QuickLoad();

        public static void QuickLoad()
        {
            int slot = GetCurrentSlot();
            ModHooks.OnQuickLoadTriggered(slot);
            orig_QuickLoad();
        }

        private static int GetCurrentSlot()
        {
            try
            {
                if (GameManager.instance == null) return 0;
                var field = typeof(GameManager).GetField("saveSlot",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (field != null)
                    return (int)field.GetValue(GameManager.instance);
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
