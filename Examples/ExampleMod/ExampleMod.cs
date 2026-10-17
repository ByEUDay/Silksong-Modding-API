using System;
using Modding;
using UnityEngine;

namespace ExampleMod
{
    [Serializable]
    public class GlobalData { public int launches; }

    [Serializable]
    public class SaveData { public int enemiesKilled; }


    public class ExampleMod : Mod, IGlobalSettings<GlobalData>, ILocalSettings<SaveData>
    {
        private GlobalData _global = new GlobalData();
        private SaveData _save = new SaveData();

        public ExampleMod() : base("ExampleMod") { }

        public override string GetVersion() => "1.1.0";

        public void OnLoadGlobal(GlobalData s) => _global = s;
        public GlobalData OnSaveGlobal() => _global;
        public void OnLoadLocal(SaveData s) => _save = s;
        public SaveData OnSaveLocal() => _save;

        public override void Initialize()
        {
            _global.launches++;
            Log($"第 {_global.launches} 次启动");

            ModHooks.SceneEnteredHook += scene => Log($"进入场景 {scene}");
            ModHooks.GameStateChangedHook += state => Log($"状态 -> {state}");


            ModHooks.TakeDamageHook += (hazard, damage) => damage > 1 ? damage / 2 : damage;


            ModHooks.AddHealthHook += amount => amount * 2;
            ModHooks.AddSilkHook += amount => amount + 1;


            ModHooks.CurrencyChangeHook += (type, amount) => amount > 0 ? amount + amount / 10 : amount;


            ModHooks.EnemyDeathHook += hm =>
            {
                _save.enemiesKilled++;
                Log($"击杀 {hm.name}，本存档共 {_save.enemiesKilled}");
            };

            ModHooks.HeroJumpHook += () => Log("跳跃");
            ModHooks.HeroDashHook += () => Log("冲刺");
        }
    }
}
