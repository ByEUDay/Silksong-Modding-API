using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::TriggerManager")]
    public class patch_TriggerManager : MonoBehaviour
    {
        public extern void orig_OnTriggerEnter2D(Collider2D other);

        private void OnTriggerEnter2D(Collider2D other)
        {
            orig_OnTriggerEnter2D(other);
            if (other != null && other.CompareTag("Player"))
            {
                ModHooks.OnPlayerTriggerEnter(other);
            }
            ModHooks.OnTriggerEnter(GetComponent<Collider>(), other != null ? other.GetComponent<Collider>() : null);
        }

        public extern void orig_OnTriggerStay2D(Collider2D other);

        private void OnTriggerStay2D(Collider2D other)
        {
            orig_OnTriggerStay2D(other);
            ModHooks.OnTriggerStay(GetComponent<Collider>(), other != null ? other.GetComponent<Collider>() : null);
        }

        public extern void orig_OnTriggerExit2D(Collider2D other);

        private void OnTriggerExit2D(Collider2D other)
        {
            orig_OnTriggerExit2D(other);
            if (other != null && other.CompareTag("Player"))
            {
                ModHooks.OnPlayerTriggerExit(other);
            }
            ModHooks.OnTriggerExit(GetComponent<Collider>(), other != null ? other.GetComponent<Collider>() : null);
        }

        public extern void orig_OnCollisionEnter2D(Collision2D collision);

        private void OnCollisionEnter2D(Collision2D collision)
        {
            orig_OnCollisionEnter2D(collision);
            if (collision != null && collision.collider.CompareTag("Player"))
            {
                ModHooks.OnPlayerCollisionEnter(collision.collider);
            }
        }

        public extern void orig_OnCollisionExit2D(Collision2D collision);

        private void OnCollisionExit2D(Collision2D collision)
        {
            orig_OnCollisionExit2D(collision);
            if (collision != null && collision.collider.CompareTag("Player"))
            {
                ModHooks.OnPlayerCollisionExit(collision.collider);
            }
        }
    }
}
