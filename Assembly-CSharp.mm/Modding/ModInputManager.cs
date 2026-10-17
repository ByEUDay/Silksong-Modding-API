using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModInputManager
    {
        private static readonly Dictionary<string, KeyCode> KeyBindings =
            new Dictionary<string, KeyCode>();

        private static readonly Dictionary<string, bool> InputLocks =
            new Dictionary<string, bool>();




        public static void SetKeyBinding(string action, KeyCode key)
        {
            KeyBindings[action] = key;
            ModHooks.OnKeyBindingChanged(action, key.ToString());
        }




        public static KeyCode GetKeyBinding(string action, KeyCode defaultKey = KeyCode.None)
        {
            return KeyBindings.TryGetValue(action, out var key) ? key : defaultKey;
        }




        public static bool GetKeyDown(string action)
        {
            if (IsInputLocked()) return false;
            var key = GetKeyBinding(action);
            return key != KeyCode.None && Input.GetKeyDown(key);
        }




        public static bool GetKey(string action)
        {
            if (IsInputLocked()) return false;
            var key = GetKeyBinding(action);
            return key != KeyCode.None && Input.GetKey(key);
        }




        public static bool GetKeyUp(string action)
        {
            if (IsInputLocked()) return false;
            var key = GetKeyBinding(action);
            return key != KeyCode.None && Input.GetKeyUp(key);
        }




        public static void LockInput(string lockId)
        {
            InputLocks[lockId] = true;
            ModHooks.OnInputLockChanged(true);
        }




        public static void UnlockInput(string lockId)
        {
            InputLocks.Remove(lockId);
            ModHooks.OnInputLockChanged(IsInputLocked());
        }




        public static bool IsInputLocked()
        {
            return InputLocks.Count > 0;
        }




        public static void ClearAllBindings()
        {
            KeyBindings.Clear();
        }




        public static void ClearAllLocks()
        {
            InputLocks.Clear();
            ModHooks.OnInputLockChanged(false);
        }
    }




    public static class ModAudioManager
    {
        private static readonly Dictionary<string, AudioSource> AudioSources =
            new Dictionary<string, AudioSource>();

        private static float _masterVolume = 1f;
        private static float _musicVolume = 1f;
        private static float _sfxVolume = 1f;




        public static AudioSource CreateAudioSource(string name, AudioClip clip = null, bool loop = false)
        {
            var go = new GameObject($"AudioSource_{name}");
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = loop;
            source.playOnAwake = false;
            AudioSources[name] = source;
            ModHooks.OnAudioSourceCreated(source);
            return source;
        }




        public static void PlaySound(string name, float volume = 1f)
        {
            if (AudioSources.TryGetValue(name, out var source))
            {
                source.volume = volume * _masterVolume * _sfxVolume;
                source.Play();
                ModHooks.OnSoundPlayed(name, source.volume);
            }
        }




        public static void StopSound(string name)
        {
            if (AudioSources.TryGetValue(name, out var source))
            {
                source.Stop();
                ModHooks.OnSoundStopped(name);
            }
        }




        public static void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            ModHooks.OnMasterVolumeChanged(_masterVolume);
        }




        public static void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            ModHooks.OnMusicVolumeChanged(_musicVolume);
        }




        public static void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ModHooks.OnSFXVolumeChanged(_sfxVolume);
        }




        public static AudioSource GetAudioSource(string name)
        {
            return AudioSources.TryGetValue(name, out var source) ? source : null;
        }




        public static void RemoveAudioSource(string name)
        {
            if (AudioSources.TryGetValue(name, out var source))
            {
                ModHooks.OnAudioSourceDestroyed(source);
                UnityEngine.Object.Destroy(source.gameObject);
                AudioSources.Remove(name);
            }
        }




        public static void ClearAll()
        {
            foreach (var kvp in AudioSources)
            {
                ModHooks.OnAudioSourceDestroyed(kvp.Value);
                UnityEngine.Object.Destroy(kvp.Value.gameObject);
            }
            AudioSources.Clear();
        }
    }




    public static class ModParticleSystemManager
    {
        private static readonly Dictionary<string, ParticleSystem> ParticleSystems =
            new Dictionary<string, ParticleSystem>();




        public static ParticleSystem CreateParticleSystem(string name, Vector3 position, Quaternion rotation)
        {
            var go = new GameObject($"ParticleSystem_{name}");
            go.transform.position = position;
            go.transform.rotation = rotation;
            var ps = go.AddComponent<ParticleSystem>();
            ps.playOnAwake = false;
            ParticleSystems[name] = ps;
            return ps;
        }




        public static void Play(string name)
        {
            if (ParticleSystems.TryGetValue(name, out var ps))
            {
                ps.Play();
            }
        }




        public static void Stop(string name)
        {
            if (ParticleSystems.TryGetValue(name, out var ps))
            {
                ps.Stop();
            }
        }




        public static ParticleSystem GetParticleSystem(string name)
        {
            return ParticleSystems.TryGetValue(name, out var ps) ? ps : null;
        }




        public static void RemoveParticleSystem(string name)
        {
            if (ParticleSystems.TryGetValue(name, out var ps))
            {
                UnityEngine.Object.Destroy(ps.gameObject);
                ParticleSystems.Remove(name);
            }
        }




        public static void ClearAll()
        {
            foreach (var kvp in ParticleSystems)
            {
                UnityEngine.Object.Destroy(kvp.Value.gameObject);
            }
            ParticleSystems.Clear();
        }
    }
}
