using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core
{
    public class StylusUI : ItemBrowser
    {
        [SerializeField]
        private HoloStylusManager _manager;

        /// <summary>
        /// The BLE Devices that were found during the last/current scanning
        /// </summary>
        private List<IBLEDevice> _hmuDevices = new List<IBLEDevice>();

        /// <summary>
        /// A prefab for the items in the list
        /// </summary>
        [SerializeField]
        private GameObject _bleDeviceItem;

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

        /// <summary>
        /// This bool is used to know, when to update the list in the main thread, when a new ble device is found
        /// </summary>
        private bool _showUpdateList = false;
        private bool _hideConnectingWindow = false;
        private bool _connecting = false;
        private IBLEDevice _connectDevice = null;
        private bool _searching = false;

        private void Awake()
        {
            foundDevicesWindow.SetActive(true);
            _manager.StartSearchEvent += _manager_StartSearchEvent;
            _manager.DeviceFoundEvent += _manager_DeviceFoundEvent;
            _manager.DeviceSearchCompleteEvent += _manager_DeviceSearchCompleteEvent;
            _manager.ConnectingStarted += _manager_ConnectingStarted;
            _manager.ConnectionTimeoutEvent += _manager_ConnectionTimeoutEvent;
            _manager.OnStylusReady += _onStylusReady;
        }

        private void _manager_StartSearchEvent(object sender, EventArgs e)
        {
            _hmuDevices.Clear();
            _hmuDevices = new List<IBLEDevice>();
            _searching = true;
            _emptyText.text = "Searching...";
            _showUpdateList = true;
        }

        private void _manager_ConnectionTimeoutEvent(object sender, EventArgs e)
        {
            _manager.StartStylus();
        }

        private void _manager_ConnectingStarted(object sender, IBLEDevice connectingDevice)
        {
            _connecting = true;
            _connectDevice = connectingDevice;
        }

        private void _manager_DeviceFoundEvent(object sender, IBLEDevice foundDevice)
        {
            for (int i = 0; i < _hmuDevices.Count; i++)
            {
                if (_hmuDevices[i].ID == foundDevice.ID)
                {
                    return;
                }
            }
            _hmuDevices.Add(foundDevice);
            _showUpdateList = true;
        }

        private void _onStylusReady(object sender, IBLEDevice connectedDevice)
        {
            _hmuDevices.Clear();
            _searching = false;
            _hideConnectingWindow = true;
        }

        /// <summary>
        /// Connects and displays the Connecting Window where you can see the name and ID of the device you are connecting to
        /// </summary>
        private void OpenConnectingWindow()
        {
            foundDevicesWindow.SetActive(false);
            _connectingWindow.SetActive(true);
            _connectingWindow.GetComponentInChildren<TextMeshPro>().text = "Connecting to\n" + _connectDevice.Name + "\n" + _manager.GetCleanMACAddress(_connectDevice.ID);
            // could not connect
        }

        /// <summary>
        /// Lists the HMU Devices that are found
        /// </summary>
        private void ListFoundHMUDevices()
        {
            if (_manager.IsConnecting) return;
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
                    file.GetComponent<BLEDeviceItem>().Init(_hmuDevices[deviceIndex], _manager);
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

            if (_searching == false)
            {
                _retryButton.SetActive(true);
            }

            HandlePageButtons();
        }


        /// <summary>
        /// When scanning is finished, show up the retry button and if no devices found, change the text
        /// </summary>
        private void _manager_DeviceSearchCompleteEvent(object sender, List<IBLEDevice> foundDevicesList)
        {
            _searching = false;
            _hmuDevices = foundDevicesList;
            _showUpdateList = true;
            if (_hmuDevices.Count == 0)
            {
                _emptyText.text = "No HMUs found.\nMake sure that it is turned on.";
            }
        }

        public void SearchAgain()
        {
            _emptyText.text = "Searching ...";
            _hmuDevices.Clear();
            ListFoundHMUDevices();
            _manager.StartStylus();
        }

        // Update is called once per frame
        void Update()
        {
            if (_showUpdateList)
            {
                _showUpdateList = false;
                ListFoundHMUDevices();
            }

            if (_connecting)
            {
                _connecting = false;
                OpenConnectingWindow();
            }

            if (_hideConnectingWindow)
            {
                _hideConnectingWindow = false;
                _connectingWindow.SetActive(false);
                foundDevicesWindow.SetActive(false);
            }
        }
    }
}