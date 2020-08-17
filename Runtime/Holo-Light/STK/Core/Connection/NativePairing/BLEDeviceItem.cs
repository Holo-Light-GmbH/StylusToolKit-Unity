using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// The Item represents a HMU BLE Device that was found during search
    /// </summary>
    internal class BLEDeviceItem : MonoBehaviour
    {
        private NativePairingManager _pairingManager;

        private IBLEDevice _device;

        [SerializeField]
        private TextMeshPro _deviceNameText;
        [SerializeField]
        private TextMeshPro _deviceIdText;

        internal void Init(IBLEDevice device, NativePairingManager pairingManager)
        {
            _pairingManager = pairingManager;

            _device = device;

            _deviceNameText.text = _device.Name;
            _deviceIdText.text = pairingManager.GetCleanMACAddress(device.ID);
        }

        public void Connect()
        {
            _pairingManager.Connect(_device);
        }
    }
}