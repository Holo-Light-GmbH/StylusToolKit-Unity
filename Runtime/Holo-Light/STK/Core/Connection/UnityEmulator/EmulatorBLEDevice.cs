
#if UNITY_EDITOR

namespace HoloLight.STK.Core.Emulator
{
    public class EmulatorBLEDevice : IBLEDevice
    {
        public string Name { get; private set; }
        public string ID { get; private set; }
        public bool IsPaired { get; private set; }
        public bool IsConnectable()
        {
            return true;
        }

        private DeviceInformation _deviceInformation;

        public DeviceInformation DeviceInformation
        {
            get { return _deviceInformation; }
            set
            {
                _deviceInformation = value;
                Name = value.Name;
                ID = value.Id;
                IsPaired = value.Pairing.IsPaired;
            }
        }
    }

    public class DeviceInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DeviceInformationPairing Pairing { get; set; }
    }

    public class DeviceInformationPairing
    {
        public bool IsPaired { get; set; }
    }
}

#endif