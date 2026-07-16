using System;

namespace SilksongModLoader
{
    /// <summary>
    /// Prepatcher 打桩成功后,会往 Assembly-CSharp 程序集上附加这个 Attribute,
    /// 用来判断"是否已经打过桩",避免重复插入调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class PatchedMarkerAttribute : Attribute
    {
    }
}
