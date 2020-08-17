
using HoloLight.STK.Core;

namespace HoloLight.UnityDriver
{
#if !WINDOWS_UWP
    public class UnityBLEDevice : IBLEDevice
    {
        public DeviceInformation DeviceInformation;
        public UnityBLEDevice(DeviceInformation devInfo)
        {
            DeviceInformation = devInfo;
        }

        public string Name => DeviceInformation.Name;

        public string ID => DeviceInformation.DeviceID;

        public bool IsPaired => DeviceInformation.IsConnected;

        public bool IsConnectable()
        {
            return DeviceInformation.IsConnectable;
        }
    }
#endif
}
