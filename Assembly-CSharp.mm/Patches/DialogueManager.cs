using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::DialogueManager")]
    public class patch_DialogueManager
    {
        public static extern void orig_StartDialogue(string dialogueId);

        public static void StartDialogue(string dialogueId)
        {
            ModHooks.OnDialogueStarted(dialogueId);
            orig_StartDialogue(dialogueId);
        }

        public static extern void orig_EndDialogue(string dialogueId);

        public static void EndDialogue(string dialogueId)
        {
            ModHooks.OnDialogueEnded(dialogueId);
            orig_EndDialogue(dialogueId);
        }

        public static extern void orig_ShowLine(string dialogueId, int lineIndex, string text);

        public static void ShowLine(string dialogueId, int lineIndex, string text)
        {
            ModHooks.OnDialogueLineShown(dialogueId, lineIndex, text);
            orig_ShowLine(dialogueId, lineIndex, text);
        }

        public static extern void orig_SkipLine(string dialogueId, int lineIndex);

        public static void SkipLine(string dialogueId, int lineIndex)
        {
            ModHooks.OnDialogueLineSkipped(dialogueId, lineIndex);
            orig_SkipLine(dialogueId, lineIndex);
        }

        public static extern void orig_ShowOptions(string dialogueId, string[] options);

        public static void ShowOptions(string dialogueId, string[] options)
        {
            ModHooks.OnDialogueOptionsShown(dialogueId, options);
            orig_ShowOptions(dialogueId, options);
        }

        public static extern void orig_SelectOption(string dialogueId, int optionIndex);

        public static void SelectOption(string dialogueId, int optionIndex)
        {
            ModHooks.OnDialogueOptionSelected(dialogueId, optionIndex);
            orig_SelectOption(dialogueId, optionIndex);
        }

        public static extern void orig_SetText(string dialogueId, string newText);

        public static void SetText(string dialogueId, string newText)
        {
            ModHooks.OnDialogueTextChanged(dialogueId, newText);
            orig_SetText(dialogueId, newText);
        }

        public static extern void orig_PlaySound(string dialogueId, string soundName);

        public static void PlaySound(string dialogueId, string soundName)
        {
            ModHooks.OnDialogueSoundPlayed(dialogueId, soundName);
            orig_PlaySound(dialogueId, soundName);
        }

        public static extern void orig_PlayAnimation(string dialogueId, string animationName);

        public static void PlayAnimation(string dialogueId, string animationName)
        {
            ModHooks.OnDialogueAnimationPlayed(dialogueId, animationName);
            orig_PlayAnimation(dialogueId, animationName);
        }

        public static extern void orig_SelectBranch(string dialogueId, string branchName);

        public static void SelectBranch(string dialogueId, string branchName)
        {
            ModHooks.OnDialogueBranchSelected(dialogueId, branchName);
            orig_SelectBranch(dialogueId, branchName);
        }

        public static extern void orig_SetVariable(string dialogueId, string varName, string varValue);

        public static void SetVariable(string dialogueId, string varName, string varValue)
        {
            ModHooks.OnDialogueVariableSet(dialogueId, varName, varValue);
            orig_SetVariable(dialogueId, varName, varValue);
        }

        public static extern void orig_PauseDialogue(string dialogueId);

        public static void PauseDialogue(string dialogueId)
        {
            ModHooks.OnDialoguePaused(dialogueId);
            orig_PauseDialogue(dialogueId);
        }

        public static extern void orig_ResumeDialogue(string dialogueId);

        public static void ResumeDialogue(string dialogueId)
        {
            ModHooks.OnDialogueResumed(dialogueId);
            orig_ResumeDialogue(dialogueId);
        }

        public static extern void orig_SkipDialogue(string dialogueId);

        public static void SkipDialogue(string dialogueId)
        {
            ModHooks.OnDialogueSkipped(dialogueId);
            orig_SkipDialogue(dialogueId);
        }
    }
}
