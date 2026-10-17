#if !NO_FSMUTIL
using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding.PlayMakerSupport
{



    public static class FsmDataExtensions
    {



        public static Dictionary<string, object> SerializeVariables(this Fsm fsm)
        {
            var dict = new Dictionary<string, object>();
            if (fsm == null || fsm.Variables == null) return dict;

            foreach (var v in fsm.Variables.BoolVariables)
                dict["bool:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.IntVariables)
                dict["int:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.FloatVariables)
                dict["float:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.StringVariables)
                dict["string:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.Vector2Variables)
                dict["vector2:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.Vector3Variables)
                dict["vector3:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.ColorVariables)
                dict["color:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.RectVariables)
                dict["rect:" + v.Name] = v.Value;
            foreach (var v in fsm.Variables.GameObjectVariables)
                dict["gameObject:" + v.Name] = v.Value;

            return dict;
        }




        public static void DeserializeVariables(this Fsm fsm, Dictionary<string, object> dict)
        {
            if (fsm == null || fsm.Variables == null || dict == null) return;

            foreach (var kvp in dict)
            {
                int colonIdx = kvp.Key.IndexOf(':');
                if (colonIdx < 0) continue;
                string typeName = kvp.Key.Substring(0, colonIdx);
                string varName = kvp.Key.Substring(colonIdx + 1);

                try
                {
                    switch (typeName)
                    {
                        case "bool":
                            var bv = fsm.Variables.FindVariable(varName) as FsmBool;
                            if (bv != null) bv.Value = Convert.ToBoolean(kvp.Value);
                            break;
                        case "int":
                            var iv = fsm.Variables.FindVariable(varName) as FsmInt;
                            if (iv != null) iv.Value = Convert.ToInt32(kvp.Value);
                            break;
                        case "float":
                            var fv = fsm.Variables.FindVariable(varName) as FsmFloat;
                            if (fv != null) fv.Value = Convert.ToSingle(kvp.Value);
                            break;
                        case "string":
                            var sv = fsm.Variables.FindVariable(varName) as FsmString;
                            if (sv != null) sv.Value = Convert.ToString(kvp.Value);
                            break;
                        case "gameObject":
                            var gv = fsm.Variables.FindVariable(varName) as FsmGameObject;
                            if (gv != null) gv.Value = kvp.Value as GameObject;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"[FsmDataExtensions] 恢复变量 {varName} 失败: {e.Message}");
                }
            }
        }




        public static string[] GetStateNames(this Fsm fsm)
        {
            if (fsm == null || fsm.States == null) return new string[0];
            var names = new string[fsm.States.Length];
            for (int i = 0; i < fsm.States.Length; i++)
                names[i] = fsm.States[i].Name;
            return names;
        }




        public static string GetCurrentStateName(this Fsm fsm)
        {
            return fsm?.ActiveState?.Name ?? "";
        }
    }




    public static class FsmStateExtensions
    {



        public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
        {
            if (state == null || state.Actions == null || index < 0 || index >= state.Actions.Length)
                return null;
            return state.Actions[index] as T;
        }




        public static List<T> GetActions<T>(this FsmState state) where T : FsmStateAction
        {
            var result = new List<T>();
            if (state == null || state.Actions == null) return result;
            foreach (var action in state.Actions)
            {
                if (action is T t) result.Add(t);
            }
            return result;
        }




        public static FsmTransition FindTransition(this FsmState state, string eventName)
        {
            if (state == null || state.Transitions == null) return null;
            foreach (var transition in state.Transitions)
            {
                if (transition.FsmEvent != null && transition.FsmEvent.Name == eventName)
                    return transition;
            }
            return null;
        }




        public static void ClearTransitions(this FsmState state)
        {
            if (state != null)
                state.Transitions = new FsmTransition[0];
        }




        public static void ClearActions(this FsmState state)
        {
            if (state != null)
                state.Actions = new FsmStateAction[0];
        }




        public static void CopyTo(this FsmState source, FsmState target)
        {
            if (source == null || target == null) return;
            target.Name = source.Name;
            target.Actions = source.Actions;
            target.Transitions = source.Transitions;
        }
    }
}
#endif
