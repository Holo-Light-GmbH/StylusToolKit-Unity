
#if WINDOWS_UWP

using HoloLight.DriverLibrary.Devices;

#else

using HoloLight.UnityDriver;

#endif

namespace HoloLight.STK.Core
{
    public delegate void DataCallback(byte[] data);
    public delegate void ConnectedCallback(IBLEDevice device);
    public delegate void DisConnectedCallback(IBLEDevice disconnectedDevice);

    public delegate void DFUFinished();
    public delegate void DFUProgressChanged(float progress);
    public interface IConnection
    {
        void Connect(IBLEDevice Device);
        void Disconnect();
        bool IsConnected();
        void RegisterDataCallback(DataCallback Callback);
        void UnRegisterDataCallback(DataCallback Callback);
        void RegisterDataCallback(ConnectedCallback Callback);
        void UnRegisterDataCallback(ConnectedCallback Callback);
        void RegisterDisconnectCallback(DisConnectedCallback Callback);
        void UnRegisterDisconnectCallback(DisConnectedCallback Callback);
        void SendData(byte[] sendData);
        StylusControl GetStylusControl();
    }
}