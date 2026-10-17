using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::Inventory")]
    public class patch_Inventory
    {
        public static extern void orig_AddItem(string itemId, int count);

        public static void AddItem(string itemId, int count)
        {
            int oldCount = GetItemCount(itemId);
            orig_AddItem(itemId, count);
            int newCount = GetItemCount(itemId);
            ModHooks.OnItemCountChanged(itemId, oldCount, newCount);
            ModHooks.OnItemObtained(itemId);
        }

        public static extern void orig_RemoveItem(string itemId, int count);

        public static void RemoveItem(string itemId, int count)
        {
            int oldCount = GetItemCount(itemId);
            orig_RemoveItem(itemId, count);
            int newCount = GetItemCount(itemId);
            ModHooks.OnItemCountChanged(itemId, oldCount, newCount);
            if (newCount <= 0)
            {
                ModHooks.OnItemLost(itemId);
            }
        }

        public static extern int orig_GetItemCount(string itemId);

        public static int GetItemCount(string itemId)
        {
            return orig_GetItemCount(itemId);
        }

        public static extern void orig_UseItem(string itemId);

        public static void UseItem(string itemId)
        {
            ModHooks.OnItemUsed(itemId);
            orig_UseItem(itemId);
        }

        public static extern void orig_EquipItem(string itemId);

        public static void EquipItem(string itemId)
        {
            ModHooks.OnItemEquipped(itemId);
            orig_EquipItem(itemId);
        }

        public static extern void orig_UnequipItem(string itemId);

        public static void UnequipItem(string itemId)
        {
            ModHooks.OnItemUnequipped(itemId);
            orig_UnequipItem(itemId);
        }

        public static extern void orig_UnlockItem(string itemId);

        public static void UnlockItem(string itemId)
        {
            ModHooks.OnItemUnlocked(itemId);
            orig_UnlockItem(itemId);
        }

        public static extern void orig_OpenInventory();

        public static void OpenInventory()
        {
            ModHooks.OnInventoryOpened();
            orig_OpenInventory();
        }

        public static extern void orig_CloseInventory();

        public static void CloseInventory()
        {
            ModHooks.OnInventoryClosed();
            orig_CloseInventory();
        }

        public static extern void orig_SelectItem(string itemId);

        public static void SelectItem(string itemId)
        {
            ModHooks.OnInventorySelectionChanged(itemId);
            orig_SelectItem(itemId);
        }
    }
}
