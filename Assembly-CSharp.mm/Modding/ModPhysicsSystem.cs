using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{



    public static class ModPhysicsSystem
    {



        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity)
        {
            bool result = Physics.Raycast(origin, direction, out hit, maxDistance);
            if (result)
            {
                ModHooks.OnRaycastHit(hit);
            }
            return result;
        }




        public static RaycastHit2D Raycast2D(Vector2 origin, Vector2 direction, float maxDistance = Mathf.Infinity)
        {
            var hit = Physics2D.Raycast(origin, direction, maxDistance);
            if (hit.collider != null)
            {
                ModHooks.OnRaycast2DHit(hit);
            }
            return hit;
        }




        public static Collider[] OverlapSphere(Vector3 position, float radius)
        {
            return Physics.OverlapSphere(position, radius);
        }




        public static Collider2D[] OverlapCircle(Vector2 position, float radius)
        {
            return Physics2D.OverlapCircleAll(position, radius);
        }




        public static Collider[] OverlapBox(Vector3 center, Vector3 size, Quaternion orientation)
        {
            return Physics.OverlapBox(center, size, orientation);
        }




        public static Collider2D[] OverlapBox(Vector2 center, Vector2 size, float angle)
        {
            return Physics2D.OverlapBoxAll(center, size, angle);
        }




        public static void SetGravity(Vector3 gravity)
        {
            Physics.gravity = gravity;
            ModHooks.OnGravityChanged(gravity);
        }




        public static void SetGravity2D(Vector2 gravity)
        {
            Physics2D.gravity = gravity;
            ModHooks.OnGravity2DChanged(gravity);
        }




        public static Vector3 GetGravity()
        {
            return Physics.gravity;
        }




        public static Vector2 GetGravity2D()
        {
            return Physics2D.gravity;
        }
    }




    public static class ModRenderSystem
    {



        public static void SetLayer(GameObject obj, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                obj.layer = layer;
            }
        }




        public static void SetLayerRecursive(GameObject obj, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                SetLayerRecursiveInternal(obj, layer);
            }
        }

        private static void SetLayerRecursiveInternal(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursiveInternal(child.gameObject, layer);
            }
        }




        public static void SetSortingLayer(SpriteRenderer renderer, string layerName)
        {
            renderer.sortingLayerName = layerName;
        }




        public static void SetSortingOrder(SpriteRenderer renderer, int order)
        {
            renderer.sortingOrder = order;
        }




        public static void SetMaterial(Renderer renderer, Material material)
        {
            renderer.material = material;
        }




        public static void SetShaderProperty(Renderer renderer, string propertyName, float value)
        {
            renderer.material.SetFloat(propertyName, value);
        }




        public static void SetShaderProperty(Renderer renderer, string propertyName, Color value)
        {
            renderer.material.SetColor(propertyName, value);
        }




        public static void SetShaderProperty(Renderer renderer, string propertyName, Vector4 value)
        {
            renderer.material.SetVector(propertyName, value);
        }




        public static void SetShaderProperty(Renderer renderer, string propertyName, Texture value)
        {
            renderer.material.SetTexture(propertyName, value);
        }
    }




    public static class ModUISystem
    {



        public static Text CreateText(Transform parent, string text, int fontSize = 14, Color? color = null)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color ?? Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.alignment = TextAnchor.MiddleCenter;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return textComp;
        }




        public static Image CreateImage(Transform parent, Color color)
        {
            var go = new GameObject("Image");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return image;
        }




        public static Button CreateButton(Transform parent, string text, Action onClick)
        {
            var go = new GameObject("Button");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f);
            var button = go.AddComponent<Button>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textComp = textGo.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 18;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.alignment = TextAnchor.MiddleCenter;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            button.onClick.AddListener(() => onClick?.Invoke());

            return button;
        }




        public static void SetVisible(GameObject obj, bool visible)
        {
            if (obj != null)
            {
                obj.SetActive(visible);
            }
        }




        public static void SetCanvasSortingOrder(Canvas canvas, int order)
        {
            if (canvas != null)
            {
                canvas.sortingOrder = order;
            }
        }
    }
}
