using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// Handles all the native Connection stuff. 
    /// </summary>
    public class NativePairingManager : MonoBehaviour
    {
        private HoloStylusManager _manager;

        /// <summary>
        /// The BLE Devices that were found during the last/current scanning
        /// </summary>
        private List<IBLEDevice> _hmuDevices;

        /// <summary>
        /// The ID of the saved HMU. First check with HasSavedDevice() and then access the SavedDeviceID
        /// </summary>
        public string SavedDeviceID { get; private set; }


        private Stopwatch _connectingtimeOut;

        private IBLEDevice _connectDevice = null;

        /// <summary>
        /// The path where the last HMU ID is saved.
        /// </summary>
        private string _fullPath;

        private Coroutine _searching;

        public void Init(HoloStylusManager manager)
        {
            _manager = manager;

            _connectingtimeOut = new Stopwatch();

            _hmuDevices = new List<IBLEDevice>();

            _fullPath = Application.persistentDataPath + "\\" + "SavedDeviceID.hmu";

            if (File.Exists(_fullPath))
            {
                SavedDeviceID = File.ReadAllText(_fullPath, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Saves the ID for later app sessions so it can automatically connect to the last one
        /// </summary>
        /// <param name="deviceID"></param>
        public void SaveID(string deviceID)
        {
            SavedDeviceID = deviceID;
            File.WriteAllText(_fullPath, SavedDeviceID, Encoding.UTF8);
        }

        /// <summary>
        /// Checks if a HMU device was paired in earlier app sessions so it can use that ID
        /// </summary>
        /// <returns></returns>
        internal bool HasSavedDevice()
        {
            return !string.IsNullOrEmpty(SavedDeviceID);
        }

        /// <summary>
        /// When the device is already in the list, remove it and add it as new.
        /// </summary>
        /// <param name="device"></param>
        internal void AddDevice(IBLEDevice device)
        {
            for (int i = 0; i < _hmuDevices.Count; i++)
            {
                if (_hmuDevices[i].ID == device.ID)
                {
                    return;
                }
            }
            _hmuDevices.Add(device);
        }

        public void Connect(IBLEDevice device)
        {
            _connectDevice = device;
            _connectingtimeOut.Restart();
            _manager.IsConnecting = true;
            _manager.Connector.RegisterDataCallback(OnConnected);
            _manager.Connector.Connect(_connectDevice);
        }

        /// <summary>
        /// When connected, save the ID of that HMU so next time we can reconnect to it faster
        /// </summary>
        /// <param name="connectedDevice"></param>
        private void OnConnected(IBLEDevice connectedDevice)
        {
            _connectingtimeOut.Reset();
            _connectingtimeOut.Stop();
            StopCoroutine(_searching);
            _manager.Connector.UnRegisterDataCallback(OnConnected);

            SaveID(connectedDevice.ID);
        }

        void Update()
        {
            if (_connectingtimeOut.ElapsedMilliseconds > 8000 && !_manager.IsPaired)
            {
                // trying to connect since 6s...no success
                _connectingtimeOut.Reset();
                _connectingtimeOut.Stop();
                // TODO invoke timeout event
                _manager.OnConnectionTimeoutEvent();
            }
        }

        /// <summary>
        /// Clears the list and starts searching for active HMU devices
        /// </summary>
        public void SearchAndListHMUs()
        {
            _hmuDevices.Clear();
            _manager.ScanForHMUs();

            if (_searching != null)
            {
                StopCoroutine(_searching);
            }
            _searching = StartCoroutine(StopSearching());
        }

        /// <summary>
        /// After 20s the SeachCompleted function should call, when during that time it still has not found the saved device
        /// </summary>
        /// <returns></returns>
        private IEnumerator StopSearching()
        {
            yield return new WaitForSeconds(20);
            if (!_manager.IsConnecting && !_manager.IsPaired)
            {
                _manager.DeviceScanner.StopScanning();
                _manager.OnDeviceSearchComplete(_hmuDevices);
            }
        }

        internal void EmptyStreamHandling()
        {
            byte[] rebootCommand = new byte[] { 0xFF, 01 };
            _manager.Connector.SendData(rebootCommand);
            _manager.ReConnectDelayed(1);
        }
    }
}