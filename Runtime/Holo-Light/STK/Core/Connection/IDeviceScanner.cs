using System.Collections.Generic;

namespace HoloLight.STK.Core
{
    public delegate void ScanCompleted(List<IBLEDevice> DeviceList);
    public delegate void DeviceFound(IBLEDevice Device);
    public delegate void DeviceUpdated(IBLEDevice Device);
    public interface IDeviceScanner
    {
        List<IBLEDevice> DeviceList { get; set; }
        void StartScanning();
        void StopScanning();
        void RegisterScanCompletedCallback(ScanCompleted callback);
        void RegisterDeviceFoundCallback(DeviceFound callback);
        void RegisterDeviceUpdatedCallback(DeviceUpdated callback);
        void UnRegisterScanCompletedCallback(ScanCompleted callback);
        void UnRegisterDeviceFoundCallback(DeviceFound callback);
        void UnRegisterDeviceUpdatedCallback(DeviceUpdated callback);
    }
}