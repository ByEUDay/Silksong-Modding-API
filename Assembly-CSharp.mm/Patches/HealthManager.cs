using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HealthManager")]
    public class patch_HealthManager : global::HealthManager
    {
        public extern void orig_TakeDamage(HitInstance hitInstance);

        private void TakeDamage(HitInstance hitInstance)
        {
            hitInstance = ModHooks.OnHealthManagerTakeDamage(this, hitInstance);
            orig_TakeDamage(hitInstance);
        }

        public extern void orig_Start();

        protected void Start()
        {
            orig_Start();
            ModHooks.OnEnemyStart(this);
        }

        public extern void orig_Die(float? attackDirection, AttackTypes attackType, NailElements nailElement, GameObject damageSource, bool ignoreEvasion, float corpseFlingMultiplier, bool overrideSpecialDeath, bool disallowDropFling);

        public void Die(float? attackDirection, AttackTypes attackType, NailElements nailElement, GameObject damageSource, bool ignoreEvasion = false, float corpseFlingMultiplier = 1f, bool overrideSpecialDeath = false, bool disallowDropFling = false)
        {
            bool wasDead = isDead;
            orig_Die(attackDirection, attackType, nailElement, damageSource, ignoreEvasion, corpseFlingMultiplier, overrideSpecialDeath, disallowDropFling);
            if (!wasDead && isDead)
                ModHooks.OnEnemyDeath(this);
        }

        public extern IHitResponder.HitResponse orig_Hit(HitInstance hitInstance);

        public IHitResponder.HitResponse Hit(HitInstance hitInstance)
        {
            hitInstance = ModHooks.OnHealthManagerHit(this, hitInstance);
            return orig_Hit(hitInstance);
        }
    }
}
