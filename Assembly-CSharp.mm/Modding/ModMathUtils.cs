using System;
using UnityEngine;

namespace Modding
{



    public static class ModMathUtils
    {



        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Mathf.Clamp01(t);
        }




        public static float InverseLerp(float a, float b, float value)
        {
            if (Mathf.Approximately(a, b)) return 0f;
            return Mathf.Clamp01((value - a) / (b - a));
        }




        public static float SmoothStep(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return a + (b - a) * t;
        }




        public static float SmootherStep(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * t * (t * (t * 6f - 15f) + 10f);
            return a + (b - a) * t;
        }




        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t));
        }




        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t), Lerp(a.z, b.z, t));
        }




        public static Color Lerp(Color a, Color b, float t)
        {
            return new Color(Lerp(a.r, b.r, t), Lerp(a.g, b.g, t), Lerp(a.b, b.b, t), Lerp(a.a, b.a, t));
        }




        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Mathf.DeltaAngle(a, b);
            return a + delta * Mathf.Clamp01(t);
        }




        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }




        public static float Clamp01(float value)
        {
            return Mathf.Clamp01(value);
        }




        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = InverseLerp(fromMin, fromMax, value);
            return Lerp(toMin, toMax, t);
        }




        public static float Distance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }




        public static float Distance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }




        public static float SqrDistance(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude;
        }




        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude;
        }




        public static float Angle(Vector2 from, Vector2 to)
        {
            return Vector2.Angle(from, to);
        }




        public static float SignedAngle(Vector2 from, Vector2 to)
        {
            float angle = Vector2.Angle(from, to);
            float sign = Mathf.Sign(from.x * to.y - from.y * to.x);
            return angle * sign;
        }




        public static float Deg2Rad(float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }




        public static float Rad2Deg(float radians)
        {
            return radians * Mathf.Rad2Deg;
        }




        public static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }




        public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }




        public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }




        public static Vector2 RandomPointInCircle(Vector2 center, float radius)
        {
            return center + UnityEngine.Random.insideUnitCircle * radius;
        }




        public static Vector3 RandomPointInSphere(Vector3 center, float radius)
        {
            return center + UnityEngine.Random.insideUnitSphere * radius;
        }




        public static Vector2 RandomPointInRect(Rect rect)
        {
            return new Vector2(
                UnityEngine.Random.Range(rect.xMin, rect.xMax),
                UnityEngine.Random.Range(rect.yMin, rect.yMax)
            );
        }




        public static Vector3 RandomPointInBox(Bounds bounds)
        {
            return new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }
    }




    public static class ModColorUtils
    {



        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;
            return Color.white;
        }




        public static string ToHex(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }




        public static Color FromHSV(float h, float s, float v)
        {
            return Color.HSVToRGB(h, s, v);
        }




        public static void ToHSV(Color color, out float h, out float s, out float v)
        {
            Color.RGBToHSV(color, out h, out s, out v);
        }




        public static Color WithBrightness(Color color, float brightness)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v = Mathf.Clamp01(v * brightness);
            return Color.HSVToRGB(h, s, v);
        }




        public static Color WithSaturation(Color color, float saturation)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            s = Mathf.Clamp01(saturation);
            return Color.HSVToRGB(h, s, v);
        }




        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }




        public static Color Invert(Color color)
        {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        }




        public static Color Grayscale(Color color)
        {
            float gray = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            return new Color(gray, gray, gray, color.a);
        }




        public static Color Blend(Color a, Color b, float t)
        {
            return ModMathUtils.Lerp(a, b, t);
        }




        public static Color Multiply(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }




        public static Color Screen(Color a, Color b)
        {
            return new Color(
                1f - (1f - a.r) * (1f - b.r),
                1f - (1f - a.g) * (1f - b.g),
                1f - (1f - a.b) * (1f - b.b),
                a.a
            );
        }




        public static Color Overlay(Color a, Color b)
        {
            return new Color(
                a.r < 0.5f ? 2f * a.r * b.r : 1f - 2f * (1f - a.r) * (1f - b.r),
                a.g < 0.5f ? 2f * a.g * b.g : 1f - 2f * (1f - a.g) * (1f - b.g),
                a.b < 0.5f ? 2f * a.b * b.b : 1f - 2f * (1f - a.b) * (1f - b.b),
                a.a
            );
        }
    }
}
