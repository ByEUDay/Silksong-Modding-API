using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::AudioManager")]
    public class patch_AudioManager
    {
        public static extern void orig_PlaySound(string soundName, float volume);

        public static void PlaySound(string soundName, float volume)
        {
            ModHooks.OnSoundPlayed(soundName, volume);
            orig_PlaySound(soundName, volume);
        }

        public static extern void orig_StopSound(string soundName);

        public static void StopSound(string soundName)
        {
            ModHooks.OnSoundStopped(soundName);
            orig_StopSound(soundName);
        }

        public static extern void orig_PlayBGM(string bgmName, float fadeTime);

        public static void PlayBGM(string bgmName, float fadeTime)
        {
            ModHooks.OnBGMChanged(bgmName, fadeTime);
            orig_PlayBGM(bgmName, fadeTime);
        }

        public static extern void orig_FadeInBGM(string bgmName, float fadeTime);

        public static void FadeInBGM(string bgmName, float fadeTime)
        {
            ModHooks.OnBGMFadedIn(bgmName, fadeTime);
            orig_FadeInBGM(bgmName, fadeTime);
        }

        public static extern void orig_FadeOutBGM(string bgmName, float fadeTime);

        public static void FadeOutBGM(string bgmName, float fadeTime)
        {
            ModHooks.OnBGMFadedOut(bgmName, fadeTime);
            orig_FadeOutBGM(bgmName, fadeTime);
        }

        public static extern void orig_SetMasterVolume(float volume);

        public static void SetMasterVolume(float volume)
        {
            ModHooks.OnMasterVolumeChanged(volume);
            orig_SetMasterVolume(volume);
        }

        public static extern void orig_SetMusicVolume(float volume);

        public static void SetMusicVolume(float volume)
        {
            ModHooks.OnMusicVolumeChanged(volume);
            orig_SetMusicVolume(volume);
        }

        public static extern void orig_SetSFXVolume(float volume);

        public static void SetSFXVolume(float volume)
        {
            ModHooks.OnSFXVolumeChanged(volume);
            orig_SetSFXVolume(volume);
        }

        public static extern void orig_SetVoiceVolume(float volume);

        public static void SetVoiceVolume(float volume)
        {
            ModHooks.OnVoiceVolumeChanged(volume);
            orig_SetVoiceVolume(volume);
        }

        public static extern void orig_SetMuted(bool muted);

        public static void SetMuted(bool muted)
        {
            ModHooks.OnAudioMuted(muted);
            orig_SetMuted(muted);
        }
    }
}
