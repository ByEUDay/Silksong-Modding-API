using System;

namespace Modding
{

    public abstract class Mod : IMod
    {
        public string Name { get; }

        protected Mod() : this(null) { }

        protected Mod(string name)
        {
            Name = string.IsNullOrEmpty(name) ? GetType().Name : name;
        }

        public virtual string GetName() => Name;

        public virtual string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public virtual void Initialize() { }

        public virtual int LoadPriority() => 1;

        protected void Log(object message) => Logger.Log(Name, message);
        protected void LogWarn(object message) => Logger.LogWarn(Name, message);
        protected void LogError(object message) => Logger.LogError(Name, message);
    }
}
