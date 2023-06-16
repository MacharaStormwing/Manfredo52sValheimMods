using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

// Fix crosshair state when logging out with bow equipped.
namespace CustomizableCamera
{   
    [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
    public class Hud_UpdateCrosshair_Patch : CustomizeableCameraBase
    {
        // Lerp Variables
        public static bool targetCrosshairHasBeenReached;
        public static float timeDuration = CustomizableCamera.timeCameraPosDuration.Value;
        public static float timePos = 0; 

        public static bool crosshairStateChanged;
        public static characterState crosshairStatePrev = characterState.standing;
        public static characterState crosshairState = characterState.standing;

        private static bool checkLerpDuration(float timeElapsed)
        {
            if (CustomizableCamera.lastSetCrosshairPos == CustomizableCamera.targetCrosshairPos || timeElapsed >= timeDuration)
            {
                timePos = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void moveToNewCrosshairPosition(Hud __instance, float time)
        {
            __instance.m_crosshair.transform.position = Vector3.Lerp(CustomizableCamera.lastSetCrosshairPos, CustomizableCamera.targetCrosshairPos, time);
            __instance.m_crosshairBow.transform.position = Vector3.Lerp(CustomizableCamera.lastSetCrosshairPos, CustomizableCamera.targetCrosshairPos, time);

            __instance.m_hidden.transform.position = Vector3.Lerp(CustomizableCamera.lastSetCrosshairPos, CustomizableCamera.targetCrosshairPos, time);
            __instance.m_targeted.transform.position = Vector3.Lerp(CustomizableCamera.lastSetCrosshairPos, CustomizableCamera.targetCrosshairPos, time);
            __instance.m_targetedAlert.transform.position = Vector3.Lerp(CustomizableCamera.lastSetCrosshairPos, CustomizableCamera.targetCrosshairPos, time);

            __instance.m_stealthBar.transform.position = Vector3.Lerp(CustomizableCamera.lastSetStealthBarPos, CustomizableCamera.targetStealthBarPos, time);

            CustomizableCamera.lastSetCrosshairPos = __instance.m_crosshair.transform.position;
            CustomizableCamera.lastSetStealthBarPos = __instance.m_crosshairBow.transform.position;
        }

        private static void setTargetPositions()
        {
            if (crosshairState == characterState.bowequipped)
            {
                CustomizableCamera.targetCrosshairPos = new Vector3(CustomizableCamera.playerInitialCrosshairX + CustomizableCamera.playerBowCrosshairX.Value, CustomizableCamera.playerInitialCrosshairY + CustomizableCamera.playerBowCrosshairY.Value, 0);
                CustomizableCamera.targetStealthBarPos = new Vector3(CustomizableCamera.playerInitialStealthBarX + CustomizableCamera.playerBowCrosshairX.Value, CustomizableCamera.playerInitialStealthBarY + CustomizableCamera.playerBowCrosshairY.Value * 3, 0);
            }
            else
            {
                CustomizableCamera.targetCrosshairPos = new Vector3(CustomizableCamera.playerInitialCrosshairX, CustomizableCamera.playerInitialCrosshairY, 0);
                CustomizableCamera.targetStealthBarPos = new Vector3(CustomizableCamera.playerInitialStealthBarX, CustomizableCamera.playerInitialStealthBarY, 0);
            }
        }

        private static void setCrosshairState()
        {
            crosshairStatePrev = crosshairState;

            if ((characterAiming || characterEquippedBow) && !isFirstPerson)
                crosshairState = characterState.bowequipped;
            else
                crosshairState = characterState.standing;

            if (crosshairState != crosshairStatePrev)
            {
                timePos = 0;
                crosshairStateChanged = true;
                crosshairStatePrev = crosshairState;
            }
            else
            {
                crosshairStateChanged = false;
            }
        }

        public static void Postfix(Hud __instance)
        {
            if (!CustomizableCamera.isEnabled.Value || !__instance)
                return;

            if (CustomizableCamera.playerBowCrosshairEditsEnabled.Value)
            {
                setCrosshairState();
                setTargetPositions();
                targetCrosshairHasBeenReached = checkLerpDuration(timePos);

                if (!targetCrosshairHasBeenReached)
                {
                    timePos += Time.deltaTime;
                    moveToNewCrosshairPosition(__instance, timePos / CustomizableCamera.timeCameraPosDuration.Value);
                }
            }          
        }
    }
}