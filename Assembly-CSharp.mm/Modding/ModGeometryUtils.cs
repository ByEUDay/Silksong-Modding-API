using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModGeometryUtils
    {



        public static float DistanceToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            if (lineLength == 0) return Vector3.Distance(point, lineStart);

            Vector3 projection = Vector3.Project(point - lineStart, lineDirection);
            Vector3 closestPoint = lineStart + projection;

            if (projection.magnitude > lineLength)
            {
                closestPoint = lineEnd;
            }
            else if (projection.magnitude < 0)
            {
                closestPoint = lineStart;
            }

            return Vector3.Distance(point, closestPoint);
        }




        public static float DistanceToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return Vector3.Dot(planeNormal, point - planePoint);
        }




        public static float DistanceToSphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            return Mathf.Max(0, Vector3.Distance(point, sphereCenter) - sphereRadius);
        }




        public static float DistanceToBox(Vector3 point, Bounds bounds)
        {
            Vector3 closestPoint = bounds.ClosestPoint(point);
            return Vector3.Distance(point, closestPoint);
        }




        public static bool IsPointInSphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            return Vector3.Distance(point, sphereCenter) <= sphereRadius;
        }




        public static bool IsPointInBox(Vector3 point, Bounds bounds)
        {
            return bounds.Contains(point);
        }




        public static bool IsPointInCylinder(Vector3 point, Vector3 cylinderCenter, float cylinderRadius, float cylinderHeight)
        {
            float horizontalDistance = Vector2.Distance(
                new Vector2(point.x, point.z),
                new Vector2(cylinderCenter.x, cylinderCenter.z)
            );
            float verticalDistance = Mathf.Abs(point.y - cylinderCenter.y);

            return horizontalDistance <= cylinderRadius && verticalDistance <= cylinderHeight / 2f;
        }




        public static bool IsPointInCone(Vector3 point, Vector3 coneCenter, Vector3 coneDirection, float coneAngle, float coneHeight)
        {
            Vector3 toPoint = point - coneCenter;
            float distance = Vector3.Dot(toPoint, coneDirection.normalized);

            if (distance < 0 || distance > coneHeight) return false;

            float radiusAtDistance = Mathf.Tan(coneAngle * Mathf.Deg2Rad / 2f) * distance;
            Vector3 projectedPoint = coneCenter + coneDirection.normalized * distance;
            float horizontalDistance = Vector3.Distance(point, projectedPoint);

            return horizontalDistance <= radiusAtDistance;
        }




        public static Vector3? LineIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
        {
            Vector3 direction1 = line1End - line1Start;
            Vector3 direction2 = line2End - line2Start;

            Vector3 cross = Vector3.Cross(direction1, direction2);
            float crossMagnitude = cross.magnitude;

            if (crossMagnitude < 0.0001f) return null;

            Vector3 startDiff = line2Start - line1Start;
            float t = Vector3.Dot(Vector3.Cross(startDiff, direction2), cross) / (crossMagnitude * crossMagnitude);

            if (t < 0 || t > 1) return null;

            return line1Start + direction1 * t;
        }




        public static Vector2? LineIntersection2D(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
            float denominator = (line1End.x - line1Start.x) * (line2End.y - line2Start.y) -
                              (line1End.y - line1Start.y) * (line2End.x - line2Start.x);

            if (Mathf.Abs(denominator) < 0.0001f) return null;

            float t = ((line2Start.x - line1Start.x) * (line2End.y - line2Start.y) -
                      (line2Start.y - line1Start.y) * (line2End.x - line2Start.x)) / denominator;

            float u = -((line1End.x - line1Start.x) * (line2Start.y - line1Start.y) -
                       (line1End.y - line1Start.y) * (line2Start.x - line1Start.x)) / denominator;

            if (t < 0 || t > 1 || u < 0 || u > 1) return null;

            return new Vector2(
                line1Start.x + t * (line1End.x - line1Start.x),
                line1Start.y + t * (line1End.y - line1Start.y)
            );
        }




        public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
        }




        public static float PolygonArea2D(List<Vector2> vertices)
        {
            if (vertices == null || vertices.Count < 3) return 0f;

            float area = 0f;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 current = vertices[i];
                Vector2 next = vertices[(i + 1) % vertices.Count];
                area += current.x * next.y - next.x * current.y;
            }
            return Mathf.Abs(area) * 0.5f;
        }




        public static List<Vector2> ConvexHull2D(List<Vector2> points)
        {
            if (points == null || points.Count < 3) return points;

            points.Sort((a, b) => a.x.CompareTo(b.x));

            var hull = new List<Vector2>();


            foreach (var point in points)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(point);
            }


            int lowerSize = hull.Count;
            for (int i = points.Count - 2; i >= 0; i--)
            {
                var point = points[i];
                while (hull.Count > lowerSize && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(point);
            }

            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }




        public static Vector3 SnapToGrid(Vector3 point, float gridSize)
        {
            return new Vector3(
                Mathf.Round(point.x / gridSize) * gridSize,
                Mathf.Round(point.y / gridSize) * gridSize,
                Mathf.Round(point.z / gridSize) * gridSize
            );
        }




        public static Vector3 SphericalToCartesian(float radius, float theta, float phi)
        {
            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = radius * Mathf.Cos(phi);
            float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
            return new Vector3(x, y, z);
        }




        public static void CartesianToSpherical(Vector3 point, out float radius, out float theta, out float phi)
        {
            radius = point.magnitude;
            theta = Mathf.Atan2(point.z, point.x);
            phi = Mathf.Acos(point.y / radius);
        }




        public static Vector3 CylindricalToCartesian(float radius, float theta, float height)
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            return new Vector3(x, height, z);
        }




        public static void CartesianToCylindrical(Vector3 point, out float radius, out float theta, out float height)
        {
            radius = Mathf.Sqrt(point.x * point.x + point.z * point.z);
            theta = Mathf.Atan2(point.z, point.x);
            height = point.y;
        }
    }




    public static class ModRandomUtils
    {
        private static System.Random _random = new System.Random();




        public static void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }




        public static int Range(int min, int max)
        {
            return _random.Next(min, max + 1);
        }




        public static float Range(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }




        public static bool Bool()
        {
            return _random.Next(2) == 1;
        }




        public static bool Bool(float probability)
        {
            return _random.NextDouble() < probability;
        }




        public static int Sign()
        {
            return _random.Next(2) == 0 ? -1 : 1;
        }




        public static Vector2 UnitVector2D()
        {
            float angle = Range(0f, Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }




        public static Vector3 UnitVector3D()
        {
            return UnityEngine.Random.onUnitSphere;
        }




        public static Color Color()
        {
            return new Color(Range(0f, 1f), Range(0f, 1f), Range(0f, 1f), 1f);
        }




        public static Color ColorWithAlpha()
        {
            return new Color(Range(0f, 1f), Range(0f, 1f), Range(0f, 1f), Range(0f, 1f));
        }




        public static T Choice<T>(List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Range(0, list.Count - 1)];
        }




        public static T Choice<T>(T[] array)
        {
            if (array == null || array.Length == 0) return default;
            return array[Range(0, array.Length - 1)];
        }




        public static List<T> Choices<T>(List<T> list, int count)
        {
            if (list == null || list.Count == 0) return new List<T>();

            var result = new List<T>();
            var available = new List<T>(list);

            for (int i = 0; i < Mathf.Min(count, available.Count); i++)
            {
                int index = Range(0, available.Count - 1);
                result.Add(available[index]);
                available.RemoveAt(index);
            }

            return result;
        }




        public static void Shuffle<T>(List<T> list)
        {
            if (list == null) return;

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(0, i);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }




        public static T WeightedChoice<T>(Dictionary<T, float> weightedItems)
        {
            if (weightedItems == null || weightedItems.Count == 0) return default;

            float totalWeight = 0f;
            foreach (var kvp in weightedItems)
            {
                totalWeight += kvp.Value;
            }

            float randomValue = Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var kvp in weightedItems)
            {
                cumulativeWeight += kvp.Value;
                if (randomValue < cumulativeWeight)
                {
                    return kvp.Key;
                }
            }

            return default;
        }




        public static string String(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[Range(0, chars.Length - 1)];
            }
            return new string(result);
        }




        public static string UUID()
        {
            return Guid.NewGuid().ToString();
        }




        public static float Gaussian(float mean, float standardDeviation)
        {
            float u1 = 1f - (float)_random.NextDouble();
            float u2 = 1f - (float)_random.NextDouble();
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            return mean + standardDeviation * randStdNormal;
        }




        public static int Poisson(float lambda)
        {
            float L = Mathf.Exp(-lambda);
            int k = 0;
            float p = 1f;

            do
            {
                k++;
                p *= (float)_random.NextDouble();
            } while (p > L);

            return k - 1;
        }
    }
}
