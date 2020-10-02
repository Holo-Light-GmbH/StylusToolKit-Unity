using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// The Item represents a HMU BLE Device that was found during search
    /// </summary>
    internal class BLEDeviceItem : MonoBehaviour
    {
        private HoloStylusManager _manager;

        private IBLEDevice _device;

        [SerializeField]
        private TextMeshPro _deviceNameText;
        [SerializeField]
        private TextMeshPro _deviceIdText;

        internal void Init(IBLEDevice device, HoloStylusManager manager)
        {
            _manager = manager;

            _device = device;

            _deviceNameText.text = _device.Name;
            _deviceIdText.text = _manager.GetCleanMACAddress(device.ID);
        }

        public void Connect()
        {
            _manager.ConnectToHMU(_device);
        }
    }
}