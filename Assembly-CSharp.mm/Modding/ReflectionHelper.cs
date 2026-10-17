using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modding
{

    public static class ReflectionHelper
    {
        private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<(Type, string), FieldInfo> Fields = new Dictionary<(Type, string), FieldInfo>();
        private static readonly Dictionary<(Type, string), MethodInfo> Methods = new Dictionary<(Type, string), MethodInfo>();

        private static FieldInfo Field(Type t, string name)
        {
            if (Fields.TryGetValue((t, name), out FieldInfo f)) return f;
            for (Type cur = t; cur != null && f == null; cur = cur.BaseType)
                f = cur.GetField(name, All | BindingFlags.DeclaredOnly);
            Fields[(t, name)] = f;
            return f;
        }

        private static MethodInfo Method(Type t, string name)
        {
            if (Methods.TryGetValue((t, name), out MethodInfo m)) return m;
            for (Type cur = t; cur != null && m == null; cur = cur.BaseType)
                m = cur.GetMethod(name, All | BindingFlags.DeclaredOnly);
            Methods[(t, name)] = m;
            return m;
        }


        public static TField GetField<TField>(object instance, string name)
        {
            FieldInfo f = Field(instance.GetType(), name);
            return f == null ? default : (TField) f.GetValue(instance);
        }

        public static bool SetField(object instance, string name, object value)
        {
            FieldInfo f = Field(instance.GetType(), name);
            if (f == null) return false;
            f.SetValue(instance, value);
            return true;
        }

        public static TField GetStaticField<TField>(Type type, string name)
        {
            FieldInfo f = Field(type, name);
            return f == null ? default : (TField) f.GetValue(null);
        }


        public static object Call(object instance, string name, params object[] args)
        {
            MethodInfo m = Method(instance.GetType(), name);
            return m?.Invoke(instance, args);
        }
    }
}
