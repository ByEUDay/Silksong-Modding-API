using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModExtensionMethods
    {



        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }




        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() != null;
        }




        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }




        public static void DestroyChildrenImmediate(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }




        public static void SetParentKeepPosition(this Transform transform, Transform parent)
        {
            transform.SetParent(parent, true);
        }




        public static void ResetTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }




        public static void CopyFrom(this Transform transform, Transform source)
        {
            transform.position = source.position;
            transform.rotation = source.rotation;
            transform.localScale = source.localScale;
        }




        public static List<Transform> GetAllChildren(this Transform transform)
        {
            var children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            return children;
        }




        public static void SetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
        }




        public static void SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }




        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        }




        public static void SetPositionX(this Transform transform, float x)
        {
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }




        public static void SetPositionY(this Transform transform, float y)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }




        public static void SetPositionZ(this Transform transform, float z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }




        public static void SetLocalScaleX(this Transform transform, float x)
        {
            transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        }




        public static void SetLocalScaleY(this Transform transform, float y)
        {
            transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
        }




        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
        }




        public static void SetLocalRotationX(this Transform transform, float x)
        {
            transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }




        public static void SetLocalRotationY(this Transform transform, float y)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
        }




        public static void SetLocalRotationZ(this Transform transform, float z)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
        }




        public static void LookAt2D(this Transform transform, Vector2 target)
        {
            Vector2 direction = target - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }




        public static void FaceTarget2D(this Transform transform, Vector2 target)
        {
            Vector2 direction = target - (Vector2)transform.position;
            if (direction.x < 0)
            {
                transform.SetLocalScaleX(-Mathf.Abs(transform.localScale.x));
            }
            else
            {
                transform.SetLocalScaleX(Mathf.Abs(transform.localScale.x));
            }
        }




        public static Transform GetRandomChild(this Transform transform)
        {
            if (transform.childCount == 0) return null;
            int index = UnityEngine.Random.Range(0, transform.childCount);
            return transform.GetChild(index);
        }




        public static bool IsInRange(this Transform transform, Transform target, float range)
        {
            return Vector3.Distance(transform.position, target.position) <= range;
        }




        public static float DistanceTo(this Transform transform, Transform target)
        {
            return Vector3.Distance(transform.position, target.position);
        }




        public static float DistanceTo2D(this Transform transform, Transform target)
        {
            return Vector2.Distance(transform.position, target.position);
        }




        public static Vector3 DirectionTo(this Transform transform, Transform target)
        {
            return (target.position - transform.position).normalized;
        }




        public static Vector2 DirectionTo2D(this Transform transform, Transform target)
        {
            return ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
    }




    public static class Vector3Extensions
    {



        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }




        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }




        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }




        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }




        public static Vector3 Perpendicular2D(this Vector3 v)
        {
            return new Vector3(-v.y, v.x, 0);
        }




        public static Vector3 ClampMagnitude(this Vector3 v, float maxLength)
        {
            return Vector3.ClampMagnitude(v, maxLength);
        }




        public static float AngleTo(this Vector3 from, Vector3 to)
        {
            return Vector3.Angle(from, to);
        }




        public static float SignedAngleTo(this Vector3 from, Vector3 to)
        {
            return Vector3.SignedAngle(from, to, Vector3.forward);
        }




        public static Vector3 LerpTo(this Vector3 from, Vector3 to, float t)
        {
            return Vector3.Lerp(from, to, t);
        }




        public static Vector3 SmoothDampTo(this Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime)
        {
            return Vector3.SmoothDamp(current, target, ref velocity, smoothTime);
        }




        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            return Vector3.MoveTowards(current, target, maxDistanceDelta);
        }




        public static Vector3 RotateTowards(this Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
        {
            return Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
        }




        public static Vector3 ProjectOn(this Vector3 vector, Vector3 onNormal)
        {
            return Vector3.Project(vector, onNormal);
        }




        public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(vector, planeNormal);
        }




        public static Vector3 ReflectOn(this Vector3 vector, Vector3 normal)
        {
            return Vector3.Reflect(vector, normal);
        }




        public static Vector3 BounceOn(this Vector3 vector, Vector3 normal)
        {
            return Vector3.Reflect(vector, normal);
        }
    }




    public static class ColorExtensions
    {



        public static Color WithR(this Color c, float r)
        {
            return new Color(r, c.g, c.b, c.a);
        }




        public static Color WithG(this Color c, float g)
        {
            return new Color(c.r, g, c.b, c.a);
        }




        public static Color WithB(this Color c, float b)
        {
            return new Color(c.r, c.g, b, c.a);
        }




        public static Color WithA(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }




        public static string ToHex(this Color c)
        {
            return ModColorUtils.ToHex(c);
        }




        public static Color WithBrightness(this Color c, float brightness)
        {
            return ModColorUtils.WithBrightness(c, brightness);
        }




        public static Color WithSaturation(this Color c, float saturation)
        {
            return ModColorUtils.WithSaturation(c, saturation);
        }




        public static Color Inverted(this Color c)
        {
            return ModColorUtils.Invert(c);
        }




        public static Color Grayscaled(this Color c)
        {
            return ModColorUtils.Grayscale(c);
        }
    }
}
