using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modding
{




    public static class ModMenuScreen
    {
        private static readonly Dictionary<MenuScreen, ScreenState> States = new Dictionary<MenuScreen, ScreenState>();





        public static MenuScreen Create(string title, IList<ModMenuRow> rows, MenuScreen parent, Action onBack = null, bool destroyOnBack = true)
        {
            ModMenuTemplates t = ModMenuTemplates.Current;
            if (t == null || !t.IsValid)
                throw new InvalidOperationException("Mod 菜单还没有就绪（UIManager 尚未启动，或模板捕获失败，见 ModLog.txt）");

            var state = new ScreenState(t, title, rows.ToList(), parent, onBack, destroyOnBack);
            state.Build();
            States[state.Screen] = state;
            return state.Screen;
        }


        public static void Go(MenuScreen from, MenuScreen to)
        {
            UIManager ui = UIManager.instance;
            ui.StartCoroutine(GoRoutine(ui, from, to));
        }

        public static void Destroy(MenuScreen screen)
        {
            if (screen == null) return;
            States.Remove(screen);
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        internal static IEnumerator GoRoutine(UIManager ui, MenuScreen from, MenuScreen to)
        {
            if (from != null) yield return ui.StartCoroutine(ui.HideMenu(from));
            if (to != null) yield return ui.StartCoroutine(ui.ShowMenu(to));
        }



        private class ScreenState
        {
            private readonly ModMenuTemplates _t;
            private readonly string _title;
            private readonly List<ModMenuRow> _rows;
            private readonly MenuScreen _parent;
            private readonly Action _onBack;
            private readonly bool _destroyOnBack;

            private MenuButtonList _list;
            private MenuButton _back;
            private Transform _content;
            private readonly List<GameObject> _widgets = new List<GameObject>();
            private readonly List<Selectable> _selectables = new List<Selectable>();
            private MenuButton _pageButton;
            private int _page;

            internal MenuScreen Screen;

            internal ScreenState(ModMenuTemplates t, string title, List<ModMenuRow> rows, MenuScreen parent, Action onBack, bool destroyOnBack)
            {
                _t = t; _title = title; _rows = rows; _parent = parent; _onBack = onBack; _destroyOnBack = destroyOnBack;
            }

            internal void Build()
            {
                GameObject go = UnityEngine.Object.Instantiate(_t.Skeleton, _t.Skeleton.transform.parent);
                go.name = "ModMenu_" + _title;
                Screen = go.GetComponent<MenuScreen>();
                _list = go.GetComponent<MenuButtonList>();
                _back = Screen.backButton;
                _content = ModMenuTemplates.FromIndexPath(go.transform, _t.ContentIndexPath) ?? go.transform;

                if (_t.TitleIndexPath != null)
                {
                    Transform tt = ModMenuTemplates.FromIndexPath(go.transform, _t.TitleIndexPath);
                    var text = tt != null ? tt.GetComponent<Text>() : null;
                    if (text != null)
                    {
                        MenuWidgets.StripLocalize(text.gameObject);
                        text.text = _title;
                    }
                }

                if (_back != null)
                {
                    MenuWidgets.SetLabel(_back.gameObject, "Back");
                    _back.OnSubmitPressed = new UnityEvent();
                    _back.OnSubmitPressed.AddListener(GoBack);
                    MenuWidgets.SetCancel(_back, GoBack);
                }

                BuildPage(0);
            }

            private void GoBack()
            {
                try { _onBack?.Invoke(); }
                catch (Exception e) { Logger.LogError("ModMenu", "onBack 抛出异常: " + e); }

                UIManager ui = UIManager.instance;
                MenuScreen self = Screen;
                bool destroy = _destroyOnBack;
                ui.StartCoroutine(BackRoutine(ui, self, _parent, destroy));
            }

            private static IEnumerator BackRoutine(UIManager ui, MenuScreen self, MenuScreen parent, bool destroy)
            {
                yield return ui.StartCoroutine(GoRoutine(ui, self, parent));
                if (destroy) ModMenuScreen.Destroy(self);
            }

            private int PerPage
            {
                get
                {
                    int slots = _t.Slots.Count;
                    if (_t.UseLayout || slots == 0) return Math.Max(1, _rows.Count);
                    return _rows.Count > slots ? Math.Max(1, slots - 1) : slots;
                }
            }

            private bool Paged => !_t.UseLayout && _t.Slots.Count > 0 && _rows.Count > _t.Slots.Count;

            private void BuildPage(int page)
            {
                foreach (GameObject w in _widgets)
                {
                    if (w == null) continue;
                    w.SetActive(false);
                    UnityEngine.Object.Destroy(w);
                }
                _widgets.Clear();
                _selectables.Clear();
                _pageButton = null;

                int perPage = PerPage;
                int pageCount = Math.Max(1, (_rows.Count + perPage - 1) / perPage);
                _page = ((page % pageCount) + pageCount) % pageCount;

                int start = _page * perPage;
                int end = Math.Min(_rows.Count, start + perPage);
                int slot = 0;
                for (int i = start; i < end; i++, slot++)
                {
                    ModMenuRow row = _rows[i];
                    GameObject w = null;
                    try
                    {
                        if (row is ModMenuOptionRow o) w = MenuWidgets.CreateOption(_t, _content, o);
                        else if (row is ModMenuButtonRow b) w = MenuWidgets.CreateButton(_t, _content, b.Label, b.Description, b.OnSubmit);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("ModMenu", $"创建行 \"{row.Label}\" 失败: {e}");
                    }
                    if (w == null) continue;

                    Place(w, slot);
                    _widgets.Add(w);
                    var sel = w.GetComponent<Selectable>();
                    if (sel != null)
                    {
                        MenuWidgets.SetCancel(sel, GoBack);
                        _selectables.Add(sel);
                    }
                }

                if (Paged)
                {
                    string label = $"Page {_page + 1}/{pageCount}  >";
                    GameObject pb = MenuWidgets.CreateButton(_t, _content, label, null, () => BuildPage(_page + 1));
                    Place(pb, slot);
                    _widgets.Add(pb);
                    _pageButton = pb.GetComponent<MenuButton>();
                    MenuWidgets.SetCancel(_pageButton, GoBack);
                    _selectables.Add(_pageButton);
                }

                var all = new List<Selectable>(_selectables);
                if (_back != null) all.Add(_back);
                MenuReflection.SetSelectables(_list, all);


                if (Screen.gameObject.activeInHierarchy)
                {
                    _list.SetupActive();
                    Selectable first = _selectables.FirstOrDefault() ?? _back;
                    if (first != null) UIManager.HighlightSelectableNoSound(first);
                }
            }

            private void Place(GameObject w, int slot)
            {
                if (_t.UseLayout || slot >= _t.Slots.Count) return;
                var rt = w.transform as RectTransform;
                if (rt != null) _t.Slots[slot].ApplyTo(rt, Screen.transform);
            }
        }
    }


    internal static class MenuWidgets
    {





        internal static void StripNavigationTriggers(GameObject root)
        {
            foreach (EventTrigger et in root.GetComponentsInChildren<EventTrigger>(true))
            {
                if (et.triggers == null) continue;
                et.triggers.RemoveAll(e => e.eventID == EventTriggerType.Submit || e.eventID == EventTriggerType.PointerClick);
            }
        }

        internal static void StripLocalize(GameObject root)
        {
            foreach (AutoLocalizeTextUI c in root.GetComponentsInChildren<AutoLocalizeTextUI>(true))
                UnityEngine.Object.DestroyImmediate(c);
        }


        internal static void SetLabel(GameObject root, string text, Func<Text, bool> skip = null)
        {
            StripLocalize(root);

            foreach (Text t in root.GetComponentsInChildren<Text>(true))
            {
                if (skip != null && skip(t)) continue;
                t.text = text;
            }

            foreach (Component c in root.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                string n = c.GetType().Name;
                if (n != "TextMeshProUGUI" && n != "TMP_Text") continue;
                c.GetType().GetProperty("text")?.SetValue(c, text);
            }
        }

        internal static void SetCancel(Selectable s, Action action)
        {
            if (s is MenuSelectable ms) ms.cancelAction = CancelAction.DoNothing;

            EventTrigger old = s.GetComponent<EventTrigger>();
            if (old != null) UnityEngine.Object.DestroyImmediate(old);

            EventTrigger et = s.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.Cancel };
            entry.callback.AddListener(_ => action());
            et.triggers.Add(entry);
        }

        private static void ApplyDescription(Animator descriptionText, string description, Action<Animator> clear)
        {
            if (descriptionText == null) return;
            if (string.IsNullOrEmpty(description))
            {
                descriptionText.gameObject.SetActive(false);
                clear(descriptionText);
                return;
            }
            StripLocalize(descriptionText.gameObject);
            foreach (Text t in descriptionText.GetComponentsInChildren<Text>(true))
                t.text = description;
        }

        internal static GameObject CreateButton(ModMenuTemplates t, Transform parent, string label, string description, Action onSubmit)
        {
            GameObject go = UnityEngine.Object.Instantiate(t.ButtonTemplate.gameObject, parent);
            go.name = "ModButton_" + label;
            var btn = go.GetComponent<MenuButton>();
            StripNavigationTriggers(go);

            SetLabel(go, label, txt => btn.descriptionText != null && txt.transform.IsChildOf(btn.descriptionText.transform));
            ApplyDescription(btn.descriptionText, description, a => btn.descriptionText = null);

            btn.OnSubmitPressed = new UnityEvent();
            if (onSubmit != null)
                btn.OnSubmitPressed.AddListener(() =>
                {
                    try { onSubmit(); }
                    catch (Exception e) { Logger.LogError("ModMenu", $"按钮 \"{label}\" 抛出异常: {e}"); }
                });
            return go;
        }

        internal static GameObject CreateOption(ModMenuTemplates t, Transform parent, ModMenuOptionRow row)
        {
            if (row.Values == null || row.Values.Length == 0)
                throw new InvalidOperationException("选项没有任何取值");

            GameObject go = UnityEngine.Object.Instantiate(t.OptionTemplate.gameObject, parent);
            go.name = "ModOption_" + row.Label;
            var opt = go.GetComponent<MenuOptionHorizontal>();
            StripNavigationTriggers(go);


            if (opt.menuSetting != null) UnityEngine.Object.DestroyImmediate(opt.menuSetting);
            opt.menuSetting = null;
            opt.applyButton = null;
            opt.refreshListOnChange = null;
            opt.applySettingOn = MenuOptionHorizontal.ApplyOnType.Scroll;
            opt.localizeText = false;
            opt.optionList = row.Values;

            int current = 0;
            try { current = row.Getter != null ? row.Getter() : 0; }
            catch (Exception e) { Logger.LogError("ModMenu", $"读取 \"{row.Label}\" 的当前值失败: {e}"); }
            opt.selectedOptionIndex = Mathf.Clamp(current, 0, row.Values.Length - 1);

            StripLocalize(go);
            SetLabel(go, row.Label, txt =>
                txt == opt.optionText ||
                (opt.descriptionText != null && txt.transform.IsChildOf(opt.descriptionText.transform)));
            ApplyDescription(opt.descriptionText, row.Description, a => opt.descriptionText = null);


            var watcher = go.AddComponent<ModOptionWatcher>();
            watcher.Option = opt;
            watcher.Last = opt.selectedOptionIndex;
            watcher.OnChanged = row.Setter;
            watcher.Label = row.Label;


            opt.SetOptionTo(opt.selectedOptionIndex);
            return go;
        }
    }


    internal class ModOptionWatcher : MonoBehaviour
    {
        internal MenuOptionHorizontal Option;
        internal int Last;
        internal Action<int> OnChanged;
        internal string Label;

        private void Update()
        {
            if (Option == null || Option.selectedOptionIndex == Last) return;
            Last = Option.selectedOptionIndex;
            try { OnChanged?.Invoke(Last); }
            catch (Exception e) { Logger.LogError("ModMenu", $"选项 \"{Label}\" 的 Saver 抛出异常: {e}"); }
        }
    }
}
