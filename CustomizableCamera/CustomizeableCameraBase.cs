using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizableCamera
{
    public class CustomizeableCameraBase
    {

        CustomizeableCameraBase instance = null;

        // State changing
        public static bool characterStateChanged;
        public static bool characterControlledShip;
        public static bool characterStoppedShipControl;
        public static bool characterCrouched;
        public static bool characterAiming;
        public static bool characterSprinting;
        public static bool characterWalking;
        public static bool characterEquippedBow;

        public static bool playerIsMoving;
        public static bool playerInShelter;
        public static bool playerInInterior;
        public static bool isFirstPerson;
        public static bool onSwappedShoulder;
        public static bool canChangeCameraDistance;

        public static float cameraZoomSensitivityTemp = 10f;

        public enum interpolationTypes
        {
            Linear,
            SmoothStep
        }

        public enum characterState
        {
            standing,
            walking,
            sprinting,
            crouching,
            sailing,
            bowequipped,
            bowaiming
        };

        public static characterState __characterState;
        public static characterState __characterStatePrev;
    }
}
