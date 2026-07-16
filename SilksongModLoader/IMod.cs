using System.Collections.Generic;

namespace SilksongModLoader
{
    /// <summary>
    /// 所有第三方 Mod 需要实现的接口。Loader 会用反射扫描 Mods 目录下的
    /// dll,找到所有实现了此接口的非抽象类并实例化。
    /// </summary>
    public interface IMod
    {
        /// <summary>显示名称,会出现在标题画面的 mod 列表里。</summary>
        string Name { get; }

        /// <summary>版本号,建议遵循语义化版本。</summary>
        string Version { get; }

        /// <summary>
        /// 声明依赖的其他 mod 名称(必须与对方 IMod.Name 完全一致)。
        /// Loader 会保证依赖项先于本 mod 初始化;找不到依赖时会跳过并记录日志。
        /// </summary>
        IEnumerable<string> Dependencies => System.Array.Empty<string>();

        /// <summary>
        /// 游戏启动、Loader 完成基础 hook 挂载后调用一次。
        /// 在这里订阅 ModHooks 里的事件、读取/写入自己的配置文件等。
        /// </summary>
        void Initialize();
    }
}
