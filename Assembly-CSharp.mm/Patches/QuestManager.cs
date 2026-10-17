using System;
using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::QuestManager")]
    public class patch_QuestManager : global::QuestManager
    {
        public static extern void orig_ShowQuestAccepted(FullQuestBase quest, Action afterPrompt);

        public static void ShowQuestAccepted(FullQuestBase quest, Action afterPrompt)
        {
            ModHooks.OnQuestAccepted(quest);
            orig_ShowQuestAccepted(quest, afterPrompt);
        }

        public static extern void orig_ShowQuestCompleted(FullQuestBase quest, Action afterPrompt);

        public static void ShowQuestCompleted(FullQuestBase quest, Action afterPrompt)
        {
            ModHooks.OnQuestCompleted(quest);
            orig_ShowQuestCompleted(quest, afterPrompt);
        }
    }
}
