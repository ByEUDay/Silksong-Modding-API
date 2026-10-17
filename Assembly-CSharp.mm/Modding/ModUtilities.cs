using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public class ModObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();
        private readonly Transform _parent;
        private readonly int _maxSize;

        public ModObjectPool(GameObject prefab, Transform parent = null, int initialSize = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;

            var poolObj = new GameObject($"ObjectPool_{prefab.name}");
            poolObj.SetActive(false);
            if (_parent != null)
                poolObj.transform.SetParent(_parent);
            UnityEngine.Object.DontDestroyOnLoad(poolObj);

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateObject();
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public GameObject Get()
        {
            GameObject obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateObject();
            }
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
        }

        private GameObject CreateObject()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab);
            obj.name = _prefab.name;
            return obj;
        }
    }




    public static class ModCoroutineExtensions
    {



        public static IEnumerator WaitUntil(Func<bool> condition, float timeout = -1f)
        {
            float elapsed = 0f;
            while (!condition())
            {
                if (timeout > 0)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed >= timeout)
                    {
                        Logger.LogWarn("[ModCoroutineExtensions] WaitUntil 超时");
                        yield break;
                    }
                }
                yield return null;
            }
        }




        public static IEnumerator WaitWhile(Func<bool> condition, float timeout = -1f)
        {
            float elapsed = 0f;
            while (condition())
            {
                if (timeout > 0)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed >= timeout)
                    {
                        Logger.LogWarn("[ModCoroutineExtensions] WaitWhile 超时");
                        yield break;
                    }
                }
                yield return null;
            }
        }




        public static IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
                yield return null;
        }




        public static IEnumerator WaitUntilActive(GameObject obj, float timeout = -1f)
        {
            yield return WaitUntil(() => obj != null && obj.activeSelf, timeout);
        }




        public static IEnumerator WaitUntilInactive(GameObject obj, float timeout = -1f)
        {
            yield return WaitUntil(() => obj == null || !obj.activeSelf, timeout);
        }




        public static IEnumerator WaitUntilComponent<T>(GameObject obj, float timeout = -1f) where T : Component
        {
            yield return WaitUntil(() => obj != null && obj.GetComponent<T>() != null, timeout);
        }




        public static IEnumerator Sequence(params Action[] actions)
        {
            foreach (var action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Logger.LogError($"[ModCoroutineExtensions] Sequence 异常: {e}");
                }
                yield return null;
            }
        }




        public static IEnumerator Parallel(params IEnumerator[] coroutines)
        {
            var running = new List<IEnumerator>(coroutines);
            while (running.Count > 0)
            {
                for (int i = running.Count - 1; i >= 0; i--)
                {
                    if (!running[i].MoveNext())
                    {
                        running.RemoveAt(i);
                    }
                }
                yield return null;
            }
        }
    }
}
