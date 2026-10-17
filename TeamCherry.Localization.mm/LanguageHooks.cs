using System;
using UnityEngine;

namespace TeamCherry.Localization
{





    public static class LanguageHooks
    {

        public static event Func<string, string, string, string> GetHook;


        public static event Func<string, string, bool, bool> HasHook;


        public static event Action<LanguageCode> LanguageChangedHook;

        internal static string OnGet(string key, string sheet, string orig)
        {
            Func<string, string, string, string> h = GetHook;
            if (h == null) return orig;
            string res = orig;
            foreach (Delegate d in h.GetInvocationList())
            {
                try { res = ((Func<string, string, string, string>) d)(key, sheet, res); }
                catch (Exception e) { Debug.LogError("[LanguageHooks] GetHook 抛出异常: " + e); }
            }
            return res;
        }

        internal static bool OnHas(string key, string sheet, bool orig)
        {
            Func<string, string, bool, bool> h = HasHook;
            if (h == null) return orig;
            bool res = orig;
            foreach (Delegate d in h.GetInvocationList())
            {
                try { res = ((Func<string, string, bool, bool>) d)(key, sheet, res); }
                catch (Exception e) { Debug.LogError("[LanguageHooks] HasHook 抛出异常: " + e); }
            }
            return res;
        }

        internal static void OnLanguageChanged(LanguageCode code)
        {
            Action<LanguageCode> h = LanguageChangedHook;
            if (h == null) return;
            foreach (Delegate d in h.GetInvocationList())
            {
                try { ((Action<LanguageCode>) d)(code); }
                catch (Exception e) { Debug.LogError("[LanguageHooks] LanguageChangedHook 抛出异常: " + e); }
            }
        }
    }
}
