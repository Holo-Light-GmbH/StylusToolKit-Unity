
#if UNITY_EDITOR

using System;
using System.Threading.Tasks;

namespace HoloLight.STK.Core.Emulator
{
    public class EmulatorConnection : IConnection
    {
        StylusControl _stylus;
        DataCallback _onStatusUpdate;
        ConnectedCallback _onHMUConnected;
        MouseStylusControl _mouseStylusControl;

        public EmulatorConnection(MouseStylusControl mouseStylusControl)
        {
            _mouseStylusControl = mouseStylusControl;
        }

        public void OnStatusChanged(StylusEventArgs e)
        {
            _onStatusUpdate?.Invoke(e.StylusData.RawData);
        }

        public async void Connect(IBLEDevice Device)
        {
            DeviceInformation deviceInformation = ((EmulatorBLEDevice)Device).DeviceInformation;
            var config = new StylusConfig(1, deviceInformation.Id);
            _stylus = new StylusControl(config);
            await _stylus.ConnectDevice(deviceInformation);
            if (_stylus == null)
            {
                throw new System.Exception("Connection Failed");
            }

            _onHMUConnected?.Invoke(Device);
            _stylus.StatusChanged += OnStatusChanged;
        }

        public void RegisterDataCallback(DataCallback Callback)
        {
            _onStatusUpdate += Callback;
        }

        public void UnRegisterDataCallback(DataCallback Callback)
        {
            _onStatusUpdate -= Callback;
        }

        public void RegisterDataCallback(ConnectedCallback Callback)
        {
            _onHMUConnected += Callback;
        }

        public void UnRegisterDataCallback(ConnectedCallback Callback)
        {
            _onHMUConnected -= Callback;
        }

        public void SendData(byte[] sendData)
        {
            if (sendData.Length > 20)
            {
                throw new System.Exception("Cant send more than 20 bytes of Data per Send Call");
            }
        }
        public StylusControl GetStylusControl()
        {
            return _stylus;
        }

        public bool IsConnected()
        {
            return true;
        }

        UnityDriver.StylusControl IConnection.GetStylusControl()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            _mouseStylusControl.DeActivate();
        }
    }

    /// <summary>
    /// Used for configuration of the Headmounted Unit (HMU).
    /// </summary>
    public class StylusConfig
    {
        /// <summary>
        /// Id of Stylus device.
        /// </summary>
        public string DeviceId;
        public StylusType StylusVersion;

        public enum StylusType
        {
            Version1,
            Version2
        }

        public StylusConfig(StylusType stylusType)
        {
            StylusVersion = stylusType;
        }

        public StylusConfig(int stylusType, string deviceId) : this((StylusType)stylusType, deviceId)
        {
        }

        public StylusConfig(StylusType stylusType, string deviceId) : this(stylusType)
        {
            this.DeviceId = deviceId;
        }

        public StylusConfig(int stylusType) : this((StylusType)stylusType)
        {

        }
    }

    /// <summary>
    /// %Event Arguments for StylusEvent data.
    /// </summary>
    public class StylusEventArgs : EventArgs
    {
        public StylusData StylusData { get; private set; }
        public StylusEventArgs() { }

        public StylusEventArgs(StylusData data)
        {
            this.StylusData = data;
        }
    }

    /// <summary>
    /// Can be used to start a bluetooth connection to the HoloStylus device.
    /// </summary>
    public class StylusControl
    {
        #region Events

        /// <summary>
        /// Handler for Hmu Events.
        /// </summary>
        public delegate void StylusStatusEventHandler(StylusEventArgs e);

        /// <summary>
        /// Status of any parameter of the stylus has changed.
        /// </summary>
        public event StylusStatusEventHandler StatusChanged;

        internal virtual void OnStatusChanged(StylusEventArgs e)
        {
            StatusChanged?.Invoke(e);
        }

        #endregion Events

        private readonly StylusConfig _stylusConfig;

        /// <summary>
        /// Bluetooth device id of the stylus used.
        /// </summary>
        public string DeviceId => _stylusConfig.DeviceId;
        public bool IsConnected => true;

        public StylusConfig.StylusType StylusVersion => _stylusConfig.StylusVersion;

        /// <summary>
        /// Holding all status data of the connected HoloStylus.
        /// </summary>
        public StylusData StylusData;

        /// <summary>
        /// Constuctor takes a StylusConfig object by which
        /// you can configure the HoloStylus.
        /// </summary>
        public StylusControl(StylusConfig config)
        {
            _stylusConfig = config;
        }

        /// <summary>
        /// Connect with the HoloStylus hardware by bluetooth.
        /// A DeviceInformation object is needed which you can
        /// get by the DeviceWatcher in DeviceDiscovery.
        /// </summary>
        public async Task ConnectDevice(DeviceInformation deviceInfo)
        {
            await Task.Delay(300);
        }
    }
}
#endif