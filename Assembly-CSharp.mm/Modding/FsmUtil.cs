#if !NO_FSMUTIL
using System;
using System.Linq;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding
{




    public static class FsmUtil
    {
        public static PlayMakerFSM GetFsm(this GameObject go, string fsmName) =>
            go.GetComponents<PlayMakerFSM>().FirstOrDefault(f => f.FsmName == fsmName);

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName) =>
            fsm.FsmStates.FirstOrDefault(s => s.Name == stateName);

        public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction =>
            state.Actions[index] as T;

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction =>
            fsm.GetState(stateName)?.GetAction<T>(index);



        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            action.Init(state);
            state.Actions = state.Actions.Append(action).ToArray();
        }

        public static void InsertAction(this FsmState state, FsmStateAction action, int index)
        {
            action.Init(state);
            var list = state.Actions.ToList();
            list.Insert(Mathf.Clamp(index, 0, list.Count), action);
            state.Actions = list.ToArray();
        }

        public static void RemoveAction(this FsmState state, int index)
        {
            var list = state.Actions.ToList();
            if (index < 0 || index >= list.Count) return;
            list.RemoveAt(index);
            state.Actions = list.ToArray();
        }

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action) =>
            fsm.GetState(stateName)?.AddAction(action);

        public static void InsertAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action, int index) =>
            fsm.GetState(stateName)?.InsertAction(action, index);


        public static void AddMethod(this FsmState state, Action method) =>
            state.AddAction(new MethodAction { Method = method });

        public static void InsertMethod(this FsmState state, int index, Action method) =>
            state.InsertAction(new MethodAction { Method = method }, index);

        public static void AddMethod(this PlayMakerFSM fsm, string stateName, Action method) =>
            fsm.GetState(stateName)?.AddMethod(method);

        private class MethodAction : FsmStateAction
        {
            public Action Method;

            public override void OnEnter()
            {
                try { Method?.Invoke(); }
                catch (Exception e) { Logger.LogError($"FsmUtil.MethodAction: {e}"); }
                Finish();
            }
        }



        public static FsmEvent EnsureEvent(this PlayMakerFSM fsm, string eventName)
        {
            FsmEvent ev = FsmEvent.GetFsmEvent(eventName);
            if (!fsm.Fsm.Events.Contains(ev))
                fsm.Fsm.Events = fsm.Fsm.Events.Append(ev).ToArray();
            return ev;
        }

        public static void AddTransition(this PlayMakerFSM fsm, string fromState, string eventName, string toState)
        {
            FsmState from = fsm.GetState(fromState);
            FsmState to = fsm.GetState(toState);
            if (from == null || to == null) return;

            FsmEvent ev = fsm.EnsureEvent(eventName);
            from.Transitions = from.Transitions.Append(new FsmTransition
            {
                FsmEvent = ev,
                ToState = toState,
                ToFsmState = to,
            }).ToArray();
        }


        public static void ChangeTransition(this PlayMakerFSM fsm, string stateName, string eventName, string newToState)
        {
            FsmState state = fsm.GetState(stateName);
            FsmState to = fsm.GetState(newToState);
            if (state == null || to == null) return;

            foreach (FsmTransition t in state.Transitions)
            {
                if (t.FsmEvent != null && t.FsmEvent.Name == eventName)
                {
                    t.ToState = newToState;
                    t.ToFsmState = to;
                }
            }
        }

        public static void RemoveTransition(this PlayMakerFSM fsm, string stateName, string eventName)
        {
            FsmState state = fsm.GetState(stateName);
            if (state == null) return;
            state.Transitions = state.Transitions.Where(t => t.FsmEvent == null || t.FsmEvent.Name != eventName).ToArray();
        }



        public static FsmState AddState(this PlayMakerFSM fsm, string stateName)
        {
            var state = new FsmState(fsm.Fsm)
            {
                Name = stateName,
                Actions = new FsmStateAction[0],
                Transitions = new FsmTransition[0],
            };
            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();
            return state;
        }



        public static FsmBool AddBoolVariable(this PlayMakerFSM fsm, string name)
        {
            var v = new FsmBool(name);
            fsm.FsmVariables.BoolVariables = fsm.FsmVariables.BoolVariables.Append(v).ToArray();
            return v;
        }

        public static FsmInt AddIntVariable(this PlayMakerFSM fsm, string name)
        {
            var v = new FsmInt(name);
            fsm.FsmVariables.IntVariables = fsm.FsmVariables.IntVariables.Append(v).ToArray();
            return v;
        }

        public static FsmFloat AddFloatVariable(this PlayMakerFSM fsm, string name)
        {
            var v = new FsmFloat(name);
            fsm.FsmVariables.FloatVariables = fsm.FsmVariables.FloatVariables.Append(v).ToArray();
            return v;
        }

        public static FsmString AddStringVariable(this PlayMakerFSM fsm, string name)
        {
            var v = new FsmString(name);
            fsm.FsmVariables.StringVariables = fsm.FsmVariables.StringVariables.Append(v).ToArray();
            return v;
        }
    }
}
#endif
