using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{

    [MonoModPatch("global::CurrencyManager")]
    public class patch_CurrencyManager
    {
        public static extern void orig_ChangeCurrency(int amount, CurrencyType type, bool showCounter);

        public static void ChangeCurrency(int amount, CurrencyType type, bool showCounter = true)
        {
            amount = ModHooks.OnCurrencyChange(type, amount);
            if (amount == 0) return;
            orig_ChangeCurrency(amount, type, showCounter);
        }
    }
}
