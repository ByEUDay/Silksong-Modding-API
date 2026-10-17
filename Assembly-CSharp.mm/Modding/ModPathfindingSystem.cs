using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModPathfindingSystem
    {



        public static List<Vector3> CalculateStraightPath(Vector3 start, Vector3 end)
        {
            var path = new List<Vector3> { start, end };
            return path;
        }




        public static List<Vector3> CalculateBezierPath(Vector3 start, Vector3 control, Vector3 end, int segments = 20)
        {
            var path = new List<Vector3>();
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 point = ModMathUtils.Bezier(start, control, end, t);
                path.Add(point);
            }
            return path;
        }




        public static List<Vector3> CalculateCubicBezierPath(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, int segments = 30)
        {
            var path = new List<Vector3>();
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 point = ModMathUtils.CubicBezier(start, control1, control2, end, t);
                path.Add(point);
            }
            return path;
        }




        public static float CalculatePathLength(List<Vector3> path)
        {
            if (path == null || path.Count < 2) return 0f;

            float length = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                length += Vector3.Distance(path[i - 1], path[i]);
            }
            return length;
        }




        public static Vector3 GetPointOnPath(List<Vector3> path, float t)
        {
            if (path == null || path.Count == 0) return Vector3.zero;
            if (path.Count == 1) return path[0];

            t = Mathf.Clamp01(t);
            float totalLength = CalculatePathLength(path);
            float targetDistance = t * totalLength;

            float accumulatedDistance = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                float segmentLength = Vector3.Distance(path[i - 1], path[i]);
                if (accumulatedDistance + segmentLength >= targetDistance)
                {
                    float segmentT = (targetDistance - accumulatedDistance) / segmentLength;
                    return Vector3.Lerp(path[i - 1], path[i], segmentT);
                }
                accumulatedDistance += segmentLength;
            }

            return path[path.Count - 1];
        }
    }




    public static class ModGenerationSystem
    {
        private static int _seed;
        private static System.Random _random;




        public static void SetSeed(int seed)
        {
            _seed = seed;
            _random = new System.Random(seed);
        }




        public static int RandomInt(int min, int max)
        {
            if (_random == null) _random = new System.Random();
            return _random.Next(min, max + 1);
        }




        public static float RandomFloat(float min, float max)
        {
            if (_random == null) _random = new System.Random();
            return (float)(_random.NextDouble() * (max - min) + min);
        }




        public static bool RandomBool()
        {
            return RandomInt(0, 1) == 1;
        }




        public static Vector2 RandomVector2(float min, float max)
        {
            return new Vector2(RandomFloat(min, max), RandomFloat(min, max));
        }




        public static Vector3 RandomVector3(float min, float max)
        {
            return new Vector3(RandomFloat(min, max), RandomFloat(min, max), RandomFloat(min, max));
        }




        public static float PerlinNoise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }




        public static float[,] GenerateNoiseMap(int width, int height, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
        {
            var noiseMap = new float[width, height];

            if (scale <= 0) scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + offset.x;
                        float sampleY = (y - halfHeight) / scale * frequency + offset.y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }




    public static class ModPoolSystem
    {
        private static readonly Dictionary<string, ModObjectPool> Pools =
            new Dictionary<string, ModObjectPool>();




        public static ModObjectPool CreatePool(string name, GameObject prefab, int initialSize = 10, int maxSize = 100)
        {
            var pool = new ModObjectPool(prefab, null, initialSize, maxSize);
            Pools[name] = pool;
            return pool;
        }




        public static ModObjectPool GetPool(string name)
        {
            return Pools.TryGetValue(name, out var pool) ? pool : null;
        }




        public static GameObject Get(string poolName)
        {
            var pool = GetPool(poolName);
            return pool?.Get();
        }




        public static void Return(string poolName, GameObject obj)
        {
            var pool = GetPool(poolName);
            pool?.Return(obj);
        }




        public static void RemovePool(string name)
        {
            if (Pools.TryGetValue(name, out var pool))
            {
                pool.Clear();
                Pools.Remove(name);
            }
        }




        public static void ClearAll()
        {
            foreach (var pool in Pools.Values)
            {
                pool.Clear();
            }
            Pools.Clear();
        }
    }
}
