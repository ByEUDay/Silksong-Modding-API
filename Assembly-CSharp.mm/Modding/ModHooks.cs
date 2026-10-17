using System;
using GlobalEnums;
#if !NO_LANGUAGEHOOKS
using TeamCherry.Localization;
#endif
using UnityEngine;

namespace Modding
{



    public enum HeroAbility
    {
        Cast, Bind, Interact, OpenInventory, Jump, DoubleJump, Dash, HarpoonDash,
        Sprint, SuperJump, Attack, TakeDamage, TakeDamageIgnoreInvuln,
    }





    public static partial class ModHooks
    {
        #region 生命周期


        public static event Action ApplicationQuitHook;


        public static event Func<string, string> BeforeSceneLoadHook;


        public static event Action<string, string> ActiveSceneChangedHook;

        #endregion

        #region 存档

        public static event Action NewGameHook;

        public static event Action<int> SavegameLoadHook;

        public static event Action<int> SavegameSaveHook;
        public static event Action<int> SavegameClearHook;
        public static event Action<int> AfterSavegameClearHook;

        #endregion

        #region PlayerData

        public static event Func<string, bool, bool> GetPlayerBoolHook;
        public static event Func<string, bool, bool> SetPlayerBoolHook;
        public static event Func<string, int, int> GetPlayerIntHook;
        public static event Func<string, int, int> SetPlayerIntHook;
        public static event Func<string, float, float> GetPlayerFloatHook;
        public static event Func<string, float, float> SetPlayerFloatHook;
        public static event Func<string, string, string> GetPlayerStringHook;
        public static event Func<string, string, string> SetPlayerStringHook;
        public static event Func<string, Vector3, Vector3> GetPlayerVector3Hook;
        public static event Func<string, Vector3, Vector3> SetPlayerVector3Hook;

        #endregion

        #region Hero / 战斗

        public static event Action HeroUpdateHook;
        public static event Action<AttackDirection> AttackHook;
        public static event Action BeforePlayerDeadHook;
        public static event Action AfterPlayerDeadHook;


        public static event Func<HazardType, int, int> TakeDamageHook;
        public static event Action<HazardType, int> AfterTakeDamageHook;


        public static event Func<HealthManager, HitInstance, HitInstance> HealthManagerTakeDamageHook;

        #endregion

        #region 游戏状态 / 场景


        public static event Action<GameState> GameStateChangedHook;


        public static event Action<string> SceneEnteredHook;

        #endregion

        #region 主角扩展


        public static event Action HeroStartHook;
        public static event Action HeroJumpHook;
        public static event Action HeroDoubleJumpHook;
        public static event Action HeroDashHook;


        public static event Func<int, int> AddHealthHook;

        public static event Func<int, int> TakeHealthHook;

        public static event Func<int, int> AddSilkHook;

        public static event Func<int, int> TakeSilkHook;

        #endregion

        #region 货币 / 敌人




        public static event Func<CurrencyType, int, int> CurrencyChangeHook;


        public static event Action<HealthManager> EnemyStartHook;


        public static event Action<HealthManager> EnemyDeathHook;

        #endregion

        #region 每帧

        public static event Action UpdateHook;
        public static event Action FixedUpdateHook;
        public static event Action LateUpdateHook;

        public static event Action OnGUIHook;

        #endregion

        #region 游戏自带事件的转发（无需补丁，来自 NativeEventBridge）

        public static event Action SceneInitHook;
        public static event Action NextSceneWillActivateHook;
        public static event Action UnloadingLevelHook;
        public static event Action BeforeFinishedSceneTransitionHook;
        public static event Action FinishedSceneTransitionHook;
        public static event Action SavePersistentObjectsHook;
        public static event Action ResetSemiPersistentObjectsHook;
        public static event Action BossLoadedHook;
        public static event Action RefreshLanguageTextHook;
        public static event Action<bool> GamePausedChangedHook;
        public static event Action<SceneLoad> SceneTransitionBeganHook;

        public static event Action<HeroController> HeroInstanceSetHook;
        public static event Action<bool> PreHeroInPositionHook;
        public static event Action<bool> HeroInPositionHook;
        public static event Action<bool> HeroInPositionDelayedHook;
        public static event Action HeroTakenDamageHook;
        public static event Action<HeroController.DamageInfo> HeroTakenDamageExtraHook;
        public static event Action HeroDeathHook;
        public static event Action HeroHazardDeathHook;
        public static event Action HeroHazardRespawnHook;
        public static event Action HeroLeavingSceneHook;

        public static event Action<AttackToolBinding> BoundAttackToolUpdatedHook;
        public static event Action<AttackToolBinding> BoundAttackToolUsedHook;
        public static event Action<AttackToolBinding> BoundAttackToolFailedHook;

        public static event Action EquippedToolsChangedHook;

        #endregion

        #region 补丁提供的新钩子





        public static event Func<HeroAbility, bool, bool> HeroAbilityCheckHook;

        public static event Action HeroBounceHook;
        public static event Action HeroControlRelinquishedHook;
        public static event Action<bool> HeroControlRegainedHook;
        public static event Action<Transform> HeroRespawnHook;

        public static event Func<int, int> AddMaxHealthHook;


        public static event Func<HealthManager, HitInstance, HitInstance> HealthManagerHitHook;


        public static event Action<string> CrestEquippedHook;

        public static event Action<ToolItem> ToolUnlockedHook;

        public static event Action<FullQuestBase> QuestAcceptedHook;
        public static event Action<FullQuestBase> QuestCompletedHook;


        public static event Action<string, string> BenchRespawnSetHook;


        public static event Action<bool> ReturnToMainMenuHook;

        #endregion

#if !NO_LANGUAGEHOOKS
        #region 本地化


        public static event Func<string, string, string, string> LanguageGetHook
        {
            add { LanguageHooks.GetHook += value; }
            remove { LanguageHooks.GetHook -= value; }
        }


        public static event Func<string, string, bool, bool> LanguageHasHook
        {
            add { LanguageHooks.HasHook += value; }
            remove { LanguageHooks.HasHook -= value; }
        }


        public static event Action<LanguageCode> LanguageChangedHook
        {
            add { LanguageHooks.LanguageChangedHook += value; }
            remove { LanguageHooks.LanguageChangedHook -= value; }
        }

        #endregion
#endif

        #region 库存


        public static event Action<string> ItemObtainedHook;

        public static event Action<string> ItemLostHook;

        public static event Action<string> ItemUsedHook;

        public static event Action<string, int, int> ItemCountChangedHook;

        public static event Action<string> ItemEquippedHook;

        public static event Action<string> ItemUnequippedHook;

        public static event Action<string> ItemUnlockedHook;

        public static event Action InventoryOpenedHook;

        public static event Action InventoryClosedHook;

        public static event Action<string> InventorySelectionChangedHook;

        public static event Action<string> RuneObtainedHook;

        public static event Action<string> RuneEquippedHook;

        public static event Action<string> RuneUnequippedHook;

        public static event Action<int> RuneSlotChangedHook;

        public static event Action<string> ToolObtainedHook;

        public static event Action<string> ToolEquippedHook;

        public static event Action<string> ToolUnequippedHook;

        public static event Action<string, int> ToolUsageChangedHook;

        public static event Action<string> CrestObtainedHook;

        public static event Action<string> CrestUnequippedHook;

        public static event Action<string> CrestEffectTriggeredHook;

        #endregion

        #region NPC


        public static event Action<GameObject> NpcSpawnedHook;

        public static event Action<GameObject> NpcDespawnedHook;

        public static event Action<GameObject> NpcDialogueStartHook;

        public static event Action<GameObject> NpcDialogueEndHook;

        public static event Action<GameObject, int> NpcDialogueOptionSelectedHook;

        public static event Action<GameObject, string> NpcGiveItemHook;

        public static event Action<GameObject, string> NpcRequestItemHook;

        public static event Action<GameObject, string> NpcStateChangedHook;

        public static event Action<GameObject, string> NpcQuestUpdatedHook;

        public static event Action<GameObject, int> NpcAffinityChangedHook;

        public static event Action<GameObject, GameObject> NpcAttackedHook;

        public static event Action<GameObject> NpcKilledHook;

        public static event Action<GameObject, Collider2D> NpcTriggerEnterHook;

        public static event Action<GameObject, Collider2D> NpcTriggerExitHook;

        public static event Action<GameObject, string> NpcAnimationEventHook;

        public static event Action<GameObject, string, string> NpcFsmStateChangedHook;

        public static event Action<GameObject> NpcShopOpenedHook;

        public static event Action<GameObject> NpcShopClosedHook;

        public static event Action<GameObject, string, int> NpcShopItemSoldHook;

        public static event Action<GameObject, string, int> NpcShopItemBoughtHook;

        #endregion

        #region Boss


        public static event Action<GameObject> BossBattleStartHook;

        public static event Action<GameObject, bool> BossBattleEndHook;

        public static event Action<GameObject, int> BossPhaseChangedHook;

        public static event Action<GameObject, string> BossAbilityUsedHook;

        public static event Action<GameObject, string> BossAttackPatternHook;

        public static event Action<GameObject, int> BossDamagedHook;

        public static event Action<GameObject, int> BossHealedHook;

        public static event Action<GameObject> BossDeathHook;

        public static event Action<GameObject> BossSpawnedHook;

        public static event Action<GameObject> BossDespawnedHook;

        public static event Action<GameObject, string, string> BossFsmStateChangedHook;

        public static event Action<GameObject, string> BossAnimationEventHook;

        public static event Action<GameObject, GameObject> BossSpawnMinionHook;

        public static event Action<GameObject> BossInvulnerableHook;

        public static event Action<GameObject> BossVulnerableHook;

        public static event Action<GameObject, string> BossMusicChangedHook;

        public static event Action<GameObject, Bounds> BossArenaLockedHook;

        public static event Action<GameObject, Bounds> BossArenaUnlockedHook;

        public static event Action<GameObject, GameObject> BossPlayerDeathHook;

        public static event Action<GameObject, string> BossRewardHook;

        #endregion

        #region 环境


        public static event Action<string> SceneLoadedHook;

        public static event Action<string> SceneUnloadedHook;

        public static event Action<string, GameObject> SceneObjectSpawnedHook;

        public static event Action<string, GameObject> SceneObjectDestroyedHook;

        public static event Action<GameObject> TriggerActivatedHook;

        public static event Action<GameObject> TriggerDeactivatedHook;

        public static event Action<Collider2D> PlayerTriggerEnterHook;

        public static event Action<Collider2D> PlayerTriggerExitHook;

        public static event Action<Collider2D> PlayerCollisionEnterHook;

        public static event Action<Collider2D> PlayerCollisionExitHook;

        public static event Action<GameObject> HazardTriggeredHook;

        public static event Action<GameObject> BreakableObjectBrokenHook;

        public static event Action<GameObject> InteractableObjectUsedHook;

        public static event Action<GameObject, string> TeleporterUsedHook;

        public static event Action<GameObject> DoorOpenedHook;

        public static event Action<GameObject> DoorClosedHook;

        public static event Action<GameObject> PlatformMovingHook;

        public static event Action<GameObject> PlatformReachedHook;

        public static event Action<Collider2D> WaterEnterHook;

        public static event Action<Collider2D> WaterExitHook;

        public static event Action<Collider2D> WindEnterHook;

        public static event Action<Collider2D> WindExitHook;

        public static event Action<Collider2D> AcidEnterHook;

        public static event Action<Collider2D> AcidExitHook;

        public static event Action<GameObject> CheckpointActivatedHook;

        public static event Action<Bounds> MapBoundsChangedHook;

        public static event Action<float> SceneLightingChangedHook;

        public static event Action<string> WeatherChangedHook;

        public static event Action<float> TimeChangedHook;

        #endregion

        #region UI


        public static event Action<string> UIPanelOpenedHook;

        public static event Action<string> UIPanelClosedHook;

        public static event Action<string> UIButtonClickedHook;

        public static event Action<string, string> UITextChangedHook;

        public static event Action<string, Sprite> UIImageChangedHook;

        public static event Action MainMenuOpenedHook;

        public static event Action MainMenuClosedHook;

        public static event Action PauseMenuOpenedHook;

        public static event Action PauseMenuClosedHook;

        public static event Action InventoryUIOpenedHook;

        public static event Action InventoryUIClosedHook;

        public static event Action MapUIOpenedHook;

        public static event Action MapUIClosedHook;

        public static event Action SettingsUIOpenedHook;

        public static event Action SettingsUIClosedHook;

        public static event Action<string> DialogueUIOpenedHook;

        public static event Action<string> DialogueUIClosedHook;

        public static event Action<string> NotificationShownHook;

        public static event Action<string> TooltipShownHook;

        public static event Action<float> LoadingScreenShownHook;

        public static event Action LoadingScreenHiddenHook;

        public static event Action<string> CutsceneStartedHook;

        public static event Action<string> CutsceneEndedHook;

        public static event Action<string> HUDShownHook;

        public static event Action<string> HUDHiddenHook;

        public static event Action<int> HealthBarChangedHook;

        public static event Action<int> SilkBarChangedHook;

        public static event Action<int> CurrencyDisplayChangedHook;

        public static event Action<string> AbilityIconShownHook;

        public static event Action MinimapUpdatedHook;

        public static event Action<string> QuestTrackerUpdatedHook;

        public static event Action<float> ScreenShakeHook;

        public static event Action<float> ScreenFadeInHook;

        public static event Action<float> ScreenFadeOutHook;

        #endregion

        #region 输入


        public static event Action<string> KeyPressedHook;

        public static event Action<string> KeyReleasedHook;

        public static event Action<string> KeyHeldHook;

        public static event Action<int> MouseButtonPressedHook;

        public static event Action<int> MouseButtonReleasedHook;

        public static event Action<Vector2> MouseMovedHook;

        public static event Action<float> MouseScrolledHook;

        public static event Action<string> GamepadButtonPressedHook;

        public static event Action<string> GamepadButtonReleasedHook;

        public static event Action<string, Vector2> GamepadStickMovedHook;

        public static event Action<string, float> GamepadTriggerMovedHook;

        public static event Action<Vector2> InputDirectionChangedHook;

        public static event Action<Vector2> MoveInputHook;

        public static event Action<Vector2> LookInputHook;

        public static event Action AttackInputHook;

        public static event Action JumpInputHook;

        public static event Action DashInputHook;

        public static event Action InteractInputHook;

        public static event Action OpenInventoryInputHook;

        public static event Action OpenMapInputHook;

        public static event Action PauseInputHook;

        public static event Action CancelInputHook;

        public static event Action ConfirmInputHook;

        public static event Action<string> InputDeviceChangedHook;

        public static event Action<float> InputSensitivityChangedHook;

        public static event Action<string, bool> InputInversionChangedHook;

        public static event Action<string, string> KeyBindingChangedHook;

        public static event Action InputBufferClearedHook;

        public static event Action<bool> InputLockChangedHook;

        public static event Action<Vector2> TouchBeganHook;

        public static event Action<Vector2> TouchMovedHook;

        public static event Action<Vector2> TouchEndedHook;

        #endregion

        #region 音频


        public static event Action<string, float> SoundPlayedHook;

        public static event Action<string> SoundStoppedHook;

        public static event Action<string, float> BGMChangedHook;

        public static event Action<string, float> BGMFadedInHook;

        public static event Action<string, float> BGMFadedOutHook;

        public static event Action<string> AmbientSoundChangedHook;

        public static event Action<float> MasterVolumeChangedHook;

        public static event Action<float> MusicVolumeChangedHook;

        public static event Action<float> SFXVolumeChangedHook;

        public static event Action<float> VoiceVolumeChangedHook;

        public static event Action<bool> AudioMutedHook;

        public static event Action<AudioSource> AudioSourceCreatedHook;

        public static event Action<AudioSource> AudioSourceDestroyedHook;

        public static event Action<string> AudioMixerSnapshotChangedHook;

        public static event Action<string> AudioEffectAppliedHook;

        public static event Action<Vector3, string> SpatialAudioPlayedHook;

        public static event Action<string> AudioGroupChangedHook;

        public static event Action<string> AudioFadeInCompleteHook;

        public static event Action<string> AudioFadeOutCompleteHook;

        public static event Action<string> AudioLoopedHook;

        #endregion

        #region 摄像机


        public static event Action<Vector3> CameraPositionChangedHook;

        public static event Action<Quaternion> CameraRotationChangedHook;

        public static event Action<float> CameraZoomChangedHook;

        public static event Action<Transform> CameraFollowTargetChangedHook;

        public static event Action<float, float> CameraShakeStartedHook;

        public static event Action CameraShakeEndedHook;

        public static event Action<float> CameraFadeInStartedHook;

        public static event Action CameraFadeInCompletedHook;

        public static event Action<float> CameraFadeOutStartedHook;

        public static event Action CameraFadeOutCompletedHook;

        public static event Action<Bounds> CameraBoundsChangedHook;

        public static event Action<Transform> CameraLockedToTargetHook;

        public static event Action CameraUnlockedHook;

        public static event Action<string> CameraCutsceneStartedHook;

        public static event Action<string> CameraCutsceneEndedHook;

        public static event Action<float> CameraFieldOfViewChangedHook;

        public static event Action<string> CameraNoiseAppliedHook;

        public static event Action<Vector3> CameraTargetOffsetChangedHook;

        public static event Action<float> CameraDampingChangedHook;

        public static event Action<Vector2> CameraLookAheadChangedHook;

        #endregion

        #region 动画


        public static event Action<Animator, string> AnimationStateChangedHook;

        public static event Action<Animator, string> AnimationEventHook;

        public static event Action<Animator, string> AnimationStartedHook;

        public static event Action<Animator, string> AnimationCompletedHook;

        public static event Action<Animator, string> AnimationLoopedHook;

        public static event Action<Animator, string, float> AnimationParameterChangedHook;

        public static event Action<Animator, int, float> AnimationLayerWeightChangedHook;

        public static event Action<Animator, float> AnimationSpeedChangedHook;

        public static event Action<Animator, string, float> AnimationBlendTreeChangedHook;

        public static event Action<Animator> AnimationIKUpdatedHook;

        public static event Action<Animator, string, string> AnimationTransitionHook;

        public static event Action<Animator, string> AnimationClipChangedHook;

        public static event Action<Animator, Vector3> AnimationRootMotionAppliedHook;

        public static event Action<Animator, Quaternion> AnimationRootRotationAppliedHook;

        public static event Action<Animator, string> AnimationControllerChangedHook;

        public static event Action<Animator, Avatar> AnimationAvatarChangedHook;

        public static event Action<Animator, AnimatorCullingMode> AnimationCullingModeChangedHook;

        public static event Action<Animator, AnimatorUpdateMode> AnimationUpdateModeChangedHook;

        public static event Action<Animator, bool> AnimationApplyRootMotionChangedHook;

        public static event Action<Animator, bool> AnimationLinearVelocityBlendingChangedHook;

        #endregion

        #region 物理


        public static event Action<Rigidbody, Collision> RigidbodyCollisionEnterHook;

        public static event Action<Rigidbody, Collision> RigidbodyCollisionStayHook;

        public static event Action<Rigidbody, Collision> RigidbodyCollisionExitHook;

        public static event Action<Rigidbody2D, Collision2D> Rigidbody2DCollisionEnterHook;

        public static event Action<Rigidbody2D, Collision2D> Rigidbody2DCollisionStayHook;

        public static event Action<Rigidbody2D, Collision2D> Rigidbody2DCollisionExitHook;

        public static event Action<Collider, Collider> TriggerEnterHook;

        public static event Action<Collider, Collider> TriggerStayHook;

        public static event Action<Collider, Collider> TriggerExitHook;

        public static event Action<Collider2D, Collider2D> Trigger2DEnterHook;

        public static event Action<Collider2D, Collider2D> Trigger2DStayHook;

        public static event Action<Collider2D, Collider2D> Trigger2DExitHook;

        public static event Action<Joint> JointBreakHook;

        public static event Action<Joint2D> Joint2DBreakHook;

        public static event Action PhysicsBeforeStepHook;

        public static event Action PhysicsAfterStepHook;

        public static event Action<Vector3> GravityChangedHook;

        public static event Action<Vector2> Gravity2DChangedHook;

        public static event Action<PhysicsMaterial> PhysicsMaterialChangedHook;

        public static event Action<PhysicsMaterial2D> PhysicsMaterial2DChangedHook;

        public static event Action<RaycastHit> RaycastHitHook;

        public static event Action<RaycastHit2D> Raycast2DHitHook;

        public static event Action<Rigidbody> RigidbodySleepHook;

        public static event Action<Rigidbody> RigidbodyWakeHook;

        public static event Action<Rigidbody2D> Rigidbody2DSleepHook;

        public static event Action<Rigidbody2D> Rigidbody2DWakeHook;

        public static event Action<Collider, Collider, bool> CollisionFilterHook;

        public static event Action<Collider2D, Collider2D, bool> Collision2DFilterHook;

        #endregion

        #region 对话


        public static event Action<string> DialogueStartedHook;

        public static event Action<string> DialogueEndedHook;

        public static event Action<string, int, string> DialogueLineShownHook;

        public static event Action<string, int> DialogueLineSkippedHook;

        public static event Action<string, string[]> DialogueOptionsShownHook;

        public static event Action<string, int> DialogueOptionSelectedHook;

        public static event Action<string, string> DialogueTextChangedHook;

        public static event Action<string, string> DialogueSoundPlayedHook;

        public static event Action<string, string> DialogueAnimationPlayedHook;

        public static event Action<string, string> DialogueBranchSelectedHook;

        public static event Action<string, string, string> DialogueVariableSetHook;

        public static event Action<string, string, bool> DialogueConditionCheckedHook;

        public static event Action<string, string, string> DialogueFsmStateChangedHook;

        public static event Action<string> DialogueTypewriterStartedHook;

        public static event Action<string> DialogueTypewriterCompletedHook;

        public static event Action<string> DialoguePausedHook;

        public static event Action<string> DialogueResumedHook;

        public static event Action<string> DialogueSkippedHook;

        public static event Action<string> DialogueFastForwardedHook;

        public static event Action<string> DialogueReplayedHook;

        #endregion

        #region 扩展存档


        public static event Action<int> BeforeSavegameSaveHook;

        public static event Action<int> AfterSavegameSaveHook;

        public static event Action<int> BeforeSavegameLoadHook;

        public static event Action<int> AfterSavegameLoadHook;

        public static event Action<int> BeforeSavegameClearHook;

        public static event Action BeforeNewGameHook;

        public static event Action AfterNewGameHook;

        public static event Action<int, byte[]> BeforeSaveSerializeHook;

        public static event Action<int, byte[]> AfterSaveSerializeHook;

        public static event Action<int, byte[]> BeforeSaveDeserializeHook;

        public static event Action<int, byte[]> AfterSaveDeserializeHook;

        public static event Action<int, byte[]> BeforeSaveCompressHook;

        public static event Action<int, byte[]> AfterSaveCompressHook;

        public static event Action<int, byte[]> BeforeSaveDecompressHook;

        public static event Action<int, byte[]> AfterSaveDecompressHook;

        public static event Action<int, byte[]> BeforeSaveEncryptHook;

        public static event Action<int, byte[]> AfterSaveEncryptHook;

        public static event Action<int, byte[]> BeforeSaveDecryptHook;

        public static event Action<int, byte[]> AfterSaveDecryptHook;

        public static event Action<int, byte[]> BeforeSaveValidateHook;

        public static event Action<int, bool> AfterSaveValidateHook;

        public static event Action<int> SaveBackupCreatedHook;

        public static event Action<int> SaveBackupRestoredHook;

        public static event Action<int, int, int> SaveVersionMigratedHook;

        public static event Action<int, bool> SaveCorruptionDetectedHook;

        public static event Action<int> AutoSaveTriggeredHook;

        public static event Action<int> ManualSaveTriggeredHook;

        public static event Action<int> QuickSaveTriggeredHook;

        public static event Action<int> QuickLoadTriggeredHook;

        #endregion

        #region 内部调用（供 Patches 使用）

        private static void Safe(Action a)
        {
            try { a(); } catch (Exception e) { Logger.LogError($"钩子抛出异常: {e}"); }
        }

        private static T Chain<T>(Delegate ev, T value, Func<Delegate, T, T> call)
        {
            if (ev == null) return value;
            foreach (Delegate d in ev.GetInvocationList())
            {
                try { value = call(d, value); }
                catch (Exception e) { Logger.LogError($"钩子抛出异常: {e}"); }
            }
            return value;
        }


        internal static void OnApplicationQuit() => Safe(() => ApplicationQuitHook?.Invoke());
        internal static string OnBeforeSceneLoad(string scene) =>
            Chain(BeforeSceneLoadHook, scene, (d, v) => ((Func<string, string>) d)(v));
        internal static void OnActiveSceneChanged(string from, string to) =>
            Safe(() => ActiveSceneChangedHook?.Invoke(from, to));


        internal static void OnNewGame() => Safe(() => NewGameHook?.Invoke());
        internal static void OnSavegameLoad(int slot) => Safe(() => SavegameLoadHook?.Invoke(slot));
        internal static void OnSavegameSave(int slot) => Safe(() => SavegameSaveHook?.Invoke(slot));
        internal static void OnSavegameClear(int slot) => Safe(() => SavegameClearHook?.Invoke(slot));
        internal static void OnAfterSavegameClear(int slot) => Safe(() => AfterSavegameClearHook?.Invoke(slot));


        internal static bool OnGetPlayerBool(string name, bool value) =>
            Chain(GetPlayerBoolHook, value, (d, v) => ((Func<string, bool, bool>) d)(name, v));
        internal static bool OnSetPlayerBool(string name, bool value) =>
            Chain(SetPlayerBoolHook, value, (d, v) => ((Func<string, bool, bool>) d)(name, v));
        internal static int OnGetPlayerInt(string name, int value) =>
            Chain(GetPlayerIntHook, value, (d, v) => ((Func<string, int, int>) d)(name, v));
        internal static int OnSetPlayerInt(string name, int value) =>
            Chain(SetPlayerIntHook, value, (d, v) => ((Func<string, int, int>) d)(name, v));
        internal static float OnGetPlayerFloat(string name, float value) =>
            Chain(GetPlayerFloatHook, value, (d, v) => ((Func<string, float, float>) d)(name, v));
        internal static float OnSetPlayerFloat(string name, float value) =>
            Chain(SetPlayerFloatHook, value, (d, v) => ((Func<string, float, float>) d)(name, v));
        internal static string OnGetPlayerString(string name, string value) =>
            Chain(GetPlayerStringHook, value, (d, v) => ((Func<string, string, string>) d)(name, v));
        internal static string OnSetPlayerString(string name, string value) =>
            Chain(SetPlayerStringHook, value, (d, v) => ((Func<string, string, string>) d)(name, v));
        internal static Vector3 OnGetPlayerVector3(string name, Vector3 value) =>
            Chain(GetPlayerVector3Hook, value, (d, v) => ((Func<string, Vector3, Vector3>) d)(name, v));
        internal static Vector3 OnSetPlayerVector3(string name, Vector3 value) =>
            Chain(SetPlayerVector3Hook, value, (d, v) => ((Func<string, Vector3, Vector3>) d)(name, v));


        internal static void OnHeroUpdate() => Safe(() => HeroUpdateHook?.Invoke());
        internal static void OnAttack(AttackDirection dir) => Safe(() => AttackHook?.Invoke(dir));
        internal static void OnBeforePlayerDead() => Safe(() => BeforePlayerDeadHook?.Invoke());
        internal static void OnAfterPlayerDead() => Safe(() => AfterPlayerDeadHook?.Invoke());
        internal static int OnTakeDamage(HazardType hazard, int damage) =>
            Chain(TakeDamageHook, damage, (d, v) => ((Func<HazardType, int, int>) d)(hazard, v));
        internal static void OnAfterTakeDamage(HazardType hazard, int damage) =>
            Safe(() => AfterTakeDamageHook?.Invoke(hazard, damage));
        internal static HitInstance OnHealthManagerTakeDamage(HealthManager hm, HitInstance hit) =>
            Chain(HealthManagerTakeDamageHook, hit, (d, v) => ((Func<HealthManager, HitInstance, HitInstance>) d)(hm, v));


        internal static void OnGameStateChanged(GameState s) => Safe(() => GameStateChangedHook?.Invoke(s));
        internal static void OnSceneEntered(string scene) => Safe(() => SceneEnteredHook?.Invoke(scene));


        internal static void OnHeroStart() => Safe(() => HeroStartHook?.Invoke());
        internal static void OnHeroJump() => Safe(() => HeroJumpHook?.Invoke());
        internal static void OnHeroDoubleJump() => Safe(() => HeroDoubleJumpHook?.Invoke());
        internal static void OnHeroDash() => Safe(() => HeroDashHook?.Invoke());
        internal static int OnAddHealth(int amount) =>
            Chain(AddHealthHook, amount, (d, v) => ((Func<int, int>) d)(v));
        internal static int OnTakeHealth(int amount) =>
            Chain(TakeHealthHook, amount, (d, v) => ((Func<int, int>) d)(v));
        internal static int OnAddSilk(int amount) =>
            Chain(AddSilkHook, amount, (d, v) => ((Func<int, int>) d)(v));
        internal static int OnTakeSilk(int amount) =>
            Chain(TakeSilkHook, amount, (d, v) => ((Func<int, int>) d)(v));


        internal static int OnCurrencyChange(CurrencyType type, int amount) =>
            Chain(CurrencyChangeHook, amount, (d, v) => ((Func<CurrencyType, int, int>) d)(type, v));
        internal static void OnEnemyStart(HealthManager hm) => Safe(() => EnemyStartHook?.Invoke(hm));
        internal static void OnEnemyDeath(HealthManager hm) => Safe(() => EnemyDeathHook?.Invoke(hm));


        internal static void OnUpdate() => UpdateHook?.Invoke();
        internal static void OnFixedUpdate() => FixedUpdateHook?.Invoke();
        internal static void OnLateUpdate() => LateUpdateHook?.Invoke();
        internal static void OnOnGUI() => OnGUIHook?.Invoke();


        internal static void OnSceneInit() => Safe(() => SceneInitHook?.Invoke());
        internal static void OnNextSceneWillActivate() => Safe(() => NextSceneWillActivateHook?.Invoke());
        internal static void OnUnloadingLevel() => Safe(() => UnloadingLevelHook?.Invoke());
        internal static void OnBeforeFinishedSceneTransition() => Safe(() => BeforeFinishedSceneTransitionHook?.Invoke());
        internal static void OnFinishedSceneTransition() => Safe(() => FinishedSceneTransitionHook?.Invoke());
        internal static void OnSavePersistentObjects() => Safe(() => SavePersistentObjectsHook?.Invoke());
        internal static void OnResetSemiPersistentObjects() => Safe(() => ResetSemiPersistentObjectsHook?.Invoke());
        internal static void OnBossLoaded() => Safe(() => BossLoadedHook?.Invoke());
        internal static void OnRefreshLanguageText() => Safe(() => RefreshLanguageTextHook?.Invoke());
        internal static void OnGamePausedChanged(bool paused) => Safe(() => GamePausedChangedHook?.Invoke(paused));
        internal static void OnSceneTransitionBegan(SceneLoad l) => Safe(() => SceneTransitionBeganHook?.Invoke(l));
        internal static void OnHeroInstanceSet(HeroController h) => Safe(() => HeroInstanceSetHook?.Invoke(h));
        internal static void OnPreHeroInPosition(bool f) => Safe(() => PreHeroInPositionHook?.Invoke(f));
        internal static void OnHeroInPosition(bool f) => Safe(() => HeroInPositionHook?.Invoke(f));
        internal static void OnHeroInPositionDelayed(bool f) => Safe(() => HeroInPositionDelayedHook?.Invoke(f));
        internal static void OnHeroTakenDamage() => Safe(() => HeroTakenDamageHook?.Invoke());
        internal static void OnHeroTakenDamageExtra(HeroController.DamageInfo i) => Safe(() => HeroTakenDamageExtraHook?.Invoke(i));
        internal static void OnHeroDeath() => Safe(() => HeroDeathHook?.Invoke());
        internal static void OnHeroHazardDeath() => Safe(() => HeroHazardDeathHook?.Invoke());
        internal static void OnHeroHazardRespawn() => Safe(() => HeroHazardRespawnHook?.Invoke());
        internal static void OnHeroLeavingScene() => Safe(() => HeroLeavingSceneHook?.Invoke());
        internal static void OnBoundAttackToolUpdated(AttackToolBinding b) => Safe(() => BoundAttackToolUpdatedHook?.Invoke(b));
        internal static void OnBoundAttackToolUsed(AttackToolBinding b) => Safe(() => BoundAttackToolUsedHook?.Invoke(b));
        internal static void OnBoundAttackToolFailed(AttackToolBinding b) => Safe(() => BoundAttackToolFailedHook?.Invoke(b));
        internal static void OnEquippedToolsChanged() => Safe(() => EquippedToolsChangedHook?.Invoke());


        internal static bool OnHeroAbilityCheck(HeroAbility a, bool orig)
        {
            Func<HeroAbility, bool, bool> h = HeroAbilityCheckHook;
            if (h == null) return orig;
            return Chain(h, orig, (d, v) => ((Func<HeroAbility, bool, bool>) d)(a, v));
        }
        internal static void OnHeroBounce() => Safe(() => HeroBounceHook?.Invoke());
        internal static void OnHeroControlRelinquished() => Safe(() => HeroControlRelinquishedHook?.Invoke());
        internal static void OnHeroControlRegained(bool allowInput) => Safe(() => HeroControlRegainedHook?.Invoke(allowInput));
        internal static void OnHeroRespawn(Transform t) => Safe(() => HeroRespawnHook?.Invoke(t));
        internal static int OnAddMaxHealth(int amount) =>
            Chain(AddMaxHealthHook, amount, (d, v) => ((Func<int, int>) d)(v));
        internal static HitInstance OnHealthManagerHit(HealthManager hm, HitInstance hit) =>
            Chain(HealthManagerHitHook, hit, (d, v) => ((Func<HealthManager, HitInstance, HitInstance>) d)(hm, v));
        internal static void OnCrestEquipped(string id) => Safe(() => CrestEquippedHook?.Invoke(id));
        internal static void OnToolUnlocked(ToolItem t) => Safe(() => ToolUnlockedHook?.Invoke(t));
        internal static void OnQuestAccepted(FullQuestBase q) => Safe(() => QuestAcceptedHook?.Invoke(q));
        internal static void OnQuestCompleted(FullQuestBase q) => Safe(() => QuestCompletedHook?.Invoke(q));
        internal static void OnBenchRespawnSet(string marker, string scene) => Safe(() => BenchRespawnSetHook?.Invoke(marker, scene));
        internal static void OnReturnToMainMenu(bool willSave) => Safe(() => ReturnToMainMenuHook?.Invoke(willSave));


        internal static void OnItemObtained(string itemId) => Safe(() => ItemObtainedHook?.Invoke(itemId));
        internal static void OnItemLost(string itemId) => Safe(() => ItemLostHook?.Invoke(itemId));
        internal static void OnItemUsed(string itemId) => Safe(() => ItemUsedHook?.Invoke(itemId));
        internal static void OnItemCountChanged(string itemId, int oldCount, int newCount) =>
            Safe(() => ItemCountChangedHook?.Invoke(itemId, oldCount, newCount));
        internal static void OnItemEquipped(string itemId) => Safe(() => ItemEquippedHook?.Invoke(itemId));
        internal static void OnItemUnequipped(string itemId) => Safe(() => ItemUnequippedHook?.Invoke(itemId));
        internal static void OnItemUnlocked(string itemId) => Safe(() => ItemUnlockedHook?.Invoke(itemId));
        internal static void OnInventoryOpened() => Safe(() => InventoryOpenedHook?.Invoke());
        internal static void OnInventoryClosed() => Safe(() => InventoryClosedHook?.Invoke());
        internal static void OnInventorySelectionChanged(string itemId) =>
            Safe(() => InventorySelectionChangedHook?.Invoke(itemId));
        internal static void OnRuneObtained(string runeId) => Safe(() => RuneObtainedHook?.Invoke(runeId));
        internal static void OnRuneEquipped(string runeId) => Safe(() => RuneEquippedHook?.Invoke(runeId));
        internal static void OnRuneUnequipped(string runeId) => Safe(() => RuneUnequippedHook?.Invoke(runeId));
        internal static void OnRuneSlotChanged(int slotIndex) => Safe(() => RuneSlotChangedHook?.Invoke(slotIndex));
        internal static void OnToolObtained(string toolId) => Safe(() => ToolObtainedHook?.Invoke(toolId));
        internal static void OnToolEquipped(string toolId) => Safe(() => ToolEquippedHook?.Invoke(toolId));
        internal static void OnToolUnequipped(string toolId) => Safe(() => ToolUnequippedHook?.Invoke(toolId));
        internal static void OnToolUsageChanged(string toolId, int usageCount) =>
            Safe(() => ToolUsageChangedHook?.Invoke(toolId, usageCount));
        internal static void OnCrestObtained(string crestId) => Safe(() => CrestObtainedHook?.Invoke(crestId));
        internal static void OnCrestUnequipped(string crestId) => Safe(() => CrestUnequippedHook?.Invoke(crestId));
        internal static void OnCrestEffectTriggered(string crestId) =>
            Safe(() => CrestEffectTriggeredHook?.Invoke(crestId));


        internal static void OnNpcSpawned(GameObject npc) => Safe(() => NpcSpawnedHook?.Invoke(npc));
        internal static void OnNpcDespawned(GameObject npc) => Safe(() => NpcDespawnedHook?.Invoke(npc));
        internal static void OnNpcDialogueStart(GameObject npc) => Safe(() => NpcDialogueStartHook?.Invoke(npc));
        internal static void OnNpcDialogueEnd(GameObject npc) => Safe(() => NpcDialogueEndHook?.Invoke(npc));
        internal static void OnNpcDialogueOptionSelected(GameObject npc, int optionIndex) =>
            Safe(() => NpcDialogueOptionSelectedHook?.Invoke(npc, optionIndex));
        internal static void OnNpcGiveItem(GameObject npc, string itemId) =>
            Safe(() => NpcGiveItemHook?.Invoke(npc, itemId));
        internal static void OnNpcRequestItem(GameObject npc, string itemId) =>
            Safe(() => NpcRequestItemHook?.Invoke(npc, itemId));
        internal static void OnNpcStateChanged(GameObject npc, string newState) =>
            Safe(() => NpcStateChangedHook?.Invoke(npc, newState));
        internal static void OnNpcQuestUpdated(GameObject npc, string questId) =>
            Safe(() => NpcQuestUpdatedHook?.Invoke(npc, questId));
        internal static void OnNpcAffinityChanged(GameObject npc, int change) =>
            Safe(() => NpcAffinityChangedHook?.Invoke(npc, change));
        internal static void OnNpcAttacked(GameObject npc, GameObject attacker) =>
            Safe(() => NpcAttackedHook?.Invoke(npc, attacker));
        internal static void OnNpcKilled(GameObject npc) => Safe(() => NpcKilledHook?.Invoke(npc));
        internal static void OnNpcTriggerEnter(GameObject npc, Collider2D trigger) =>
            Safe(() => NpcTriggerEnterHook?.Invoke(npc, trigger));
        internal static void OnNpcTriggerExit(GameObject npc, Collider2D trigger) =>
            Safe(() => NpcTriggerExitHook?.Invoke(npc, trigger));
        internal static void OnNpcAnimationEvent(GameObject npc, string eventName) =>
            Safe(() => NpcAnimationEventHook?.Invoke(npc, eventName));
        internal static void OnNpcFsmStateChanged(GameObject npc, string fsmName, string newState) =>
            Safe(() => NpcFsmStateChangedHook?.Invoke(npc, fsmName, newState));
        internal static void OnNpcShopOpened(GameObject npc) => Safe(() => NpcShopOpenedHook?.Invoke(npc));
        internal static void OnNpcShopClosed(GameObject npc) => Safe(() => NpcShopClosedHook?.Invoke(npc));
        internal static void OnNpcShopItemSold(GameObject npc, string itemId, int price) =>
            Safe(() => NpcShopItemSoldHook?.Invoke(npc, itemId, price));
        internal static void OnNpcShopItemBought(GameObject npc, string itemId, int price) =>
            Safe(() => NpcShopItemBoughtHook?.Invoke(npc, itemId, price));


        internal static void OnBossBattleStart(GameObject boss) => Safe(() => BossBattleStartHook?.Invoke(boss));
        internal static void OnBossBattleEnd(GameObject boss, bool victory) =>
            Safe(() => BossBattleEndHook?.Invoke(boss, victory));
        internal static void OnBossPhaseChanged(GameObject boss, int phase) =>
            Safe(() => BossPhaseChangedHook?.Invoke(boss, phase));
        internal static void OnBossAbilityUsed(GameObject boss, string abilityName) =>
            Safe(() => BossAbilityUsedHook?.Invoke(boss, abilityName));
        internal static void OnBossAttackPattern(GameObject boss, string patternName) =>
            Safe(() => BossAttackPatternHook?.Invoke(boss, patternName));
        internal static void OnBossDamaged(GameObject boss, int damage) =>
            Safe(() => BossDamagedHook?.Invoke(boss, damage));
        internal static void OnBossHealed(GameObject boss, int healAmount) =>
            Safe(() => BossHealedHook?.Invoke(boss, healAmount));
        internal static void OnBossDeath(GameObject boss) => Safe(() => BossDeathHook?.Invoke(boss));
        internal static void OnBossSpawned(GameObject boss) => Safe(() => BossSpawnedHook?.Invoke(boss));
        internal static void OnBossDespawned(GameObject boss) => Safe(() => BossDespawnedHook?.Invoke(boss));
        internal static void OnBossFsmStateChanged(GameObject boss, string fsmName, string newState) =>
            Safe(() => BossFsmStateChangedHook?.Invoke(boss, fsmName, newState));
        internal static void OnBossAnimationEvent(GameObject boss, string eventName) =>
            Safe(() => BossAnimationEventHook?.Invoke(boss, eventName));
        internal static void OnBossSpawnMinion(GameObject boss, GameObject minion) =>
            Safe(() => BossSpawnMinionHook?.Invoke(boss, minion));
        internal static void OnBossInvulnerable(GameObject boss) => Safe(() => BossInvulnerableHook?.Invoke(boss));
        internal static void OnBossVulnerable(GameObject boss) => Safe(() => BossVulnerableHook?.Invoke(boss));
        internal static void OnBossMusicChanged(GameObject boss, string musicName) =>
            Safe(() => BossMusicChangedHook?.Invoke(boss, musicName));
        internal static void OnBossArenaLocked(GameObject boss, Bounds arena) =>
            Safe(() => BossArenaLockedHook?.Invoke(boss, arena));
        internal static void OnBossArenaUnlocked(GameObject boss, Bounds arena) =>
            Safe(() => BossArenaUnlockedHook?.Invoke(boss, arena));
        internal static void OnBossPlayerDeath(GameObject boss, GameObject player) =>
            Safe(() => BossPlayerDeathHook?.Invoke(boss, player));
        internal static void OnBossReward(GameObject boss, string rewardId) =>
            Safe(() => BossRewardHook?.Invoke(boss, rewardId));


        internal static void OnSceneLoaded(string sceneName) => Safe(() => SceneLoadedHook?.Invoke(sceneName));
        internal static void OnSceneUnloaded(string sceneName) => Safe(() => SceneUnloadedHook?.Invoke(sceneName));
        internal static void OnSceneObjectSpawned(string sceneName, GameObject obj) =>
            Safe(() => SceneObjectSpawnedHook?.Invoke(sceneName, obj));
        internal static void OnSceneObjectDestroyed(string sceneName, GameObject obj) =>
            Safe(() => SceneObjectDestroyedHook?.Invoke(sceneName, obj));
        internal static void OnTriggerActivated(GameObject trigger) =>
            Safe(() => TriggerActivatedHook?.Invoke(trigger));
        internal static void OnTriggerDeactivated(GameObject trigger) =>
            Safe(() => TriggerDeactivatedHook?.Invoke(trigger));
        internal static void OnPlayerTriggerEnter(Collider2D trigger) =>
            Safe(() => PlayerTriggerEnterHook?.Invoke(trigger));
        internal static void OnPlayerTriggerExit(Collider2D trigger) =>
            Safe(() => PlayerTriggerExitHook?.Invoke(trigger));
        internal static void OnPlayerCollisionEnter(Collider2D collision) =>
            Safe(() => PlayerCollisionEnterHook?.Invoke(collision));
        internal static void OnPlayerCollisionExit(Collider2D collision) =>
            Safe(() => PlayerCollisionExitHook?.Invoke(collision));
        internal static void OnHazardTriggered(GameObject hazard) =>
            Safe(() => HazardTriggeredHook?.Invoke(hazard));
        internal static void OnBreakableObjectBroken(GameObject obj) =>
            Safe(() => BreakableObjectBrokenHook?.Invoke(obj));
        internal static void OnInteractableObjectUsed(GameObject obj) =>
            Safe(() => InteractableObjectUsedHook?.Invoke(obj));
        internal static void OnTeleporterUsed(GameObject teleporter, string targetScene) =>
            Safe(() => TeleporterUsedHook?.Invoke(teleporter, targetScene));
        internal static void OnDoorOpened(GameObject door) => Safe(() => DoorOpenedHook?.Invoke(door));
        internal static void OnDoorClosed(GameObject door) => Safe(() => DoorClosedHook?.Invoke(door));
        internal static void OnPlatformMoving(GameObject platform) =>
            Safe(() => PlatformMovingHook?.Invoke(platform));
        internal static void OnPlatformReached(GameObject platform) =>
            Safe(() => PlatformReachedHook?.Invoke(platform));
        internal static void OnWaterEnter(Collider2D water) => Safe(() => WaterEnterHook?.Invoke(water));
        internal static void OnWaterExit(Collider2D water) => Safe(() => WaterExitHook?.Invoke(water));
        internal static void OnWindEnter(Collider2D wind) => Safe(() => WindEnterHook?.Invoke(wind));
        internal static void OnWindExit(Collider2D wind) => Safe(() => WindExitHook?.Invoke(wind));
        internal static void OnAcidEnter(Collider2D acid) => Safe(() => AcidEnterHook?.Invoke(acid));
        internal static void OnAcidExit(Collider2D acid) => Safe(() => AcidExitHook?.Invoke(acid));
        internal static void OnCheckpointActivated(GameObject checkpoint) =>
            Safe(() => CheckpointActivatedHook?.Invoke(checkpoint));
        internal static void OnMapBoundsChanged(Bounds bounds) =>
            Safe(() => MapBoundsChangedHook?.Invoke(bounds));
        internal static void OnSceneLightingChanged(float intensity) =>
            Safe(() => SceneLightingChangedHook?.Invoke(intensity));
        internal static void OnWeatherChanged(string weatherType) =>
            Safe(() => WeatherChangedHook?.Invoke(weatherType));
        internal static void OnTimeChanged(float time) => Safe(() => TimeChangedHook?.Invoke(time));


        internal static void OnUIPanelOpened(string panelName) => Safe(() => UIPanelOpenedHook?.Invoke(panelName));
        internal static void OnUIPanelClosed(string panelName) => Safe(() => UIPanelClosedHook?.Invoke(panelName));
        internal static void OnUIButtonClicked(string buttonName) =>
            Safe(() => UIButtonClickedHook?.Invoke(buttonName));
        internal static void OnUITextChanged(string textName, string newText) =>
            Safe(() => UITextChangedHook?.Invoke(textName, newText));
        internal static void OnUIImageChanged(string imageName, Sprite newSprite) =>
            Safe(() => UIImageChangedHook?.Invoke(imageName, newSprite));
        internal static void OnMainMenuOpened() => Safe(() => MainMenuOpenedHook?.Invoke());
        internal static void OnMainMenuClosed() => Safe(() => MainMenuClosedHook?.Invoke());
        internal static void OnPauseMenuOpened() => Safe(() => PauseMenuOpenedHook?.Invoke());
        internal static void OnPauseMenuClosed() => Safe(() => PauseMenuClosedHook?.Invoke());
        internal static void OnInventoryUIOpened() => Safe(() => InventoryUIOpenedHook?.Invoke());
        internal static void OnInventoryUIClosed() => Safe(() => InventoryUIClosedHook?.Invoke());
        internal static void OnMapUIOpened() => Safe(() => MapUIOpenedHook?.Invoke());
        internal static void OnMapUIClosed() => Safe(() => MapUIClosedHook?.Invoke());
        internal static void OnSettingsUIOpened() => Safe(() => SettingsUIOpenedHook?.Invoke());
        internal static void OnSettingsUIClosed() => Safe(() => SettingsUIClosedHook?.Invoke());
        internal static void OnDialogueUIOpened(string dialogueId) =>
            Safe(() => DialogueUIOpenedHook?.Invoke(dialogueId));
        internal static void OnDialogueUIClosed(string dialogueId) =>
            Safe(() => DialogueUIClosedHook?.Invoke(dialogueId));
        internal static void OnNotificationShown(string message) =>
            Safe(() => NotificationShownHook?.Invoke(message));
        internal static void OnTooltipShown(string tooltip) => Safe(() => TooltipShownHook?.Invoke(tooltip));
        internal static void OnLoadingScreenShown(float progress) =>
            Safe(() => LoadingScreenShownHook?.Invoke(progress));
        internal static void OnLoadingScreenHidden() => Safe(() => LoadingScreenHiddenHook?.Invoke());
        internal static void OnCutsceneStarted(string cutsceneName) =>
            Safe(() => CutsceneStartedHook?.Invoke(cutsceneName));
        internal static void OnCutsceneEnded(string cutsceneName) =>
            Safe(() => CutsceneEndedHook?.Invoke(cutsceneName));
        internal static void OnHUDShown(string hudElement) => Safe(() => HUDShownHook?.Invoke(hudElement));
        internal static void OnHUDHidden(string hudElement) => Safe(() => HUDHiddenHook?.Invoke(hudElement));
        internal static void OnHealthBarChanged(int health) => Safe(() => HealthBarChangedHook?.Invoke(health));
        internal static void OnSilkBarChanged(int silk) => Safe(() => SilkBarChangedHook?.Invoke(silk));
        internal static void OnCurrencyDisplayChanged(int currency) =>
            Safe(() => CurrencyDisplayChangedHook?.Invoke(currency));
        internal static void OnAbilityIconShown(string abilityName) =>
            Safe(() => AbilityIconShownHook?.Invoke(abilityName));
        internal static void OnMinimapUpdated() => Safe(() => MinimapUpdatedHook?.Invoke());
        internal static void OnQuestTrackerUpdated(string questId) =>
            Safe(() => QuestTrackerUpdatedHook?.Invoke(questId));
        internal static void OnScreenShake(float intensity) => Safe(() => ScreenShakeHook?.Invoke(intensity));
        internal static void OnScreenFadeIn(float duration) => Safe(() => ScreenFadeInHook?.Invoke(duration));
        internal static void OnScreenFadeOut(float duration) => Safe(() => ScreenFadeOutHook?.Invoke(duration));


        internal static void OnKeyPressed(string keyName) => Safe(() => KeyPressedHook?.Invoke(keyName));
        internal static void OnKeyReleased(string keyName) => Safe(() => KeyReleasedHook?.Invoke(keyName));
        internal static void OnKeyHeld(string keyName) => Safe(() => KeyHeldHook?.Invoke(keyName));
        internal static void OnMouseButtonPressed(int buttonIndex) =>
            Safe(() => MouseButtonPressedHook?.Invoke(buttonIndex));
        internal static void OnMouseButtonReleased(int buttonIndex) =>
            Safe(() => MouseButtonReleasedHook?.Invoke(buttonIndex));
        internal static void OnMouseMoved(Vector2 position) => Safe(() => MouseMovedHook?.Invoke(position));
        internal static void OnMouseScrolled(float scrollValue) =>
            Safe(() => MouseScrolledHook?.Invoke(scrollValue));
        internal static void OnGamepadButtonPressed(string buttonName) =>
            Safe(() => GamepadButtonPressedHook?.Invoke(buttonName));
        internal static void OnGamepadButtonReleased(string buttonName) =>
            Safe(() => GamepadButtonReleasedHook?.Invoke(buttonName));
        internal static void OnGamepadStickMoved(string stickName, Vector2 value) =>
            Safe(() => GamepadStickMovedHook?.Invoke(stickName, value));
        internal static void OnGamepadTriggerMoved(string triggerName, float value) =>
            Safe(() => GamepadTriggerMovedHook?.Invoke(triggerName, value));
        internal static void OnInputDirectionChanged(Vector2 direction) =>
            Safe(() => InputDirectionChangedHook?.Invoke(direction));
        internal static void OnMoveInput(Vector2 moveVector) => Safe(() => MoveInputHook?.Invoke(moveVector));
        internal static void OnLookInput(Vector2 lookVector) => Safe(() => LookInputHook?.Invoke(lookVector));
        internal static void OnAttackInput() => Safe(() => AttackInputHook?.Invoke());
        internal static void OnJumpInput() => Safe(() => JumpInputHook?.Invoke());
        internal static void OnDashInput() => Safe(() => DashInputHook?.Invoke());
        internal static void OnInteractInput() => Safe(() => InteractInputHook?.Invoke());
        internal static void OnOpenInventoryInput() => Safe(() => OpenInventoryInputHook?.Invoke());
        internal static void OnOpenMapInput() => Safe(() => OpenMapInputHook?.Invoke());
        internal static void OnPauseInput() => Safe(() => PauseInputHook?.Invoke());
        internal static void OnCancelInput() => Safe(() => CancelInputHook?.Invoke());
        internal static void OnConfirmInput() => Safe(() => ConfirmInputHook?.Invoke());
        internal static void OnInputDeviceChanged(string deviceType) =>
            Safe(() => InputDeviceChangedHook?.Invoke(deviceType));
        internal static void OnInputSensitivityChanged(float sensitivity) =>
            Safe(() => InputSensitivityChangedHook?.Invoke(sensitivity));
        internal static void OnInputInversionChanged(string axisName, bool inverted) =>
            Safe(() => InputInversionChangedHook?.Invoke(axisName, inverted));
        internal static void OnKeyBindingChanged(string actionName, string newKey) =>
            Safe(() => KeyBindingChangedHook?.Invoke(actionName, newKey));
        internal static void OnInputBufferCleared() => Safe(() => InputBufferClearedHook?.Invoke());
        internal static void OnInputLockChanged(bool locked) => Safe(() => InputLockChangedHook?.Invoke(locked));
        internal static void OnTouchBegan(Vector2 position) => Safe(() => TouchBeganHook?.Invoke(position));
        internal static void OnTouchMoved(Vector2 position) => Safe(() => TouchMovedHook?.Invoke(position));
        internal static void OnTouchEnded(Vector2 position) => Safe(() => TouchEndedHook?.Invoke(position));


        internal static void OnSoundPlayed(string soundName, float volume) =>
            Safe(() => SoundPlayedHook?.Invoke(soundName, volume));
        internal static void OnSoundStopped(string soundName) => Safe(() => SoundStoppedHook?.Invoke(soundName));
        internal static void OnBGMChanged(string bgmName, float fadeTime) =>
            Safe(() => BGMChangedHook?.Invoke(bgmName, fadeTime));
        internal static void OnBGMFadedIn(string bgmName, float fadeTime) =>
            Safe(() => BGMFadedInHook?.Invoke(bgmName, fadeTime));
        internal static void OnBGMFadedOut(string bgmName, float fadeTime) =>
            Safe(() => BGMFadedOutHook?.Invoke(bgmName, fadeTime));
        internal static void OnAmbientSoundChanged(string ambientName) =>
            Safe(() => AmbientSoundChangedHook?.Invoke(ambientName));
        internal static void OnMasterVolumeChanged(float volume) =>
            Safe(() => MasterVolumeChangedHook?.Invoke(volume));
        internal static void OnMusicVolumeChanged(float volume) =>
            Safe(() => MusicVolumeChangedHook?.Invoke(volume));
        internal static void OnSFXVolumeChanged(float volume) => Safe(() => SFXVolumeChangedHook?.Invoke(volume));
        internal static void OnVoiceVolumeChanged(float volume) =>
            Safe(() => VoiceVolumeChangedHook?.Invoke(volume));
        internal static void OnAudioMuted(bool muted) => Safe(() => AudioMutedHook?.Invoke(muted));
        internal static void OnAudioSourceCreated(AudioSource source) =>
            Safe(() => AudioSourceCreatedHook?.Invoke(source));
        internal static void OnAudioSourceDestroyed(AudioSource source) =>
            Safe(() => AudioSourceDestroyedHook?.Invoke(source));
        internal static void OnAudioMixerSnapshotChanged(string snapshotName) =>
            Safe(() => AudioMixerSnapshotChangedHook?.Invoke(snapshotName));
        internal static void OnAudioEffectApplied(string effectName) =>
            Safe(() => AudioEffectAppliedHook?.Invoke(effectName));
        internal static void OnSpatialAudioPlayed(Vector3 position, string soundName) =>
            Safe(() => SpatialAudioPlayedHook?.Invoke(position, soundName));
        internal static void OnAudioGroupChanged(string groupName) =>
            Safe(() => AudioGroupChangedHook?.Invoke(groupName));
        internal static void OnAudioFadeInComplete(string soundName) =>
            Safe(() => AudioFadeInCompleteHook?.Invoke(soundName));
        internal static void OnAudioFadeOutComplete(string soundName) =>
            Safe(() => AudioFadeOutCompleteHook?.Invoke(soundName));
        internal static void OnAudioLooped(string soundName) => Safe(() => AudioLoopedHook?.Invoke(soundName));


        internal static void OnCameraPositionChanged(Vector3 position) =>
            Safe(() => CameraPositionChangedHook?.Invoke(position));
        internal static void OnCameraRotationChanged(Quaternion rotation) =>
            Safe(() => CameraRotationChangedHook?.Invoke(rotation));
        internal static void OnCameraZoomChanged(float zoom) =>
            Safe(() => CameraZoomChangedHook?.Invoke(zoom));
        internal static void OnCameraFollowTargetChanged(Transform target) =>
            Safe(() => CameraFollowTargetChangedHook?.Invoke(target));
        internal static void OnCameraShakeStarted(float intensity, float duration) =>
            Safe(() => CameraShakeStartedHook?.Invoke(intensity, duration));
        internal static void OnCameraShakeEnded() => Safe(() => CameraShakeEndedHook?.Invoke());
        internal static void OnCameraFadeInStarted(float duration) =>
            Safe(() => CameraFadeInStartedHook?.Invoke(duration));
        internal static void OnCameraFadeInCompleted() => Safe(() => CameraFadeInCompletedHook?.Invoke());
        internal static void OnCameraFadeOutStarted(float duration) =>
            Safe(() => CameraFadeOutStartedHook?.Invoke(duration));
        internal static void OnCameraFadeOutCompleted() => Safe(() => CameraFadeOutCompletedHook?.Invoke());
        internal static void OnCameraBoundsChanged(Bounds bounds) =>
            Safe(() => CameraBoundsChangedHook?.Invoke(bounds));
        internal static void OnCameraLockedToTarget(Transform target) =>
            Safe(() => CameraLockedToTargetHook?.Invoke(target));
        internal static void OnCameraUnlocked() => Safe(() => CameraUnlockedHook?.Invoke());
        internal static void OnCameraCutsceneStarted(string cutsceneName) =>
            Safe(() => CameraCutsceneStartedHook?.Invoke(cutsceneName));
        internal static void OnCameraCutsceneEnded(string cutsceneName) =>
            Safe(() => CameraCutsceneEndedHook?.Invoke(cutsceneName));
        internal static void OnCameraFieldOfViewChanged(float fov) =>
            Safe(() => CameraFieldOfViewChangedHook?.Invoke(fov));
        internal static void OnCameraNoiseApplied(string noiseName) =>
            Safe(() => CameraNoiseAppliedHook?.Invoke(noiseName));
        internal static void OnCameraTargetOffsetChanged(Vector3 offset) =>
            Safe(() => CameraTargetOffsetChangedHook?.Invoke(offset));
        internal static void OnCameraDampingChanged(float damping) =>
            Safe(() => CameraDampingChangedHook?.Invoke(damping));
        internal static void OnCameraLookAheadChanged(Vector2 lookAhead) =>
            Safe(() => CameraLookAheadChangedHook?.Invoke(lookAhead));


        internal static void OnAnimationStateChanged(Animator animator, string stateName) =>
            Safe(() => AnimationStateChangedHook?.Invoke(animator, stateName));
        internal static void OnAnimationEvent(Animator animator, string eventName) =>
            Safe(() => AnimationEventHook?.Invoke(animator, eventName));
        internal static void OnAnimationStarted(Animator animator, string animationName) =>
            Safe(() => AnimationStartedHook?.Invoke(animator, animationName));
        internal static void OnAnimationCompleted(Animator animator, string animationName) =>
            Safe(() => AnimationCompletedHook?.Invoke(animator, animationName));
        internal static void OnAnimationLooped(Animator animator, string animationName) =>
            Safe(() => AnimationLoopedHook?.Invoke(animator, animationName));
        internal static void OnAnimationParameterChanged(Animator animator, string paramName, float value) =>
            Safe(() => AnimationParameterChangedHook?.Invoke(animator, paramName, value));
        internal static void OnAnimationLayerWeightChanged(Animator animator, int layerIndex, float weight) =>
            Safe(() => AnimationLayerWeightChangedHook?.Invoke(animator, layerIndex, weight));
        internal static void OnAnimationSpeedChanged(Animator animator, float speed) =>
            Safe(() => AnimationSpeedChangedHook?.Invoke(animator, speed));
        internal static void OnAnimationBlendTreeChanged(Animator animator, string blendTreeName, float value) =>
            Safe(() => AnimationBlendTreeChangedHook?.Invoke(animator, blendTreeName, value));
        internal static void OnAnimationIKUpdated(Animator animator) =>
            Safe(() => AnimationIKUpdatedHook?.Invoke(animator));
        internal static void OnAnimationTransition(Animator animator, string fromState, string toState) =>
            Safe(() => AnimationTransitionHook?.Invoke(animator, fromState, toState));
        internal static void OnAnimationClipChanged(Animator animator, string clipName) =>
            Safe(() => AnimationClipChangedHook?.Invoke(animator, clipName));
        internal static void OnAnimationRootMotionApplied(Animator animator, Vector3 rootMotion) =>
            Safe(() => AnimationRootMotionAppliedHook?.Invoke(animator, rootMotion));
        internal static void OnAnimationRootRotationApplied(Animator animator, Quaternion rootRotation) =>
            Safe(() => AnimationRootRotationAppliedHook?.Invoke(animator, rootRotation));
        internal static void OnAnimationControllerChanged(Animator animator, string controllerName) =>
            Safe(() => AnimationControllerChangedHook?.Invoke(animator, controllerName));
        internal static void OnAnimationAvatarChanged(Animator animator, Avatar avatar) =>
            Safe(() => AnimationAvatarChangedHook?.Invoke(animator, avatar));
        internal static void OnAnimationCullingModeChanged(Animator animator, AnimatorCullingMode mode) =>
            Safe(() => AnimationCullingModeChangedHook?.Invoke(animator, mode));
        internal static void OnAnimationUpdateModeChanged(Animator animator, AnimatorUpdateMode mode) =>
            Safe(() => AnimationUpdateModeChangedHook?.Invoke(animator, mode));
        internal static void OnAnimationApplyRootMotionChanged(Animator animator, bool apply) =>
            Safe(() => AnimationApplyRootMotionChangedHook?.Invoke(animator, apply));
        internal static void OnAnimationLinearVelocityBlendingChanged(Animator animator, bool enabled) =>
            Safe(() => AnimationLinearVelocityBlendingChangedHook?.Invoke(animator, enabled));


        internal static void OnRigidbodyCollisionEnter(Rigidbody rb, Collision collision) =>
            Safe(() => RigidbodyCollisionEnterHook?.Invoke(rb, collision));
        internal static void OnRigidbodyCollisionStay(Rigidbody rb, Collision collision) =>
            Safe(() => RigidbodyCollisionStayHook?.Invoke(rb, collision));
        internal static void OnRigidbodyCollisionExit(Rigidbody rb, Collision collision) =>
            Safe(() => RigidbodyCollisionExitHook?.Invoke(rb, collision));
        internal static void OnRigidbody2DCollisionEnter(Rigidbody2D rb, Collision2D collision) =>
            Safe(() => Rigidbody2DCollisionEnterHook?.Invoke(rb, collision));
        internal static void OnRigidbody2DCollisionStay(Rigidbody2D rb, Collision2D collision) =>
            Safe(() => Rigidbody2DCollisionStayHook?.Invoke(rb, collision));
        internal static void OnRigidbody2DCollisionExit(Rigidbody2D rb, Collision2D collision) =>
            Safe(() => Rigidbody2DCollisionExitHook?.Invoke(rb, collision));
        internal static void OnTriggerEnter(Collider collider, Collider other) =>
            Safe(() => TriggerEnterHook?.Invoke(collider, other));
        internal static void OnTriggerStay(Collider collider, Collider other) =>
            Safe(() => TriggerStayHook?.Invoke(collider, other));
        internal static void OnTriggerExit(Collider collider, Collider other) =>
            Safe(() => TriggerExitHook?.Invoke(collider, other));
        internal static void OnTrigger2DEnter(Collider2D collider, Collider2D other) =>
            Safe(() => Trigger2DEnterHook?.Invoke(collider, other));
        internal static void OnTrigger2DStay(Collider2D collider, Collider2D other) =>
            Safe(() => Trigger2DStayHook?.Invoke(collider, other));
        internal static void OnTrigger2DExit(Collider2D collider, Collider2D other) =>
            Safe(() => Trigger2DExitHook?.Invoke(collider, other));
        internal static void OnJointBreak(Joint joint) => Safe(() => JointBreakHook?.Invoke(joint));
        internal static void OnJoint2DBreak(Joint2D joint) => Safe(() => Joint2DBreakHook?.Invoke(joint));
        internal static void OnPhysicsBeforeStep() => Safe(() => PhysicsBeforeStepHook?.Invoke());
        internal static void OnPhysicsAfterStep() => Safe(() => PhysicsAfterStepHook?.Invoke());
        internal static void OnGravityChanged(Vector3 gravity) => Safe(() => GravityChangedHook?.Invoke(gravity));
        internal static void OnGravity2DChanged(Vector2 gravity) =>
            Safe(() => Gravity2DChangedHook?.Invoke(gravity));
        internal static void OnPhysicsMaterialChanged(PhysicsMaterial material) =>
            Safe(() => PhysicsMaterialChangedHook?.Invoke(material));
        internal static void OnPhysicsMaterial2DChanged(PhysicsMaterial2D material) =>
            Safe(() => PhysicsMaterial2DChangedHook?.Invoke(material));
        internal static void OnRaycastHit(RaycastHit hit) => Safe(() => RaycastHitHook?.Invoke(hit));
        internal static void OnRaycast2DHit(RaycastHit2D hit) => Safe(() => Raycast2DHitHook?.Invoke(hit));
        internal static void OnRigidbodySleep(Rigidbody rb) => Safe(() => RigidbodySleepHook?.Invoke(rb));
        internal static void OnRigidbodyWake(Rigidbody rb) => Safe(() => RigidbodyWakeHook?.Invoke(rb));
        internal static void OnRigidbody2DSleep(Rigidbody2D rb) => Safe(() => Rigidbody2DSleepHook?.Invoke(rb));
        internal static void OnRigidbody2DWake(Rigidbody2D rb) => Safe(() => Rigidbody2DWakeHook?.Invoke(rb));
        internal static void OnCollisionFilter(Collider collider, Collider other, bool filtered) =>
            Safe(() => CollisionFilterHook?.Invoke(collider, other, filtered));
        internal static void OnCollision2DFilter(Collider2D collider, Collider2D other, bool filtered) =>
            Safe(() => Collision2DFilterHook?.Invoke(collider, other, filtered));


        internal static void OnDialogueStarted(string dialogueId) =>
            Safe(() => DialogueStartedHook?.Invoke(dialogueId));
        internal static void OnDialogueEnded(string dialogueId) =>
            Safe(() => DialogueEndedHook?.Invoke(dialogueId));
        internal static void OnDialogueLineShown(string dialogueId, int lineIndex, string text) =>
            Safe(() => DialogueLineShownHook?.Invoke(dialogueId, lineIndex, text));
        internal static void OnDialogueLineSkipped(string dialogueId, int lineIndex) =>
            Safe(() => DialogueLineSkippedHook?.Invoke(dialogueId, lineIndex));
        internal static void OnDialogueOptionsShown(string dialogueId, string[] options) =>
            Safe(() => DialogueOptionsShownHook?.Invoke(dialogueId, options));
        internal static void OnDialogueOptionSelected(string dialogueId, int optionIndex) =>
            Safe(() => DialogueOptionSelectedHook?.Invoke(dialogueId, optionIndex));
        internal static void OnDialogueTextChanged(string dialogueId, string newText) =>
            Safe(() => DialogueTextChangedHook?.Invoke(dialogueId, newText));
        internal static void OnDialogueSoundPlayed(string dialogueId, string soundName) =>
            Safe(() => DialogueSoundPlayedHook?.Invoke(dialogueId, soundName));
        internal static void OnDialogueAnimationPlayed(string dialogueId, string animationName) =>
            Safe(() => DialogueAnimationPlayedHook?.Invoke(dialogueId, animationName));
        internal static void OnDialogueBranchSelected(string dialogueId, string branchName) =>
            Safe(() => DialogueBranchSelectedHook?.Invoke(dialogueId, branchName));
        internal static void OnDialogueVariableSet(string dialogueId, string varName, string varValue) =>
            Safe(() => DialogueVariableSetHook?.Invoke(dialogueId, varName, varValue));
        internal static void OnDialogueConditionChecked(string dialogueId, string conditionName, bool result) =>
            Safe(() => DialogueConditionCheckedHook?.Invoke(dialogueId, conditionName, result));
        internal static void OnDialogueFsmStateChanged(string dialogueId, string fsmName, string newState) =>
            Safe(() => DialogueFsmStateChangedHook?.Invoke(dialogueId, fsmName, newState));
        internal static void OnDialogueTypewriterStarted(string dialogueId) =>
            Safe(() => DialogueTypewriterStartedHook?.Invoke(dialogueId));
        internal static void OnDialogueTypewriterCompleted(string dialogueId) =>
            Safe(() => DialogueTypewriterCompletedHook?.Invoke(dialogueId));
        internal static void OnDialoguePaused(string dialogueId) =>
            Safe(() => DialoguePausedHook?.Invoke(dialogueId));
        internal static void OnDialogueResumed(string dialogueId) =>
            Safe(() => DialogueResumedHook?.Invoke(dialogueId));
        internal static void OnDialogueSkipped(string dialogueId) =>
            Safe(() => DialogueSkippedHook?.Invoke(dialogueId));
        internal static void OnDialogueFastForwarded(string dialogueId) =>
            Safe(() => DialogueFastForwardedHook?.Invoke(dialogueId));
        internal static void OnDialogueReplayed(string dialogueId) =>
            Safe(() => DialogueReplayedHook?.Invoke(dialogueId));


        internal static void OnBeforeSavegameSave(int slot) => Safe(() => BeforeSavegameSaveHook?.Invoke(slot));
        internal static void OnAfterSavegameSave(int slot) => Safe(() => AfterSavegameSaveHook?.Invoke(slot));
        internal static void OnBeforeSavegameLoad(int slot) => Safe(() => BeforeSavegameLoadHook?.Invoke(slot));
        internal static void OnAfterSavegameLoad(int slot) => Safe(() => AfterSavegameLoadHook?.Invoke(slot));
        internal static void OnBeforeSavegameClear(int slot) => Safe(() => BeforeSavegameClearHook?.Invoke(slot));
        internal static void OnBeforeNewGame() => Safe(() => BeforeNewGameHook?.Invoke());
        internal static void OnAfterNewGame() => Safe(() => AfterNewGameHook?.Invoke());
        internal static void OnBeforeSaveSerialize(int slot, byte[] data) =>
            Safe(() => BeforeSaveSerializeHook?.Invoke(slot, data));
        internal static void OnAfterSaveSerialize(int slot, byte[] data) =>
            Safe(() => AfterSaveSerializeHook?.Invoke(slot, data));
        internal static void OnBeforeSaveDeserialize(int slot, byte[] data) =>
            Safe(() => BeforeSaveDeserializeHook?.Invoke(slot, data));
        internal static void OnAfterSaveDeserialize(int slot, byte[] data) =>
            Safe(() => AfterSaveDeserializeHook?.Invoke(slot, data));
        internal static void OnBeforeSaveCompress(int slot, byte[] data) =>
            Safe(() => BeforeSaveCompressHook?.Invoke(slot, data));
        internal static void OnAfterSaveCompress(int slot, byte[] data) =>
            Safe(() => AfterSaveCompressHook?.Invoke(slot, data));
        internal static void OnBeforeSaveDecompress(int slot, byte[] data) =>
            Safe(() => BeforeSaveDecompressHook?.Invoke(slot, data));
        internal static void OnAfterSaveDecompress(int slot, byte[] data) =>
            Safe(() => AfterSaveDecompressHook?.Invoke(slot, data));
        internal static void OnBeforeSaveEncrypt(int slot, byte[] data) =>
            Safe(() => BeforeSaveEncryptHook?.Invoke(slot, data));
        internal static void OnAfterSaveEncrypt(int slot, byte[] data) =>
            Safe(() => AfterSaveEncryptHook?.Invoke(slot, data));
        internal static void OnBeforeSaveDecrypt(int slot, byte[] data) =>
            Safe(() => BeforeSaveDecryptHook?.Invoke(slot, data));
        internal static void OnAfterSaveDecrypt(int slot, byte[] data) =>
            Safe(() => AfterSaveDecryptHook?.Invoke(slot, data));
        internal static void OnBeforeSaveValidate(int slot, byte[] data) =>
            Safe(() => BeforeSaveValidateHook?.Invoke(slot, data));
        internal static void OnAfterSaveValidate(int slot, bool isValid) =>
            Safe(() => AfterSaveValidateHook?.Invoke(slot, isValid));
        internal static void OnSaveBackupCreated(int slot) => Safe(() => SaveBackupCreatedHook?.Invoke(slot));
        internal static void OnSaveBackupRestored(int slot) => Safe(() => SaveBackupRestoredHook?.Invoke(slot));
        internal static void OnSaveVersionMigrated(int slot, int oldVersion, int newVersion) =>
            Safe(() => SaveVersionMigratedHook?.Invoke(slot, oldVersion, newVersion));
        internal static void OnSaveCorruptionDetected(int slot, bool isCorrupted) =>
            Safe(() => SaveCorruptionDetectedHook?.Invoke(slot, isCorrupted));
        internal static void OnAutoSaveTriggered(int slot) => Safe(() => AutoSaveTriggeredHook?.Invoke(slot));
        internal static void OnManualSaveTriggered(int slot) => Safe(() => ManualSaveTriggeredHook?.Invoke(slot));
        internal static void OnQuickSaveTriggered(int slot) => Safe(() => QuickSaveTriggeredHook?.Invoke(slot));
        internal static void OnQuickLoadTriggered(int slot) => Safe(() => QuickLoadTriggeredHook?.Invoke(slot));

        #endregion
    }
}
