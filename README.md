# Silksong Modding API

## 安装

### 前置条件

- .NET SDK 8
- MonoMod 22.7.31.1（net6.0 或 netcoreapp 版本），解压到 `tools/MonoMod/`

### 构建

```bash
./build.sh
```
没有build.ps1，懒得写，也无法验证。

默认目标为 Steam macOS 路径（作者是Mac）。其他路径请设置环境变量：

```bash
GAME_MANAGED="/your/game/path/to/Managed" ./build.sh
```

脚本会自动备份原版 DLL（`.dll.vanilla`），首次之后都从备份出发，不会重复打补丁。

### 安装 Mod

将编译好或从Release下载打包好的 Mod DLL 放入：

```
<游戏Managed目录>/Mods/
```

### 卸载

Steam 验证游戏文件完整性，或将 `*.dll.vanilla` 改回原名。

## ExampleMod 使用的 Hook

```csharp
public class ExampleMod : Mod, IGlobalSettings<GlobalData>, ILocalSettings<SaveData>
```

### 场景与状态

| Hook | 签名 | 说明 |
|------|------|------|
| `SceneEnteredHook` | `Action<string>` | 主角进入场景完成，参数为场景名 |
| `GameStateChangedHook` | `Action<GameState>` | 游戏状态切换（如 MENU → PLAYING） |

### 战斗与数值（可修改返回值）

| Hook | 签名 | 说明 |
|------|------|------|
| `TakeDamageHook` | `Func<HazardType, int, int>` | 受伤时触发，返回修改后的伤害值 |
| `AddHealthHook` | `Func<int, int>` | 回血时触发，返回修改后的回血量 |
| `AddSilkHook` | `Func<int, int>` | 获得丝线时触发，返回修改后的数量 |
| `CurrencyChangeHook` | `Func<CurrencyType, int, int>` | 货币变化时触发，返回修改后的数量 |

### 敌人

| Hook | 签名 | 说明 |
|------|------|------|
| `EnemyDeathHook` | `Action<HealthManager>` | 敌人死亡时触发 |

### 主角动作

| Hook | 签名 | 说明 |
|------|------|------|
| `HeroJumpHook` | `Action` | 跳跃时触发 |
| `HeroDashHook` | `Action` | 冲刺时触发 |

### 设置持久化

| 接口 | 说明 |
|------|------|
| `IGlobalSettings<T>` | 全局设置，自动读写 JSON（所有存档共享） |
| `ILocalSettings<T>` | 存档设置，按存档槽自动读写 JSON |
