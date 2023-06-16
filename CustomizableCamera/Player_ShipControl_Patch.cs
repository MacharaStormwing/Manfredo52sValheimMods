using UnityEngine;
using HarmonyLib;

namespace CustomizableCamera
{
    [HarmonyPatch(typeof(Player), "StartDoodadControl")]
    public class Player_StartShipControl_Patch : CustomizeableCameraBase
    {
        public static void Postfix(Player __instance)
        {
            if (!CustomizableCamera.isEnabled.Value || !__instance)
                return;

            characterControlledShip = true;
            characterStoppedShipControl = false;
            canChangeCameraDistance = true;
        }
    }

    [HarmonyPatch(typeof(Player), "StopDoodadControl")]
    public class Player_StopShipControl_Patch : CustomizeableCameraBase
    {
        public static void Postfix(Player __instance)
        {
            if (!CustomizableCamera.isEnabled.Value || !__instance)
                return;

            characterControlledShip = false;
            characterStoppedShipControl = true;
            canChangeCameraDistance = true;
        }
    }
}