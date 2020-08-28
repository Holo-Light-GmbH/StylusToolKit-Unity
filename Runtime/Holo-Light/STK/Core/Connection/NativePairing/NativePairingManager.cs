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
    public class NativePairingManager : ItemBrowser
    {
        private HoloStylusManager _manager;

        /// <summary>
        /// A prefab for the items in the list
        /// </summary>
        [SerializeField]
        private GameObject _bleDeviceItem;

        /// <summary>
        /// The BLE Devices that were found during the last/current scanning
        /// </summary>
        private List<IBLEDevice> _hmuDevices;

        /// <summary>
        /// The ID of the saved HMU. First check with HasSavedDevice() and then access the SavedDeviceID
        /// </summary>
        public string SavedDeviceID { get; private set; }

        /// <summary>
        /// The Panel where the found devices gets listed
        /// </summary>
        [SerializeField]
        private GameObject foundDevicesWindow;

        /// <summary>
        /// The Panel which you see when it is trying to connect to device
        /// </summary>
        [SerializeField]
        private GameObject _connectingWindow;

        /// <summary>
        /// The button inside the FoundDevices Pane, that appears when searching is complete
        /// </summary>
        [SerializeField]
        private GameObject _retryButton;

        private Stopwatch _connectingtimeOut;

        private bool _connect = false;
        private IBLEDevice _connectDevice = null;
        private bool _hideConnectingWindow = false;

        /// <summary>
        /// The path where the last HMU ID is saved.
        /// </summary>
        private string _fullPath;

        private Coroutine _searching;

        /// <summary>
        /// This bool is used to know, when to update the list in the main thread, when a new ble device is found
        /// </summary>
        private bool _newDeviceAdded = false;
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
            _newDeviceAdded = true;
        }

        public void Connect(IBLEDevice device)
        {
            _connectDevice = device;
            _connect = true;
        }

        /// <summary>
        /// Connects and displays the Connecting Window where you can see the name and ID of the device you are connecting to
        /// </summary>
        private void ConnectGUI()
        {
            _connectingtimeOut.Restart();
            _manager.IsConnecting = true;
            _manager.Connector.RegisterDataCallback(OnConnected);
            Debug.Log("Connecting to: " + _connectDevice.Name);
            _manager.Connector.Connect(_connectDevice);
            foundDevicesWindow.SetActive(false);
            _connectingWindow.SetActive(true);
            _connectingWindow.GetComponentInChildren<TextMeshPro>().text = "Connecting to\n" + _connectDevice.Name + "\n" + GetCleanMACAddress(_connectDevice.ID);
        }

        /// <summary>
        /// When connected, save the ID of that HMU so next time we can reconnect to it faster
        /// </summary>
        /// <param name="connectedDevice"></param>
        private void OnConnected(IBLEDevice connectedDevice)
        {
            _connectingtimeOut.Reset();
            _connectingtimeOut.Stop();
            _hideConnectingWindow = true;
            _manager.Connector.UnRegisterDataCallback(OnConnected);

            SaveID(connectedDevice.ID);
        }

        void Update()
        {
            if (_newDeviceAdded)
            {
                _newDeviceAdded = false;
                ListFoundHMUDevices();
            }

            if (_connect)
            {
                _connect = false;
                ConnectGUI();
            }

            if (_connectingtimeOut.ElapsedMilliseconds > 6000)
            {
                // trying to connect since 6s...no success
                _connectingtimeOut.Reset();
                _connectingtimeOut.Stop();
                ConnectGUI();
            }

            if (_hideConnectingWindow)
            {
                _hideConnectingWindow = false;
                _connectingWindow.SetActive(false);
            }
        }

        /// <summary>
        /// When scanning is finished, show up the retry button and if no devices found, change the text
        /// </summary>
        internal void SearchCompleted()
        {
            _retryButton.SetActive(true);

            if (_hmuDevices.Count == 0)
            {
                _emptyText.text = "No HMUs found.\nMake sure that it is turned on.";
            }
        }

        /// <summary>
        /// Clears the list and starts searching for active HMU devices
        /// </summary>
        public void SearchAndListHMUs()
        {
            _emptyText.text = "Searching ...";
            _hmuDevices.Clear();
            ListFoundHMUDevices();
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
                SearchCompleted();
            }
        }

        /// <summary>
        /// Lists the HMU Devices that are found
        /// </summary>
        private void ListFoundHMUDevices()
        {
            _pages.Clear();
            foreach (Transform child in _list.transform)
            {
                Destroy(child.gameObject);
            }

            _connectingWindow.SetActive(false);
            foundDevicesWindow.SetActive(true);
            _retryButton.SetActive(false);

            int neededPages = (int)Math.Ceiling((decimal)(_hmuDevices.Count) / 5);
            int deviceIndex = 0;
            for (int pageIndex = 0; pageIndex < neededPages; pageIndex++)
            {
                GameObject page = new GameObject("Page" + pageIndex);
                page.SetActive(false);
                page.transform.parent = _list.transform;
                page.transform.localPosition = Vector3.zero;
                page.transform.localScale = Vector3.one;
                page.transform.localRotation = Quaternion.identity;

                int thisPageCount = 0;

                _pages.Add(page);

                for (; deviceIndex < _hmuDevices.Count; deviceIndex++)
                {
                    if (thisPageCount >= 5)
                    {
                        break;
                    }

                    var file = Instantiate(_bleDeviceItem, page.transform);
                    file.GetComponent<BLEDeviceItem>().Init(_hmuDevices[deviceIndex], this);
                    file.transform.localPosition = new Vector3(0, -file.transform.localScale.y * 1f * thisPageCount, 0);
                    thisPageCount++;
                }
            }

            if (_pages.Count > 0)
            {
                _pages[_currentOpenPageIndex].SetActive(true);
                _emptyText.gameObject.SetActive(false);
            }
            else
            {
                _emptyText.gameObject.SetActive(true);
            }

            HandlePageButtons();
        }

        internal void EmptyStreamHandling()
        {
            byte[] rebootCommand = new byte[] { 0xFF, 01 };
            _manager.Connector.SendData(rebootCommand);
            _manager.ReConnectDelayed(1);
        }

        /// <summary>
        /// Cleans up the ID/Mac and returns a common string for the user (in this format AA:BB:CC:DD:EE:DD)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal string GetCleanMACAddress(string ID)
        {
            string macAddress = ""; 
            if (ID.Length == 12)
            {
                var builder = new StringBuilder();
                int count = 0;
                foreach (var c in ID)
                {
                    builder.Append(c);
                    if ((++count % 2) == 0 && count % 12 != 0)
                    {
                        builder.Append(':');
                    }
                }
                macAddress = builder.ToString().ToUpper();
            } else if (ID.Length < 17)
            {
                macAddress = ID.ToUpper();
            }
            else
            {
                macAddress = ID.Substring(ID.Length - 17).ToUpper();
            }
            return macAddress;
        }
    }
}