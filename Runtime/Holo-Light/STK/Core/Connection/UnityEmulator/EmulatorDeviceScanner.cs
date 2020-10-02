
#if UNITY_EDITOR

using System.Collections.Generic;

namespace HoloLight.STK.Core.Emulator
{
    public class EmulatorDeviceScanner : IDeviceScanner
    {
        public List<IBLEDevice> DeviceList { get; set; }
        private DeviceFound OnDeviceFound;
        private ScanCompleted OnScanCompleted;
        private DeviceUpdated OnDeviceUpdated;

        public EmulatorDeviceScanner()
        {
            DeviceList = new List<IBLEDevice>();
        }

        public void StartScanning()
        {
            AddEmulatorBLEDevice();
        }

        public void StopScanning()
        {
        }

        void AddEmulatorBLEDevice()
        {
            // e.g. after 1 Seconds Scanning, it has found a device!
            EmulatorBLEDevice newEmulatorBleDevice = new EmulatorBLEDevice();
            DeviceInformation newDeviceInformation = new DeviceInformation();
            newDeviceInformation.Name = "EMULATOR_HMU_V_2";
            newDeviceInformation.Id = "AA:BB:CC:DD:EE:FF";

            DeviceInformationPairing pairingInfo = new DeviceInformationPairing();
            pairingInfo.IsPaired = true;

            newDeviceInformation.Pairing = pairingInfo;

            newEmulatorBleDevice.DeviceInformation = newDeviceInformation;

            DeviceList.Add(newEmulatorBleDevice);

            OnDeviceFound?.Invoke(newEmulatorBleDevice);
            OnScanCompleted?.Invoke(DeviceList);
        }

        public void RegisterDeviceFoundCallback(DeviceFound callback)
        {
            OnDeviceFound += callback;
        }

        public void UnRegisterDeviceFoundCallback(DeviceFound callback)
        {
            OnDeviceFound -= callback;
        }

        public void RegisterScanCompletedCallback(ScanCompleted callback)
        {
            OnScanCompleted += callback;
        }

        public void UnRegisterScanCompletedCallback(ScanCompleted callback)
        {
            OnScanCompleted -= callback;
        }

        public void RegisterDeviceUpdatedCallback(DeviceUpdated callback)
        {
            OnDeviceUpdated += callback;
        }

        public void UnRegisterDeviceUpdatedCallback(DeviceUpdated callback)
        {
            OnDeviceUpdated -= callback;
        }
    }
}
#endif