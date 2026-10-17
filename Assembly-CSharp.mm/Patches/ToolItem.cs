using System;
using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{

    [MonoModPatch("global::ToolItem")]
    public abstract class patch_ToolItem : global::ToolItem
    {
        public extern void orig_Unlock(Action afterTutorialMsg, PopupFlags popupFlags);

        public void Unlock(Action afterTutorialMsg = null, PopupFlags popupFlags = PopupFlags.Default)
        {
            orig_Unlock(afterTutorialMsg, popupFlags);
            ModHooks.OnToolUnlocked(this);
        }
    }
}
