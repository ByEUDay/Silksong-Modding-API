# Silksong Modding API

与 HK Modding API 同样思路的《空洞骑士：丝之歌》Modding API：对 `Assembly-CSharp.dll` 预补丁 + MonoMod 打补丁，
把一组钩子（`ModHooks`）和一个 Mod 加载器直接编进游戏程序集。

> **状态：未在真实游戏上测试。** 我手上只有反编译源码（ss.zip），没有游戏的 DLL，
> 所以补丁里用到的方法签名都已对照你的源码核对过，但整个管线（编译 → 预补丁 → MonoMod → 运行）没有跑过。
> 第一次跑很可能要按报错微调，详见文末「已知风险」。

## 管线

```
原版 Assembly-CSharp.dll
   │  PrePatcher（Cecil）：PlayerData 字段读写 → GetXxx / SetXxx(name) 调用
   ▼
预补丁版 Assembly-CSharp.dll
   │  MonoMod + Assembly-CSharp.mm.dll（补丁 + Modding 运行时）
   ▼
MONOMODDED_Assembly-CSharp.dll  → 安装为 Assembly-CSharp.dll
   │  HookGen
   ▼
MMHOOK_Assembly-CSharp.dll（给 Mod 用 On.* / IL.* 钩子）
```

## 目录

| 路径 | 作用 |
|---|---|
| `PrePatcher/` | Cecil 工具。只处理 `bool/int/float/string/Vector3` 五种 PlayerData 字段（Silksong 的 PlayerData 没有 `GetVariable<T>`，HK 版里处理泛型的部分去掉了） |
| `Assembly-CSharp.mm/Patches/` | MonoMod 补丁：`GameManager`、`PlayerData`、`HeroController`、`HealthManager`、`CurrencyManager`（sealed，补丁类不继承） |
| `TeamCherry.Localization.mm/` | 给 `TeamCherry.Localization.dll` 打补丁：`LanguageHooks`（`Language.Get/Has/SwitchLanguage`）。Assembly-CSharp 依赖 Localization，所以钩子本体放在 Localization 里，`ModHooks.LanguageGetHook` 等只是转发 |
| `Assembly-CSharp.mm/Modding/` | 运行时：`IMod`、`Mod`、`ModHooks`、`ModLoader`、`Logger` |
| `Examples/ExampleMod/` | 最小示例 Mod |
| `build.sh` | macOS 一键构建 / 安装 |

## 已实现的钩子（`ModHooks`）

**每帧**：`UpdateHook`、`FixedUpdateHook`、`LateUpdateHook`、`OnGUIHook`

**生命周期 / 场景**：`ApplicationQuitHook`、`BeforeSceneLoadHook`（可改写目标场景名）、`ActiveSceneChangedHook`、`GameStateChangedHook`、`SceneEnteredHook(sceneName)`（主角入场完成）、`ReturnToMainMenuHook`

**游戏自带事件的转发（无补丁）**：`SceneInitHook`、`NextSceneWillActivateHook`、`UnloadingLevelHook`、`BeforeFinishedSceneTransitionHook`、`FinishedSceneTransitionHook`、`SceneTransitionBeganHook(SceneLoad)`、`SavePersistentObjectsHook`、`ResetSemiPersistentObjectsHook`、`BossLoadedHook`、`RefreshLanguageTextHook`、`GamePausedChangedHook(bool)`

**存档**：`NewGameHook`、`SavegameLoadHook(slot)`、`SavegameSaveHook(slot)`、`SavegameClearHook`、`AfterSavegameClearHook`、`BenchRespawnSetHook(标记名, 场景名)`

**PlayerData**：`Get/SetPlayerBool/Int/Float/String/Vector3Hook`（链式传递返回值）。`IncrementInt/DecrementInt/IntAdd` 也改成经过 `SetInt`

**主角**：
- 事件：`HeroInstanceSetHook`、`HeroStartHook`、`HeroUpdateHook`、`PreHeroInPositionHook`、`HeroInPositionHook`、`HeroInPositionDelayedHook`、`HeroLeavingSceneHook`、`HeroRespawnHook`、`BeforePlayerDeadHook`、`AfterPlayerDeadHook`、`HeroDeathHook`、`HeroHazardDeathHook`、`HeroHazardRespawnHook`、`HeroControlRelinquishedHook`、`HeroControlRegainedHook`
- 动作：`AttackHook`、`HeroJumpHook`、`HeroDoubleJumpHook`、`HeroDashHook`、`HeroBounceHook`
- 伤害：`TakeDamageHook`（可改）、`AfterTakeDamageHook`、`HeroTakenDamageHook`、`HeroTakenDamageExtraHook(DamageInfo)`
- 数值（可改量）：`AddHealthHook`、`TakeHealthHook`、`AddMaxHealthHook`、`AddSilkHook`、`TakeSilkHook`、`CurrencyChangeHook(类型, 变化量)`
- **能力检查**：`HeroAbilityCheckHook(HeroAbility, 原判断) -> 结果`，覆盖 Cast / Bind / Interact / OpenInventory / Jump / DoubleJump / Dash / HarpoonDash / Sprint / SuperJump / Attack / TakeDamage / TakeDamageIgnoreInvuln

**敌人**：`EnemyStartHook`、`EnemyDeathHook`、`HealthManagerHitHook`（最早，可改 `HitInstance`）、`HealthManagerTakeDamageHook`

**工具 / 徽章 / 任务**：`CrestEquippedHook(crestId)`、`EquippedToolsChangedHook`、`ToolUnlockedHook`、`BoundAttackToolUpdated/Used/FailedHook`、`QuestAcceptedHook`、`QuestCompletedHook`

**本地化**：`LanguageGetHook(key, sheet, 原文本)`、`LanguageHasHook`、`LanguageChangedHook`（需要 Localization 补丁成功）

Mod 从 `…/Data/Managed/Mods/` 递归加载所有 dll；日志写到 `ModLog.txt`（Unity 的 `persistentDataPath` 下）。

## 其他 API

- `IGlobalSettings<T>` / `ILocalSettings<T>`：Mod 继承即可自动读写 json（全局一份 / 每个存档槽一份，T 用 Unity `JsonUtility` 可序列化的类）。见 `Examples/ExampleMod`
- `ModLoader.GetMod<T>()` / `GetMod(name)` / `IsModLoaded(name)`
- `ReflectionHelper`：读写私有字段、调用私有方法（带缓存）
- `ModRunner`：给非 MonoBehaviour 代码跑协程、`After(秒, 动作)`、`AfterFrames(帧数, 动作)`
- **`IPreloadMod`**：从游戏场景里取预制体（敌人、特效等）。返回 `(场景名, 物体路径)`，加载完成后在 `OnPreloaded` 收到未激活的副本。内部通过 Addressables 叠加加载 `Scenes/<场景名>`，加载时那个场景自己的脚本会跑一次 Awake，所以请求的场景越少越好
- **`FsmUtil`**：PlayMaker FSM 编辑（`GetFsm/GetState/AddAction/InsertAction/RemoveAction/AddMethod/AddTransition/ChangeTransition/AddState/Add*Variable`）。FSM 的启动钩子可以用 MMHOOK 的 `On.PlayMakerFSM.OnEnable`
- **`ITogglableMod`** 与 `ModLoader.SetEnabled(mod, bool)`：运行时开关 Mod，禁用状态保存在 `ModsDisabled.txt`
- **`ModResources`**：读取嵌入资源（`LoadTexture / LoadSprite / LoadString / LoadBytes`）
- `UnityExtensions`：`GetOrAddComponent`、`FindChildRecursive`、`SetPosition2D` 等

Mod 从 `…/Data/Managed/Mods/` 递归加载所有 dll；日志写到 `ModLog.txt`（Unity 的 `persistentDataPath` 下）。

## PlayMaker 补丁（`PlayMaker.mm/`）

给 `PlayMaker.dll` 打补丁，加 FSM 钩子。PlayMaker 不能反过来引用 Assembly-CSharp，所以钩子本体（`Modding.PlayMakerHooks`）放在 PlayMaker.dll 里，
Assembly-CSharp 里的 `ModHooks.Fsm*Hook` 只做转发，`PlayMakerBridge` 把它接到已有的 `FsmEventHooks` / `FsmVariableHooks` 上。

| 钩子（`ModHooks.*` 与 `PlayMakerHooks.*` 等价） | 时机 |
|---|---|
| `FsmAwakeHook(PlayMakerFSM)` | `PlayMakerFSM.Awake` 完成后：状态/事件/变量就绪，还没运行。**改 FSM 的最佳时机** |
| `FsmEnableHook(PlayMakerFSM)` | `OnEnable` 之后（物体每次激活都触发） |
| `FsmStartHook(PlayMakerFSM)` | 第一次 `Start` 之前，只触发一次 |
| `FsmDestroyHook(PlayMakerFSM)` | 销毁之前 |
| `FsmEventHook(Fsm, FsmEvent) -> FsmEvent` | 处理事件之前，可替换，返回 `null` 吞掉（热路径） |
| `FsmStateEnterHook(Fsm, FsmState)` / `FsmStateExitHook` | 进入 / 离开状态之前 |
| `FsmTransitionHook(Fsm, FsmTransition, bool isGlobal)` | 跳转执行之前 |
| `PlayMakerHooks.FsmFloat/Int/Bool/StringSetHook(Fsm, 变量名, 新值) -> 实际值` | 变量通过 `Value` 属性写入时（`RawValue` / `SafeAssign` 不经过）。仅 PlayMakerHooks 提供，默认关闭，`PlayMakerHooks.VariableHooksEnabled = true` 才生效；注册 `FsmVariableHooks` 监听器/修改器时会自动打开 |

按名字注册（只在 `PlayMakerHooks` 上）：

```csharp
PlayMakerHooks.OnFsm("Knight", "Spell Control", fsm => fsm.AddMethod("Fireball Cast", () => Log("放火球")));  // FsmUtil
PlayMakerHooks.OnStateEnter("Spell Control", "Q2 Land", (fsm, state) => Log("落地"));
PlayMakerHooks.RemoveFsmEdit(handler);          // ITogglableMod.Unload 里取消
PlayMakerHooks.RemoveStateListener(callback);
```

接到已有系统上的效果：`FsmEventHooks.RegisterEventListener/RegisterEventFilter(fsmName, …)` 在 FSM 处理事件时触发（过滤器返回 false 吞掉事件）；
`FsmVariableHooks.RegisterVariableListener/RegisterVariableModifier(fsmName, varName, …)` 在该 FSM 执行期间写变量时触发
（用 `FsmExecutionStack.ExecutingFsm` 判断是哪个 FSM，FSM 之外写入的不触发）。

构建：`build.sh` 第 4 步。补丁失败会自动降级：变量写入钩子（属性 setter 的补丁，最容易出问题）失败 → 关掉它重试；再失败 → 整个 PlayMaker 补丁跳过，
`ModHooks.Fsm*Hook` 和 `PlayMakerBridge` 随 `NO_PLAYMAKERHOOKS` 一起排除，`FsmUtil` 等不受影响。
没有做 per-Action 钩子（FsmState 执行 Action 的循环要整段重写，风险太大），需要时用 `FsmUtil.InsertAction / InsertMethod`。

**没在真机上验证**：补丁签名对照 `de.zip` 的 PlayMaker 反编译源码（`PlayMakerFSM.Awake/OnEnable/Start/OnDestroy`、`Fsm.ProcessEvent/EnterState/ExitState/DoTransition`、`FsmFloat/Int/Bool/String.Value`）写成。

## 构建与安装（macOS）

需要：.NET SDK 8、`git`，以及 MonoMod 发布包。

1. 从 MonoMod 的 GitHub Releases 下载 **22.7.31.1**（net6.0 或 netcoreapp 版本）解压到 `tools/MonoMod/`
   （里面要有 `MonoMod.dll`（或 `.exe`）、`MonoMod.RuntimeDetour.HookGen.*`、`MonoMod.Utils.dll`、`MonoMod.RuntimeDetour.dll`、`Mono.Cecil*.dll`）。
2. `./build.sh`（默认路径是 Steam 的 `~/Library/Application Support/Steam/steamapps/common/Hollow Knight Silksong/...app/Contents/Resources/Data/Managed`；不同请设 `GAME_MANAGED`）。
3. 脚本会先备份 `Assembly-CSharp.dll.vanilla`，之后每次都从这份备份出发，不会重复打补丁。
4. 写 Mod：参考 `Examples/ExampleMod`，编译出的 dll 放进 `Managed/Mods/`。

卸载：Steam 验证游戏文件完整性，或把 `Assembly-CSharp.dll.vanilla` 改回 `Assembly-CSharp.dll`。
