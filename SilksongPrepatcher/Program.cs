using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SilksongPrepatcher
{
    internal static class Program
    {
        private const string MarkerAttributeName = "PatchedMarkerAttribute";
        private const string TargetTypeName = "GameManager";
        private const string TargetMethodName = "Awake";

        private static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("=== Prepatcher 出现未处理异常,以下是详细信息 ===");
                Console.WriteLine(e);
                Console.WriteLine("=== 请把上面这段完整内容发给开发者排查 ===");
                return 1;
            }
        }
        private static int Run(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("用法: SilksongPrepatcher <游戏 Managed 目录路径> [Loader dll 路径]");
                return 1;
            }
            var managedDir = args[0];
            var loaderDllPath = args.Length > 1
                ? args[1]
                : Path.Combine(AppContext.BaseDirectory, "SilksongModLoader.dll");
            var gameDllPath = Path.Combine(managedDir, "Assembly-CSharp.dll");
            var backupPath = gameDllPath + ".orig";
            if (!File.Exists(gameDllPath))
            {
                Console.WriteLine($"找不到 {gameDllPath},请确认路径是否为游戏的 *_Data/Managed 目录。");
                return 1;
            }
            if (!File.Exists(loaderDllPath))
            {
                Console.WriteLine($"找不到 SilksongModLoader.dll: {loaderDllPath},请先编译 Loader 工程。");
                return 1;
            }
            if (!File.Exists(backupPath))
            {
                File.Copy(gameDllPath, backupPath);
                Console.WriteLine($"已备份原始文件到 {backupPath}");
            }
            using var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedDir);
            var readParams = new ReaderParameters { AssemblyResolver = resolver };
            string tempPath = gameDllPath + ".patched.tmp";
            using (var gameAsm = AssemblyDefinition.ReadAssembly(gameDllPath, readParams))
            {
                if (IsAlreadyPatched(gameAsm))
                {
                    Console.WriteLine("检测到已经打过桩,跳过(如需重新打桩,先用 .orig 备份还原)。");
                    return 0;
                }
                using var loaderAsm = AssemblyDefinition.ReadAssembly(loaderDllPath);
                var gameManagerType = gameAsm.MainModule.Types.FirstOrDefault(t => t.Name == TargetTypeName);
                if (gameManagerType == null)
                {
                    Console.WriteLine($"没找到类型 {TargetTypeName},请用 dnSpy 核对真实的入口类名并修改 Program.cs 里的 TargetTypeName。");
                    return 1;
                }
                var awakeMethod = gameManagerType.Methods.FirstOrDefault(m => m.Name == TargetMethodName && m.Parameters.Count == 0);
                if (awakeMethod == null)
                {
                    Console.WriteLine($"没找到 {TargetTypeName}.{TargetMethodName}(),请核对方法名/签名后修改 Program.cs。");
                    return 1;
                }
                var entryType = loaderAsm.MainModule.Types.First(t => t.Name == "Entry");
                var initializeMethod = entryType.Methods.First(m => m.Name == "Initialize" && m.Parameters.Count == 0);
                var initializeRef = gameAsm.MainModule.ImportReference(initializeMethod);
                var il = awakeMethod.Body.GetILProcessor();
                var firstInstruction = awakeMethod.Body.Instructions.First();
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, initializeRef));
                MarkAsPatched(gameAsm, loaderAsm);
                gameAsm.Write(tempPath);
            }
            File.Delete(gameDllPath);
            File.Move(tempPath, gameDllPath);
            Console.WriteLine($"打桩完成: {TargetTypeName}.{TargetMethodName}() 现在会调用 SilksongModLoader.Entry.Initialize()。");
            Console.WriteLine("请把 SilksongModLoader.dll 及其依赖(MonoMod.RuntimeDetour 等)一起复制到 Managed 目录。");
            return 0;
        }
        private static bool IsAlreadyPatched(AssemblyDefinition asm) =>
            asm.CustomAttributes.Any(a => a.AttributeType.Name == MarkerAttributeName);
        private static void MarkAsPatched(AssemblyDefinition gameAsm, AssemblyDefinition loaderAsm)
        {
            var markerType = loaderAsm.MainModule.Types.First(t => t.Name == MarkerAttributeName);
            var ctor = markerType.Methods.First(m => m.IsConstructor && m.Parameters.Count == 0);
            var ctorRef = gameAsm.MainModule.ImportReference(ctor);
            gameAsm.CustomAttributes.Add(new CustomAttribute(ctorRef));
        }
    }
}
