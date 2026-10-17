using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modding
{



    public static class ModPermissionManager
    {
        private static readonly Dictionary<string, HashSet<Permission>> Permissions = new Dictionary<string, HashSet<Permission>>();
        private static bool _permissionCheckEnabled = false;

        public static void SetPermissionCheckEnabled(bool enabled)
        {
            _permissionCheckEnabled = enabled;
            Logger.Log($"[Permission] 权限检查: {(enabled ? "启用" : "禁用")}");
        }

        public static bool IsPermissionCheckEnabled() => _permissionCheckEnabled;

        public static void GrantPermission(string modName, Permission permission)
        {
            if (!Permissions.ContainsKey(modName))
                Permissions[modName] = new HashSet<Permission>();
            Permissions[modName].Add(permission);
            Logger.Log($"[Permission] 授权 {modName}: {permission}");
        }

        public static void RevokePermission(string modName, Permission permission)
        {
            if (Permissions.ContainsKey(modName))
            {
                Permissions[modName].Remove(permission);
                Logger.Log($"[Permission] 撤销 {modName}: {permission}");
            }
        }

        public static bool HasPermission(string modName, Permission permission)
        {
            if (!_permissionCheckEnabled) return true;
            if (!Permissions.ContainsKey(modName)) return false;
            return Permissions[modName].Contains(permission);
        }

        public static IEnumerable<Permission> GetPermissions(string modName)
        {
            return Permissions.TryGetValue(modName, out var perms) ? perms : Enumerable.Empty<Permission>();
        }

        public static void GrantAllPermissions(string modName)
        {
            foreach (Permission perm in Enum.GetValues(typeof(Permission)))
                GrantPermission(modName, perm);
        }

        public static void ClearPermissions(string modName)
        {
            if (Permissions.ContainsKey(modName))
            {
                Permissions[modName].Clear();
                Logger.Log($"[Permission] 清除 {modName} 的所有权限");
            }
        }

        public static bool CheckAndWarn(string modName, Permission permission, string action)
        {
            if (!HasPermission(modName, permission))
            {
                Logger.LogWarn($"[Permission] {modName} 尝试执行未授权操作: {action} (需要 {permission})");
                return false;
            }
            return true;
        }
    }

    public enum Permission
    {
        ReadSaveData,
        WriteSaveData,
        ModifyPlayer,
        ModifyEnemies,
        ModifyUI,
        ModifyAudio,
        ModifyCamera,
        ModifyPhysics,
        ModifyInput,
        AccessNetwork,
        AccessFileSystem,
        ExecuteCommands,
        DebugMode,
        AdminMode
    }




    public static class ModSandbox
    {
        private static readonly Dictionary<string, SandboxConfig> Configs = new Dictionary<string, SandboxConfig>();

        public static void SetConfig(string modName, SandboxConfig config)
        {
            Configs[modName] = config;
            Logger.Log($"[Sandbox] 设置沙箱配置: {modName}");
        }

        public static SandboxConfig GetConfig(string modName)
        {
            return Configs.TryGetValue(modName, out var config) ? config : SandboxConfig.Default;
        }

        public static bool CanAccessGameObject(string modName, GameObject go)
        {
            var config = GetConfig(modName);
            if (!config.RestrictGameObjectAccess) return true;

            if (config.AllowedTags != null && config.AllowedTags.Length > 0)
            {
                if (!config.AllowedTags.Contains(go.tag))
                    return false;
            }

            if (config.AllowedLayers != null && config.AllowedLayers.Length > 0)
            {
                if (!config.AllowedLayers.Contains(go.layer))
                    return false;
            }

            return true;
        }

        public static bool CanModifyComponent(string modName, Component component)
        {
            var config = GetConfig(modName);
            if (!config.RestrictComponentModification) return true;

            var type = component.GetType();
            if (config.BlockedComponentTypes != null)
            {
                if (config.BlockedComponentTypes.Any(t => t.IsAssignableFrom(type)))
                    return false;
            }

            return true;
        }

        public static bool CanInstantiatePrefab(string modName, GameObject prefab)
        {
            var config = GetConfig(modName);
            return config.AllowPrefabInstantiation;
        }

        public static bool CanDestroyObject(string modName, GameObject go)
        {
            var config = GetConfig(modName);
            if (!config.AllowObjectDestruction) return false;

            if (config.ProtectedTags != null && config.ProtectedTags.Contains(go.tag))
                return false;

            return true;
        }

        public static bool CanLoadScene(string modName, string sceneName)
        {
            var config = GetConfig(modName);
            if (!config.AllowSceneLoading) return false;

            if (config.BlockedScenes != null && config.BlockedScenes.Contains(sceneName))
                return false;

            return true;
        }

        public static bool CanAccessNetwork(string modName)
        {
            var config = GetConfig(modName);
            return config.AllowNetworkAccess;
        }

        public static bool CanAccessFileSystem(string modName)
        {
            var config = GetConfig(modName);
            return config.AllowFileSystemAccess;
        }
    }

    public class SandboxConfig
    {
        public static readonly SandboxConfig Default = new SandboxConfig
        {
            RestrictGameObjectAccess = false,
            RestrictComponentModification = false,
            AllowPrefabInstantiation = true,
            AllowObjectDestruction = true,
            AllowSceneLoading = true,
            AllowNetworkAccess = true,
            AllowFileSystemAccess = true
        };

        public static readonly SandboxConfig Strict = new SandboxConfig
        {
            RestrictGameObjectAccess = true,
            RestrictComponentModification = true,
            AllowPrefabInstantiation = false,
            AllowObjectDestruction = false,
            AllowSceneLoading = false,
            AllowNetworkAccess = false,
            AllowFileSystemAccess = false
        };

        public bool RestrictGameObjectAccess { get; set; }
        public bool RestrictComponentModification { get; set; }
        public bool AllowPrefabInstantiation { get; set; }
        public bool AllowObjectDestruction { get; set; }
        public bool AllowSceneLoading { get; set; }
        public bool AllowNetworkAccess { get; set; }
        public bool AllowFileSystemAccess { get; set; }

        public string[] AllowedTags { get; set; }
        public int[] AllowedLayers { get; set; }
        public Type[] BlockedComponentTypes { get; set; }
        public string[] ProtectedTags { get; set; }
        public string[] BlockedScenes { get; set; }
    }
}
