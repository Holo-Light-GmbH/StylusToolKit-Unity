
#if WINDOWS_UWP

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Devices.Enumeration;
using HoloLight.DriverLibrary;
using HoloLight.DriverLibrary.Devices;
using HoloLight.DriverLibrary.Events;
using HoloLight.DriverLibrary.DeviceDiscovery;
using HoloLight.DriverLibrary.Data;
using HoloLight.STK.Core.Tracker;

namespace HoloLight.STK.Core
{

    public class UWPConnection : IConnection
    {
        private StylusControl _stylus;
        DataCallback _onStatusUpdate;
        ConnectedCallback _onHMUConnected;
        private DisConnectedCallback _onHMUDisconnected;

        private bool _isConnecting = false;
        private IBLEDevice _device = null;

        public StylusControl GetStylusControl() 
        {
            return _stylus;
        }

        public void OnStatusChanged(StylusEventArgs e)
        {
            _onStatusUpdate?.Invoke(e.StylusData.RawData);
        }

        public void OnFirstValueRecieved(StylusEventArgs e) 
        {
            _stylus.StatusChanged -= OnFirstValueRecieved;
            _onHMUConnected?.Invoke(_device);
        }

        private void OnDisconnected() 
        {
            _onHMUDisconnected?.Invoke(_device);
        }

        public async void Connect(IBLEDevice Device)
        {
            if (_isConnecting) return;
            _isConnecting = true;
            if (_stylus != null)
            {
                _stylus.StatusChanged -= OnStatusChanged;
                _stylus.StatusChanged -= OnFirstValueRecieved;
                _stylus.OnDisconnected -= OnDisconnected;
                _stylus = null;
            }
            try 
            {
                DeviceInformation deviceInformation = ((UWPBLEDevice)Device).DeviceInformation;
                var config = new StylusConfig(1, deviceInformation.Id);
                _stylus = new StylusControl(config);
                await _stylus.ConnectDevice(deviceInformation);
                if (_stylus == null)
                {
                    throw new System.Exception("Connection Failed");
                }
                await _stylus.StartServices();
                _device = Device;
                _stylus.StatusChanged += OnStatusChanged;
                _stylus.StatusChanged += OnFirstValueRecieved;
                _stylus.OnDisconnected += OnDisconnected;
            } 
            catch (Exception e) 
            {
                Debug.Log("Conencting failed: " + e.Message);
            } 
            finally 
            {
                _isConnecting = false;
            }
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
            if (sendData.Length > 20)
            {
                Debug.Log("Cant send more than 20 bytes of Data per Send Call:");
                return;
            }
            _stylus.SendBytes(sendData);
        }

        public void Disconnect() 
        {
            _stylus.CloseConnection();    
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
}

#endif