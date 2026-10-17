using System;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{



    [MonoModPatch("global::InputHandler")]
    public class patch_InputHandler
    {
        public extern void orig_Update();

        private void Update()
        {
            orig_Update();

            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        ModHooks.OnKeyPressed(key.ToString());
                    }
                    if (Input.GetKeyUp(key))
                    {
                        ModHooks.OnKeyReleased(key.ToString());
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    ModHooks.OnMouseButtonPressed(i);
                }
                if (Input.GetMouseButtonUp(i))
                {
                    ModHooks.OnMouseButtonReleased(i);
                }
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ModHooks.OnMouseScrolled(scroll);
            }

            Vector2 move = GetMoveVector();
            if (move.sqrMagnitude > 0.01f)
            {
                ModHooks.OnMoveInput(move);
            }

            Vector2 look = GetLookVector();
            if (look.sqrMagnitude > 0.01f)
            {
                ModHooks.OnLookInput(look);
            }

            if (GetAttackDown())
            {
                ModHooks.OnAttackInput();
            }
            if (GetJumpDown())
            {
                ModHooks.OnJumpInput();
            }
            if (GetDashDown())
            {
                ModHooks.OnDashInput();
            }
            if (GetInteractDown())
            {
                ModHooks.OnInteractInput();
            }
            if (GetInventoryDown())
            {
                ModHooks.OnOpenInventoryInput();
            }
            if (GetMapDown())
            {
                ModHooks.OnOpenMapInput();
            }
            if (GetPauseDown())
            {
                ModHooks.OnPauseInput();
            }
            if (GetCancelDown())
            {
                ModHooks.OnCancelInput();
            }
            if (GetConfirmDown())
            {
                ModHooks.OnConfirmInput();
            }
        }

        private Vector2 GetMoveVector()
        {
            try
            {
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            catch
            {
                return Vector2.zero;
            }
        }

        private Vector2 GetLookVector()
        {
            try
            {
                return new Vector2(Input.GetAxisRaw("LookHorizontal"), Input.GetAxisRaw("LookVertical"));
            }
            catch
            {
                return Vector2.zero;
            }
        }

        private bool GetAttackDown()
        {
            try { return Input.GetButtonDown("Attack"); }
            catch { return false; }
        }

        private bool GetJumpDown()
        {
            try { return Input.GetButtonDown("Jump"); }
            catch { return false; }
        }

        private bool GetDashDown()
        {
            try { return Input.GetButtonDown("Dash"); }
            catch { return false; }
        }

        private bool GetInteractDown()
        {
            try { return Input.GetButtonDown("Interact"); }
            catch { return false; }
        }

        private bool GetInventoryDown()
        {
            try { return Input.GetButtonDown("Inventory"); }
            catch { return false; }
        }

        private bool GetMapDown()
        {
            try { return Input.GetButtonDown("Map"); }
            catch { return false; }
        }

        private bool GetPauseDown()
        {
            try { return Input.GetButtonDown("Pause"); }
            catch { return false; }
        }

        private bool GetCancelDown()
        {
            try { return Input.GetButtonDown("Cancel"); }
            catch { return false; }
        }

        private bool GetConfirmDown()
        {
            try { return Input.GetButtonDown("Confirm"); }
            catch { return false; }
        }
    }
}
