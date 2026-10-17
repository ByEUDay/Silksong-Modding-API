using GlobalEnums;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroController")]
    public class patch_HeroController : global::HeroController
    {
        public extern void orig_Start();

        private void Start()
        {
            orig_Start();
            ModHooks.OnHeroStart();
        }

        public extern void orig_Update();

        private void Update()
        {
            orig_Update();
            ModHooks.OnHeroUpdate();
        }

        public extern void orig_Attack(AttackDirection attackDir);

        private void Attack(AttackDirection attackDir)
        {
            ModHooks.OnAttack(attackDir);
            orig_Attack(attackDir);
        }


        public extern void orig_HeroJump(bool checkSprint);

        private void HeroJump(bool checkSprint)
        {
            orig_HeroJump(checkSprint);
            ModHooks.OnHeroJump();
        }

        public extern void orig_DoubleJump();

        private void DoubleJump()
        {
            orig_DoubleJump();
            ModHooks.OnHeroDoubleJump();
        }

        public extern void orig_HeroDash(bool startAlreadyDashing);

        private void HeroDash(bool startAlreadyDashing)
        {
            orig_HeroDash(startAlreadyDashing);
            ModHooks.OnHeroDash();
        }

        public extern void orig_TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags);

        public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        {
            damageAmount = ModHooks.OnTakeDamage(hazardType, damageAmount);
            orig_TakeDamage(go, damageSide, damageAmount, hazardType, damagePropertyFlags);
            ModHooks.OnAfterTakeDamage(hazardType, damageAmount);
        }

        public extern void orig_AddHealth(int amount);

        public void AddHealth(int amount)
        {
            amount = ModHooks.OnAddHealth(amount);
            if (amount <= 0) return;
            orig_AddHealth(amount);
        }

        public extern void orig_TakeHealth(int amount);

        public void TakeHealth(int amount)
        {
            amount = ModHooks.OnTakeHealth(amount);
            if (amount <= 0) return;
            orig_TakeHealth(amount);
        }


        public extern void orig_AddSilk(int amount, bool heroEffect, SilkSpool.SilkAddSource source, bool forceCanBindEffect);

        public void AddSilk(int amount, bool heroEffect, SilkSpool.SilkAddSource source, bool forceCanBindEffect)
        {
            amount = ModHooks.OnAddSilk(amount);
            orig_AddSilk(amount, heroEffect, source, forceCanBindEffect);
        }

        public extern void orig_TakeSilk(int amount, SilkSpool.SilkTakeSource source);

        public void TakeSilk(int amount, SilkSpool.SilkTakeSource source)
        {
            amount = ModHooks.OnTakeSilk(amount);
            orig_TakeSilk(amount, source);
        }


        public extern bool orig_CanCast();
        public bool CanCast() => ModHooks.OnHeroAbilityCheck(HeroAbility.Cast, orig_CanCast());

        public extern bool orig_CanBind();
        public bool CanBind() => ModHooks.OnHeroAbilityCheck(HeroAbility.Bind, orig_CanBind());

        public extern bool orig_CanInteract();
        public bool CanInteract() => ModHooks.OnHeroAbilityCheck(HeroAbility.Interact, orig_CanInteract());

        public extern bool orig_CanOpenInventory();
        public bool CanOpenInventory() => ModHooks.OnHeroAbilityCheck(HeroAbility.OpenInventory, orig_CanOpenInventory());

        public extern bool orig_CanJump();
        public bool CanJump() => ModHooks.OnHeroAbilityCheck(HeroAbility.Jump, orig_CanJump());

        public extern bool orig_CanDoubleJump(bool checkControlState);
        public bool CanDoubleJump(bool checkControlState = true) => ModHooks.OnHeroAbilityCheck(HeroAbility.DoubleJump, orig_CanDoubleJump(checkControlState));

        public extern bool orig_CanDash();
        public bool CanDash() => ModHooks.OnHeroAbilityCheck(HeroAbility.Dash, orig_CanDash());

        public extern bool orig_CanHarpoonDash();
        public bool CanHarpoonDash() => ModHooks.OnHeroAbilityCheck(HeroAbility.HarpoonDash, orig_CanHarpoonDash());

        public extern bool orig_CanSprint();
        public bool CanSprint() => ModHooks.OnHeroAbilityCheck(HeroAbility.Sprint, orig_CanSprint());

        public extern bool orig_CanSuperJump();
        public bool CanSuperJump() => ModHooks.OnHeroAbilityCheck(HeroAbility.SuperJump, orig_CanSuperJump());

        public extern bool orig_CanAttack();
        public bool CanAttack() => ModHooks.OnHeroAbilityCheck(HeroAbility.Attack, orig_CanAttack());

        public extern bool orig_CanTakeDamage();
        public bool CanTakeDamage() => ModHooks.OnHeroAbilityCheck(HeroAbility.TakeDamage, orig_CanTakeDamage());

        public extern bool orig_CanTakeDamageIgnoreInvul();
        public bool CanTakeDamageIgnoreInvul() => ModHooks.OnHeroAbilityCheck(HeroAbility.TakeDamageIgnoreInvuln, orig_CanTakeDamageIgnoreInvul());


        public extern bool orig_Bounce();

        public bool Bounce()
        {
            bool bounced = orig_Bounce();
            if (bounced) ModHooks.OnHeroBounce();
            return bounced;
        }

        public extern void orig_RelinquishControl();

        public void RelinquishControl()
        {
            orig_RelinquishControl();
            ModHooks.OnHeroControlRelinquished();
        }


        public extern void orig_RegainControl(bool allowInput);

        public void RegainControl(bool allowInput)
        {
            orig_RegainControl(allowInput);
            ModHooks.OnHeroControlRegained(allowInput);
        }

        public extern void orig_AddToMaxHealth(int amount);

        public void AddToMaxHealth(int amount)
        {
            amount = ModHooks.OnAddMaxHealth(amount);
            if (amount <= 0) return;
            orig_AddToMaxHealth(amount);
        }

        public extern System.Collections.IEnumerator orig_Respawn(Transform spawnPoint);

        public System.Collections.IEnumerator Respawn(Transform spawnPoint)
        {
            yield return orig_Respawn(spawnPoint);
            ModHooks.OnHeroRespawn(spawnPoint);
        }
    }
}
