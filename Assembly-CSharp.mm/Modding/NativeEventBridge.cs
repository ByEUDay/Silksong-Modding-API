using System;
using System.Runtime.CompilerServices;

namespace Modding
{




    internal static class NativeEventBridge
    {
        private static readonly ConditionalWeakTable<object, object> Attached = new ConditionalWeakTable<object, object>();
        private static readonly object Marker = new object();

        private static bool MarkOnce(object o)
        {
            if (o == null) return false;
            if (Attached.TryGetValue(o, out _)) return false;
            Attached.Add(o, Marker);
            return true;
        }

        internal static void Attach()
        {
            try
            {

                GameManager.SceneTransitionBegan += l => ModHooks.OnSceneTransitionBegan(l);
                HeroController.OnHeroInstanceSet += AttachHero;
                ToolItemManager.BoundAttackToolUpdated += b => ModHooks.OnBoundAttackToolUpdated(b);
                ToolItemManager.BoundAttackToolUsed += b => ModHooks.OnBoundAttackToolUsed(b);
                ToolItemManager.BoundAttackToolFailed += b => ModHooks.OnBoundAttackToolFailed(b);
                ToolItemManager.OnEquippedStateChanged += () => ModHooks.OnEquippedToolsChanged();


                AttachGameManager(GameManager._instance);


                try { AttachHero(HeroController.SilentInstance); } catch {   }
            }
            catch (Exception e)
            {
                Logger.LogError("NativeEventBridge 挂接失败: " + e);
            }
        }

        private static void AttachGameManager(GameManager gm)
        {
            if (!MarkOnce(gm)) return;
            gm.SceneInit += () => ModHooks.OnSceneInit();
            gm.NextSceneWillActivate += () => ModHooks.OnNextSceneWillActivate();
            gm.UnloadingLevel += () => ModHooks.OnUnloadingLevel();
            gm.OnBeforeFinishedSceneTransition += () => ModHooks.OnBeforeFinishedSceneTransition();
            gm.OnFinishedSceneTransition += () => ModHooks.OnFinishedSceneTransition();
            gm.SavePersistentObjects += () => ModHooks.OnSavePersistentObjects();
            gm.ResetSemiPersistentObjects += () => ModHooks.OnResetSemiPersistentObjects();
            gm.OnLoadedBoss += () => ModHooks.OnBossLoaded();
            gm.RefreshLanguageText += () => ModHooks.OnRefreshLanguageText();
            gm.GamePausedChange += paused => ModHooks.OnGamePausedChanged(paused);
        }

        private static void AttachHero(HeroController hero)
        {
            if (!MarkOnce(hero)) return;
            ModHooks.OnHeroInstanceSet(hero);
            hero.preHeroInPosition += f => ModHooks.OnPreHeroInPosition(f);
            hero.heroInPosition += f => ModHooks.OnHeroInPosition(f);
            hero.heroInPositionDelayed += f => ModHooks.OnHeroInPositionDelayed(f);
            hero.OnTakenDamage += () => ModHooks.OnHeroTakenDamage();
            hero.OnTakenDamageExtra += i => ModHooks.OnHeroTakenDamageExtra(i);
            hero.OnDeath += () => ModHooks.OnHeroDeath();
            hero.OnHazardDeath += () => ModHooks.OnHeroHazardDeath();
            hero.OnHazardRespawn += () => ModHooks.OnHeroHazardRespawn();
            hero.HeroLeavingScene += () => ModHooks.OnHeroLeavingScene();
        }
    }
}
