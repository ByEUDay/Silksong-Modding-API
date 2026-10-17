using UnityEngine;

namespace Modding
{

    public static class UnityExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }


        public static Transform FindChildRecursive(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = child.FindChildRecursive(name);
                if (found != null) return found;
            }
            return null;
        }

        public static GameObject FindChildRecursive(this GameObject parent, string name) =>
            parent.transform.FindChildRecursive(name)?.gameObject;

        public static void SetPosition2D(this Transform t, float x, float y) =>
            t.position = new Vector3(x, y, t.position.z);

        public static void SetPositionX(this Transform t, float x) =>
            t.position = new Vector3(x, t.position.y, t.position.z);

        public static void SetPositionY(this Transform t, float y) =>
            t.position = new Vector3(t.position.x, y, t.position.z);

        public static void SetScaleX(this Transform t, float x) =>
            t.localScale = new Vector3(x, t.localScale.y, t.localScale.z);

        public static void SetScaleY(this Transform t, float y) =>
            t.localScale = new Vector3(t.localScale.x, y, t.localScale.z);

        public static void SetAlpha(this SpriteRenderer sr, float a)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
