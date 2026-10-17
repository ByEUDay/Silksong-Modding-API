using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::EnvironmentManager")]
    public class patch_EnvironmentManager
    {
        public static extern void orig_LoadScene(string sceneName);

        public static void LoadScene(string sceneName)
        {
            ModHooks.OnSceneLoaded(sceneName);
            orig_LoadScene(sceneName);
        }

        public static extern void orig_UnloadScene(string sceneName);

        public static void UnloadScene(string sceneName)
        {
            ModHooks.OnSceneUnloaded(sceneName);
            orig_UnloadScene(sceneName);
        }

        public static extern void orig_ActivateTrigger(GameObject trigger);

        public static void ActivateTrigger(GameObject trigger)
        {
            ModHooks.OnTriggerActivated(trigger);
            orig_ActivateTrigger(trigger);
        }

        public static extern void orig_DeactivateTrigger(GameObject trigger);

        public static void DeactivateTrigger(GameObject trigger)
        {
            ModHooks.OnTriggerDeactivated(trigger);
            orig_DeactivateTrigger(trigger);
        }

        public static extern void orig_BreakObject(GameObject obj);

        public static void BreakObject(GameObject obj)
        {
            ModHooks.OnBreakableObjectBroken(obj);
            orig_BreakObject(obj);
        }

        public static extern void orig_UseInteractable(GameObject obj);

        public static void UseInteractable(GameObject obj)
        {
            ModHooks.OnInteractableObjectUsed(obj);
            orig_UseInteractable(obj);
        }

        public static extern void orig_UseTeleporter(GameObject teleporter, string targetScene);

        public static void UseTeleporter(GameObject teleporter, string targetScene)
        {
            ModHooks.OnTeleporterUsed(teleporter, targetScene);
            orig_UseTeleporter(teleporter, targetScene);
        }

        public static extern void orig_OpenDoor(GameObject door);

        public static void OpenDoor(GameObject door)
        {
            ModHooks.OnDoorOpened(door);
            orig_OpenDoor(door);
        }

        public static extern void orig_CloseDoor(GameObject door);

        public static void CloseDoor(GameObject door)
        {
            ModHooks.OnDoorClosed(door);
            orig_CloseDoor(door);
        }

        public static extern void orig_ActivateCheckpoint(GameObject checkpoint);

        public static void ActivateCheckpoint(GameObject checkpoint)
        {
            ModHooks.OnCheckpointActivated(checkpoint);
            orig_ActivateCheckpoint(checkpoint);
        }

        public static extern void orig_SetWeather(string weatherType);

        public static void SetWeather(string weatherType)
        {
            ModHooks.OnWeatherChanged(weatherType);
            orig_SetWeather(weatherType);
        }

        public static extern void orig_SetTime(float time);

        public static void SetTime(float time)
        {
            ModHooks.OnTimeChanged(time);
            orig_SetTime(time);
        }

        public static extern void orig_SetLighting(float intensity);

        public static void SetLighting(float intensity)
        {
            ModHooks.OnSceneLightingChanged(intensity);
            orig_SetLighting(intensity);
        }
    }
}
