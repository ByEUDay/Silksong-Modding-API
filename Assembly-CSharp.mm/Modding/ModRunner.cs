using System.Collections;
using UnityEngine;

namespace Modding
{

    public static class ModRunner
    {
        private class Runner : MonoBehaviour
        {
            private void Update()
            {
                ModHooks.OnUpdate();
            }
            private void FixedUpdate() => ModHooks.OnFixedUpdate();
            private void LateUpdate() => ModHooks.OnLateUpdate();
            private void OnGUI() => ModHooks.OnOnGUI();
        }

        private static Runner _runner;

        private static Runner Instance
        {
            get
            {
                if (_runner != null) return _runner;
                var go = new GameObject("ModRunner") { hideFlags = HideFlags.HideAndDontSave };
                Object.DontDestroyOnLoad(go);
                _runner = go.AddComponent<Runner>();
                return _runner;
            }
        }


        internal static void Ensure() { var _ = Instance; }

        public static Coroutine Start(IEnumerator routine) => Instance.StartCoroutine(routine);

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null && _runner != null) _runner.StopCoroutine(coroutine);
        }


        public static Coroutine After(float seconds, System.Action action) => Start(Delay(seconds, action));


        public static Coroutine AfterFrames(int frames, System.Action action) => Start(DelayFrames(frames, action));

        private static IEnumerator Delay(float seconds, System.Action action)
        {
            yield return new WaitForSeconds(seconds);
            try { action(); } catch (System.Exception e) { Logger.LogError($"ModRunner.After: {e}"); }
        }

        private static IEnumerator DelayFrames(int frames, System.Action action)
        {
            for (int i = 0; i < frames; i++) yield return null;
            try { action(); } catch (System.Exception e) { Logger.LogError($"ModRunner.AfterFrames: {e}"); }
        }
    }
}
