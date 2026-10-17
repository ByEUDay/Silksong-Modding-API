#!/usr/bin/env bash
# Silksong Modding API 构建 / 安装脚本（macOS，Steam 版）
#
# 流程：
#   1. 备份并读取原版 Assembly-CSharp.dll、TeamCherry.Localization.dll、PlayMaker.dll
#   2. 构建 PrePatcher
#   3. 给 TeamCherry.Localization.dll 打补丁（LanguageHooks）。失败会自动跳过，只是没有语言钩子
#   4. 给 PlayMaker.dll 打补丁（PlayMakerHooks）。失败会自动跳过；变量写入钩子失败会单独关掉重试
#   5. 编译 Assembly-CSharp.mm（补丁 + Modding 运行时）
#   6. PrePatcher + MonoMod：把补丁合并进 Assembly-CSharp.dll
#   7. HookGen：生成 MMHOOK_Assembly-CSharp / MMHOOK_PlayMaker / MMHOOK_TeamCherry.*
#   8. 汇总并安装到游戏的 Managed 目录
#
# 用法:  ./build.sh [--no-install]
# 环境变量:
#   GAME_MANAGED   游戏 Managed 目录（默认 Steam 路径）
#   MONOMOD_DIR    打补丁用的 MonoMod 目录（需要 MonoMod.dll 或 MonoMod.exe）。默认 ./tools/MonoMod
#   HOOKGEN_DIR    HookGen 所在目录，默认同 MONOMOD_DIR
#   RUNTIME_DIR    要随 API 分发的运行时 dll 目录（MonoMod.Core/RuntimeDetour/Utils/Cecil 等），默认同 MONOMOD_DIR
#   HOOK_TARGETS   要生成 MMHOOK 的程序集名（空格分隔），默认自动取 PlayMaker 和全部 TeamCherry.*
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
GAME_MANAGED="${GAME_MANAGED:-$HOME/Library/Application Support/Steam/steamapps/common/Hollow Knight Silksong/Hollow Knight Silksong.app/Contents/Resources/Data/Managed}"
MONOMOD_DIR="${MONOMOD_DIR:-$ROOT/tools/MonoMod}"
HOOKGEN_DIR="${HOOKGEN_DIR:-$MONOMOD_DIR}"
RUNTIME_DIR="${RUNTIME_DIR:-$MONOMOD_DIR}"
VANILLA="$ROOT/Vanilla"
WORK="$ROOT/work"
LOC_WORK="$ROOT/work_loc"
PM_WORK="$ROOT/work_pm"
OUT="$ROOT/OutputFinal"
INSTALL=1
[[ "${1:-}" == "--no-install" ]] && INSTALL=0

[[ -d "$GAME_MANAGED" ]] || { echo "找不到游戏目录: $GAME_MANAGED（用 GAME_MANAGED 指定）"; exit 1; }

# 兼容 MonoMod 的 .dll(.NET Core) 与 .exe(mono) 两种发布形式
run_tool() {   # run_tool <目录> <工具名> 参数...
  local dir="$1" name="$2"; shift 2
  if   [[ -f "$dir/$name.dll" ]]; then dotnet --roll-forward Major "$dir/$name.dll" "$@"
  elif [[ -f "$dir/$name.exe" ]]; then mono "$dir/$name.exe" "$@"
  else echo "在 $dir 找不到 $name.dll / $name.exe"; exit 1; fi
}

backup() {   # backup <程序集名>：首次备份为 .vanilla，之后永远从备份出发
  local n="$1"
  if [[ ! -f "$GAME_MANAGED/$n.dll.vanilla" ]]; then
    cp "$GAME_MANAGED/$n.dll" "$GAME_MANAGED/$n.dll.vanilla"
    echo "    已备份原版 -> $n.dll.vanilla"
  fi
}

echo "==> 1/8 准备原版文件"
backup Assembly-CSharp
HAS_LOC=0
if [[ -f "$GAME_MANAGED/TeamCherry.Localization.dll" ]]; then backup TeamCherry.Localization; HAS_LOC=1; fi
HAS_PM=0
if [[ -f "$GAME_MANAGED/PlayMaker.dll" ]]; then backup PlayMaker; HAS_PM=1; fi
rm -rf "$VANILLA" "$WORK" "$LOC_WORK" "$PM_WORK"
mkdir -p "$VANILLA" "$WORK"
# 复制 Managed 下的全部 dll（排除我们装进去的 MonoMod/MMHOOK 文件，以及要从备份还原的两个）
find "$GAME_MANAGED" -maxdepth 1 -name '*.dll' \
  ! -name 'MMHOOK_*' ! -name 'MonoMod*' ! -name 'Mono.Cecil*' \
  ! -name 'Assembly-CSharp.dll' ! -name 'TeamCherry.Localization.dll' ! -name 'PlayMaker.dll' \
  -exec cp {} "$VANILLA/" \;
cp "$GAME_MANAGED/Assembly-CSharp.dll.vanilla" "$VANILLA/Assembly-CSharp.dll"
[[ $HAS_LOC -eq 1 ]] && cp "$GAME_MANAGED/TeamCherry.Localization.dll.vanilla" "$VANILLA/TeamCherry.Localization.dll"
[[ $HAS_PM -eq 1 ]] && cp "$GAME_MANAGED/PlayMaker.dll.vanilla" "$VANILLA/PlayMaker.dll"

echo "==> 2/8 构建 PrePatcher"
dotnet build "$ROOT/PrePatcher/PrePatcher.csproj" -c Release -o "$ROOT/PrePatcher/out" --nologo -v q

echo "==> 3/8 TeamCherry.Localization 补丁（语言钩子）"
build_localization() {
  dotnet build "$ROOT/TeamCherry.Localization.mm/TeamCherry.Localization.mm.csproj" -c Release \
    -o "$ROOT/TeamCherry.Localization.mm/out" -p:VanillaDir="$VANILLA" --nologo -v q || return 1
  mkdir -p "$LOC_WORK"
  cp "$VANILLA"/*.dll "$LOC_WORK/" || return 1
  cp "$ROOT/TeamCherry.Localization.mm/out/TeamCherry.Localization.mm.dll" "$LOC_WORK/" || return 1
  ( cd "$LOC_WORK" && run_tool "$MONOMOD_DIR" MonoMod TeamCherry.Localization.dll ) || return 1
  [[ -f "$LOC_WORK/MONOMODDED_TeamCherry.Localization.dll" ]] || return 1
  cp "$LOC_WORK/MONOMODDED_TeamCherry.Localization.dll" "$VANILLA/TeamCherry.Localization.dll" || return 1
  return 0
}
NO_LANG=true
if [[ $HAS_LOC -eq 1 ]]; then
  if build_localization; then
    NO_LANG=false
    echo "    Localization 补丁成功"
  else
    echo "    !! Localization 补丁失败，已跳过（没有 LanguageGetHook 等语言钩子）。"
    echo "       多半是 Silksong 的 Language 类和补丁假设的签名不一致，把上面的报错贴出来即可。"
    cp "$GAME_MANAGED/TeamCherry.Localization.dll.vanilla" "$VANILLA/TeamCherry.Localization.dll"
  fi
else
  echo "    游戏目录里没有 TeamCherry.Localization.dll，跳过"
fi

echo "==> 4/8 PlayMaker 补丁（FSM 钩子）"
build_playmaker() {   # build_playmaker <true|false>：是否关闭变量写入钩子
  local no_vars="${1:-false}"
  rm -rf "$PM_WORK" "$ROOT/PlayMaker.mm/out" "$ROOT/PlayMaker.mm/obj"
  dotnet build "$ROOT/PlayMaker.mm/PlayMaker.mm.csproj" -c Release \
    -o "$ROOT/PlayMaker.mm/out" -p:VanillaDir="$VANILLA" -p:NoVariableHooks="$no_vars" --nologo -v q || return 1
  mkdir -p "$PM_WORK"
  cp "$VANILLA"/*.dll "$PM_WORK/" || return 1
  cp "$ROOT/PlayMaker.mm/out/PlayMaker.mm.dll" "$PM_WORK/" || return 1
  ( cd "$PM_WORK" && run_tool "$MONOMOD_DIR" MonoMod PlayMaker.dll ) || return 1
  [[ -f "$PM_WORK/MONOMODDED_PlayMaker.dll" ]] || return 1
  cp "$PM_WORK/MONOMODDED_PlayMaker.dll" "$VANILLA/PlayMaker.dll" || return 1
  return 0
}
NO_PM=true
PM_VARS=关闭
if [[ $HAS_PM -eq 1 ]]; then
  if build_playmaker false; then
    NO_PM=false; PM_VARS=开启
    echo "    PlayMaker 补丁成功（含变量写入钩子）"
  else
    echo "    !! PlayMaker 补丁失败，关闭变量写入钩子后重试"
    cp "$GAME_MANAGED/PlayMaker.dll.vanilla" "$VANILLA/PlayMaker.dll"
    if build_playmaker true; then
      NO_PM=false
      echo "    PlayMaker 补丁成功（没有变量写入钩子；FsmVariableHooks 不会触发）"
    else
      echo "    !! PlayMaker 补丁失败，已跳过（没有 FSM 钩子，FsmUtil 仍然可用）。把上面的报错贴出来即可。"
      cp "$GAME_MANAGED/PlayMaker.dll.vanilla" "$VANILLA/PlayMaker.dll"
    fi
  fi
else
  echo "    游戏目录里没有 PlayMaker.dll，跳过"
fi

echo "==> 5/8 编译 Assembly-CSharp.mm（Language 钩子转发: ${NO_LANG/true/关闭}${NO_LANG/false/开启}；PlayMaker 钩子转发: ${NO_PM/true/关闭}${NO_PM/false/开启}）"
build_mm() {   # build_mm [额外宏]
  dotnet build "$ROOT/Assembly-CSharp.mm/Assembly-CSharp.mm.csproj" -c Release -o "$ROOT/Assembly-CSharp.mm/out" \
    -p:VanillaDir="$VANILLA" -p:NoLanguageHooks="$NO_LANG" -p:NoPlayMakerHooks="$NO_PM" ${1:+-p:ExtraDefines=$1} --nologo -v q
}
if ! build_mm; then
  echo "    !! 完整编译失败。关闭 FsmUtil（依赖 PlayMaker API）和 Preloader（依赖 Addressables API）后重试"
  echo "       （上面的报错请保留，贴给我可以修好它们）"
  build_mm "NO_FSMUTIL%3BNO_PRELOADER"
fi

echo "==> 6/8 PrePatcher + MonoMod 合并补丁"
cp "$VANILLA"/*.dll "$WORK/"                       # 依赖 DLL（含原版 Assembly-CSharp，下一步会被覆盖；Localization 已是打过补丁的版本）
dotnet "$ROOT/PrePatcher/out/PrePatcher.dll" "$VANILLA/Assembly-CSharp.dll" "$WORK/Assembly-CSharp.dll" "$VANILLA"
cp "$ROOT/Assembly-CSharp.mm/out/Assembly-CSharp.mm.dll" "$WORK/"
( cd "$WORK" && run_tool "$MONOMOD_DIR" MonoMod Assembly-CSharp.dll )
[[ -f "$WORK/MONOMODDED_Assembly-CSharp.dll" ]] || { echo "MonoMod 没有产出 MONOMODDED_Assembly-CSharp.dll"; exit 1; }

echo "==> 7/8 HookGen（为 Assembly-CSharp、PlayMaker、TeamCherry.* 各生成 MMHOOK）"
# Assembly-CSharp 用打完补丁的版本；其余对 WORK 里的版本生成（Localization 已含 LanguageHooks）
cp "$WORK/MONOMODDED_Assembly-CSharp.dll" "$WORK/Assembly-CSharp.hookgen.dll"
( cd "$WORK" && run_tool "$HOOKGEN_DIR" MonoMod.RuntimeDetour.HookGen --private Assembly-CSharp.hookgen.dll MMHOOK_Assembly-CSharp.dll )
cp "$WORK/MMHOOK_Assembly-CSharp.dll" "$OUT/"

if [[ -z "${HOOK_TARGETS:-}" ]]; then
  HOOK_TARGETS=""
  [[ -f "$VANILLA/PlayMaker.dll" ]] && HOOK_TARGETS="PlayMaker"
  for f in "$VANILLA"/TeamCherry.*.dll; do
    [[ -f "$f" ]] && HOOK_TARGETS="$HOOK_TARGETS $(basename "$f" .dll)"
  done
fi
echo "    目标: Assembly-CSharp $HOOK_TARGETS"
for t in $HOOK_TARGETS; do
  if ( cd "$WORK" && run_tool "$HOOKGEN_DIR" MonoMod.RuntimeDetour.HookGen --private "$t.dll" "MMHOOK_$t.dll" ) >"$WORK/hookgen_$t.log" 2>&1; then
    cp "$WORK/MMHOOK_$t.dll" "$OUT/" && echo "    ok  MMHOOK_$t.dll"
  else
    echo "    失败 $t（日志: $WORK/hookgen_$t.log），已跳过"
  fi
done

echo "==> 8/8 汇总产物"
cp "$WORK/MONOMODDED_Assembly-CSharp.dll" "$OUT/Assembly-CSharp.dll"
[[ "$NO_LANG" == "false" ]] && cp "$VANILLA/TeamCherry.Localization.dll" "$OUT/TeamCherry.Localization.dll"
[[ "$NO_PM" == "false" ]] && cp "$VANILLA/PlayMaker.dll" "$OUT/PlayMaker.dll"
# 运行时依赖（MonoMod 25.x 会有 Core/Iced/Backports/ILHelpers；22.x 没有，缺的自动跳过）
for f in MonoMod.Utils MonoMod.RuntimeDetour MonoMod.Core MonoMod.Iced MonoMod.Backports MonoMod.ILHelpers \
         Mono.Cecil Mono.Cecil.Mdb Mono.Cecil.Pdb Mono.Cecil.Rocks System.ValueTuple; do
  [[ -f "$RUNTIME_DIR/$f.dll" ]] && cp "$RUNTIME_DIR/$f.dll" "$OUT/"
done
mkdir -p "$OUT/Mods"

if [[ $INSTALL -eq 1 ]]; then
  find "$OUT" -maxdepth 1 -name '*.dll' -exec cp {} "$GAME_MANAGED/" \;
  mkdir -p "$GAME_MANAGED/Mods"
  echo "已安装到: $GAME_MANAGED"
  echo "把 Mod 的 dll 放进: $GAME_MANAGED/Mods/"
else
  echo "产物在: $OUT"
fi
echo "卸载: 在 Steam 中验证游戏文件完整性，或把 *.dll.vanilla 改回原名"
