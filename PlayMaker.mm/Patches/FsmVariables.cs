using HutongGames.PlayMaker;
using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{




    [MonoModPatch("global::HutongGames.PlayMaker.FsmFloat")]
    public class patch_FsmFloat : global::HutongGames.PlayMaker.FsmFloat
    {
        public extern void orig_set_Value(float value);

        public new float Value
        {
            [MonoModIgnore] get { return 0f; }
            set
            {
                if (PlayMakerHooks.VariableHooksEnabled) value = PlayMakerHooks.OnFloatSet(Name, value);
                orig_set_Value(value);
            }
        }
    }

    [MonoModPatch("global::HutongGames.PlayMaker.FsmInt")]
    public class patch_FsmInt : global::HutongGames.PlayMaker.FsmInt
    {
        public extern void orig_set_Value(int value);

        public new int Value
        {
            [MonoModIgnore] get { return 0; }
            set
            {
                if (PlayMakerHooks.VariableHooksEnabled) value = PlayMakerHooks.OnIntSet(Name, value);
                orig_set_Value(value);
            }
        }
    }

    [MonoModPatch("global::HutongGames.PlayMaker.FsmBool")]
    public class patch_FsmBool : global::HutongGames.PlayMaker.FsmBool
    {
        public extern void orig_set_Value(bool value);

        public new bool Value
        {
            [MonoModIgnore] get { return false; }
            set
            {
                if (PlayMakerHooks.VariableHooksEnabled) value = PlayMakerHooks.OnBoolSet(Name, value);
                orig_set_Value(value);
            }
        }
    }

    [MonoModPatch("global::HutongGames.PlayMaker.FsmString")]
    public class patch_FsmString : global::HutongGames.PlayMaker.FsmString
    {
        public extern void orig_set_Value(string value);

        public new string Value
        {
            [MonoModIgnore] get { return null; }
            set
            {
                if (PlayMakerHooks.VariableHooksEnabled) value = PlayMakerHooks.OnStringSet(Name, value);
                orig_set_Value(value);
            }
        }
    }
}
