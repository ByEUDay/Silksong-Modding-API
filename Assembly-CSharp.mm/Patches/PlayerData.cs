using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{




    [MonoModPatch("global::PlayerData")]
    public class patch_PlayerData : global::PlayerData
    {
        public extern bool orig_GetBool(string boolName);
        public bool GetBool(string boolName) => ModHooks.OnGetPlayerBool(boolName, orig_GetBool(boolName));

        public extern void orig_SetBool(string boolName, bool value);
        public void SetBool(string boolName, bool value) => orig_SetBool(boolName, ModHooks.OnSetPlayerBool(boolName, value));

        public extern int orig_GetInt(string intName);
        public int GetInt(string intName) => ModHooks.OnGetPlayerInt(intName, orig_GetInt(intName));

        public extern void orig_SetInt(string intName, int value);
        public void SetInt(string intName, int value) => orig_SetInt(intName, ModHooks.OnSetPlayerInt(intName, value));

        public extern float orig_GetFloat(string floatName);
        public float GetFloat(string floatName) => ModHooks.OnGetPlayerFloat(floatName, orig_GetFloat(floatName));

        public extern void orig_SetFloat(string floatName, float value);
        public void SetFloat(string floatName, float value) => orig_SetFloat(floatName, ModHooks.OnSetPlayerFloat(floatName, value));

        public extern string orig_GetString(string stringName);
        public string GetString(string stringName) => ModHooks.OnGetPlayerString(stringName, orig_GetString(stringName));

        public extern void orig_SetString(string stringName, string value);
        public void SetString(string stringName, string value) => orig_SetString(stringName, ModHooks.OnSetPlayerString(stringName, value));

        public extern Vector3 orig_GetVector3(string vectorName);
        public Vector3 GetVector3(string vectorName) => ModHooks.OnGetPlayerVector3(vectorName, orig_GetVector3(vectorName));

        public extern void orig_SetVector3(string vectorName, Vector3 value);
        public void SetVector3(string vectorName, Vector3 value) => orig_SetVector3(vectorName, ModHooks.OnSetPlayerVector3(vectorName, value));


        [MonoModReplace]
        public void IncrementInt(string intName) => SetInt(intName, GetInt(intName) + 1);

        [MonoModReplace]
        public void DecrementInt(string intName) => SetInt(intName, GetInt(intName) - 1);

        [MonoModReplace]
        public void IntAdd(string intName, int amount) => SetInt(intName, GetInt(intName) + amount);


        public extern void orig_SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType);

        public void SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType)
        {
            orig_SetBenchRespawn(spawnMarker, sceneName, spawnType);
            ModHooks.OnBenchRespawnSet(spawnMarker != null ? spawnMarker.name : null, sceneName);
        }

        public extern void orig_SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight);

        public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
        {
            orig_SetBenchRespawn(spawnMarker, sceneName, spawnType, facingRight);
            ModHooks.OnBenchRespawnSet(spawnMarker, sceneName);
        }
    }
}
