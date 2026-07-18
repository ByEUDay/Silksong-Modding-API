using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongModLoader
{
    internal static class ModMenuUI
    {
        public static bool IsOpen;
        private static Vector2 _scroll;
        private static GUIStyle? _titleStyle;
        private static GUIStyle? _labelStyle;
        private static GUIStyle? _hintStyle;
        private static readonly List<GraphicRaycaster> DisabledRaycasters = new();
        private static bool _blocking;

        public static void Toggle() => IsOpen = !IsOpen;

        public static void EnforceInputBlock()
        {
            if (IsOpen && !_blocking)
            {
                DisabledRaycasters.Clear();
                foreach (var raycaster in UnityEngine.Object.FindObjectsOfType<GraphicRaycaster>())
                {
                    if (raycaster.enabled)
                    {
                        raycaster.enabled = false;
                        DisabledRaycasters.Add(raycaster);
                    }
                }
                _blocking = true;
            }
            else if (!IsOpen && _blocking)
            {
                foreach (var raycaster in DisabledRaycasters)
                {
                    if (raycaster != null)
                    {
                        raycaster.enabled = true;
                    }
                }
                DisabledRaycasters.Clear();
                _blocking = false;
            }
        }

        public static void Draw()
        {
            if (!IsOpen) return;

            _titleStyle ??= new GUIStyle(GUI.skin.label)
            { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _labelStyle ??= new GUIStyle(GUI.skin.label)
            { fontSize = 16, normal = { textColor = Color.white } };
            _hintStyle ??= new GUIStyle(GUI.skin.label)
            { fontSize = 13, normal = { textColor = Color.gray } };

            float w = Mathf.Min(560f, Screen.width - 40f);
            float h = Mathf.Min(480f, Screen.height - 40f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");
            GUILayout.BeginArea(new Rect(x + 16, y + 12, w - 32, h - 24));

            GUILayout.Label("Mods", _titleStyle);
            GUILayout.Label("勾选可以启用/禁用 mod;修改后需要重启游戏才会生效。(默认按 F9 打开此面板)", _hintStyle);
            GUILayout.Space(8);

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            if (ModStatus.Mods.Count == 0)
            {
                GUILayout.Label("没有发现任何 mod。把 mod 的 dll 放进 Mods 文件夹后重启游戏。", _labelStyle);
            }

            foreach (var entry in ModStatus.Mods)
            {
                GUILayout.BeginHorizontal();

                bool wasEnabled = entry.State != ModLoadState.Disabled;
                bool nowEnabled = GUILayout.Toggle(wasEnabled, "", GUILayout.Width(24));
                if (nowEnabled != wasEnabled)
                {
                    ModConfig.SetDisabled(entry.Mod.Name, !nowEnabled);
                    entry.PendingRestart = true;
                }

                string statusText = entry.State switch
                {
                    ModLoadState.Loaded => "已加载",
                    ModLoadState.Failed => "加载失败",
                    ModLoadState.Disabled => "已禁用",
                    _ => "等待中"
                };
                if (entry.PendingRestart) statusText += ",需重启生效";

                GUILayout.Label($"{entry.Mod.Name}  v{entry.Mod.Version}  [{statusText}]", _labelStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(8);
            if (GUILayout.Button("关闭", GUILayout.Height(28)))
            {
                IsOpen = false;
            }

            GUILayout.EndArea();
        }
    }
}
