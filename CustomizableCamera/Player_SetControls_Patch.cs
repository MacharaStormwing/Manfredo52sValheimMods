﻿using UnityEngine;
using HarmonyLib;

namespace CustomizableCamera
{
    [HarmonyPatch(typeof(Player), "SetControls")]
    public class Player_SetControls_Patch : CustomizeableCameraBase
    {
        public static bool isPlayerAbleToCrouch;

        public static void Prefix(Player __instance, ref bool block, ref bool blockHold)
        {
            if (!CustomizableCamera.isEnabled.Value || !__instance)
                return;

            ItemDrop.ItemData playerItemEquipped = __instance.GetCurrentWeapon();

            if (playerItemEquipped != null && (playerItemEquipped.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow))
                characterEquippedBow = true;
            else
                characterEquippedBow = false;

            // Zoom in with the bow if enabled.
            if (CustomizableCamera.bowZoomEnabled.Value)
            {
                // Check to make sure if the user is drawing their bow.
                if (__instance.InAttack() && characterEquippedBow) //__instance.IsHoldingAttack()
                {
                    // Cancel draw key, auto zoom, toggle zoom, or hold zoom
                    if (Input.GetKey(CustomizableCamera.bowCancelDrawKey.Value.MainKey))
                    {
                        block = true;
                        blockHold = true;
                        characterAiming = false;
                    }
                    else if (CustomizableCamera.bowZoomOnDraw.Value)
                    {
                        characterAiming = true;
                    }
                    else
                    {
                        block = false;
                        blockHold = false;
                        
                        if (CustomizableCamera.bowZoomKeyToggle.Value)
                        {                       
                            if (Input.GetKeyDown(CustomizableCamera.bowZoomKey.Value.MainKey))
                                characterAiming = !characterAiming;
                        }
                        else
                        {
                            if (Input.GetKey(CustomizableCamera.bowZoomKey.Value.MainKey))
                                characterAiming = true;
                            else
                                characterAiming = false;
                        }
                    }
                }
                else
                {
                    characterAiming = false;
                }

                // Change sensitivity when zooming in with the bow if enabled.
                if (CustomizableCamera.bowZoomSensitivityEnabled.Value)
                {
                    if (characterAiming)
                        PlayerController.m_mouseSens = (CustomizableCamera.playerMouseSensitivity * CustomizableCamera.bowZoomSensitivity.Value);
                    else
                        PlayerController.m_mouseSens = CustomizableCamera.playerMouseSensitivity;
                }
            }
        }

        public static void Postfix(Player __instance, Vector3 movedir, bool blockHold, bool crouch, bool run, bool autoRun)
        {
            if (!__instance)
                return;

            if (movedir.magnitude != 0)
                playerIsMoving = true;
            else
                playerIsMoving = false;

            if (characterCrouched && crouch)
            {
                characterCrouched = false;
                return;
            }

            characterWalking = __instance.GetWalk();

            if (run || autoRun || blockHold)
            {
                isPlayerAbleToCrouch = false;
                characterCrouched = false;
            }              
            else
                isPlayerAbleToCrouch = true;

            if (playerIsMoving && run && !blockHold)
                characterSprinting = true;
            else
                characterSprinting = false;

            if (isPlayerAbleToCrouch && crouch)
                characterCrouched = true;
        }
    }
}