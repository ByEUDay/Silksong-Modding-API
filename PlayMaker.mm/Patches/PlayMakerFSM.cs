using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::PlayMakerFSM")]
    public class patch_PlayMakerFSM : global::PlayMakerFSM
    {
        public extern void orig_Awake();


        private void Awake()
        {
            orig_Awake();
            PlayMakerHooks.OnFsmAwake(this);
        }

        public extern void orig_OnEnable();

        private void OnEnable()
        {
            orig_OnEnable();
            PlayMakerHooks.OnFsmEnable(this);
        }

        public extern void orig_Start();

        private void Start()
        {

            if (!Fsm.Started)
                PlayMakerHooks.OnFsmStart(this);
            orig_Start();
        }

        public extern void orig_OnDestroy();

        private void OnDestroy()
        {
            PlayMakerHooks.OnFsmDestroy(this);
            orig_OnDestroy();
        }
    }
}
