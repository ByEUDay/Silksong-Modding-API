using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace SilksongPrepatcher
{











    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PrePatcher <Original Assembly-CSharp.dll> <Patched output.dll> [search dir for references]");
                return 1;
            }

            string input = args[0];
            string output = args[1];
            string searchDir = args.Length > 2 ? args[2] : System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(input));

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(searchDir);

            using ModuleDefinition module = ModuleDefinition.ReadModule(input, new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadWrite = false,
                InMemory = true,
            });

            TypeDefinition pd = module.GetType("PlayerData");
            if (pd == null)
            {
                Console.Error.WriteLine("找不到 PlayerData 类型，这不是 Silksong 的 Assembly-CSharp.dll？");
                return 2;
            }

            MethodDefinition Find(string name) =>
                pd.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count >= 1 && !m.IsStatic)
                ?? throw new InvalidOperationException("PlayerData 缺少方法 " + name);

            var getters = new Dictionary<string, MethodDefinition>
            {
                ["System.Boolean"] = Find("GetBool"),
                ["System.Int32"] = Find("GetInt"),
                ["System.Single"] = Find("GetFloat"),
                ["System.String"] = Find("GetString"),
                ["UnityEngine.Vector3"] = Find("GetVector3"),
            };

            var swappedSetters = new Dictionary<string, MethodDefinition>
            {
                ["System.Boolean"] = GenerateSwapped(pd, Find("SetBool")),
                ["System.Int32"] = GenerateSwapped(pd, Find("SetInt")),
                ["System.Single"] = GenerateSwapped(pd, Find("SetFloat")),
                ["System.String"] = GenerateSwapped(pd, Find("SetString")),
                ["UnityEngine.Vector3"] = GenerateSwapped(pd, Find("SetVector3")),
            };


            var skipInPd = new HashSet<string>
            {
                "SetupNewPlayerData", "AddGGPlayerDataOverrides", "SetupExistingPlayerData",
                "OnDeserialized", ".ctor", ".cctor",
            };

            int reads = 0, writes = 0;

            foreach (TypeDefinition type in module.Types.ToList())
            {
                foreach (MethodDefinition method in GetMethodsRecursively(type).ToList())
                {
                    if (!method.HasBody) continue;
                    if (method.DeclaringType == pd && skipInPd.Contains(method.Name)) continue;

                    if (method.DeclaringType == pd && method.Name.EndsWith("SwappedArgs")) continue;

                    method.Body.SimplifyMacros();
                    var il = method.Body.GetILProcessor();

                    bool changed = true;
                    while (changed)
                    {
                        changed = false;
                        foreach (Instruction instr in method.Body.Instructions)
                        {
                            if (instr.Operand is not FieldReference field) continue;
                            if (field.DeclaringType.FullName != pd.FullName) continue;
                            if (instr.OpCode != OpCodes.Ldfld && instr.OpCode != OpCodes.Stfld) continue;

                            FieldDefinition def = field.Resolve();
                            if (def == null || def.IsStatic || def.IsLiteral) continue;

                            string typeName = field.FieldType.FullName;
                            if (!getters.ContainsKey(typeName)) continue;

                            if (instr.OpCode == OpCodes.Ldfld)
                            {

                                il.InsertAfter(instr, Instruction.Create(OpCodes.Callvirt, module.ImportReference(getters[typeName])));
                                instr.OpCode = OpCodes.Ldstr;
                                instr.Operand = field.Name;
                                reads++;
                            }
                            else
                            {

                                il.InsertAfter(instr, Instruction.Create(OpCodes.Callvirt, swappedSetters[typeName]));
                                instr.OpCode = OpCodes.Ldstr;
                                instr.Operand = field.Name;
                                writes++;
                            }

                            changed = true;
                            break;
                        }
                    }

                    method.Body.OptimizeMacros();
                }
            }

            module.Write(output);
            Console.WriteLine($"PrePatcher: 改写了 {reads} 处读取 / {writes} 处写入 -> {output}");
            return 0;
        }

        private static IEnumerable<MethodDefinition> GetMethodsRecursively(TypeDefinition type)
        {
            foreach (var m in type.Methods) yield return m;
            foreach (var nested in type.NestedTypes)
                foreach (var m in GetMethodsRecursively(nested))
                    yield return m;
        }


        private static MethodDefinition GenerateSwapped(TypeDefinition pd, MethodDefinition original)
        {
            var swapped = new MethodDefinition(
                original.Name + "SwappedArgs",
                MethodAttributes.Assembly | MethodAttributes.HideBySig,
                pd.Module.TypeSystem.Void);

            swapped.Parameters.Add(new ParameterDefinition("value", Mono.Cecil.ParameterAttributes.None, original.Parameters[1].ParameterType));
            swapped.Parameters.Add(new ParameterDefinition("name", Mono.Cecil.ParameterAttributes.None, original.Parameters[0].ParameterType));

            var il = swapped.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, original);
            il.Emit(OpCodes.Ret);

            pd.Methods.Add(swapped);
            return swapped;
        }
    }
}
