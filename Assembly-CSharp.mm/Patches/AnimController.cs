using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::AnimController")]
    public class patch_AnimController : MonoBehaviour
    {
        public extern void orig_OnAnimationStateChange(string stateName);

        private void OnAnimationStateChange(string stateName)
        {
            ModHooks.OnAnimationStateChanged(GetComponent<Animator>(), stateName);
            orig_OnAnimationStateChange(stateName);
        }

        public extern void orig_OnAnimationEvent(string eventName);

        private void OnAnimationEvent(string eventName)
        {
            ModHooks.OnAnimationEvent(GetComponent<Animator>(), eventName);
            orig_OnAnimationEvent(eventName);
        }

        public extern void orig_PlayAnimation(string animationName);

        public void PlayAnimation(string animationName)
        {
            ModHooks.OnAnimationStarted(GetComponent<Animator>(), animationName);
            orig_PlayAnimation(animationName);
        }

        public extern void orig_StopAnimation(string animationName);

        public void StopAnimation(string animationName)
        {
            ModHooks.OnAnimationCompleted(GetComponent<Animator>(), animationName);
            orig_StopAnimation(animationName);
        }

        public extern void orig_SetAnimationSpeed(float speed);

        public void SetAnimationSpeed(float speed)
        {
            ModHooks.OnAnimationSpeedChanged(GetComponent<Animator>(), speed);
            orig_SetAnimationSpeed(speed);
        }

        public extern void orig_SetAnimationParameter(string paramName, float value);

        public void SetAnimationParameter(string paramName, float value)
        {
            ModHooks.OnAnimationParameterChanged(GetComponent<Animator>(), paramName, value);
            orig_SetAnimationParameter(paramName, value);
        }

        public extern void orig_SetLayerWeight(int layerIndex, float weight);

        public void SetLayerWeight(int layerIndex, float weight)
        {
            ModHooks.OnAnimationLayerWeightChanged(GetComponent<Animator>(), layerIndex, weight);
            orig_SetLayerWeight(layerIndex, weight);
        }
    }
}
