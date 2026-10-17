#if !NO_PLAYMAKERHOOKS
using System;
using HutongGames.PlayMaker;

namespace Modding
{



    public static partial class ModHooks
    {

        public static event Action<PlayMakerFSM> FsmAwakeHook
        {
            add { PlayMakerHooks.FsmAwakeHook += value; }
            remove { PlayMakerHooks.FsmAwakeHook -= value; }
        }


        public static event Action<PlayMakerFSM> FsmEnableHook
        {
            add { PlayMakerHooks.FsmEnableHook += value; }
            remove { PlayMakerHooks.FsmEnableHook -= value; }
        }


        public static event Action<PlayMakerFSM> FsmStartHook
        {
            add { PlayMakerHooks.FsmStartHook += value; }
            remove { PlayMakerHooks.FsmStartHook -= value; }
        }


        public static event Action<PlayMakerFSM> FsmDestroyHook
        {
            add { PlayMakerHooks.FsmDestroyHook += value; }
            remove { PlayMakerHooks.FsmDestroyHook -= value; }
        }


        public static event Func<Fsm, FsmEvent, FsmEvent> FsmEventHook
        {
            add { PlayMakerHooks.FsmEventHook += value; }
            remove { PlayMakerHooks.FsmEventHook -= value; }
        }


        public static event Action<Fsm, FsmState> FsmStateEnterHook
        {
            add { PlayMakerHooks.FsmStateEnterHook += value; }
            remove { PlayMakerHooks.FsmStateEnterHook -= value; }
        }


        public static event Action<Fsm, FsmState> FsmStateExitHook
        {
            add { PlayMakerHooks.FsmStateExitHook += value; }
            remove { PlayMakerHooks.FsmStateExitHook -= value; }
        }


        public static event Action<Fsm, FsmTransition, bool> FsmTransitionHook
        {
            add { PlayMakerHooks.FsmTransitionHook += value; }
            remove { PlayMakerHooks.FsmTransitionHook -= value; }
        }
    }
}
#endif
