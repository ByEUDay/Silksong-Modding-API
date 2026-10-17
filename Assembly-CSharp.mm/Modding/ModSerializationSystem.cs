using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{



    public static class ModSerializationSystem
    {



        public static string ToJson<T>(T obj, bool prettyPrint = false)
        {
            try
            {
                return JsonUtility.ToJson(obj, prettyPrint);
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 序列化失败: {e}");
                return null;
            }
        }




        public static T FromJson<T>(string json)
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 反序列化失败: {e}");
                return default;
            }
        }




        public static byte[] ToBytes<T>(T obj)
        {
            try
            {
                string json = ToJson(obj);
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 序列化失败: {e}");
                return null;
            }
        }




        public static T FromBytes<T>(byte[] bytes)
        {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                return FromJson<T>(json);
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 反序列化失败: {e}");
                return default;
            }
        }




        public static void SaveToFile<T>(string path, T obj, bool prettyPrint = false)
        {
            try
            {
                string json = ToJson(obj, prettyPrint);
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 保存失败: {e}");
            }
        }




        public static T LoadFromFile<T>(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    return FromJson<T>(json);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[ModSerializationSystem] 加载失败: {e}");
            }
            return default;
        }
    }




    public static class ModEncryptionSystem
    {



        public static byte[] XOR(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }




        public static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }




        public static byte[] Base64Decode(string base64)
        {
            return Convert.FromBase64String(base64);
        }




        public static string MD5Hash(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }




        public static string SHA256Hash(byte[] data)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }




    public static class ModCompressionSystem
    {



        public static byte[] GZipCompress(byte[] data)
        {
            using (var output = new System.IO.MemoryStream())
            {
                using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }




        public static byte[] GZipDecompress(byte[] data)
        {
            using (var input = new System.IO.MemoryStream(data))
            {
                using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        gzip.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }
    }
}
