using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{

    internal static class MenuReflection
    {
        private const BindingFlags Priv = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly FieldInfo EntriesField = typeof(MenuButtonList).GetField("entries", Priv);
        private static readonly Type EntryType = typeof(MenuButtonList).GetNestedType("Entry", BindingFlags.NonPublic);
        private static readonly FieldInfo SelectableField = EntryType?.GetField("selectable", Priv);
        private static readonly FieldInfo ConditionField = EntryType?.GetField("condition", Priv);

        internal static bool Available => EntriesField != null && EntryType != null && SelectableField != null;

        internal static List<Selectable> GetSelectables(MenuButtonList list, out List<bool> hasCondition)
        {
            hasCondition = new List<bool>();
            var result = new List<Selectable>();
            if (!Available) return result;

            var array = EntriesField.GetValue(list) as Array;
            if (array == null) return result;

            foreach (object entry in array)
            {
                result.Add(SelectableField.GetValue(entry) as Selectable);
                hasCondition.Add(ConditionField != null && ConditionField.GetValue(entry) as UnityEngine.Object != null);
            }
            return result;
        }

        internal static List<Selectable> GetSelectables(MenuButtonList list) => GetSelectables(list, out _);

        internal static void SetSelectables(MenuButtonList list, IList<Selectable> selectables)
        {
            if (!Available) throw new InvalidOperationException("MenuButtonList.Entry 的字段名和预期不一致");

            Array array = Array.CreateInstance(EntryType, selectables.Count);
            for (int i = 0; i < selectables.Count; i++)
            {
                object entry = Activator.CreateInstance(EntryType, true);
                SelectableField.SetValue(entry, selectables[i]);
                array.SetValue(entry, i);
            }
            EntriesField.SetValue(list, array);
        }
    }


    internal static class MenuDebug
    {

        internal static void DumpEvents(string title, GameObject go)
        {
            if (go == null) return;
            Logger.Log("ModMenu", $"==== 事件转储: {title} ({go.name}) ====");

            var btn = go.GetComponent<MenuButton>();
            if (btn != null && btn.OnSubmitPressed != null)
                DumpPersistent("OnSubmitPressed", btn.OnSubmitPressed);

            foreach (UnityEngine.EventSystems.EventTrigger et in go.GetComponents<UnityEngine.EventSystems.EventTrigger>())
            {
                if (et.triggers == null) continue;
                foreach (UnityEngine.EventSystems.EventTrigger.Entry e in et.triggers)
                    DumpPersistent("EventTrigger." + e.eventID, e.callback);
            }
        }

        private static void DumpPersistent(string label, UnityEngine.Events.UnityEventBase ev)
        {
            int n = ev.GetPersistentEventCount();
            if (n == 0) { Logger.Log("ModMenu", $"  {label}: (无持久化监听)"); return; }
            for (int i = 0; i < n; i++)
            {
                UnityEngine.Object target = ev.GetPersistentTarget(i);
                Logger.Log("ModMenu", $"  {label}[{i}] -> {(target != null ? target.GetType().Name : "null")}.{ev.GetPersistentMethodName(i)}");
            }
        }

        internal static void Dump(string title, Transform root, int maxDepth = 6)
        {
            if (root == null) return;
            Logger.Log("ModMenu", $"==== 层级转储: {title} ====");
            DumpRecursive(root, 0, maxDepth);
        }

        private static void DumpRecursive(Transform t, int depth, int maxDepth)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(new string(' ', depth * 2)).Append(t.name).Append(t.gameObject.activeSelf ? "" : " (inactive)");

            var rt = t as RectTransform;
            if (rt != null)
                sb.Append($" pos=({rt.anchoredPosition.x:0},{rt.anchoredPosition.y:0}) size=({rt.sizeDelta.x:0},{rt.sizeDelta.y:0}) anchors=({rt.anchorMin.x:0.##},{rt.anchorMin.y:0.##})-({rt.anchorMax.x:0.##},{rt.anchorMax.y:0.##})");

            var comps = new List<string>();
            foreach (Component c in t.GetComponents<Component>())
            {
                if (c == null || c is Transform) continue;
                comps.Add(c.GetType().Name);
            }
            if (comps.Count > 0) sb.Append(" [").Append(string.Join(",", comps)).Append("]");

            Logger.Log("ModMenu", sb.ToString());

            if (depth >= maxDepth) return;
            foreach (Transform child in t)
                DumpRecursive(child, depth + 1, maxDepth);
        }
    }
}
