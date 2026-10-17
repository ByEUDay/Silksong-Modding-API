using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Modding
{

    public static class ModResources
    {
        public static string FindResourceName(Assembly asm, string nameSuffix) =>
            asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(nameSuffix, StringComparison.OrdinalIgnoreCase));

        public static byte[] LoadBytes(Assembly asm, string nameSuffix)
        {
            string name = FindResourceName(asm, nameSuffix);
            if (name == null)
            {
                Logger.LogError($"{asm.GetName().Name} 里没有嵌入资源 {nameSuffix}");
                return null;
            }
            using (Stream s = asm.GetManifestResourceStream(name))
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string LoadString(Assembly asm, string nameSuffix)
        {
            byte[] b = LoadBytes(asm, nameSuffix);
            return b == null ? null : System.Text.Encoding.UTF8.GetString(b);
        }

        public static Texture2D LoadTexture(Assembly asm, string nameSuffix, FilterMode filter = FilterMode.Bilinear)
        {
            byte[] bytes = LoadBytes(asm, nameSuffix);
            if (bytes == null) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes, false))
            {
                Logger.LogError($"无法解码贴图 {nameSuffix}");
                return null;
            }
            tex.filterMode = filter;
            return tex;
        }

        public static Sprite LoadSprite(Assembly asm, string nameSuffix, float pixelsPerUnit = 100f, Vector2? pivot = null, FilterMode filter = FilterMode.Bilinear)
        {
            Texture2D tex = LoadTexture(asm, nameSuffix, filter);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
