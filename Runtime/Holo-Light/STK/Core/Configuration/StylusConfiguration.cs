using UnityEngine;

namespace HoloLight.STK.Core
{
    [CreateAssetMenu(fileName = "StylusConfiguration", menuName = "Holo-Light/StylusConfiguration", order = 1)]
    public class StylusConfiguration : ScriptableObject
    {
        public enum StartupBehaviorType
        {
            ManualStart = 0,
            AutoStart = 1
        }

        [Range(10f, 50f)]
        [SerializeField]
        [Tooltip("Smoothnessfactor. The lower the value the smoother it will move. But that means that the reaction is slower")]
        private float _smoothness = 28;
        public float Smoothness => _smoothness;

        [Tooltip("If set to true, you have to pair the device in the Bluetooth Settings and then start the Application")]
        [SerializeField]
        public bool UseBluetoothSettings = false;

        [Tooltip("If set to true, it will try to reconnect, when the HMU disconnects for whatever reason.")]
        [SerializeField]
        public bool ReconnectAfterDisconnection = false;

        [Tooltip("Should searching/connecting to Stylus happen automatically after start or manully?")]
        [SerializeField]
        public StartupBehaviorType StartupBehavior = StartupBehaviorType.AutoStart;

        [Tooltip("If set to true, it will use the mouse as Stylus inside the Unity Editor.")]
        [SerializeField]
        public bool IsEmulator = true;
    }
}