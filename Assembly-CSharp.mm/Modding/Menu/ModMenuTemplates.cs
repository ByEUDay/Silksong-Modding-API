using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{






    internal struct SlotInfo
    {
        public Vector3 LocalCenter;

        internal static Vector3 WorldCenter(RectTransform rt) => rt.TransformPoint(rt.rect.center);

        public static SlotInfo From(RectTransform rt, Transform screenRoot) => new SlotInfo
        {
            LocalCenter = screenRoot.InverseTransformPoint(WorldCenter(rt)),
        };


        public void ApplyTo(RectTransform rt, Transform screenRoot)
        {
            Vector3 target = screenRoot.TransformPoint(LocalCenter);
            rt.position += target - WorldCenter(rt);
        }
    }





    internal class ModMenuTemplates
    {
        internal static ModMenuTemplates Current;

        internal UIManager Ui;
        internal MenuScreen OptionsScreen;
        internal MenuButtonList OptionsList;
        internal MenuButton ButtonTemplate;
        internal MenuOptionHorizontal OptionTemplate;
        internal GameObject Skeleton;
        internal List<int> ContentIndexPath;
        internal List<int> TitleIndexPath;
        internal List<SlotInfo> Slots = new List<SlotInfo>();
        internal bool UseLayout;

        internal bool IsValid => Skeleton != null && ButtonTemplate != null && OptionTemplate != null;

        internal static ModMenuTemplates Capture(UIManager ui)
        {
            var t = new ModMenuTemplates { Ui = ui, OptionsScreen = ui.optionsMenuScreen };
            if (t.OptionsScreen == null) throw new InvalidOperationException("UIManager.optionsMenuScreen 为空");

            t.OptionsList = t.OptionsScreen.GetComponent<MenuButtonList>();
            if (t.OptionsList == null) throw new InvalidOperationException("选项页上没有 MenuButtonList");
            if (!MenuReflection.Available) throw new InvalidOperationException("MenuButtonList 的内部结构和预期不一致");

            List<Selectable> sels = MenuReflection.GetSelectables(t.OptionsList, out List<bool> hasCond);
            MenuButton back = t.OptionsScreen.backButton;


            var normal = new List<Selectable>();
            for (int i = 0; i < sels.Count; i++)
                if (sels[i] != null && sels[i] != back) normal.Add(sels[i]);

            t.ButtonTemplate = null;
            for (int i = 0; i < sels.Count; i++)
            {
                if (sels[i] is MenuButton mb && mb != back && !hasCond[i]) { t.ButtonTemplate = mb; break; }
            }
            if (t.ButtonTemplate == null)
                t.ButtonTemplate = normal.OfType<MenuButton>().FirstOrDefault();
            if (t.ButtonTemplate == null) throw new InvalidOperationException("选项页里找不到可用的 MenuButton 模板");

            foreach (Selectable s in normal)
            {
                var rt = s.transform as RectTransform;
                if (rt != null) t.Slots.Add(SlotInfo.From(rt, t.OptionsScreen.transform));
            }
            t.FixDegenerateSlots();
            Transform contentParent = t.ButtonTemplate.transform.parent;
            t.UseLayout = contentParent.GetComponent<LayoutGroup>() != null;

            t.OptionTemplate = FindOptionTemplate(ui);
            if (t.OptionTemplate == null) throw new InvalidOperationException("找不到可用的 MenuOptionHorizontal 模板");

            t.BuildSkeleton(back);
            return t;
        }





        private void FixDegenerateSlots()
        {
            if (Slots.Count < 2) return;

            float spread = 0f;
            for (int i = 1; i < Slots.Count; i++)
                spread = Mathf.Max(spread, (Slots[i].LocalCenter - Slots[0].LocalCenter).magnitude);
            if (spread > 1f) return;

            var rt = ButtonTemplate.transform as RectTransform;
            float step = rt != null ? Mathf.Max(rt.rect.height, 1f) : 60f;
            Vector3 origin = Slots[0].LocalCenter;
            for (int i = 0; i < Slots.Count; i++)
                Slots[i] = new SlotInfo { LocalCenter = origin + new Vector3(0f, -step * i, 0f) };

            Logger.LogWarn("ModMenu", $"选项页的 {Slots.Count} 个槽位位置重合，已按高度 {step:0} 自动排开");
        }

        private static MenuOptionHorizontal FindOptionTemplate(UIManager ui)
        {
            MenuScreen[] screens = { ui.gameOptionsMenuScreen, ui.audioMenuScreen, ui.videoMenuScreen };
            foreach (MenuScreen screen in screens)
            {
                if (screen == null) continue;
                var list = screen.GetComponent<MenuButtonList>();
                if (list == null) continue;
                List<Selectable> sels = MenuReflection.GetSelectables(list, out List<bool> hasCond);
                for (int i = 0; i < sels.Count; i++)
                    if (sels[i] is MenuOptionHorizontal opt && opt.optionText != null && !hasCond[i]) return opt;
                foreach (Selectable s in sels)
                    if (s is MenuOptionHorizontal opt && opt.optionText != null) return opt;
            }
            return null;
        }

        private void BuildSkeleton(MenuButton originalBack)
        {
            Skeleton = UnityEngine.Object.Instantiate(OptionsScreen.gameObject, OptionsScreen.transform.parent);
            Skeleton.name = "ModMenuSkeleton";
            Skeleton.SetActive(false);

            var screen = Skeleton.GetComponent<MenuScreen>();
            var list = Skeleton.GetComponent<MenuButtonList>();
            MenuButton back = screen.backButton;

            List<Selectable> cloneSels = MenuReflection.GetSelectables(list);
            Transform contentParent = null;
            foreach (Selectable s in cloneSels)
            {
                if (s == null || s == back) continue;
                if (contentParent == null) contentParent = s.transform.parent;
                UnityEngine.Object.DestroyImmediate(s.gameObject);
            }
            if (contentParent == null) contentParent = back != null ? back.transform.parent : Skeleton.transform;

            ContentIndexPath = IndexPath(Skeleton.transform, contentParent);


            Text best = null;
            foreach (Text txt in Skeleton.GetComponentsInChildren<Text>(true))
            {
                if (txt.GetComponentInParent<Selectable>(true) != null) continue;
                if (best == null || txt.fontSize > best.fontSize) best = txt;
            }
            TitleIndexPath = best != null ? IndexPath(Skeleton.transform, best.transform) : null;

            MenuReflection.SetSelectables(list, back != null ? new List<Selectable> { back } : new List<Selectable>());
        }

        internal static List<int> IndexPath(Transform root, Transform t)
        {
            var path = new List<int>();
            while (t != null && t != root)
            {
                path.Insert(0, t.GetSiblingIndex());
                t = t.parent;
            }
            return path;
        }

        internal static Transform FromIndexPath(Transform root, List<int> path)
        {
            Transform t = root;
            foreach (int i in path)
            {
                if (i < 0 || i >= t.childCount) return null;
                t = t.GetChild(i);
            }
            return t;
        }
    }
}
