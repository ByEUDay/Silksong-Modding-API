using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{

    [MonoModPatch("global::UIManager")]
    public class patch_UIManager : global::UIManager
    {
        public extern void orig_Start();

        private void Start()
        {
            orig_Start();
            ModMenu.OnUIManagerStarted(this);
        }
    }
}
