using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SilksongPrepatcher
{
    /// <summary>
    /// 用法: SilksongPrepatcher.exe "<游戏Managed目录路径>"
    ///
    /// 做的事:
    ///   1. 备份原始 Assembly-CSharp.dll 为 Assembly-CSharp.dll.orig(仅第一次)
    ///   2. 往 GameManager.Awake() 方法开头插入一行:
    ///        SilksongModLoader.Entry.Initialize();
    ///   3. 用一个自定义 Attribute 标记"已打过桩",防止重复运行时二次插入
    ///
    /// 注意:GameManager/Awake 是否真的存在、是不是合适的插桩点,
    /// 必须先用 dnSpy 打开 Assembly-CSharp.dll 确认。这里给的是最常见的
    /// Hollow Knight 系列入口写法,不保证 Silksong 100% 一致。
    /// </summary>
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

            // 首次运行才备份,防止反复打桩的 dll 被当成"原始文件"覆盖备份
            if (!File.Exists(backupPath))
            {
                File.Copy(gameDllPath, backupPath);
                Console.WriteLine($"已备份原始文件到 {backupPath}");
            }

            using var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedDir);

            // 注意:不用 ReadWrite=true 直接改原文件,而是读普通只读流,
            // 改完写到临时文件,最后再替换,避免读写同一个文件句柄冲突导致异常/崩溃
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
            } // using 块结束,文件句柄在这里被释放

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
