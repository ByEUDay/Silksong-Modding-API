# SilksongModLoader(不依赖 BepInEx 的模组 API 骨架)

这是一套仿照 `hk-modding/api`(Hollow Knight 官方 Modding API,同样不依赖 BepInEx)
思路搭的骨架:**Mono.Cecil 预打桩 + MonoMod.RuntimeDetour 运行时 hook**。

> **重要:这是骨架,不是可以直接用的成品。** 我(Claude)没有网络访问权限,无法帮你
> `dotnet restore` 下载 Mono.Cecil / MonoMod.RuntimeDetour,也没有 Silksong 的游戏文件,
> 所以以下内容**没有经过实际编译和游戏内测试**。你需要在自己的开发环境里完成编译验证。

## GodModeMod:一个有实际效果的示例

`GodModeMod/` 是一个"按 F1 切换无敌模式"的示例,通过反射持续把玩家生命值字段写成
最大值。**代码里有 3 个占位符必须你自己核对修改**(`PlayerDataTypeName` /
`InstanceMemberName` / `HealthFieldName`),具体做法:

1. 用 dnSpy/ilspycmd 打开 `Assembly-CSharp.dll`
2. 全局搜索 `PlayerData`、`HeroController`、`PlayerStats` 这类类名,找到管理生命值的那个类
3. 在这个类里搜 `health`、`hp`、`hitPoint` 之类字段名,记下真实名字
4. 找这个类是怎么被访问的(通常有个静态的 `instance`/`Instance` 成员),记下真实成员名
5. 把这三个值改进 `GodModeMod/GodModeMod.cs` 顶部的常量,重新编译

编译时如果报 `Input`/`KeyCode` 找不到,检查你游戏 Managed 目录里到底有没有叫
`UnityEngine.InputLegacyModule.dll` 的文件,没有的话看看叫什么名字,改
`GodModeMod.csproj` 里的 `Reference Include` 和 `HintPath`。

## OneHitKillMod:一击必杀示例

`OneHitKillMod/` 和 GodModeMod 是同一个写法(纯反射轮询,不用 MonoMod hook):按 F3
切换后,持续把 `PlayerData` 里的攻击力字段强行改成一个很大的数。**同样有 3 个占位符
必须核对**(`PlayerDataTypeName` / `InstanceMemberName` / `AttackDamageFieldName`),
流程和 GodModeMod 的说明完全一样,去 dnSpy 里搜 `PlayerData` 类里带 `damage`/`attack`/
`nail` 关键词的 int 字段即可。

注意:这种"堆高攻击力数值"的打法,如果游戏本身有伤害上限、抗性、boss 处决前的免疫阶段
之类的机制,可能打不穿——遇到这种情况说明需要换成直接 hook 敌人受伤方法
(`HealthManager.TakeDamage` 一类,把伤害/血量直接归零)的思路,比现在这版更复杂,
可以之后单独讨论。

## RandomItemMod:随机物品示例

`RandomItemMod/` 是一个"拿到的道具被随机换成物品池里另一个道具"的示例,思路照抄
hk-modding 系的常见做法:hook `PlayerData.SetBool(string, bool)`,在游戏把某个收集品
flag 设为 `true`(代表拿到了)的那一刻,把 flag 名换成随机抽到的另一个,再放行。

**默认是关着的**:`TriggerFlags` 和 `ItemPool` 两个数组默认是空的占位符,mod 加载后只会
打一行日志提醒你还没配置,不会动游戏任何状态,避免瞎猜字段名导致存档异常。你需要:

1. 用 dnSpy/ILSpy 打开 `PlayerData` 类,找到工具/护符解锁之类的 bool 字段
2. 确认这些字段是在哪个"拾取/收集"脚本里被 `SetBool(fieldName, true)` 赋值的
3. 把字段名填进 `RandomItemMod/RandomItemMod.cs` 顶部的 `TriggerFlags`(会被随机替换的
   触发点)和 `ItemPool`(随机替换成什么),建议先放"工具/护符解锁"这类互不依赖的收集品,
   避免随出打不过的开局(比如把开地图必需的道具也扔进池子里)
4. 重新编译,丢进 `Managed/Mods/` 目录

同样地,`PlayerDataTypeName` / `SetBoolMethodName` / `GetBoolMethodName` /
`InstanceMemberName` 这几个类名/方法名也需要你核对后再定,和 GodModeMod 是一样的流程。

## 加载进度条 + 左上角 Mod 列表

现在游戏启动后会看到:
- 屏幕中间偏上的一条加载进度条,mod 逐个加载时实时刷新(`3/8` 这种）
- 屏幕左上角常驻显示所有已加载的 mod 及版本号,加载失败的会标红并显示"加载失败"

这是用 Unity 自带的 `OnGUI`(IMGUI)实现的,不依赖游戏本身的 UI 系统。如果编译时报
`UnityEngine.IMGUIModule` 找不到,去 Managed 目录确认真实文件名(某些 Unity 版本可能
拆分模块方式不同),改 `SilksongModLoader.csproj` 里对应的 `HintPath`。

## 目录结构

```
SilksongModding/
├── SilksongModding.sln
├── Directory.Build.props      ← 改这里的游戏路径
├── SilksongModLoader/         ← 核心库:Entry / IMod / ModHooks / 内部 Hook
├── SilksongPrepatcher/        ← 命令行工具:给 Assembly-CSharp.dll 打桩
└── SampleMod/                 ← 示例第三方 mod
```

## 使用前必须做的事(我这边做不了的部分)

1. **确认 Unity backend**:用 dnSpy/ILSpy 打开
   `Hollow Knight Silksong_Data/Managed/Assembly-CSharp.dll`。
   - 能正常看到 C# 类和方法体 → Mono backend,骨架可以直接用。
   - 打不开或者全是壳 → IL2CPP backend,这套方案的运行时 hook 部分需要
     额外引入 Il2CppInterop 生成的 interop 程序集,工作量显著增加,
     需要单独讨论方案。

2. **核对插桩点**:`SilksongPrepatcher/Program.cs` 里写死了
   `GameManager.Awake()` 作为插桩目标,这是 Hollow Knight 系列常见的入口,
   **但没有验证过在 Silksong 里是否叫这个名字/是否可行**。打开 dnSpy 搜索
   `GameManager` 类,确认存在 `Awake()` 方法且在游戏启动早期必然执行。
   如果类名/方法名不同,改 `Program.cs` 里的 `TargetTypeName` /
   `TargetMethodName` 常量。

3. **核对 InternalHooks.cs 里的类名**:同理,`HeroController.Update` 这类
   写死的类名/方法名需要用 dnSpy 核对,并按需要补充更多游戏内 hook 点
   (比如存档相关的方法)。

## 构建步骤

```bash
# 1. 指定你本地游戏的 Managed 目录(不再有默认值,必须显式传,
#    否则 Directory.Build.props 会直接报错终止,而不是给一堆看不懂的引用错误)
export SILKSONG_MANAGED="<你的游戏 Managed 目录路径>"

# 2. 还原并编译 Loader
dotnet build SilksongModLoader -p:SilksongManagedDir="$SILKSONG_MANAGED"

# 3. 编译 Prepatcher(这一步不需要游戏路径,Prepatcher 只用 Mono.Cecil 读写 dll)
dotnet build SilksongPrepatcher

# 4. 运行 Prepatcher,对游戏 dll 打桩(会自动备份 .orig)
dotnet run --project SilksongPrepatcher -- "$SILKSONG_MANAGED" \
    SilksongModLoader/bin/Debug/netstandard2.1/SilksongModLoader.dll

# 5. 把 Loader 及其依赖(MonoMod.RuntimeDetour.dll 等)复制到游戏 Managed 目录

# 6. 编译示例 mod,丢进 Managed/Mods/ 目录
dotnet build SampleMod -p:SilksongManagedDir="$SILKSONG_MANAGED"
```

Windows 下用 PowerShell 的 `$env:SILKSONG_MANAGED = "..."` 替代 `export` 即可,后面命令原样用。

## 发布到 GitHub / 跨平台编译

**先弄清楚一件事:这个仓库里的 mod dll(netstandard2.1)本身不用"分平台编译"。**
它们是纯 IL,编译一次,Windows/Linux/macOS 上的游戏都能直接加载同一份 dll——
不存在"Windows 版 GodModeMod.dll"和"macOS 版 GodModeMod.dll"的区别。真正需要
考虑平台的只有两件事:

**1. 编译时引用的 `Assembly-CSharp.dll` / `UnityEngine*.dll` 是游戏本体文件,受版权保护**

这些文件不能提交进公开仓库,GitHub Actions 的 CI 环境里也不会有(没人会把游戏装到
CI 机器上,也不应该把游戏文件传上去)。所以:

- 仓库里只放源码,`.gitignore` 已经排除了 `bin/`、`obj/`、`*.dll`
- 想要编译 Loader/Mod 项目的人,必须自己拥有正版游戏,在本地通过
  `-p:SilksongManagedDir=` 指定自己的路径去编译,这部分做不了自动化 CI
- README 里已经把这个流程写清楚了(见上面"构建步骤"),GitHub 上的用户
  照着做就行,不需要你额外为三个平台各出一份编译教程——步骤对三个平台是一样的,
  区别只在游戏安装路径长什么样(见 `Directory.Build.props` 里的注释)

**2. `SilksongPrepatcher` 是普通 .NET 控制台程序,不依赖游戏文件,这个可以出跨平台成品**

它只用 Mono.Cecil 读写 IL,不引用任何游戏 dll,可以放心在 CI 里编译。仓库里加了
`.github/workflows/build.yml`,推到 GitHub 后会自动用 `dotnet publish -r <RID>
--self-contained` 分别产出 `win-x64` / `linux-x64` / `osx-x64` / `osx-arm64`
四份独立可执行文件(不需要用户装 .NET 运行时),在 Actions 页面能下载。
本地手动跑同样的命令也行,比如 Windows 版:

```bash
dotnet publish SilksongPrepatcher -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

把 `win-x64` 换成 `linux-x64` / `osx-x64` / `osx-arm64` 就是对应平台的产物,
在哪台机器上跑这条命令都行,不需要真的在 Windows 上才能编出 Windows 版。

**3. 发布前检查清单**

- [ ] `Directory.Build.props` 里确认没有残留任何人的真实路径/用户名
- [ ] `git status` / `git diff` 过一遍,确认没有 `bin/`、`obj/`、`*.dll`、
      `*.orig` 被意外加进暂存区
- [ ] README 里如果贴过日志截图,检查有没有暴露本机用户名或存档路径
- [ ] 仓库描述/README 里说明清楚这是"教学/框架性质的骨架",很多类名方法名
      是占位符需要使用者自己用 dnSpy 核对,管理预期,避免有人直接拿去用崩了
      来找你

## 卸载/恢复

Prepatcher 第一次运行时会生成 `Assembly-CSharp.dll.orig`。恢复原版只需要:

```bash
copy "Assembly-CSharp.dll.orig" "Assembly-CSharp.dll"   # Windows
cp "Assembly-CSharp.dll.orig" "Assembly-CSharp.dll"      # Linux/macOS
```

## 还没做、需要你继续扩展的部分

- 存档读写相关的 hook(`OnSaveGameLoaded` / `OnSaveGameSaved` 目前只声明了事件,没有真正接到游戏方法上)
- Mod 自己的配置文件读写工具类
- 图形化的 Mod 管理器(参考 Lumafly/Needlelight 的做法,可以是独立的 Avalonia/WPF 项目)
- 版本校验:游戏更新后 `Assembly-CSharp.dll` 的方法体会变,需要有机制检测"当前打桩是否仍然有效",避免用旧插桩的 dll 崩溃
- 如果确认是 IL2CPP backend,需要重新设计运行时 hook 部分
