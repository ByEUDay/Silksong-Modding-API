using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModAnimationSystem
    {



        public static void PlayAnimation(Animator animator, string stateName, int layer = 0)
        {
            if (animator != null)
            {
                animator.Play(stateName, layer);
                ModHooks.OnAnimationStarted(animator, stateName);
            }
        }




        public static void SetParameter(Animator animator, string paramName, float value)
        {
            if (animator != null)
            {
                animator.SetFloat(paramName, value);
                ModHooks.OnAnimationParameterChanged(animator, paramName, value);
            }
        }




        public static void SetParameter(Animator animator, string paramName, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(paramName, value);
            }
        }




        public static void SetParameter(Animator animator, string paramName, int value)
        {
            if (animator != null)
            {
                animator.SetInteger(paramName, value);
            }
        }




        public static void SetSpeed(Animator animator, float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
                ModHooks.OnAnimationSpeedChanged(animator, speed);
            }
        }




        public static void SetLayerWeight(Animator animator, int layerIndex, float weight)
        {
            if (animator != null)
            {
                animator.SetLayerWeight(layerIndex, weight);
                ModHooks.OnAnimationLayerWeightChanged(animator, layerIndex, weight);
            }
        }




        public static AnimatorStateInfo GetCurrentState(Animator animator, int layer = 0)
        {
            return animator != null ? animator.GetCurrentAnimatorStateInfo(layer) : default;
        }




        public static bool IsInState(Animator animator, string stateName, int layer = 0)
        {
            if (animator == null) return false;
            var state = animator.GetCurrentAnimatorStateInfo(layer);
            return state.fullPathHash == Animator.StringToHash(stateName);
        }




        public static float GetProgress(Animator animator, int layer = 0)
        {
            if (animator == null) return 0f;
            var state = animator.GetCurrentAnimatorStateInfo(layer);
            return state.normalizedTime;
        }
    }




    public static class ModCameraSystem
    {
        private static Camera _mainCamera;




        public static Camera GetMainCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }




        public static void SetPosition(Camera camera, Vector3 position)
        {
            if (camera != null)
            {
                camera.transform.position = position;
                ModHooks.OnCameraPositionChanged(position);
            }
        }




        public static void SetRotation(Camera camera, Quaternion rotation)
        {
            if (camera != null)
            {
                camera.transform.rotation = rotation;
                ModHooks.OnCameraRotationChanged(rotation);
            }
        }




        public static void SetZoom(Camera camera, float zoom)
        {
            if (camera != null)
            {
                camera.orthographicSize = zoom;
                ModHooks.OnCameraZoomChanged(zoom);
            }
        }




        public static void SetFollowTarget(Transform target)
        {
            ModHooks.OnCameraFollowTargetChanged(target);
        }




        public static void Shake(float intensity, float duration)
        {
            ModHooks.OnCameraShakeStarted(intensity, duration);
        }




        public static void StopShake()
        {
            ModHooks.OnCameraShakeEnded();
        }




        public static void FadeIn(float duration)
        {
            ModHooks.OnCameraFadeInStarted(duration);
        }




        public static void FadeOut(float duration)
        {
            ModHooks.OnCameraFadeOutStarted(duration);
        }




        public static Vector3 WorldToScreenPoint(Vector3 worldPosition)
        {
            var camera = GetMainCamera();
            return camera != null ? camera.WorldToScreenPoint(worldPosition) : Vector3.zero;
        }




        public static Vector3 ScreenToWorldPoint(Vector3 screenPosition)
        {
            var camera = GetMainCamera();
            return camera != null ? camera.ScreenToWorldPoint(screenPosition) : Vector3.zero;
        }




        public static Vector3 WorldToViewportPoint(Vector3 worldPosition)
        {
            var camera = GetMainCamera();
            return camera != null ? camera.WorldToViewportPoint(worldPosition) : Vector3.zero;
        }




        public static Vector3 ViewportToWorldPoint(Vector3 viewportPosition)
        {
            var camera = GetMainCamera();
            return camera != null ? camera.ViewportToWorldPoint(viewportPosition) : Vector3.zero;
        }
    }




    public static class ModTimeSystem
    {
        private static float _timeScale = 1f;
        private static bool _isPaused;




        public static void SetTimeScale(float scale)
        {
            _timeScale = Mathf.Clamp(scale, 0f, 10f);
            Time.timeScale = _timeScale;
        }




        public static float GetTimeScale()
        {
            return _timeScale;
        }




        public static void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
        }




        public static void Resume()
        {
            _isPaused = false;
            Time.timeScale = _timeScale;
        }




        public static bool IsPaused()
        {
            return _isPaused;
        }




        public static float GetCurrentTime()
        {
            return Time.time;
        }




        public static float GetUnscaledTime()
        {
            return Time.unscaledTime;
        }




        public static float GetDeltaTime()
        {
            return Time.deltaTime;
        }




        public static float GetUnscaledDeltaTime()
        {
            return Time.unscaledDeltaTime;
        }




        public static int GetFrameCount()
        {
            return Time.frameCount;
        }




        public static float GetFixedDeltaTime()
        {
            return Time.fixedDeltaTime;
        }




        public static void SetFixedDeltaTime(float deltaTime)
        {
            Time.fixedDeltaTime = deltaTime;
        }
    }
}
