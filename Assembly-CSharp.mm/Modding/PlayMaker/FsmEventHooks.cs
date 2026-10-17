#if !NO_FSMUTIL
using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding.PlayMakerSupport
{



    public static class FsmEventHooks
    {
        private static readonly Dictionary<string, List<Action<string, string>>> EventListeners =
            new Dictionary<string, List<Action<string, string>>>();

        private static readonly Dictionary<string, List<Func<string, string, bool>>> EventFilters =
            new Dictionary<string, List<Func<string, string, bool>>>();




        public static void RegisterEventListener(string fsmName, Action<string, string> listener)
        {
            if (!EventListeners.ContainsKey(fsmName))
            {
                EventListeners[fsmName] = new List<Action<string, string>>();
            }
            EventListeners[fsmName].Add(listener);
        }




        public static void UnregisterEventListener(string fsmName, Action<string, string> listener)
        {
            if (EventListeners.ContainsKey(fsmName))
            {
                EventListeners[fsmName].Remove(listener);
            }
        }




        public static void RegisterEventFilter(string fsmName, Func<string, string, bool> filter)
        {
            if (!EventFilters.ContainsKey(fsmName))
            {
                EventFilters[fsmName] = new List<Func<string, string, bool>>();
            }
            EventFilters[fsmName].Add(filter);
        }




        public static void UnregisterEventFilter(string fsmName, Func<string, string, bool> filter)
        {
            if (EventFilters.ContainsKey(fsmName))
            {
                EventFilters[fsmName].Remove(filter);
            }
        }




        internal static bool TriggerEvent(string fsmName, string eventName)
        {
            if (EventFilters.ContainsKey(fsmName))
            {
                foreach (var filter in EventFilters[fsmName])
                {
                    try
                    {
                        if (!filter(fsmName, eventName))
                        {
                            Logger.Log($"[FsmEventHooks] 事件 {eventName} 被过滤器阻止 (FSM: {fsmName})");
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"[FsmEventHooks] 过滤器异常: {e}");
                    }
                }
            }

            if (EventListeners.ContainsKey(fsmName))
            {
                foreach (var listener in EventListeners[fsmName])
                {
                    try
                    {
                        listener(fsmName, eventName);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"[FsmEventHooks] 监听器异常: {e}");
                    }
                }
            }

            return true;
        }




        public static void ClearAll()
        {
            EventListeners.Clear();
            EventFilters.Clear();
        }




        public static int GetListenerCount(string fsmName)
        {
            return EventListeners.ContainsKey(fsmName) ? EventListeners[fsmName].Count : 0;
        }




        public static int GetFilterCount(string fsmName)
        {
            return EventFilters.ContainsKey(fsmName) ? EventFilters[fsmName].Count : 0;
        }
    }




    public static class FsmVariableHooks
    {
        private static readonly Dictionary<string, Dictionary<string, List<Action<object>>>> VariableListeners =
            new Dictionary<string, Dictionary<string, List<Action<object>>>>();

        private static readonly Dictionary<string, Dictionary<string, List<Func<object, object>>>> VariableModifiers =
            new Dictionary<string, Dictionary<string, List<Func<object, object>>>>();




        public static void RegisterVariableListener(string fsmName, string varName, Action<object> listener)
        {
#if !NO_PLAYMAKERHOOKS
            PlayMakerBridge.EnableVariableHooks();
#endif
            if (!VariableListeners.ContainsKey(fsmName))
            {
                VariableListeners[fsmName] = new Dictionary<string, List<Action<object>>>();
            }
            if (!VariableListeners[fsmName].ContainsKey(varName))
            {
                VariableListeners[fsmName][varName] = new List<Action<object>>();
            }
            VariableListeners[fsmName][varName].Add(listener);
        }




        public static void UnregisterVariableListener(string fsmName, string varName, Action<object> listener)
        {
            if (VariableListeners.ContainsKey(fsmName) && VariableListeners[fsmName].ContainsKey(varName))
            {
                VariableListeners[fsmName][varName].Remove(listener);
            }
        }




        public static void RegisterVariableModifier(string fsmName, string varName, Func<object, object> modifier)
        {
#if !NO_PLAYMAKERHOOKS
            PlayMakerBridge.EnableVariableHooks();
#endif
            if (!VariableModifiers.ContainsKey(fsmName))
            {
                VariableModifiers[fsmName] = new Dictionary<string, List<Func<object, object>>>();
            }
            if (!VariableModifiers[fsmName].ContainsKey(varName))
            {
                VariableModifiers[fsmName][varName] = new List<Func<object, object>>();
            }
            VariableModifiers[fsmName][varName].Add(modifier);
        }




        public static void UnregisterVariableModifier(string fsmName, string varName, Func<object, object> modifier)
        {
            if (VariableModifiers.ContainsKey(fsmName) && VariableModifiers[fsmName].ContainsKey(varName))
            {
                VariableModifiers[fsmName][varName].Remove(modifier);
            }
        }




        internal static object TriggerVariableChange(string fsmName, string varName, object value)
        {
            if (VariableModifiers.ContainsKey(fsmName) && VariableModifiers[fsmName].ContainsKey(varName))
            {
                foreach (var modifier in VariableModifiers[fsmName][varName])
                {
                    try
                    {
                        value = modifier(value);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"[FsmVariableHooks] 修改器异常: {e}");
                    }
                }
            }

            if (VariableListeners.ContainsKey(fsmName) && VariableListeners[fsmName].ContainsKey(varName))
            {
                foreach (var listener in VariableListeners[fsmName][varName])
                {
                    try
                    {
                        listener(value);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"[FsmVariableHooks] 监听器异常: {e}");
                    }
                }
            }

            return value;
        }




        public static void ClearAll()
        {
            VariableListeners.Clear();
            VariableModifiers.Clear();
        }
    }
}
#endif
