using HutongGames.PlayMaker;
using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HutongGames.PlayMaker.Fsm")]
    public class patch_Fsm : global::HutongGames.PlayMaker.Fsm
    {
        public extern void orig_ProcessEvent(FsmEvent fsmEvent, FsmEventData eventData);

        public void ProcessEvent(FsmEvent fsmEvent, FsmEventData eventData = null)
        {

            if (PlayMakerHooks.HasEventHook && Active)
                fsmEvent = PlayMakerHooks.OnFsmEvent(this, fsmEvent);
            orig_ProcessEvent(fsmEvent, eventData);
        }

        public extern void orig_EnterState(FsmState state);

        private void EnterState(FsmState state)
        {
            PlayMakerHooks.OnStateEnter(this, state);
            orig_EnterState(state);
        }

        public extern void orig_ExitState(FsmState state);

        private void ExitState(FsmState state)
        {
            PlayMakerHooks.OnStateExit(this, state);
            orig_ExitState(state);
        }

        public extern bool orig_DoTransition(FsmTransition transition, bool isGlobal);

        private bool DoTransition(FsmTransition transition, bool isGlobal)
        {
            PlayMakerHooks.OnTransition(this, transition, isGlobal);
            return orig_DoTransition(transition, isGlobal);
        }
    }
}
