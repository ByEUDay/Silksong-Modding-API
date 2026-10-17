using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modding
{



    public static class ModPerformanceMonitor
    {
        private static readonly Dictionary<string, ModPerformanceStats> Stats = new Dictionary<string, ModPerformanceStats>();
        private static bool _monitoringEnabled = false;

        public static void SetMonitoringEnabled(bool enabled)
        {
            _monitoringEnabled = enabled;
            Logger.Log($"[Performance] 性能监控: {(enabled ? "启用" : "禁用")}");
        }

        public static bool IsMonitoringEnabled() => _monitoringEnabled;

        public static ModPerformanceStats GetStats(string modName)
        {
            if (!Stats.ContainsKey(modName))
                Stats[modName] = new ModPerformanceStats(modName);
            return Stats[modName];
        }

        public static IEnumerable<ModPerformanceStats> GetAllStats() => Stats.Values;

        public static void RecordUpdate(string modName, float elapsedMs)
        {
            if (!_monitoringEnabled) return;
            var stats = GetStats(modName);
            stats.RecordUpdate(elapsedMs);
        }

        public static void RecordHook(string modName, string hookName, float elapsedMs)
        {
            if (!_monitoringEnabled) return;
            var stats = GetStats(modName);
            stats.RecordHook(hookName, elapsedMs);
        }

        public static void RecordAllocation(string modName, long bytes)
        {
            if (!_monitoringEnabled) return;
            var stats = GetStats(modName);
            stats.RecordAllocation(bytes);
        }

        public static void ResetStats(string modName)
        {
            if (Stats.ContainsKey(modName))
                Stats[modName].Reset();
        }

        public static void ResetAllStats()
        {
            foreach (var stats in Stats.Values)
                stats.Reset();
        }

        public static void LogStats(string modName)
        {
            if (!Stats.TryGetValue(modName, out var stats))
            {
                Logger.Log($"[Performance] {modName}: 无统计数据");
                return;
            }

            Logger.Log($"=== {modName} 性能统计 ===");
            Logger.Log($"  Update: 平均 {stats.AverageUpdateMs:F3}ms, 最大 {stats.MaxUpdateMs:F3}ms");
            Logger.Log($"  Hook 调用: {stats.TotalHookCalls} 次, 平均 {stats.AverageHookMs:F3}ms");
            Logger.Log($"  内存分配: {stats.TotalAllocations / 1024.0:F2} KB");
            Logger.Log($"  GC 收集: {stats.GCCollections} 次");
        }

        public static void LogAllStats()
        {
            Logger.Log("=== 所有 Mod 性能统计 ===");
            foreach (var stats in Stats.Values)
                LogStats(stats.ModName);
        }
    }

    public class ModPerformanceStats
    {
        public string ModName { get; }

        private readonly List<float> _updateTimes = new List<float>();
        private readonly List<float> _hookTimes = new List<float>();
        private long _totalAllocations;
        private int _gcCollections;

        public ModPerformanceStats(string modName)
        {
            ModName = modName;
        }

        public void RecordUpdate(float elapsedMs)
        {
            _updateTimes.Add(elapsedMs);
            if (_updateTimes.Count > 1000)
                _updateTimes.RemoveAt(0);
        }

        public void RecordHook(string hookName, float elapsedMs)
        {
            _hookTimes.Add(elapsedMs);
            if (_hookTimes.Count > 10000)
                _hookTimes.RemoveAt(0);
        }

        public void RecordAllocation(long bytes)
        {
            _totalAllocations += bytes;
        }

        public void RecordGCCollection()
        {
            _gcCollections++;
        }

        public float AverageUpdateMs => _updateTimes.Count > 0 ? _updateTimes.Average() : 0;
        public float MaxUpdateMs => _updateTimes.Count > 0 ? _updateTimes.Max() : 0;
        public float MinUpdateMs => _updateTimes.Count > 0 ? _updateTimes.Min() : 0;

        public float AverageHookMs => _hookTimes.Count > 0 ? _hookTimes.Average() : 0;
        public float MaxHookMs => _hookTimes.Count > 0 ? _hookTimes.Max() : 0;
        public int TotalHookCalls => _hookTimes.Count;

        public long TotalAllocations => _totalAllocations;
        public int GCCollections => _gcCollections;

        public void Reset()
        {
            _updateTimes.Clear();
            _hookTimes.Clear();
            _totalAllocations = 0;
            _gcCollections = 0;
        }
    }




    public static class ModMemoryTracker
    {
        private static readonly Dictionary<string, List<TrackedObject>> TrackedObjects = new Dictionary<string, List<TrackedObject>>();

        public static void TrackObject(string modName, UnityEngine.Object obj, string label = null)
        {
            if (obj == null) return;

            if (!TrackedObjects.ContainsKey(modName))
                TrackedObjects[modName] = new List<TrackedObject>();

            TrackedObjects[modName].Add(new TrackedObject
            {
                Object = obj,
                Label = label ?? obj.name,
                CreatedAt = DateTime.Now,
                Type = obj.GetType().Name
            });
        }

        public static void UntrackObject(string modName, UnityEngine.Object obj)
        {
            if (TrackedObjects.ContainsKey(modName))
                TrackedObjects[modName].RemoveAll(t => t.Object == obj);
        }

        public static IEnumerable<TrackedObject> GetTrackedObjects(string modName)
        {
            return TrackedObjects.TryGetValue(modName, out var objects) ? objects : Enumerable.Empty<TrackedObject>();
        }

        public static void CleanupDestroyedObjects(string modName)
        {
            if (TrackedObjects.ContainsKey(modName))
                TrackedObjects[modName].RemoveAll(t => t.Object == null);
        }

        public static void CleanupAllDestroyed()
        {
            foreach (var kvp in TrackedObjects)
                kvp.Value.RemoveAll(t => t.Object == null);
        }

        public static int GetTrackedCount(string modName)
        {
            return TrackedObjects.TryGetValue(modName, out var objects) ? objects.Count : 0;
        }

        public static void LogTrackedObjects(string modName)
        {
            if (!TrackedObjects.TryGetValue(modName, out var objects))
            {
                Logger.Log($"[Memory] {modName}: 无跟踪对象");
                return;
            }

            Logger.Log($"=== {modName} 跟踪对象 ({objects.Count}) ===");
            foreach (var obj in objects)
            {
                if (obj.Object != null)
                    Logger.Log($"  - {obj.Label} ({obj.Type}) - 创建: {obj.CreatedAt:HH:mm:ss}");
            }
        }
    }

    public class TrackedObject
    {
        public UnityEngine.Object Object { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
