using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::CameraController")]
    public class patch_CameraController : MonoBehaviour
    {
        public extern void orig_Update();

        private void Update()
        {
            Vector3 oldPos = transform.position;
            Quaternion oldRot = transform.rotation;

            orig_Update();

            if (transform.position != oldPos)
            {
                ModHooks.OnCameraPositionChanged(transform.position);
            }
            if (transform.rotation != oldRot)
            {
                ModHooks.OnCameraRotationChanged(transform.rotation);
            }
        }

        public extern void orig_SetZoom(float zoom);

        public void SetZoom(float zoom)
        {
            ModHooks.OnCameraZoomChanged(zoom);
            orig_SetZoom(zoom);
        }

        public extern void orig_SetFollowTarget(Transform target);

        public void SetFollowTarget(Transform target)
        {
            ModHooks.OnCameraFollowTargetChanged(target);
            orig_SetFollowTarget(target);
        }

        public extern void orig_StartShake(float intensity, float duration);

        public void StartShake(float intensity, float duration)
        {
            ModHooks.OnCameraShakeStarted(intensity, duration);
            orig_StartShake(intensity, duration);
        }

        public extern void orig_StopShake();

        public void StopShake()
        {
            ModHooks.OnCameraShakeEnded();
            orig_StopShake();
        }

        public extern void orig_FadeIn(float duration);

        public void FadeIn(float duration)
        {
            ModHooks.OnCameraFadeInStarted(duration);
            orig_FadeIn(duration);
        }

        public extern void orig_FadeOut(float duration);

        public void FadeOut(float duration)
        {
            ModHooks.OnCameraFadeOutStarted(duration);
            orig_FadeOut(duration);
        }

        public extern void orig_SetBounds(Bounds bounds);

        public void SetBounds(Bounds bounds)
        {
            ModHooks.OnCameraBoundsChanged(bounds);
            orig_SetBounds(bounds);
        }

        public extern void orig_LockToTarget(Transform target);

        public void LockToTarget(Transform target)
        {
            ModHooks.OnCameraLockedToTarget(target);
            orig_LockToTarget(target);
        }

        public extern void orig_Unlock();

        public void Unlock()
        {
            ModHooks.OnCameraUnlocked();
            orig_Unlock();
        }
    }
}
