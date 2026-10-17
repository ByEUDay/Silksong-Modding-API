using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::ToolItemManager")]
    public class patch_ToolItemManager : global::ToolItemManager
    {
        public static extern void orig_SetEquippedCrest(string crestId);

        public static void SetEquippedCrest(string crestId)
        {
            orig_SetEquippedCrest(crestId);
            ModHooks.OnCrestEquipped(crestId);
        }
    }
}
