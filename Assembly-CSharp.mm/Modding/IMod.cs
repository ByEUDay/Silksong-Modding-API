using System.Collections.Generic;
using UnityEngine;

namespace Modding
{

    public interface IMod
    {
        string GetName();
        string GetVersion();

        void Initialize();

        int LoadPriority();
    }


    public interface ITogglableMod : IMod
    {
        void Unload();
    }

#if !NO_PRELOADER





    public interface IPreloadMod : IMod
    {
        List<(string, string)> GetPreloadNames();
        void OnPreloaded(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);
    }
#endif
}
