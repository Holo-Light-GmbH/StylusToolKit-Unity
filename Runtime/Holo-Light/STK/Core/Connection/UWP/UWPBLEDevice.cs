
#if WINDOWS_UWP

using Windows.Devices.Enumeration;

namespace HoloLight.STK.Core
{
    public class UWPBLEDevice : IBLEDevice
    {
        public string Name { get; private set; }
        public string ID { get; private set; }
        public bool IsPaired { get; private set; }

        public bool IsConnectable()
        {
            bool isConnectable = (bool?)_deviceInformation.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
            bool isConnected = (bool?)_deviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
            return isConnectable || isConnected;
        }

        private DeviceInformation _deviceInformation;
        public DeviceInformation DeviceInformation
        {
            get
            {
                return _deviceInformation;
            }
            set
            {
                _deviceInformation = value;
                Name = value.Name;
                ID = value.Id;
                IsPaired = value.Pairing.IsPaired;
            }
        }
    }
}
#endif