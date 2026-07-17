using System.Collections.Generic;

namespace SilksongModLoader
{
    public interface IMod
    {
        string Name { get; }
        string Version { get; }
        IEnumerable<string> Dependencies => System.Array.Empty<string>();
        void Initialize();
    }
}
