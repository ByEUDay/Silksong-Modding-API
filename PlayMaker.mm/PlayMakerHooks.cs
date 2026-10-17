using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding
{














    public static class PlayMakerHooks
    {
        #region FSM 生命周期 / 事件 / 状态





        public static event Action<PlayMakerFSM> FsmAwakeHook;


        public static event Action<PlayMakerFSM> FsmEnableHook;


        public static event Action<PlayMakerFSM> FsmStartHook;


        public static event Action<PlayMakerFSM> FsmDestroyHook;





        public static event Func<Fsm, FsmEvent, FsmEvent> FsmEventHook;


        public static event Action<Fsm, FsmState> FsmStateEnterHook;


        public static event Action<Fsm, FsmState> FsmStateExitHook;


        public static event Action<Fsm, FsmTransition, bool> FsmTransitionHook;

        internal static bool HasEventHook => FsmEventHook != null;

        #endregion

        #region 变量写入






        public static bool VariableHooksEnabled;


        public static event Func<Fsm, string, float, float> FsmFloatSetHook;
        public static event Func<Fsm, string, int, int> FsmIntSetHook;
        public static event Func<Fsm, string, bool, bool> FsmBoolSetHook;
        public static event Func<Fsm, string, string, string> FsmStringSetHook;

        [ThreadStatic] private static bool _inVariableHook;

        private static T RunVariableHook<T>(Delegate ev, string name, T value, Func<Delegate, Fsm, T, T> call)
        {
            if (ev == null || _inVariableHook) return value;
            Fsm fsm = FsmExecutionStack.ExecutingFsm;
            if (fsm == null) return value;

            _inVariableHook = true;
            try
            {
                foreach (Delegate d in ev.GetInvocationList())
                {
                    try { value = call(d, fsm, value); }
                    catch (Exception e) { Debug.LogError("[PlayMakerHooks] 变量钩子抛出异常: " + e); }
                }
            }
            finally { _inVariableHook = false; }
            return value;
        }

        internal static float OnFloatSet(string name, float value) =>
            RunVariableHook(FsmFloatSetHook, name, value, (d, f, v) => ((Func<Fsm, string, float, float>) d)(f, name, v));

        internal static int OnIntSet(string name, int value) =>
            RunVariableHook(FsmIntSetHook, name, value, (d, f, v) => ((Func<Fsm, string, int, int>) d)(f, name, v));

        internal static bool OnBoolSet(string name, bool value) =>
            RunVariableHook(FsmBoolSetHook, name, value, (d, f, v) => ((Func<Fsm, string, bool, bool>) d)(f, name, v));

        internal static string OnStringSet(string name, string value) =>
            RunVariableHook(FsmStringSetHook, name, value, (d, f, v) => ((Func<Fsm, string, string, string>) d)(f, name, v));

        #endregion

        #region 按名字注册的 FSM 修改

        private sealed class FsmEdit
        {
            public string GameObjectName;
            public string FsmName;
            public Action<PlayMakerFSM> Apply;
        }

        private sealed class StateListener
        {
            public string GameObjectName;
            public string FsmName;
            public string StateName;
            public Action<PlayMakerFSM, FsmState> Callback;
        }

        private static readonly Dictionary<string, List<FsmEdit>> Edits = new Dictionary<string, List<FsmEdit>>();
        private static readonly Dictionary<string, List<StateListener>> Listeners = new Dictionary<string, List<StateListener>>();





        public static void OnFsm(string fsmName, Action<PlayMakerFSM> edit, bool applyToExisting = false) =>
            OnFsm(null, fsmName, edit, applyToExisting);


        public static void OnFsm(string gameObjectName, string fsmName, Action<PlayMakerFSM> edit, bool applyToExisting = false)
        {
            if (string.IsNullOrEmpty(fsmName) || edit == null) return;

            var entry = new FsmEdit { GameObjectName = gameObjectName, FsmName = fsmName, Apply = edit };
            if (!Edits.TryGetValue(fsmName, out List<FsmEdit> list))
                Edits[fsmName] = list = new List<FsmEdit>();
            list.Add(entry);

            if (!applyToExisting) return;
            foreach (PlayMakerFSM fsm in new List<PlayMakerFSM>(PlayMakerFSM.FsmList))
            {
                if (fsm == null || !Matches(fsm, gameObjectName, fsmName)) continue;
                SafeInvoke(() => edit(fsm), "OnFsm(" + fsmName + ")");
            }
        }


        public static void RemoveFsmEdit(Action<PlayMakerFSM> edit)
        {
            foreach (List<FsmEdit> list in Edits.Values)
                list.RemoveAll(e => e.Apply == edit);
        }


        public static void OnStateEnter(string fsmName, string stateName, Action<PlayMakerFSM, FsmState> callback) =>
            OnStateEnter(null, fsmName, stateName, callback);

        public static void OnStateEnter(string gameObjectName, string fsmName, string stateName, Action<PlayMakerFSM, FsmState> callback)
        {
            if (string.IsNullOrEmpty(fsmName) || string.IsNullOrEmpty(stateName) || callback == null) return;

            var entry = new StateListener
            {
                GameObjectName = gameObjectName, FsmName = fsmName, StateName = stateName, Callback = callback,
            };
            if (!Listeners.TryGetValue(fsmName, out List<StateListener> list))
                Listeners[fsmName] = list = new List<StateListener>();
            list.Add(entry);
        }

        public static void RemoveStateListener(Action<PlayMakerFSM, FsmState> callback)
        {
            foreach (List<StateListener> list in Listeners.Values)
                list.RemoveAll(l => l.Callback == callback);
        }

        private static bool Matches(PlayMakerFSM fsm, string gameObjectName, string fsmName)
        {
            if (fsm.FsmName != fsmName) return false;
            if (gameObjectName == null) return true;
            string n = fsm.gameObject.name;
            return n == gameObjectName || n == gameObjectName + "(Clone)";
        }

        #endregion

        #region 内部调用（供 Patches 使用）

        private static void SafeInvoke(Action a, string what)
        {
            try { a(); }
            catch (Exception e) { Debug.LogError("[PlayMakerHooks] " + what + " 抛出异常: " + e); }
        }

        private static void Raise(Action<PlayMakerFSM> ev, PlayMakerFSM fsm, string what)
        {
            if (ev == null) return;
            foreach (Delegate d in ev.GetInvocationList())
                SafeInvoke(() => ((Action<PlayMakerFSM>) d)(fsm), what);
        }

        internal static void OnFsmAwake(PlayMakerFSM fsm)
        {
            if (fsm == null) return;
            Raise(FsmAwakeHook, fsm, "FsmAwakeHook");

            if (Edits.Count == 0) return;
            string name;
            try { name = fsm.FsmName; }
            catch { return; }
            if (name == null || !Edits.TryGetValue(name, out List<FsmEdit> list) || list.Count == 0) return;

            foreach (FsmEdit e in list.ToArray())
            {
                if (e.GameObjectName != null && !Matches(fsm, e.GameObjectName, name)) continue;
                SafeInvoke(() => e.Apply(fsm), "OnFsm(" + name + ")");
            }
        }

        internal static void OnFsmEnable(PlayMakerFSM fsm) => Raise(FsmEnableHook, fsm, "FsmEnableHook");
        internal static void OnFsmStart(PlayMakerFSM fsm) => Raise(FsmStartHook, fsm, "FsmStartHook");
        internal static void OnFsmDestroy(PlayMakerFSM fsm) => Raise(FsmDestroyHook, fsm, "FsmDestroyHook");

        internal static FsmEvent OnFsmEvent(Fsm fsm, FsmEvent ev)
        {
            Func<Fsm, FsmEvent, FsmEvent> h = FsmEventHook;
            if (h == null) return ev;
            foreach (Delegate d in h.GetInvocationList())
            {
                try { ev = ((Func<Fsm, FsmEvent, FsmEvent>) d)(fsm, ev); }
                catch (Exception e) { Debug.LogError("[PlayMakerHooks] FsmEventHook 抛出异常: " + e); }
                if (ev == null) break;
            }
            return ev;
        }

        internal static void OnStateEnter(Fsm fsm, FsmState state)
        {
            Action<Fsm, FsmState> h = FsmStateEnterHook;
            if (h != null)
            {
                foreach (Delegate d in h.GetInvocationList())
                    SafeInvoke(() => ((Action<Fsm, FsmState>) d)(fsm, state), "FsmStateEnterHook");
            }

            if (Listeners.Count == 0 || state == null) return;
            string fsmName = fsm.Name;
            if (fsmName == null || !Listeners.TryGetValue(fsmName, out List<StateListener> list) || list.Count == 0) return;

            PlayMakerFSM component = fsm.FsmComponent;
            if (component == null) return;
            foreach (StateListener l in list.ToArray())
            {
                if (l.StateName != state.Name) continue;
                if (l.GameObjectName != null && !Matches(component, l.GameObjectName, fsmName)) continue;
                SafeInvoke(() => l.Callback(component, state), "OnStateEnter(" + fsmName + "/" + l.StateName + ")");
            }
        }

        internal static void OnStateExit(Fsm fsm, FsmState state)
        {
            Action<Fsm, FsmState> h = FsmStateExitHook;
            if (h == null) return;
            foreach (Delegate d in h.GetInvocationList())
                SafeInvoke(() => ((Action<Fsm, FsmState>) d)(fsm, state), "FsmStateExitHook");
        }

        internal static void OnTransition(Fsm fsm, FsmTransition transition, bool isGlobal)
        {
            Action<Fsm, FsmTransition, bool> h = FsmTransitionHook;
            if (h == null) return;
            foreach (Delegate d in h.GetInvocationList())
                SafeInvoke(() => ((Action<Fsm, FsmTransition, bool>) d)(fsm, transition, isGlobal), "FsmTransitionHook");
        }

        #endregion
    }
}
