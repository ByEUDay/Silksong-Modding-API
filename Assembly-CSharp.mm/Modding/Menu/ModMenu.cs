using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modding
{




    internal static class ModMenu
    {
        private static readonly ConditionalWeakTable<UIManager, object> Built = new ConditionalWeakTable<UIManager, object>();

        private static readonly Dictionary<IMod, bool> Pending = new Dictionary<IMod, bool>();
        private static MenuScreen _modListScreen;


        internal static void OnUIManagerStarted(UIManager ui)
        {
            if (ui == null || Built.TryGetValue(ui, out _)) return;
            Built.Add(ui, new object());

            try
            {
                ModMenuTemplates tmpl = ModMenuTemplates.Capture(ui);
                ModMenuTemplates.Current = tmpl;
                AddModsButton(ui, tmpl);
                Logger.Log("ModMenu", "Mod 菜单已就绪");
            }
            catch (Exception e)
            {
                Logger.LogError("ModMenu", "构建 Mod 菜单失败: " + e);
                try { MenuDebug.Dump("optionsMenuScreen", ui.optionsMenuScreen != null ? ui.optionsMenuScreen.transform : null); }
                catch {   }
            }
        }



        private static void AddModsButton(UIManager ui, ModMenuTemplates t)
        {
            MenuButton back = t.OptionsScreen.backButton;
            List<Selectable> sels = MenuReflection.GetSelectables(t.OptionsList);
            int backIdx = back != null ? sels.IndexOf(back) : -1;
            if (backIdx < 0) backIdx = sels.Count;

            GameObject go = UnityEngine.Object.Instantiate(t.ButtonTemplate.gameObject, t.ButtonTemplate.transform.parent);
            go.name = "ModsButton";
            var btn = go.GetComponent<MenuButton>();


            MenuDebug.DumpEvents("模板按钮", t.ButtonTemplate.gameObject);



            MenuWidgets.StripNavigationTriggers(go);


            MenuWidgets.SetLabel(go, "Mods", txt => btn.descriptionText != null && txt.transform.IsChildOf(btn.descriptionText.transform));
            if (btn.descriptionText != null) btn.descriptionText.gameObject.SetActive(false);
            btn.descriptionText = null;
            btn.OnSubmitPressed = new UnityEvent();
            btn.OnSubmitPressed.AddListener(() => OpenModList(ui, t));

            PositionBeforeBack(t, btn, back, sels, backIdx);

            sels.Insert(backIdx, btn);
            MenuReflection.SetSelectables(t.OptionsList, sels);
        }

        private static void PositionBeforeBack(ModMenuTemplates t, MenuButton btn, MenuButton back, List<Selectable> sels, int backIdx)
        {
            var rt = btn.transform as RectTransform;
            if (rt == null) return;
            Transform parent = btn.transform.parent;
            Transform root = t.OptionsScreen.transform;

            if (t.UseLayout)
            {

                if (back != null && back.transform.parent == parent)
                    btn.transform.SetSiblingIndex(back.transform.GetSiblingIndex());
                var prt = parent as RectTransform;
                if (prt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(prt);
                if (!OverlapsAny(rt, sels, btn)) return;
            }

            if (t.Slots.Count < 1) return;


            SlotInfo last = t.Slots[t.Slots.Count - 1];
            Vector3 stepLocal = t.Slots.Count >= 2
                ? last.LocalCenter - t.Slots[t.Slots.Count - 2].LocalCenter
                : Vector3.zero;
            if (stepLocal.sqrMagnitude < 1f) stepLocal = new Vector3(0f, -Mathf.Max(rt.rect.height, 1f), 0f);

            new SlotInfo { LocalCenter = last.LocalCenter + stepLocal }.ApplyTo(rt, root);


            var backRt = back != null ? back.transform as RectTransform : null;
            if (backRt != null && Overlaps(rt, backRt))
                backRt.position += root.TransformVector(stepLocal);

            Logger.Log("ModMenu", $"Mods 按钮已放到槽位 {t.Slots.Count}，步长 {stepLocal}");
        }

        private static bool Overlaps(RectTransform a, RectTransform b)
        {
            float threshold = Mathf.Max(a.rect.height * Mathf.Abs(a.lossyScale.y), 1f) * 0.5f;
            return Vector3.Distance(SlotInfo.WorldCenter(a), SlotInfo.WorldCenter(b)) < threshold;
        }

        private static bool OverlapsAny(RectTransform rt, List<Selectable> sels, MenuButton self)
        {
            foreach (Selectable s in sels)
            {
                if (s == null || s == self) continue;
                var other = s.transform as RectTransform;
                if (other != null && Overlaps(rt, other)) return true;
            }
            return false;
        }



        private static void OpenModList(UIManager ui, ModMenuTemplates t)
        {
            try
            {
                Pending.Clear();
                List<ModMenuRow> rows = BuildModRows();
                _modListScreen = ModMenuScreen.Create("Mods", rows, t.OptionsScreen, onBack: ApplyPending);
                ModMenuScreen.Go(t.OptionsScreen, _modListScreen);
            }
            catch (Exception e)
            {
                Logger.LogError("ModMenu", "打开 Mod 列表失败: " + e);
            }
        }

        private static List<ModMenuRow> BuildModRows()
        {
            var rows = new List<ModMenuRow>();
            string off = "Off", on = "On";

            foreach (IMod mod in ModLoader.LoadedMods)
            {
                IMod m = mod;
                string name = SafeName(m);
                string version = SafeVersion(m);

                bool toggleInside = (m is IMenuMod mm && mm.ToggleButtonInsideMenu)
                                    || (m is ICustomMenuMod cm && cm.ToggleButtonInsideMenu);
                bool hasMenu = m is IMenuMod || m is ICustomMenuMod;

                var toggle = new ModToggleDelegates
                {
                    SetModEnabled = enabled => Pending[m] = enabled,
                    GetModEnabled = () => Pending.TryGetValue(m, out bool p) ? p : ModLoader.IsEnabled(m),
                };

                if (!toggleInside)
                {
                    string note = m is ITogglableMod ? "" : "  (restart to apply)";
                    rows.Add(ModMenuRow.Option(name, new[] { off, on },
                        () => toggle.GetModEnabled() ? 1 : 0,
                        v => toggle.SetModEnabled(v == 1),
                        $"v{version}{note}"));
                }

                if (hasMenu)
                {
                    ModToggleDelegates? passToggle = toggleInside ? toggle : (ModToggleDelegates?) null;
                    rows.Add(ModMenuRow.Button(GetMenuButtonText(m), () => OpenModScreen(m, passToggle), $"v{version}"));
                }
            }

            if (rows.Count == 0)
                rows.Add(ModMenuRow.Button("No mods loaded", null));

            return rows;
        }

        private static void OpenModScreen(IMod mod, ModToggleDelegates? toggle)
        {
            try
            {
                MenuScreen screen;
                if (mod is IMenuMod menuMod)
                {
                    MenuEntry toggleEntry = null;
                    if (toggle.HasValue)
                    {
                        ModToggleDelegates d = toggle.Value;
                        toggleEntry = new MenuEntry
                        {
                            Name = SafeName(mod),
                            Values = new[] { "Off", "On" },
                            Saver = v => d.SetModEnabled(v == 1),
                            Loader = () => d.GetModEnabled() ? 1 : 0,
                        };
                    }

                    List<MenuEntry> entries = menuMod.GetMenuData(toggleEntry) ?? new List<MenuEntry>();
                    List<ModMenuRow> rows = entries.Select(ModMenuRow.FromEntry).ToList();
                    screen = ModMenuScreen.Create(SafeName(mod), rows, _modListScreen);
                }
                else if (mod is ICustomMenuMod custom)
                {
                    screen = custom.GetMenuScreen(_modListScreen, toggle);
                }
                else
                {
                    return;
                }

                if (screen != null) ModMenuScreen.Go(_modListScreen, screen);
            }
            catch (Exception e)
            {
                Logger.LogError("ModMenu", $"打开 {SafeName(mod)} 的菜单失败: {e}");
            }
        }

        private static void ApplyPending()
        {
            foreach (KeyValuePair<IMod, bool> kv in Pending.ToList())
            {
                try { ModLoader.SetEnabled(kv.Key, kv.Value); }
                catch (Exception e) { Logger.LogError("ModMenu", $"切换 {SafeName(kv.Key)} 失败: {e}"); }
            }
            Pending.Clear();
        }



        private static string SafeName(IMod m)
        {
            try { return m.GetName(); } catch { return m.GetType().Name; }
        }

        private static string SafeVersion(IMod m)
        {
            try { return m.GetVersion(); } catch { return "???"; }
        }

        private static string GetMenuButtonText(IMod m)
        {
            try
            {
                if (m is IMenuButtonText t) return t.GetMenuButtonText();
            }
            catch {   }
            return SafeName(m) + " Settings";
        }
    }
}
