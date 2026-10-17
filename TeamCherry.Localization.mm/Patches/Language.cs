using MonoMod;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace TeamCherry.Localization.Patches
{

    [MonoModPatch("global::TeamCherry.Localization.Language")]
    public static class patch_Language
    {

        public static extern string orig_Get(string key, string sheetTitle);

        public static string Get(string key, string sheetTitle)
        {
            return LanguageHooks.OnGet(key, sheetTitle, orig_Get(key, sheetTitle));
        }


        public static extern bool orig_Has(string key, string sheetTitle);

        public static bool Has(string key, string sheetTitle)
        {
            return LanguageHooks.OnHas(key, sheetTitle, orig_Has(key, sheetTitle));
        }



        public static extern bool orig_SwitchLanguage(LanguageCode code);

        public static bool SwitchLanguage(LanguageCode code)
        {
            LanguageCode before = Language.CurrentLanguage();
            bool result = orig_SwitchLanguage(code);
            LanguageCode after = Language.CurrentLanguage();
            if (after != before)
                LanguageHooks.OnLanguageChanged(after);
            return result;
        }
    }
}
