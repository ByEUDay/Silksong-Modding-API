using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModDebugUtils
    {
        private static bool _debugMode;
        private static readonly List<string> DebugLog = new List<string>();
        private static readonly int MaxLogEntries = 1000;




        public static bool DebugMode
        {
            get => _debugMode;
            set
            {
                _debugMode = value;
                Logger.Log($"[ModDebugUtils] 调试模式: {(value ? "启用" : "禁用")}");
            }
        }




        public static void LogDebug(string message)
        {
            if (!_debugMode) return;

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            DebugLog.Add(logEntry);

            if (DebugLog.Count > MaxLogEntries)
            {
                DebugLog.RemoveAt(0);
            }

            Logger.Log($"[DEBUG] {message}");
        }




        public static void LogWarning(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] [WARN] {message}";
            DebugLog.Add(logEntry);

            if (DebugLog.Count > MaxLogEntries)
            {
                DebugLog.RemoveAt(0);
            }

            Logger.LogWarn($"[DEBUG] {message}");
        }




        public static void LogError(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] [ERROR] {message}";
            DebugLog.Add(logEntry);

            if (DebugLog.Count > MaxLogEntries)
            {
                DebugLog.RemoveAt(0);
            }

            Logger.LogError($"[DEBUG] {message}");
        }




        public static IReadOnlyList<string> GetDebugLog()
        {
            return DebugLog.AsReadOnly();
        }




        public static void ClearDebugLog()
        {
            DebugLog.Clear();
        }




        public static void InspectGameObject(GameObject obj)
        {
            if (obj == null)
            {
                LogDebug("GameObject: null");
                return;
            }

            LogDebug($"GameObject: {obj.name}");
            LogDebug($"  Position: {obj.transform.position}");
            LogDebug($"  Rotation: {obj.transform.rotation.eulerAngles}");
            LogDebug($"  Scale: {obj.transform.localScale}");
            LogDebug($"  Active: {obj.activeSelf}");
            LogDebug($"  Layer: {LayerMask.LayerToName(obj.layer)}");
            LogDebug($"  Tag: {obj.tag}");

            var components = obj.GetComponents<Component>();
            LogDebug($"  Components ({components.Length}):");
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    LogDebug($"    - {comp.GetType().Name}");
                }
            }
        }




        public static void InspectTransform(Transform t)
        {
            if (t == null)
            {
                LogDebug("Transform: null");
                return;
            }

            LogDebug($"Transform: {t.name}");
            LogDebug($"  Local Position: {t.localPosition}");
            LogDebug($"  Local Rotation: {t.localRotation.eulerAngles}");
            LogDebug($"  Local Scale: {t.localScale}");
            LogDebug($"  World Position: {t.position}");
            LogDebug($"  World Rotation: {t.rotation.eulerAngles}");
            LogDebug($"  Child Count: {t.childCount}");
            LogDebug($"  Parent: {(t.parent != null ? t.parent.name : "null")}");
        }




        public static void InspectRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                LogDebug("Rigidbody: null");
                return;
            }

            LogDebug($"Rigidbody: {rb.gameObject.name}");
            LogDebug($"  Mass: {rb.mass}");
            LogDebug($"  Velocity: {rb.velocity}");
            LogDebug($"  Angular Velocity: {rb.angularVelocity}");
            LogDebug($"  Is Kinematic: {rb.isKinematic}");
            LogDebug($"  Use Gravity: {rb.useGravity}");
            LogDebug($"  Drag: {rb.drag}");
            LogDebug($"  Angular Drag: {rb.angularDrag}");
        }




        public static void InspectCollider(Collider collider)
        {
            if (collider == null)
            {
                LogDebug("Collider: null");
                return;
            }

            LogDebug($"Collider: {collider.gameObject.name}");
            LogDebug($"  Type: {collider.GetType().Name}");
            LogDebug($"  Is Trigger: {collider.isTrigger}");
            LogDebug($"  Bounds: {collider.bounds}");
            LogDebug($"  Center: {collider.bounds.center}");
            LogDebug($"  Size: {collider.bounds.size}");
        }




        public static void InspectScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            LogDebug($"Scene: {scene.name}");
            LogDebug($"  Build Index: {scene.buildIndex}");
            LogDebug($"  Is Loaded: {scene.isLoaded}");
            LogDebug($"  Root Count: {scene.rootCount}");

            var rootObjects = scene.GetRootGameObjects();
            LogDebug($"  Root Objects ({rootObjects.Length}):");
            foreach (var obj in rootObjects)
            {
                LogDebug($"    - {obj.name}");
            }
        }




        public static void InspectMods()
        {
            var mods = ModLoader.LoadedMods;
            LogDebug($"Loaded Mods ({mods.Count}):");
            foreach (var mod in mods)
            {
                LogDebug($"  - {mod.GetName()} v{mod.GetVersion()}");
                LogDebug($"    Enabled: {ModLoader.IsEnabled(mod)}");
                LogDebug($"    Type: {mod.GetType().FullName}");
            }
        }




        public static class PerformanceMonitor
        {
            private static float _lastFrameTime;
            private static float _fps;
            private static int _frameCount;
            private static float _elapsedTime;

            public static float FPS => _fps;
            public static float FrameTime => _lastFrameTime * 1000f;

            public static void Update()
            {
                _lastFrameTime = Time.deltaTime;
                _frameCount++;
                _elapsedTime += Time.deltaTime;

                if (_elapsedTime >= 1f)
                {
                    _fps = _frameCount / _elapsedTime;
                    _frameCount = 0;
                    _elapsedTime = 0f;
                }
            }

            public static void LogPerformance()
            {
                LogDebug($"FPS: {_fps:F2}, Frame Time: {FrameTime:F2}ms");
            }
        }
    }
}
