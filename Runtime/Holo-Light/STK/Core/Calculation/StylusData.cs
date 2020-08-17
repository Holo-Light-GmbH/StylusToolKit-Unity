using UnityEngine;

namespace HoloLight.STK.Core
{
    public class StylusData
    {
        public Vector3 Position;
        public int BatteryPercentage;
        /// <summary>
        // Currently the Rotation Values are experimental
        /// </summary>
        public Vector3 Rotation;
        public bool[] Buttons = { false, false };
        public byte[] RawData { get; set; }
    }
}