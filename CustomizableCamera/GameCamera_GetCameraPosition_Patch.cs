using System;
using UnityEngine;
using HarmonyLib;

namespace CustomizableCamera
{
    [HarmonyPatch(typeof(GameCamera), "GetCameraPosition")]
    public class GameCamera_GetCameraPosition_Patch : CustomizeableCameraBase
    {
        // Reimplement camera settings reset
        // Reset settings on settings save.
        private static void resetCameraSettings(GameCamera __instance)
        {
            __instance.m_fov = CustomizableCamera.defaultFOV;
            __instance.m_3rdOffset = CustomizableCamera.defaultPosition;
        }

        private static void moveToNewCameraPosition(GameCamera __instance, Vector3 targetVector, float time)
        {
            __instance.m_3rdOffset = Vector3.Lerp(__instance.m_3rdOffset, targetVector, time / CustomizableCamera.timeCameraPosDuration.Value);
            CustomizableCamera.lastSetPos = __instance.m_3rdOffset;
        }

        private static void moveToNewCameraFOV(GameCamera __instance, float targetFOV, float time)
        {
            __instance.m_fov = Mathf.Lerp(CustomizableCamera.lastSetFOV, targetFOV, time / CustomizableCamera.timeFOVDuration.Value);
            CustomizableCamera.lastSetFOV = __instance.m_fov;
        }

        private static void moveToNewCameraFOVBowZoom(GameCamera __instance, float targetFOV, float time, interpolationTypes interpType)
        {
            if (interpType == interpolationTypes.SmoothStep)
                __instance.m_fov = Mathf.SmoothStep(CustomizableCamera.lastSetFOV, targetFOV, time / CustomizableCamera.timeBowZoomFOVDuration.Value);
            else
                __instance.m_fov = Mathf.Lerp(CustomizableCamera.lastSetFOV, targetFOV, time / CustomizableCamera.timeBowZoomFOVDuration.Value);

            CustomizableCamera.lastSetFOV = __instance.m_fov;
        }

        private static bool checkBowZoomFOVLerpDuration(GameCamera __instance, float timeElapsed)
        {
            if (CustomizableCamera.lastSetFOV == CustomizableCamera.targetFOV || timeElapsed >= CustomizableCamera.timeFOVDuration.Value)
            {
                __instance.m_fov = CustomizableCamera.targetFOV;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool checkFOVLerpDuration(GameCamera __instance, float timeElapsed)
        {
            if (CustomizableCamera.lastSetFOV == CustomizableCamera.targetFOV)
            {
                __instance.m_fov = CustomizableCamera.targetFOV;
                return true;
            }
            else if (timeElapsed >= CustomizableCamera.timeFOVDuration.Value)
            {
                CustomizableCamera.timeFOV = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool checkCameraLerpDuration(GameCamera __instance, float timeElapsed)
        {
            if (CustomizableCamera.lastSetPos == CustomizableCamera.targetPos)
            {
                __instance.m_3rdOffset = CustomizableCamera.targetPos;
                return true;
            }
            else if (timeElapsed >= CustomizableCamera.timeCameraPosDuration.Value)
            {
                CustomizableCamera.timeCameraPos = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void setValuesBasedOnCharacterState(Player __instance, bool isFirstPerson)
        {
            __characterStatePrev = __characterState;

            if (isFirstPerson)
            {
                if (characterAiming && CustomizableCamera.bowZoomFirstPersonEnabled.Value)
                {
                    CustomizableCamera.targetFOV = CustomizableCamera.cameraBowZoomFirstPersonFOV.Value;
                    __characterState = characterState.bowaiming;
                } 
                else
                {
                    CustomizableCamera.targetFOV = CustomizableCamera.cameraFirstPersonFOV.Value;
                    __characterState = characterState.standing;
                }
            }
            else if (characterAiming && CustomizableCamera.bowZoomEnabled.Value)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraBowZoomFOV.Value;
                if (characterEquippedBow && CustomizableCamera.cameraBowSettingsEnabled.Value)
                    CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraBowX.Value, CustomizableCamera.cameraBowY.Value, CustomizableCamera.cameraBowZ.Value);
                else
                    CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraX.Value, CustomizableCamera.cameraY.Value, CustomizableCamera.cameraZ.Value);
                __characterState = characterState.bowaiming;
            }
            else if (characterControlledShip)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraBoatFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraBoatX.Value, CustomizableCamera.cameraBoatY.Value, CustomizableCamera.cameraBoatZ.Value);
                __characterState = characterState.sailing;
            }
            else if (characterEquippedBow && CustomizableCamera.cameraBowSettingsEnabled.Value)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraBowX.Value, CustomizableCamera.cameraBowY.Value, CustomizableCamera.cameraBowZ.Value);
                __characterState = characterState.bowequipped;

            }
            else if (characterSprinting)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraSprintFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraSprintX.Value, CustomizableCamera.cameraSprintY.Value, CustomizableCamera.cameraSprintZ.Value);
                __characterState = characterState.sprinting;
            }
            else if (characterCrouched)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraSneakFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraSneakX.Value, CustomizableCamera.cameraSneakY.Value, CustomizableCamera.cameraSneakZ.Value);
                __characterState = characterState.crouching;
            }
            else if (characterWalking)
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraWalkFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraWalkX.Value, CustomizableCamera.cameraWalkY.Value, CustomizableCamera.cameraWalkZ.Value);
                __characterState = characterState.walking;
            }
            else
            {
                CustomizableCamera.targetFOV = CustomizableCamera.cameraFOV.Value;
                CustomizableCamera.targetPos = new Vector3(CustomizableCamera.cameraX.Value, CustomizableCamera.cameraY.Value, CustomizableCamera.cameraZ.Value);
                __characterState = characterState.standing;
            }

            // When the player swaps shoulder views.
            float swappedShoulderX = CustomizableCamera.targetPos.x * (float) -1.0;
            if (Input.GetKeyDown(CustomizableCamera.swapShoulderViewKey.Value.MainKey) && !isFirstPerson)
            {
                CustomizableCamera.timeCameraPos = 0;
                onSwappedShoulder = !onSwappedShoulder;
            }

            if (onSwappedShoulder)
                CustomizableCamera.targetPos.x = swappedShoulderX;

            if (__characterState != __characterStatePrev)
            {
                characterStateChanged = true;
                __characterStatePrev = __characterState;
            }
            else
            {
                characterStateChanged = false;
            }
        }

        private static bool checkIfFirstPerson(float ___m_distance)
        {
            if (___m_distance <= 0.0)
                return true;

            return false;
        }

        public static void Postfix(GameCamera __instance, ref Vector3 pos, ref float ___m_distance)
        {
            Player localPlayer = Player.m_localPlayer;

            if (!CustomizableCamera.isEnabled.Value || !__instance || !localPlayer)
                return;

            isFirstPerson = checkIfFirstPerson(___m_distance);
            setValuesBasedOnCharacterState(localPlayer, isFirstPerson);

            if (characterAiming)
            {
                CustomizableCamera.targetFOVHasBeenReached = checkBowZoomFOVLerpDuration(__instance, localPlayer.GetAttackDrawPercentage());

                if (!CustomizableCamera.targetFOVHasBeenReached)
                    moveToNewCameraFOVBowZoom(__instance, CustomizableCamera.targetFOV, localPlayer.GetAttackDrawPercentage(), CustomizableCamera.timeBowZoomInterpolationType.Value);
            }
            else
            {
                CustomizableCamera.targetFOVHasBeenReached = checkFOVLerpDuration(__instance, CustomizableCamera.timeFOV);

                if (!CustomizableCamera.targetFOVHasBeenReached)
                {
                    if (characterStateChanged)
                        CustomizableCamera.timeFOV = 0;
                    else
                        CustomizableCamera.timeFOV += Time.deltaTime;

                    moveToNewCameraFOV(__instance, CustomizableCamera.targetFOV, CustomizableCamera.timeFOV);
                }
            }

            // Skip the new target camera position below if the character is in first person.
            if (isFirstPerson)
                return;

            if (CustomizableCamera.cameraLockedBoatYEnabled.Value && characterControlledShip)
                pos.y = CustomizableCamera.cameraLockedBoatY.Value;

            CustomizableCamera.targetPosHasBeenReached = checkCameraLerpDuration(__instance, CustomizableCamera.timeCameraPos);

            if (!CustomizableCamera.targetPosHasBeenReached)
            {
                if (characterStateChanged)
                    CustomizableCamera.timeCameraPos = 0;
                else
                    CustomizableCamera.timeCameraPos += Time.deltaTime;

                moveToNewCameraPosition(__instance, CustomizableCamera.targetPos, CustomizableCamera.timeCameraPos);
            }
        }
    }
}