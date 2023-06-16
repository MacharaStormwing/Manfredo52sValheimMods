using System;
using UnityEngine;
using HarmonyLib;

namespace CustomizableCamera
{
    [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
    public class GameCamera_UpdateCamera_Patch : CustomizeableCameraBase
    {
        // Lerp Variables
        public static bool targetDistanceHasBeenReached;
        public static float timeDuration = CustomizableCamera.smoothZoomSpeed;
        public static float timePos = 0;

        // Distance Variables
        public static float targetDistance = CustomizableCamera.cameraDistance.Value;
        public static float lastSetDistance;
        public static float camDistance;
        public static float zoomSens;

        private static bool checkLerpDuration(float timeElapsed)
        {
            if (lastSetDistance == targetDistance || timeElapsed >= timeDuration)
            {
                timePos = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void checkInteriorChange(Player player)
        {        
            if (playerInInterior != player.InInterior())
            {
                playerInInterior = player.InInterior();
                canChangeCameraDistance = true;
            }
            else if (playerInShelter != player.InShelter())
            {
                playerInShelter = player.InShelter();
                canChangeCameraDistance = true;
            }
        }

        private static void moveToNewCameraDistance(float time, ref float ___m_distance)
        {
            // Removes the delay when the player is going into first person.
            if (___m_distance <= 0.1 && targetDistance == 0)        
                ___m_distance = targetDistance;
            else
                ___m_distance = Mathf.Lerp(lastSetDistance, targetDistance, time);

            lastSetDistance = ___m_distance;
        }

        public static void Postfix(GameCamera __instance, ref float ___m_distance, ref float ___m_zoomSens)
        {
            if (!CustomizableCamera.isEnabled.Value || GameCamera.InFreeFly())
                return;
            
            Player localPlayer = Player.m_localPlayer;

            if (!localPlayer)
                return;

            checkInteriorChange(localPlayer);
            ___m_zoomSens = CustomizableCamera.cameraZoomSensitivity.Value;

            // Separate camera distances for different scenarios.
            if (canChangeCameraDistance)
            {
                if (CustomizableCamera.cameraDistanceInteriorsEnabled.Value && playerInInterior)
                    targetDistance = CustomizableCamera.cameraDistanceInteriors.Value;
                else if (CustomizableCamera.cameraDistanceShelterEnabled.Value && playerInShelter)
                    targetDistance = CustomizableCamera.cameraDistanceShelter.Value;
                else if (CustomizableCamera.cameraDistanceBoatEnabled.Value && characterControlledShip)
                    targetDistance = CustomizableCamera.cameraDistanceBoat.Value;
                else if (CustomizableCamera.cameraDistanceBoatEnabled.Value && characterStoppedShipControl)
                    targetDistance = CustomizableCamera.cameraDistance.Value;
                else if (CustomizableCamera.cameraDistanceExteriorsEnabled.Value && (!playerInShelter && !playerInInterior))
                    targetDistance = CustomizableCamera.cameraDistance.Value;

                canChangeCameraDistance = false;
            }

            if (CustomizableCamera.smoothZoomEnabled.Value)
            {
                // Disable the games default zooming in and out. Otherwise, the distance will flicker.
                ___m_zoomSens = 0;

                if ((Chat.instance && Chat.instance.HasFocus() || (Console.IsVisible() || InventoryGui.IsVisible()) || (StoreGui.IsVisible() || Menu.IsVisible() || (Minimap.IsOpen() || localPlayer.InCutscene())) ? 0 : (!localPlayer.InPlaceMode() ? 1 : 0)) != 0)
                {
                    float minDistance = __instance.m_minDistance;
                    float maxDistance = localPlayer.GetControlledShip() != null ? CustomizableCamera.cameraMaxDistanceBoat.Value : CustomizableCamera.cameraMaxDistance.Value;

                    float prevTargetDistance = targetDistance;
                    targetDistance -= Input.GetAxis("Mouse ScrollWheel") * CustomizableCamera.cameraZoomSensitivity.Value;    
                    targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

                    // Reset time when player changes zoom distance (scrollwheel)
                    if (prevTargetDistance != targetDistance)
                        timePos = 0;
                }

                targetDistanceHasBeenReached = checkLerpDuration(timePos);

                if (!targetDistanceHasBeenReached)
                {
                    timePos += Time.deltaTime;
                    moveToNewCameraDistance(timePos / timeDuration, ref ___m_distance);
                }
            }
        }
    }
}