﻿using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Runtime.Remoting.Channels;

namespace CustomizableCamera
{
    [BepInPlugin("manfredo52.CustomizableCamera", "Customizable Camera Mod", "1.3.4")]
    [BepInProcess("valheim.exe")]
    public class CustomizableCamera : BaseUnityPlugin
    {
        // Main Settings
        public static ConfigEntry<bool> isEnabled;
        public static ConfigEntry<int> nexusID;

        // Default Values
        public static float defaultValue = 0;
        public static int defaultCameraDistance = 4;
        public static int defaultCameraMaxDistance = 8;
        public static int defaultCameraMaxDistanceBoat = 16;
        public static int defaultZoomSensitivity = 10;
        public static float defaultSmoothness = 0.1f;
        public static float defaultFOV = 65.0f;
        public static float defaultFPFOV = 65.0f;
        public static float defaultBowZoomFOV = 55.0f;
        public static float defaultBowZoomFPFOV = 55.0f;
        public static float defaultTimeDuration = 5.0f;
        public static float defaultBowZoomTimeDuration = 3.0f;
        public static Vector3 defaultPosition = new Vector3(0.25f, 0.25f, 0.00f);
        public static float smoothZoomSpeed = 3f;

        // Mouse Sensitivity
        public static float playerMouseSensitivity;
        public static ConfigEntry<bool> bowZoomSensitivityEnabled;
        public static ConfigEntry<float> bowZoomSensitivity;

        // Crosshair
        public static float playerInitialCrosshairX;
        public static float playerInitialCrosshairY;

        public static Vector3 lastSetCrosshairPos;
        public static Vector3 lastSetStealthBarPos;
        public static Vector3 targetCrosshairPos;
        public static Vector3 targetStealthBarPos;

        // Sneak Hud
        public static float playerInitialStealthBarX;
        public static float playerInitialStealthBarY;

        // Bow Crosshair Settings
        public static ConfigEntry<bool> playerBowCrosshairEditsEnabled;
        public static ConfigEntry<float> playerBowCrosshairX;
        public static ConfigEntry<float> playerBowCrosshairY;

        // Normal Camera Settings
        public static ConfigEntry<float> cameraFOV;
        public static ConfigEntry<float> cameraX;
        public static ConfigEntry<float> cameraY;
        public static ConfigEntry<float> cameraZ;

        // First Person Camera Mod Settings
        public static ConfigEntry<bool> bowZoomFirstPersonEnabled;
        public static ConfigEntry<float> cameraFirstPersonFOV;
        public static ConfigEntry<float> cameraBowZoomFirstPersonFOV;

        // Sneak Camera Settings
        public static ConfigEntry<float> cameraSneakFOV;
        public static ConfigEntry<float> cameraSneakX;
        public static ConfigEntry<float> cameraSneakY;
        public static ConfigEntry<float> cameraSneakZ;

        // Sprinting Camera Settings
        public static ConfigEntry<float> cameraSprintFOV;
        public static ConfigEntry<float> cameraSprintX;
        public static ConfigEntry<float> cameraSprintY;
        public static ConfigEntry<float> cameraSprintZ;

        // Walk Camera Settings
        public static ConfigEntry<float> cameraWalkFOV;
        public static ConfigEntry<float> cameraWalkX;
        public static ConfigEntry<float> cameraWalkY;
        public static ConfigEntry<float> cameraWalkZ;

        // Boat Camera Settings
        public static ConfigEntry<float> cameraBoatFOV;
        public static ConfigEntry<float> cameraBoatX;
        public static ConfigEntry<float> cameraBoatY;
        public static ConfigEntry<float> cameraBoatZ;
        public static ConfigEntry<bool> cameraLockedBoatYEnabled;
        public static ConfigEntry<float> cameraLockedBoatY;

        // Bow Camera Setting
        public static ConfigEntry<bool> cameraBowSettingsEnabled;
        public static ConfigEntry<float> cameraBowX;
        public static ConfigEntry<float> cameraBowY;
        public static ConfigEntry<float> cameraBowZ;

        // Bow Zoom Camera Settings
        public static ConfigEntry<bool> bowZoomEnabled;
        public static ConfigEntry<bool> bowZoomOnDraw;
        public static ConfigEntry<bool> bowZoomKeyToggle;
        public static ConfigEntry<KeyboardShortcut> bowZoomKey;
        public static ConfigEntry<KeyboardShortcut> bowCancelDrawKey;
        public static ConfigEntry<float> cameraBowZoomFOV;

        // Bow Zoom Interpolation Settings
        public static ConfigEntry<float> timeFOVDuration;
        public static ConfigEntry<float> timeBowZoomFOVDuration;
        public static ConfigEntry<CustomizeableCameraBase.interpolationTypes> timeBowZoomInterpolationType;
        public static ConfigEntry<float> timeCameraPosDuration;

        // Misc Camera Settings
        public static ConfigEntry<float> cameraSmoothness;
        public static ConfigEntry<float> cameraZoomSensitivity;
        public static ConfigEntry<KeyboardShortcut> swapShoulderViewKey;
        public static ConfigEntry<bool> smoothZoomEnabled;

        // Misc Camera Settings - Distance
        public static ConfigEntry<float> cameraDistance;
        public static ConfigEntry<float> cameraDistanceBoat;
        public static ConfigEntry<bool> cameraDistanceBoatEnabled;
        public static ConfigEntry<bool> cameraDistanceExteriorsEnabled;
        public static ConfigEntry<float> cameraDistanceInteriors;
        public static ConfigEntry<bool> cameraDistanceInteriorsEnabled;
        public static ConfigEntry<float> cameraDistanceShelter;
        public static ConfigEntry<bool> cameraDistanceShelterEnabled;
        public static ConfigEntry<float> cameraMaxDistance;
        public static ConfigEntry<float> cameraMaxDistanceBoat;

        // Variables for FOV linear interpolation
        public static float timeFOV = 0;
        public static float targetFOV;
        public static float lastSetFOV;
        public static bool targetFOVHasBeenReached;

        // Variables for camera position linear interpolation
        public static float timeCameraPos = 0;
        public static Vector3 targetPos;
        public static Vector3 lastSetPos;
        public static bool targetPosHasBeenReached;

        public void Awake()
        {
            // Main
            isEnabled   = Config.Bind<bool>("- General -", "Enable Mod", true, "Enable or disable the mod");
            nexusID     = Config.Bind<int>("- General -", "NexusID", 396, "Nexus mod ID for updates");

            // Misc Settings
            cameraSmoothness        = Config.Bind<float>("- Misc -", "cameraSmoothness", defaultSmoothness, new ConfigDescription("Camera smoothing. Determines how smoothly/quickly the camera will follow your player.", new AcceptableValueRange<float>(0, 20f)));   
            cameraZoomSensitivity   = Config.Bind<float>("- Misc -", "cameraZoomSensitivity", defaultZoomSensitivity, new ConfigDescription("How much the camera zooms in or out when changing camera distance with the scroll wheel. Takes effect on game restart.", new AcceptableValueRange<float>(1, 25)));
            swapShoulderViewKey     = Config.Bind<KeyboardShortcut>("- Misc -", "swapShoulderViewKey", new KeyboardShortcut(KeyCode.B), "Keyboard shortcut or mouse button to swap shoulder views.");
            smoothZoomEnabled       = Config.Bind<bool>("- Misc -", "smoothZoomEnabled", true, "Enable if the zooming in and out to be smooth instead of an instant change.");

            // Misc Settings - Camera Distance
            cameraDistance                  = Config.Bind<float>("- Misc Camera Distance -", "cameraDistance", defaultCameraDistance, new ConfigDescription("Default camera distance from the player.", new AcceptableValueRange<float>(0, 100)));
            cameraDistanceBoat              = Config.Bind<float>("- Misc Camera Distance -", "cameraDistanceBoat", defaultCameraDistance, new ConfigDescription("Default camera distance when you start control of a ship.", new AcceptableValueRange<float>(0, 100)));
            cameraDistanceBoatEnabled       = Config.Bind<bool>("- Misc Camera Distance -", "cameraDistanceBoatEnabled", false, "Enable separate distance edit for boats.");
            cameraDistanceInteriors         = Config.Bind<float>("- Misc Camera Distance -", "cameraDistanceInteriors", defaultCameraDistance, new ConfigDescription("Default camera distance when you go into an interior such as a dungeon.", new AcceptableValueRange<float>(0, 100)));
            cameraDistanceInteriorsEnabled  = Config.Bind<bool>("- Misc Camera Distance -", "cameraDistanceInteriorsEnabled", false, "Enable separate distance edit for interiors.");
            cameraDistanceExteriorsEnabled  = Config.Bind<bool>("- Misc Camera Distance -", "cameraDistanceExteriorsEnabled", false, "Enable separate distance edit for exteriors. Uses cameraDistance value. This is used if you want to automatically go to the set camera distance when you move to an exterior.");
            cameraDistanceShelter           = Config.Bind<float>("- Misc Camera Distance -", "cameraDistanceShelter", defaultCameraDistance, new ConfigDescription("Default camera distance when you are under shelter.", new AcceptableValueRange<float>(0, 100)));
            cameraDistanceShelterEnabled    = Config.Bind<bool>("- Misc Camera Distance -", "cameraDistanceShelterEnabled", false, "Enable separate distance edit for shelter.");
            cameraMaxDistance               = Config.Bind<float>("- Misc Camera Distance -", "cameraMaxDistance", defaultCameraMaxDistance, new ConfigDescription("Maximum distance you can zoom out.", new AcceptableValueRange<float>(1, 100)));
            cameraMaxDistanceBoat           = Config.Bind<float>("- Misc Camera Distance -", "cameraMaxDistanceBoat", defaultCameraMaxDistanceBoat, new ConfigDescription("Maximum distance you can zoom out when on a boat.", new AcceptableValueRange<float>(1, 100)));

            // Time Settings
            timeFOVDuration              = Config.Bind<float>("- Misc Time Values -", "timeFOVDuration", defaultTimeDuration, new ConfigDescription("How quickly the fov changes.", new AcceptableValueRange<float>(0.001f, 50f)));
            timeBowZoomFOVDuration       = Config.Bind<float>("- Misc Time Values -", "timeBowZoomFOVDuration", defaultBowZoomTimeDuration, new ConfigDescription("How quickly the bow zooms in.", new AcceptableValueRange<float>(0.001f, 50f)));
            timeBowZoomInterpolationType = Config.Bind<CustomizeableCameraBase.interpolationTypes>("- Misc Time Values -", "timeBowZoomInterpolationType", CustomizeableCameraBase.interpolationTypes.Linear, new ConfigDescription("Interpolation method for the bow zoom."));
            timeCameraPosDuration        = Config.Bind<float>("- Misc Time Values -", "timeCameraPosDuration", defaultTimeDuration, new ConfigDescription("How quickly the camera moves to the new camera position", new AcceptableValueRange<float>(0.001f, 50f)));

            // Default
            cameraFOV = Config.Bind<float>("Camera Settings", "cameraFOV", defaultFOV, "The camera fov.");
            cameraX   = Config.Bind<float>("Camera Settings", "cameraX", defaultPosition.x, "The third person camera x position.");
            cameraY   = Config.Bind<float>("Camera Settings", "cameraY", defaultPosition.y, "The third person camera y position.");
            cameraZ   = Config.Bind<float>("Camera Settings", "cameraZ", defaultPosition.z, "The third person camera z position.");

            // Sneak
            cameraSneakFOV  = Config.Bind<float>("Camera Settings - Sneak", "cameraSneakFOV", defaultFOV, "Camera fov when sneaking.");
            cameraSneakX    = Config.Bind<float>("Camera Settings - Sneak", "cameraSneakX", defaultPosition.x, "Camera X position when sneaking.");
            cameraSneakY    = Config.Bind<float>("Camera Settings - Sneak", "cameraSneakY", defaultPosition.y, "Camera Y position when sneaking.");
            cameraSneakZ    = Config.Bind<float>("Camera Settings - Sneak", "cameraSneakZ", defaultPosition.z, "Camera Z position when sneaking.");

            // Sprinting
            cameraSprintFOV = Config.Bind<float>("Camera Settings - Sprinting", "cameraSprintFOV", defaultFOV, "Camera fov when sprinting.");
            cameraSprintX   = Config.Bind<float>("Camera Settings - Sprinting", "cameraSprintX", defaultPosition.x, "Camera X position when sprinting.");
            cameraSprintY   = Config.Bind<float>("Camera Settings - Sprinting", "cameraSprintY", defaultPosition.y, "Camera Y position when sprinting.");
            cameraSprintZ   = Config.Bind<float>("Camera Settings - Sprinting", "cameraSprintZ", defaultPosition.z, "Camera Z position when sprinting.");

            // Walking
            cameraWalkFOV   = Config.Bind<float>("Camera Settings - Walk", "cameraWalkFOV", defaultFOV, "Camera fov when walking.");
            cameraWalkX     = Config.Bind<float>("Camera Settings - Walk", "cameraWalkX", defaultPosition.x, "Camera X position when walking.");
            cameraWalkY     = Config.Bind<float>("Camera Settings - Walk", "cameraWalkY", defaultPosition.y, "Camera Y position when walking.");
            cameraWalkZ     = Config.Bind<float>("Camera Settings - Walk", "cameraWalkZ", defaultPosition.z, "Camera Z position when walking.");

            // Boat
            cameraBoatFOV               = Config.Bind<float>("Camera Settings - Boat", "cameraBoatFOV", defaultFOV, "Camera fov when sailing.");
            cameraBoatX                 = Config.Bind<float>("Camera Settings - Boat", "cameraBoatX", defaultPosition.x, "Camera X position when sailing.");
            cameraBoatY                 = Config.Bind<float>("Camera Settings - Boat", "cameraBoatY", defaultPosition.y, "Camera Y position when sailing.");
            cameraBoatZ                 = Config.Bind<float>("Camera Settings - Boat", "cameraBoatZ", defaultPosition.z, "Camera Z position when sailing.");
            cameraLockedBoatYEnabled    = Config.Bind<bool>("Camera Settings - Boat", "cameraLockedBoatYEnabled", false, "Enable or disable the y axis being locked to a specific value when sailing. Reduces motion sickness.");
            cameraLockedBoatY           = Config.Bind<float>("Camera Settings - Boat", "cameraLockedBoatY", 33, "The y axis value that the camera should be locked to when enabled. Clipping with the ocean/waves can occur if set too low.");

            // Bow
            cameraBowSettingsEnabled = Config.Bind<bool>("Camera Settings - Bow", "bowSettingsEnable", false, "Enable or disable if there should be separate camera settings when holding a bow.");
            cameraBowX               = Config.Bind<float>("Camera Settings - Bow", "cameraBowX", defaultPosition.x, "Camera X position when holding a bow.");
            cameraBowY               = Config.Bind<float>("Camera Settings - Bow", "cameraBowY", defaultPosition.y, "Camera Y position when holding a bow.");
            cameraBowZ               = Config.Bind<float>("Camera Settings - Bow", "cameraBowZ", defaultPosition.z, "Camera Z position when holding a bow.");

            // Bow Zoom
            bowZoomEnabled              = Config.Bind<bool>("Camera Settings - Bow Zoom", "bowZoomEnable", false, "Enable or disable bow zoom");
            bowZoomOnDraw               = Config.Bind<bool>("Camera Settings - Bow Zoom", "bowZoomOnDraw", true, "Zoom in automatically when drawing the bow.");
            bowZoomKeyToggle            = Config.Bind<bool>("Camera Settings - Bow Zoom", "bowZoomKeyToggle", true, "Zoom key toggles zoom if enabled, otherwise hold the zoom key.");
            bowZoomKey                  = Config.Bind<KeyboardShortcut>("Camera Settings - Bow Zoom", "bowZoomKey", new KeyboardShortcut(KeyCode.Mouse1), "Keyboard shortcut or mouse button for zooming in with the bow.");
            bowZoomSensitivityEnabled   = Config.Bind<bool>("Camera Settings - Bow Zoom", "bowZoomSensitivityEnable", false, "Enable or disable bow zoom sensitivity.");
            bowZoomSensitivity          = Config.Bind<float>("Camera Settings - Bow Zoom", "bowZoomSensitivity", 0.5f, new ConfigDescription("Mouse sensitivity multiplier when zooming in with the bow.", new AcceptableValueRange<float>(0f, 1f)));
            bowCancelDrawKey            = Config.Bind<KeyboardShortcut>("Camera Settings - Bow Zoom", "bowCancelDrawKey", new KeyboardShortcut(KeyCode.Mouse4), "Keyboard shortcut or mouse button to cancel bow draw. This is only necessary when your zoom key interferes with the block key.");
            cameraBowZoomFOV            = Config.Bind<float>("Camera Settings - Bow Zoom", "cameraBowZoomFOV", defaultBowZoomFOV, "FOV when zooming in with the bow.");
            
            // FP Compatibility
            bowZoomFirstPersonEnabled   = Config.Bind<bool>("Camera Settings - First Person Mod Compatibility", "bowZoomFirstPersonEnable", false, "Enable or disable bow zoom when in first person. Ensures compatibility with first person mods.");
            cameraFirstPersonFOV        = Config.Bind<float>("Camera Settings - First Person Mod Compatibility", "cameraFirstPersonFOV", defaultFPFOV, "The camera fov when you are in first person. This is only used to ensure compatibility for first person mods and first person bow zoom.");
            cameraBowZoomFirstPersonFOV = Config.Bind<float>("Camera Settings - First Person Mod Compatibility", "cameraBowZoomFirstPersonFOV", defaultBowZoomFPFOV, "FOV when zooming in with the bow when in first person.");

            // Crosshair
            playerBowCrosshairEditsEnabled  = Config.Bind<bool>("Crosshair Settings", "bowCrosshairEditsEnable", true, "Enable or disable crosshair edits when using a bow.");
            playerBowCrosshairX             = Config.Bind<float>("Crosshair Settings", "bowCrosshairX", defaultValue, "Bow crosshair x position.");
            playerBowCrosshairY             = Config.Bind<float>("Crosshair Settings", "bowCrosshairY", defaultValue, "Bow crosshair y position.");

            DoPatching();
        }

        private static void setMiscCameraSettings(GameCamera __instance)
        {   
            __instance.m_smoothness         = cameraSmoothness.Value;  
            __instance.m_maxDistance        = cameraMaxDistance.Value;
            __instance.m_maxDistanceBoat    = cameraMaxDistanceBoat.Value;            
        }

        public static void DoPatching() => new Harmony("CustomizableCamera").PatchAll();

        [HarmonyPatch(typeof(GameCamera), "Awake")]
        public static class GameCamera_Awake_Patch
        {
            public static void Postfix(GameCamera __instance, ref float ___m_distance, ref float ___m_zoomSens)
            {
                if (!isEnabled.Value)
                    return;

                setMiscCameraSettings(__instance);
                ___m_distance = cameraDistance.Value;
                ___m_zoomSens = cameraZoomSensitivity.Value;
                CustomizeableCameraBase.cameraZoomSensitivityTemp = cameraZoomSensitivity.Value;
            }
        }

        [HarmonyPatch(typeof(GameCamera), "ApplySettings")]
        public static class GameCamera_ApplySettings_Patch
        {
            public static void Postfix(GameCamera __instance)
            {
                if (!isEnabled.Value)
                    return;

                setMiscCameraSettings(__instance);
            }
        }

        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class Hud_CrosshairAwake_Patch
        {
            public static void Postfix(Hud __instance)
            {
                playerInitialCrosshairX = __instance.m_crosshair.transform.position.x;
                playerInitialCrosshairY = __instance.m_crosshair.transform.position.y;

                playerInitialStealthBarX = __instance.m_stealthBar.transform.position.x;
                playerInitialStealthBarY = __instance.m_stealthBar.transform.position.y;

                lastSetCrosshairPos     = __instance.m_crosshair.transform.position;
                lastSetStealthBarPos    = __instance.m_stealthBar.transform.position;
                targetCrosshairPos      = __instance.m_crosshair.transform.position;
                targetStealthBarPos     = __instance.m_stealthBar.transform.position;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static class PlayerController_SetSensAwake_Patch
        {
            public static void Postfix()
            {           
                playerMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens);
            }
        }
    }
}