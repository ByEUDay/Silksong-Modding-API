using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modding
{



    public class ModUIBuilder
    {
        private readonly GameObject _root;
        private readonly List<GameObject> _children = new List<GameObject>();

        private ModUIBuilder(GameObject root)
        {
            _root = root;
        }

        public static ModUIBuilder Create(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return new ModUIBuilder(go);
        }

        public ModUIBuilder WithRectTransform(Action<RectTransform> configure)
        {
            var rt = _root.GetComponent<RectTransform>();
            if (rt != null) configure(rt);
            return this;
        }

        public ModUIBuilder WithImage(Color color, Sprite sprite = null)
        {
            var img = _root.AddComponent<Image>();
            img.color = color;
            if (sprite != null) img.sprite = sprite;
            return this;
        }

        public ModUIBuilder WithText(string text, int fontSize = 14, Color? color = null, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var txt = _root.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color ?? Color.white;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return this;
        }

        public ModUIBuilder WithButton(Action onClick, Color? normalColor = null, Color? highlightColor = null)
        {
            var btn = _root.AddComponent<Button>();
            var colors = btn.colors;
            if (normalColor.HasValue) colors.normalColor = normalColor.Value;
            if (highlightColor.HasValue) colors.highlightedColor = highlightColor.Value;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(new UnityAction(onClick));
            return this;
        }

        public ModUIBuilder WithToggle(bool initialValue, Action<bool> onValueChanged)
        {
            var toggle = _root.AddComponent<Toggle>();
            toggle.isOn = initialValue;
            if (onValueChanged != null) toggle.onValueChanged.AddListener(new UnityAction<bool>(onValueChanged));
            return this;
        }

        public ModUIBuilder WithSlider(float minValue, float maxValue, float initialValue, Action<float> onValueChanged)
        {
            var slider = _root.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = initialValue;
            if (onValueChanged != null) slider.onValueChanged.AddListener(new UnityAction<float>(onValueChanged));
            return this;
        }

        public ModUIBuilder WithInputField(string placeholder, string initialValue, Action<string> onValueChanged)
        {
            var input = _root.AddComponent<InputField>();
            input.placeholder = CreatePlaceholderText(placeholder);
            input.text = initialValue ?? "";
            if (onValueChanged != null) input.onValueChanged.AddListener(new UnityAction<string>(onValueChanged));
            return this;
        }

        public ModUIBuilder WithDropdown(List<string> options, int initialValue, Action<int> onValueChanged)
        {
            var dropdown = _root.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            if (options != null) dropdown.AddOptions(options);
            dropdown.value = initialValue;
            if (onValueChanged != null) dropdown.onValueChanged.AddListener(new UnityAction<int>(onValueChanged));
            return this;
        }

        public ModUIBuilder WithScrollRect()
        {
            _root.AddComponent<ScrollRect>();
            return this;
        }

        public ModUIBuilder WithLayoutGroup(LayoutGroup layout)
        {
            _root.AddComponent(layout.GetType());
            return this;
        }

        public ModUIBuilder WithContentSizeFitter(ContentSizeFitter.FitMode horizontal, ContentSizeFitter.FitMode vertical)
        {
            var fitter = _root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = horizontal;
            fitter.verticalFit = vertical;
            return this;
        }

        public ModUIBuilder WithMask()
        {
            _root.AddComponent<Mask>();
            _root.AddComponent<Image>().color = Color.white;
            return this;
        }

        public ModUIBuilder AddChild(string name, Action<ModUIBuilder> configure)
        {
            var child = Create(name, _root.transform);
            configure?.Invoke(child);
            _children.Add(child._root);
            return this;
        }

        public ModUIBuilder AddChild(GameObject prefab)
        {
            var go = UnityEngine.Object.Instantiate(prefab, _root.transform);
            _children.Add(go);
            return this;
        }

        public GameObject Build()
        {
            return _root;
        }

        public void Destroy()
        {
            if (_root != null)
                UnityEngine.Object.Destroy(_root);
        }

        private Text CreatePlaceholderText(string placeholder)
        {
            var go = new GameObject("Placeholder", typeof(RectTransform));
            go.transform.SetParent(_root.transform, false);
            var txt = go.AddComponent<Text>();
            txt.text = placeholder ?? "";
            txt.fontStyle = FontStyle.Italic;
            txt.color = new Color(1, 1, 1, 0.5f);
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return txt;
        }
    }




    public static class ModUIExtensions
    {
        public static RectTransform SetAnchors(this RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            return rt;
        }

        public static RectTransform SetSize(this RectTransform rt, Vector2 size)
        {
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform SetPosition(this RectTransform rt, Vector2 pos)
        {
            rt.anchoredPosition = pos;
            return rt;
        }

        public static RectTransform SetPivot(this RectTransform rt, Vector2 pivot)
        {
            rt.pivot = pivot;
            return rt;
        }

        public static RectTransform Stretch(this RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        public static RectTransform StretchHorizontal(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1, rt.anchorMax.y);
            return rt;
        }

        public static RectTransform StretchVertical(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(rt.anchorMin.x, 0);
            rt.anchorMax = new Vector2(rt.anchorMax.x, 1);
            return rt;
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        public static void SetLayerRecursive(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                child.gameObject.SetLayerRecursive(layer);
        }

        public static void SetActiveRecursive(this GameObject go, bool active)
        {
            go.SetActive(active);
            foreach (Transform child in go.transform)
                child.gameObject.SetActiveRecursive(active);
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
