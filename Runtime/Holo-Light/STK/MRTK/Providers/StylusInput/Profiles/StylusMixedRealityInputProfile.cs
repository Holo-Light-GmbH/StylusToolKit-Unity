
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

namespace HoloLight.STK.MRTK
{
    [CreateAssetMenu(
        menuName = "Mixed Reality Toolkit/Profiles/Stylus Mixed Reality Input Profile",
        fileName = "StylusMixedRealityInputProfile",
        order = (int) CreateProfileMenuItemIndices.Input)]
    [MixedRealityServiceProfile(typeof(StylusDeviceManager))]
    [HelpURL("https://github.com/Holo-Light/HoloStylusToolkit-Unity")]
    public class StylusMixedRealityInputProfile : BaseMixedRealityProfile
    {
        [Header("Stylus Settigns")]

        [Header("Unity Stylus Emulator")]

        [SerializeField]
        [Tooltip("Distance of the stylus tip at start.")]
        private float _startingDistance = 0.8f;
        public float StartingDistance => _startingDistance;

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("Key to move stylus forward")]
        private KeyBinding _stylusForwardKey = KeyBinding.FromKey(KeyCode.Z);
        public KeyBinding StylusForwardKey => _stylusForwardKey;

        [SerializeField]
        [Tooltip("Key to move stylus backward")]
        private KeyBinding _stylusBackwardKey = KeyBinding.FromKey(KeyCode.H);
        public KeyBinding StylusBackwardKey => _stylusBackwardKey;
#endif
        [SerializeField]
        [Tooltip("Speed of changing the depth.")]
        private float _depthSpeed = 1;
        public float DepthSpeed => _depthSpeed;
    }
}