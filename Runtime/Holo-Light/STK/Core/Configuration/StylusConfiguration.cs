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
        [Tooltip("Responsiveness Factor. The higher the value, the faster it will react to position changes. The lower the value the smoother it will react.")]
        private float _responsiveness = 30;
        public float Responsiveness { get => _responsiveness; set => _responsiveness = value; }

        [Tooltip("If set to true, you have to pair the device in the Bluetooth Settings and then start the Application")]
        [SerializeField]
        public bool UseBluetoothSettings = false;

        [Tooltip("If set to true, it will try to reconnect, when the HMU disconnects for whatever reason.")]
        [SerializeField]
        public bool ReconnectAfterDisconnection = false;

        [Tooltip("If set to true, it won't read the NNF everytime on every connection.")]
        [SerializeField]
        public bool AllowFastConnection = true;

        [Tooltip("Should searching/connecting to Stylus happen automatically after start or manully?")]
        [SerializeField]
        public StartupBehaviorType StartupBehavior = StartupBehaviorType.AutoStart;

        [Tooltip("If this option is on, it will connect to the last connected HMU Device when it finds that one.")]
        [SerializeField]
        public bool ConnectToLastDevice = true;

        [Tooltip("If set to true, it will use the mouse as Stylus inside the Unity Editor.")]
        [SerializeField]
        public bool IsEmulator = true;
    }
}