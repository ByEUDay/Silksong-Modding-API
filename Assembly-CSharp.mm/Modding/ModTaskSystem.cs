using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public class ModStateMachine
    {
        private readonly Dictionary<string, State> _states = new Dictionary<string, State>();
        private State _currentState;
        private string _currentStateName;

        public string CurrentStateName => _currentStateName;
        public State CurrentState => _currentState;




        public void AddState(string name, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            _states[name] = new State
            {
                Name = name,
                OnEnter = onEnter,
                OnUpdate = onUpdate,
                OnExit = onExit
            };
        }




        public void ChangeState(string newStateName)
        {
            if (!_states.ContainsKey(newStateName))
            {
                Logger.LogError($"[ModStateMachine] 状态不存在: {newStateName}");
                return;
            }

            _currentState?.OnExit?.Invoke();
            _currentState = _states[newStateName];
            _currentStateName = newStateName;
            _currentState.OnEnter?.Invoke();
        }




        public void Update()
        {
            _currentState?.OnUpdate?.Invoke();
        }




        public class State
        {
            public string Name { get; set; }
            public Action OnEnter { get; set; }
            public Action OnUpdate { get; set; }
            public Action OnExit { get; set; }
        }
    }




    public class ModTimer
    {
        private float _interval;
        private float _elapsedTime;
        private Action _callback;
        private bool _repeat;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public ModTimer(float interval, Action callback, bool repeat = false)
        {
            _interval = interval;
            _callback = callback;
            _repeat = repeat;
        }




        public void Start()
        {
            _isRunning = true;
            _elapsedTime = 0f;
        }




        public void Stop()
        {
            _isRunning = false;
        }




        public void Reset()
        {
            _elapsedTime = 0f;
        }




        public void Update()
        {
            if (!_isRunning) return;

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _interval)
            {
                try
                {
                    _callback?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError($"[ModTimer] 回调异常: {e}");
                }

                if (_repeat)
                {
                    _elapsedTime = 0f;
                }
                else
                {
                    _isRunning = false;
                }
            }
        }
    }




    public class ModTaskSystem
    {
        private readonly Queue<ModTask> _taskQueue = new Queue<ModTask>();
        private ModTask _currentTask;
        private bool _isProcessing;

        public bool IsProcessing => _isProcessing;




        public void AddTask(ModTask task)
        {
            _taskQueue.Enqueue(task);
            if (!_isProcessing)
            {
                ProcessNextTask();
            }
        }




        private void ProcessNextTask()
        {
            if (_taskQueue.Count == 0)
            {
                _isProcessing = false;
                _currentTask = null;
                return;
            }

            _isProcessing = true;
            _currentTask = _taskQueue.Dequeue();

            try
            {
                _currentTask.Execute(() =>
                {
                    ProcessNextTask();
                });
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModTaskSystem] 任务执行失败: {e}");
                ProcessNextTask();
            }
        }




        public class ModTask
        {
            public string Name { get; set; }
            public Action<Action> Execute { get; set; }
        }
    }




    public static class ModCacheSystem
    {
        private static readonly Dictionary<string, CacheEntry> _cache =
            new Dictionary<string, CacheEntry>();




        public static void Set<T>(string key, T value, float expirationSeconds = -1)
        {
            _cache[key] = new CacheEntry
            {
                Value = value,
                ExpirationTime = expirationSeconds > 0 ? Time.realtimeSinceStartup + expirationSeconds : -1
            };
        }




        public static T Get<T>(string key, T defaultValue = default)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime < 0 || Time.realtimeSinceStartup < entry.ExpirationTime)
                {
                    return (T)entry.Value;
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return defaultValue;
        }




        public static bool Has(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime < 0 || Time.realtimeSinceStartup < entry.ExpirationTime)
                {
                    return true;
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return false;
        }




        public static void Remove(string key)
        {
            _cache.Remove(key);
        }




        public static void Clear()
        {
            _cache.Clear();
        }




        public static void ClearExpired()
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in _cache)
            {
                if (kvp.Value.ExpirationTime > 0 && Time.realtimeSinceStartup >= kvp.Value.ExpirationTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        private class CacheEntry
        {
            public object Value { get; set; }
            public float ExpirationTime { get; set; }
        }
    }
}
