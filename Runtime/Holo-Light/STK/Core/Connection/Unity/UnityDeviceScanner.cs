using System;
using System.Collections.Generic;
using UnityEngine;
using HoloLight.STK.Core;

namespace HoloLight.UnityDriver
{
#if !WINDOWS_UWP
    public class UnityDeviceScanner : IDeviceScanner
    {
        private StylusManager _stylusManager => _stylusControl.StylusManager;

        private DeviceFound OnDeviceFound;
        private ScanCompleted OnScanCompleted;
        private DeviceUpdated OnDeviceUpdatedCallback;

        private bool _handlersRegistered = false;

        public List<IBLEDevice> DeviceList { get; set; }

        private StylusControl _stylusControl;

        public UnityDeviceScanner(StylusControl stylusControl)
        {
            _stylusControl = stylusControl;
            DeviceList = new List<IBLEDevice>();
        }

        private void OnDiscoveringCompleted(object sender, IntPtr Radio, int Error)
        {
            _stylusManager.Manager.OnDeviceFound -= OnNewDeviceFound;
            _stylusManager.Manager.OnDiscoveringCompleted -= OnDiscoveringCompleted;
            _handlersRegistered = false;
            _stylusManager.CurrentRadio = Radio;
            OnScanCompleted?.Invoke(DeviceList);
        }

        private void OnNewDeviceFound(object sender, IntPtr Radio, long Address)
        {
            _stylusManager.CurrentRadio = Radio;

            string bleDeviceName;
            _stylusManager.Manager.GetRemoteName(Radio, Address, out bleDeviceName);

            bool isConnectAble;
            _stylusManager.Manager.GetConnectable(Radio, out isConnectAble);

            DeviceInformation deviceInformation = new DeviceInformation();
            deviceInformation.Name = bleDeviceName;
            deviceInformation.DeviceID = Address.ToString("X2");
            deviceInformation.IsConnectable = isConnectAble;

            UnityBLEDevice bLEDevice = new UnityBLEDevice(deviceInformation);

            DeviceList.Add(bLEDevice);

            OnDeviceFound?.Invoke(bLEDevice);
        }

        private void Discover()
        {
            IntPtr radio = GetRadio();

            if (radio != null)
            {
                Int32 devicesDiscoveryError = _stylusManager.Manager.Discover(radio, 20);

                if (devicesDiscoveryError != BluetoothErrors.WCL_E_SUCCESS)
                {
                    _stylusManager.Errorlist = devicesDiscoveryError.ToString();
                }
            }
        }

        private IntPtr GetRadio()
        {
            if (_stylusManager.Manager.Count == 1)
            {
                return _stylusManager.Manager[0];
            }

            for (Int32 i = 0; i < _stylusManager.Manager.Count; i++)
            {
                if (_stylusManager.Manager.IsRadioAvailable(_stylusManager.Manager[i]))
                {
                    return _stylusManager.Manager[i];
                }
            }
            return IntPtr.Zero;
        }

        public void StartScanning()
        {
            DeviceList.Clear();

            if (_stylusManager.Manager == null)
            {
                _stylusControl.Init();
            }

            if (!_handlersRegistered)
            {
                _stylusManager.Manager.OnDeviceFound += OnNewDeviceFound;
                _stylusManager.Manager.OnDiscoveringCompleted += OnDiscoveringCompleted;
                _handlersRegistered = true;

                Int32 Res = _stylusManager.Manager.Open();

                if (Res != BluetoothErrors.WCL_E_SUCCESS)
                {
                //    Debug.Log("Failed to open Bluetooth Manager: 0x" + Res.ToString("X8"));
                }
            }

            Discover();
        }

        public void StopScanning()
        {
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
            OnDeviceUpdatedCallback += callback;
        }

        public void UnRegisterDeviceUpdatedCallback(DeviceUpdated callback)
        {
            OnDeviceUpdatedCallback -= callback;
        }

    }
#endif
}