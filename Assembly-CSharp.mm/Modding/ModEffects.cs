using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModPhysicsEffects
    {



        public static void AddExplosion(Vector3 position, float radius, float force, float upwardsModifier = 0f)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (collider.transform.position - position).normalized;
                    float distance = Vector3.Distance(position, collider.transform.position);
                    float forceMultiplier = 1f - (distance / radius);
                    rb.AddForce(direction * force * forceMultiplier + Vector3.up * upwardsModifier, ForceMode.Impulse);
                }
            }
        }




        public static void AddGravity(Vector3 position, float radius, float force)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (position - collider.transform.position).normalized;
                    rb.AddForce(direction * force, ForceMode.Force);
                }
            }
        }




        public static void AddRepulsion(Vector3 position, float radius, float force)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (collider.transform.position - position).normalized;
                    rb.AddForce(direction * force, ForceMode.Force);
                }
            }
        }




        public static void AddWind(Vector3 position, float radius, Vector3 windDirection, float force)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb != null)
                {
                    rb.AddForce(windDirection.normalized * force, ForceMode.Force);
                }
            }
        }




        public static void AddVortex(Vector3 position, float radius, float force, float upwardsForce = 0f)
        {
            var colliders = Physics.OverlapSphere(position, radius);
            foreach (var collider in colliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (position - collider.transform.position).normalized;
                    Vector3 tangent = Vector3.Cross(direction, Vector3.up).normalized;
                    rb.AddForce(tangent * force + Vector3.up * upwardsForce, ForceMode.Force);
                }
            }
        }




        public static void AddShockwave(Vector3 position, float radius, float force, float duration = 0.5f)
        {
            ModRunner.Start(ShockwaveCoroutine(position, radius, force, duration));
        }

        private static System.Collections.IEnumerator ShockwaveCoroutine(Vector3 position, float radius, float force, float duration)
        {
            float elapsed = 0f;
            float currentRadius = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                currentRadius = radius * t;
                float currentForce = force * (1f - t);

                var colliders = Physics.OverlapSphere(position, currentRadius);
                foreach (var collider in colliders)
                {
                    var rb = collider.attachedRigidbody;
                    if (rb != null)
                    {
                        float distance = Vector3.Distance(position, collider.transform.position);
                        if (distance > currentRadius - 1f && distance < currentRadius)
                        {
                            Vector3 direction = (collider.transform.position - position).normalized;
                            rb.AddForce(direction * currentForce, ForceMode.Impulse);
                        }
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }




    public static class ModVisualEffects
    {



        public static void CreateFlash(Vector3 position, float duration = 0.2f, Color? color = null)
        {
            var go = new GameObject("FlashEffect");
            go.transform.position = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color ?? Color.white;
            light.intensity = 3f;
            light.range = 10f;

            ModRunner.Start(FlashCoroutine(go, light, duration));
        }

        private static System.Collections.IEnumerator FlashCoroutine(GameObject go, Light light, float duration)
        {
            float elapsed = 0f;
            float startIntensity = light.intensity;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                light.intensity = startIntensity * (1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            UnityEngine.Object.Destroy(go);
        }




        public static void ScreenShake(float intensity, float duration)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                ModRunner.Start(ScreenShakeCoroutine(camera, intensity, duration));
            }
        }

        private static System.Collections.IEnumerator ScreenShakeCoroutine(Camera camera, float intensity, float duration)
        {
            Vector3 originalPosition = camera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float currentIntensity = intensity * (1f - t);

                float offsetX = UnityEngine.Random.Range(-currentIntensity, currentIntensity);
                float offsetY = UnityEngine.Random.Range(-currentIntensity, currentIntensity);

                camera.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            camera.transform.localPosition = originalPosition;
        }




        public static void FadeIn(float duration, Action onComplete = null)
        {
            ModHooks.OnScreenFadeIn(duration);
            ModRunner.Start(FadeInCoroutine(duration, onComplete));
        }

        private static System.Collections.IEnumerator FadeInCoroutine(float duration, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            onComplete?.Invoke();
        }




        public static void FadeOut(float duration, Action onComplete = null)
        {
            ModHooks.OnScreenFadeOut(duration);
            ModRunner.Start(FadeOutCoroutine(duration, onComplete));
        }

        private static System.Collections.IEnumerator FadeOutCoroutine(float duration, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            onComplete?.Invoke();
        }




        public static ParticleSystem CreateParticleEffect(Vector3 position, string particleName = "Default")
        {
            var go = new GameObject($"ParticleEffect_{particleName}");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.maxParticles = 100;

            var emission = ps.emission;
            emission.rateOverTime = 10;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            ps.Play();

            ModRunner.Start(DestroyAfterDelay(go, main.duration + main.startLifetime.constantMax));

            return ps;
        }

        private static System.Collections.IEnumerator DestroyAfterDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            UnityEngine.Object.Destroy(go);
        }
    }




    public static class ModAudioEffects
    {



        public static void PlayWithEcho(AudioSource source, float delay = 0.3f, float decay = 0.5f, int repeat = 3)
        {
            if (source == null || source.clip == null) return;

            for (int i = 1; i <= repeat; i++)
            {
                float volume = Mathf.Pow(decay, i);
                float delayTime = delay * i;

                ModRunner.Start(DelayedPlay(source, delayTime, volume));
            }
        }

        private static System.Collections.IEnumerator DelayedPlay(AudioSource source, float delay, float volume)
        {
            yield return new WaitForSeconds(delay);

            var go = new GameObject("EchoAudio");
            go.transform.position = source.transform.position;

            var echoSource = go.AddComponent<AudioSource>();
            echoSource.clip = source.clip;
            echoSource.volume = source.volume * volume;
            echoSource.Play();

            yield return new WaitForSeconds(source.clip.length);
            UnityEngine.Object.Destroy(go);
        }




        public static void PitchBend(AudioSource source, float startPitch, float endPitch, float duration)
        {
            if (source == null) return;
            ModRunner.Start(PitchBendCoroutine(source, startPitch, endPitch, duration));
        }

        private static System.Collections.IEnumerator PitchBendCoroutine(AudioSource source, float startPitch, float endPitch, float duration)
        {
            float elapsed = 0f;
            source.pitch = startPitch;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                source.pitch = Mathf.Lerp(startPitch, endPitch, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            source.pitch = endPitch;
        }




        public static void FadeIn(AudioSource source, float duration, float targetVolume = 1f)
        {
            if (source == null) return;
            ModRunner.Start(VolumeFadeCoroutine(source, 0f, targetVolume, duration));
        }




        public static void FadeOut(AudioSource source, float duration)
        {
            if (source == null) return;
            ModRunner.Start(VolumeFadeCoroutine(source, source.volume, 0f, duration));
        }

        private static System.Collections.IEnumerator VolumeFadeCoroutine(AudioSource source, float startVolume, float endVolume, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                source.volume = Mathf.Lerp(startVolume, endVolume, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            source.volume = endVolume;
        }
    }
}
