#if !NO_PLAYMAKERHOOKS && !NO_FSMUTIL
using System;
using HutongGames.PlayMaker;

namespace Modding.PlayMakerSupport
{





    internal static class PlayMakerBridge
    {
        private static bool _attached;

        internal static void Attach()
        {
            if (_attached) return;
            _attached = true;

            try
            {
                PlayMakerHooks.FsmEventHook += OnFsmEvent;
                PlayMakerHooks.FsmFloatSetHook += OnFloat;
                PlayMakerHooks.FsmIntSetHook += OnInt;
                PlayMakerHooks.FsmBoolSetHook += OnBool;
                PlayMakerHooks.FsmStringSetHook += OnString;
                Logger.Log("PlayMaker", "PlayMaker 钩子已接入 FsmEventHooks / FsmVariableHooks");
            }
            catch (Exception e)
            {
                Logger.LogError("PlayMaker", "接入 PlayMaker 钩子失败: " + e);
            }
        }


        internal static void EnableVariableHooks() => PlayMakerHooks.VariableHooksEnabled = true;

        private static FsmEvent OnFsmEvent(Fsm fsm, FsmEvent ev)
        {
            if (fsm == null || ev == null) return ev;
            string fsmName = fsm.Name;
            if (fsmName == null) return ev;

            return FsmEventHooks.TriggerEvent(fsmName, ev.Name) ? ev : null;
        }

        private static float OnFloat(Fsm fsm, string name, float value)
        {
            object r = Trigger(fsm, name, value);
            return r is float f ? f : value;
        }

        private static int OnInt(Fsm fsm, string name, int value)
        {
            object r = Trigger(fsm, name, value);
            return r is int i ? i : value;
        }

        private static bool OnBool(Fsm fsm, string name, bool value)
        {
            object r = Trigger(fsm, name, value);
            return r is bool b ? b : value;
        }

        private static string OnString(Fsm fsm, string name, string value)
        {
            object r = Trigger(fsm, name, value);
            return r is string s ? s : value;
        }

        private static object Trigger(Fsm fsm, string varName, object value)
        {
            string fsmName = fsm != null ? fsm.Name : null;
            if (fsmName == null || varName == null) return value;
            return FsmVariableHooks.TriggerVariableChange(fsmName, varName, value);
        }
    }
}
#endif
