using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModCommandSystem
    {
        private static readonly Dictionary<string, CommandInfo> Commands =
            new Dictionary<string, CommandInfo>();




        public static void RegisterCommand(string name, string description, Action<string[]> callback)
        {
            Commands[name] = new CommandInfo
            {
                Name = name,
                Description = description,
                Callback = callback
            };
        }




        public static void UnregisterCommand(string name)
        {
            Commands.Remove(name);
        }




        public static void ExecuteCommand(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine)) return;

            string[] parts = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string commandName = parts[0].ToLower();
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, args.Length);

            if (Commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    command.Callback(args);
                    Logger.Log($"[Command] 执行命令: {commandLine}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"[Command] 命令执行失败: {e}");
                }
            }
            else
            {
                Logger.LogWarn($"[Command] 未知命令: {commandName}");
            }
        }




        public static IEnumerable<CommandInfo> GetAllCommands()
        {
            return Commands.Values;
        }




        public static void ClearAll()
        {
            Commands.Clear();
        }




        public class CommandInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Action<string[]> Callback { get; set; }
        }




        public static void RegisterDefaultCommands()
        {
            RegisterCommand("help", "显示所有命令", (args) =>
            {
                Logger.Log("可用命令:");
                foreach (var cmd in Commands.Values)
                {
                    Logger.Log($"  {cmd.Name}: {cmd.Description}");
                }
            });

            RegisterCommand("mods", "显示所有已加载的 Mod", (args) =>
            {
                var mods = ModLoader.LoadedMods;
                Logger.Log($"已加载 {mods.Count} 个 Mod:");
                foreach (var mod in mods)
                {
                    Logger.Log($"  {mod.GetName()} v{mod.GetVersion()}");
                }
            });

            RegisterCommand("version", "显示 API 版本", (args) =>
            {
                Logger.Log($"SilksongModdingAPI v{ModLoader.ApiVersion}");
            });

            RegisterCommand("debug", "切换调试模式", (args) =>
            {
                ModDebugUtils.DebugMode = !ModDebugUtils.DebugMode;
                Logger.Log($"调试模式: {(ModDebugUtils.DebugMode ? "启用" : "禁用")}");
            });

            RegisterCommand("inspect", "检查 GameObject", (args) =>
            {
                if (args.Length > 0)
                {
                    var obj = GameObject.Find(args[0]);
                    if (obj != null)
                    {
                        ModDebugUtils.InspectGameObject(obj);
                    }
                    else
                    {
                        Logger.LogWarn($"找不到 GameObject: {args[0]}");
                    }
                }
                else
                {
                    Logger.LogWarn("用法: inspect <GameObject名称>");
                }
            });

            RegisterCommand("scene", "显示当前场景信息", (args) =>
            {
                ModDebugUtils.InspectScene();
            });

            RegisterCommand("performance", "显示性能信息", (args) =>
            {
                ModDebugUtils.PerformanceMonitor.LogPerformance();
            });

            RegisterCommand("profiler", "显示性能分析结果", (args) =>
            {
                ModProfiler.LogAll();
            });

            RegisterCommand("clear", "清除调试日志", (args) =>
            {
                ModDebugUtils.ClearDebugLog();
                Logger.Log("调试日志已清除");
            });
        }
    }
}
