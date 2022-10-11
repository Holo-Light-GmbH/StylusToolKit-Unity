
using HoloLight.STK.Core;
using System;
using UnityEngine;

namespace HoloLight.UnityDriver
{
#if !WINDOWS_UWP 
    public class UnityConnection : IConnection
    {
        private StylusControl _stylus;
        private DataCallback _onStatusUpdate;
        private ConnectedCallback _onHMUConnected;

        private DisConnectedCallback _onHMUDisconnected;

        private UnityBLEDevice _connectedDevice;
        private UnityBLEDevice _connectingDevice;

        private bool _isConnecting = false;
        private bool _handlersRegistered = false;

        private bool _isConnected = true;

        public UnityConnection()
        {
            _stylus = new StylusControl();
        }

        private void _stylus_StatusChanged(byte[] data)
        {
            _onStatusUpdate?.Invoke(data);
        }

        public void Connect(IBLEDevice device)
        {
            if (_isConnecting) return;

            _isConnecting = true;
            _connectingDevice = (UnityBLEDevice)device;
            DeviceInformation deviceInformation = _connectingDevice.DeviceInformation;
            var config = new StylusConfig(1, deviceInformation.DeviceID);

            _stylus.SetConfig(config);

            if (!_handlersRegistered)
            {
                _stylus.StatusChanged += _stylus_FirstDataRecieved;
                _stylus.OnDisconnected += _stylus_OnDisConnected;
            }

            _handlersRegistered = true;

            try
            {
                _stylus.ConnectToDevice(_connectingDevice.ID);


            }
            catch (Exception e)
            {
                Debug.Log("Connecting failed: " + e.Message);
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private void _stylus_FirstDataRecieved(byte[] data)
        {
            // is connected only, when first data recieved...
            _stylus.StatusChanged -= _stylus_FirstDataRecieved;
            _stylus.StatusChanged += _stylus_StatusChanged;
            _stylus_OnConnected();
        }

        private void _stylus_OnDisConnected()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _handlersRegistered = false;
                _stylus.StatusChanged -= _stylus_StatusChanged;
                _stylus.OnDisconnected -= _stylus_OnDisConnected;
                _onHMUDisconnected?.Invoke(_connectedDevice);
            }
        }

        private void _stylus_OnConnected()
        {
           // _stylus.OnConnected -= _stylus_OnConnected;
            _handlersRegistered = false;
            _connectedDevice = _connectingDevice;
            _isConnected = true;
            _onHMUConnected?.Invoke(_connectedDevice);
        }

        public StylusControl GetStylusControl()
        {
            return _stylus;
        }

        public bool IsConnected()
        {
            return _stylus.IsConnected;
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
            _stylus.SendBytes(sendData);
        }

        public void Disconnect()
        {
            if (_stylus.StylusManager != null)
            {
                _stylus.CloseConnection();
            }
        }

        public void RegisterDisconnectCallback(DisConnectedCallback Callback)
        {
            _onHMUDisconnected += Callback;
        }

        public void UnRegisterDisconnectCallback(DisConnectedCallback Callback)
        {
            _onHMUDisconnected -= Callback;
        }
    }
#endif
}
