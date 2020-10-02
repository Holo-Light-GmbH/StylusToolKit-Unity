
using HoloLight.STK.Core;
using System;
using UnityEngine;

namespace HoloLight.UnityDriver
{
#if !WINDOWS_UWP 
    public class UnityConnection : IConnection
    {
        private StylusControl _stylus;
        DataCallback _onStatusUpdate;
        ConnectedCallback _onHMUConnected;
        /// <summary>
        /// Unity Driver doesn't yet support OnDisconnected event
        /// </summary>
        DisConnectedCallback _onHMUDisconnected;

        UnityBLEDevice connectedDevice;
        UnityBLEDevice connectingDevice;

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

            //_stylus.Init();
            _isConnecting = true;
            connectingDevice = (UnityBLEDevice)device;
            DeviceInformation deviceInformation = connectingDevice.DeviceInformation;
            var config = new StylusConfig(1, deviceInformation.DeviceID);

            _stylus.SetConfig(config);

            if (!_handlersRegistered)
            {
                _stylus.StatusChanged += _stylus_StatusChanged;
                _stylus.OnConnected += _stylus_OnConnected;
                _stylus.OnDisconnected += _stylus_OnDisConnected;
            }

            _handlersRegistered = true;

            try
            {
                _stylus.ConnectingtoDevice(connectingDevice.ID);
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
        private void _stylus_OnDisConnected()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _handlersRegistered = false;
                _stylus.StatusChanged -= _stylus_StatusChanged;
                _stylus.OnDisconnected -= _stylus_OnDisConnected;
                _onHMUDisconnected?.Invoke(connectedDevice);
            }
        }

        private void _stylus_OnConnected()
        {
            _stylus.OnConnected -= _stylus_OnConnected;
            _handlersRegistered = false;
            connectedDevice = connectingDevice;
            _isConnected = true;
            _onHMUConnected?.Invoke(connectedDevice);
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
            if (_stylus.StylusManager.Client != null)
            {
                _stylus.StylusManager.Client.Disconnect();
                _stylus.StylusManager.Manager.Close();
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
