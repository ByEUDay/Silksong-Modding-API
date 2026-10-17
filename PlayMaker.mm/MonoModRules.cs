using Mono.Cecil;
using MonoMod.InlineRT;

namespace MonoMod
{

    public static class MonoModRules
    {
        static MonoModRules()
        {
            MonoModRule.Modder.ShouldCleanupAttrib = IsUselessAttrib;
        }

        public static bool IsUselessAttrib(ICustomAttributeProvider holder, TypeReference attribType)
        {
            return (attribType.Namespace.StartsWith("MonoMod") && attribType.Name.StartsWith("MonoMod"))
                   || attribType.Namespace.StartsWith("Modding.Patches");
        }
    }
}
