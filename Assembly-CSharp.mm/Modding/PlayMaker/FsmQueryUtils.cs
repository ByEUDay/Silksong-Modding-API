using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding.PlayMaker
{



    public static class FsmQueryUtils
    {
        public static PlayMakerFSM FindFsm(GameObject go, string fsmName)
        {
            var fsms = go.GetComponents<PlayMakerFSM>();
            return fsms.FirstOrDefault(f => f.FsmName == fsmName);
        }

        public static PlayMakerFSM[] FindAllFsms(GameObject go)
        {
            return go.GetComponents<PlayMakerFSM>();
        }

        public static PlayMakerFSM FindFsmInScene(string fsmName)
        {
            return UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>()
                .FirstOrDefault(f => f.FsmName == fsmName);
        }

        public static PlayMakerFSM[] FindAllFsmsInScene()
        {
            return UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>();
        }

        public static FsmState FindState(PlayMakerFSM fsm, string stateName)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == stateName);
        }

        public static FsmStateAction FindAction(FsmState state, Type actionType)
        {
            return state.Actions.FirstOrDefault(a => a.GetType() == actionType);
        }

        public static T FindAction<T>(FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().FirstOrDefault();
        }

        public static FsmStateAction[] FindAllActions(FsmState state, Type actionType)
        {
            return state.Actions.Where(a => a.GetType() == actionType).ToArray();
        }

        public static T[] FindAllActions<T>(FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().ToArray();
        }

        public static NamedVariable FindVariable(Fsm fsm, string variableName)
        {
            NamedVariable result = null;
            result = fsm.Variables.BoolVariables.FirstOrDefault(v => v.Name == variableName);
            if (result != null) return result;
            result = fsm.Variables.IntVariables.FirstOrDefault(v => v.Name == variableName);
            if (result != null) return result;
            result = fsm.Variables.FloatVariables.FirstOrDefault(v => v.Name == variableName);
            if (result != null) return result;
            result = fsm.Variables.StringVariables.FirstOrDefault(v => v.Name == variableName);
            if (result != null) return result;
            result = fsm.Variables.GameObjectVariables.FirstOrDefault(v => v.Name == variableName);
            if (result != null) return result;
            return null;
        }

        public static FsmFloat FindFloatVariable(Fsm fsm, string variableName)
        {
            return fsm.Variables.FloatVariables.FirstOrDefault(v => v.Name == variableName);
        }

        public static FsmInt FindIntVariable(Fsm fsm, string variableName)
        {
            return fsm.Variables.IntVariables.FirstOrDefault(v => v.Name == variableName);
        }

        public static FsmBool FindBoolVariable(Fsm fsm, string variableName)
        {
            return fsm.Variables.BoolVariables.FirstOrDefault(v => v.Name == variableName);
        }

        public static FsmString FindStringVariable(Fsm fsm, string variableName)
        {
            return fsm.Variables.StringVariables.FirstOrDefault(v => v.Name == variableName);
        }

        public static FsmGameObject FindGameObjectVariable(Fsm fsm, string variableName)
        {
            return fsm.Variables.GameObjectVariables.FirstOrDefault(v => v.Name == variableName);
        }

        public static void SetVariableValue(Fsm fsm, string variableName, object value)
        {
            var variable = FindVariable(fsm, variableName);
            if (variable != null)
            {
                try
                {
                    if (variable is FsmFloat f) f.Value = Convert.ToSingle(value);
                    else if (variable is FsmInt i) i.Value = Convert.ToInt32(value);
                    else if (variable is FsmBool b) b.Value = Convert.ToBoolean(value);
                    else if (variable is FsmString s) s.Value = Convert.ToString(value);
                    else if (variable is FsmGameObject g) g.Value = value as GameObject;
                    else if (variable is FsmVector2 v2) v2.Value = (Vector2)value;
                    else if (variable is FsmVector3 v3) v3.Value = (Vector3)value;
                    else if (variable is FsmColor c) c.Value = (Color)value;
                    else if (variable is FsmRect r) r.Value = (Rect)value;
                }
                catch (Exception e)
                {
                    Logger.LogError($"[FsmQuery] 设置变量 {variableName} 失败: {e.Message}");
                }
            }
        }

        public static object GetVariableValue(Fsm fsm, string variableName)
        {
            var variable = FindVariable(fsm, variableName);
            if (variable == null) return null;

            if (variable is FsmFloat f) return f.Value;
            if (variable is FsmInt i) return i.Value;
            if (variable is FsmBool b) return b.Value;
            if (variable is FsmString s) return s.Value;
            if (variable is FsmGameObject g) return g.Value;
            if (variable is FsmVector2 v2) return v2.Value;
            if (variable is FsmVector3 v3) return v3.Value;
            if (variable is FsmColor c) return c.Value;
            if (variable is FsmRect r) return r.Value;

            return null;
        }

        public static void LogFsmInfo(PlayMakerFSM fsm)
        {
            Logger.Log($"=== FSM: {fsm.FsmName} ===");
            Logger.Log($"  Owner: {fsm.gameObject.name}");
            Logger.Log($"  Start State: {fsm.Fsm.StartState ?? "null"}");
            Logger.Log($"  States: {fsm.Fsm.States.Length}");
            foreach (var state in fsm.Fsm.States)
            {
                Logger.Log($"    - {state.Name} (Actions: {state.Actions.Length}, Transitions: {state.Transitions.Length})");
            }
            var allVars = new List<NamedVariable>();
            allVars.AddRange(fsm.Fsm.Variables.BoolVariables);
            allVars.AddRange(fsm.Fsm.Variables.IntVariables);
            allVars.AddRange(fsm.Fsm.Variables.FloatVariables);
            allVars.AddRange(fsm.Fsm.Variables.StringVariables);
            allVars.AddRange(fsm.Fsm.Variables.GameObjectVariables);
            Logger.Log($"  Variables: {allVars.Count}");
            foreach (var variable in allVars)
            {
                Logger.Log($"    - {variable.Name}: {variable.VariableType} = {GetVariableValue(fsm.Fsm, variable.Name)}");
            }
        }
    }




    public static class FsmModifyUtils
    {
        public static void InsertAction(PlayMakerFSM fsm, string stateName, int index, FsmStateAction action)
        {
            var state = FsmQueryUtils.FindState(fsm, stateName);
            if (state != null)
            {
                var actions = state.Actions.ToList();
                actions.Insert(index, action);
                state.Actions = actions.ToArray();
            }
        }

        public static void RemoveAction(PlayMakerFSM fsm, string stateName, int index)
        {
            var state = FsmQueryUtils.FindState(fsm, stateName);
            if (state != null && index >= 0 && index < state.Actions.Length)
            {
                var actions = state.Actions.ToList();
                actions.RemoveAt(index);
                state.Actions = actions.ToArray();
            }
        }

        public static void ReplaceAction(PlayMakerFSM fsm, string stateName, int index, FsmStateAction newAction)
        {
            var state = FsmQueryUtils.FindState(fsm, stateName);
            if (state != null && index >= 0 && index < state.Actions.Length)
            {
                var actions = state.Actions;
                actions[index] = newAction;
            }
        }

        public static void AddTransition(PlayMakerFSM fsm, string fromState, string eventName, string toState)
        {
            var state = FsmQueryUtils.FindState(fsm, fromState);
            if (state != null)
            {
                var transitions = state.Transitions.ToList();
                transitions.Add(new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent(eventName),
                    ToFsmState = FsmQueryUtils.FindState(fsm, toState),
                    ToState = toState
                });
                state.Transitions = transitions.ToArray();
            }
        }

        public static void RemoveTransition(PlayMakerFSM fsm, string fromState, string eventName)
        {
            var state = FsmQueryUtils.FindState(fsm, fromState);
            if (state != null)
            {
                var transitions = state.Transitions.Where(t => t.FsmEvent?.Name != eventName).ToArray();
                state.Transitions = transitions;
            }
        }

        public static void ReplaceTransition(PlayMakerFSM fsm, string fromState, string eventName, string newToState)
        {
            var state = FsmQueryUtils.FindState(fsm, fromState);
            if (state != null)
            {
                var transition = state.Transitions.FirstOrDefault(t => t.FsmEvent?.Name == eventName);
                if (transition != null)
                {
                    transition.ToFsmState = FsmQueryUtils.FindState(fsm, newToState);
                    transition.ToState = newToState;
                }
            }
        }

        public static void AddGlobalTransition(PlayMakerFSM fsm, string eventName, string toState)
        {
            var globalTransitions = fsm.Fsm.GlobalTransitions?.ToList() ?? new List<FsmTransition>();
            globalTransitions.Add(new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToFsmState = FsmQueryUtils.FindState(fsm, toState),
                ToState = toState
            });
            fsm.Fsm.GlobalTransitions = globalTransitions.ToArray();
        }

        public static void SetStartState(PlayMakerFSM fsm, string stateName)
        {
            fsm.Fsm.StartState = stateName;
        }

        public static void AddState(PlayMakerFSM fsm, string stateName, FsmStateAction[] actions = null, FsmTransition[] transitions = null)
        {
            var states = fsm.Fsm.States.ToList();
            var newState = new FsmState(fsm.Fsm)
            {
                Name = stateName,
                Actions = actions ?? new FsmStateAction[0],
                Transitions = transitions ?? new FsmTransition[0]
            };
            states.Add(newState);
            fsm.Fsm.States = states.ToArray();
        }

        public static void RemoveState(PlayMakerFSM fsm, string stateName)
        {
            var states = fsm.Fsm.States.Where(s => s.Name != stateName).ToArray();
            fsm.Fsm.States = states;
        }

        public static void RenameState(PlayMakerFSM fsm, string oldName, string newName)
        {
            var state = FsmQueryUtils.FindState(fsm, oldName);
            if (state != null)
                state.Name = newName;
        }
    }
}
