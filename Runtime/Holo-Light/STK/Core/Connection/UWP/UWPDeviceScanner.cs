
#if WINDOWS_UWP

using System.Collections.Generic;
using UnityEngine;
using HoloLight.DriverLibrary.Events;
using Windows.Devices.Enumeration;
using HoloLight.DriverLibrary;
using HoloLight.DriverLibrary.DeviceDiscovery;
using HoloLight.DriverLibrary.Devices;
using HoloLight.DriverLibrary.Data;
using HoloLight.STK.Core.Tracker;
using System.Threading.Tasks;

namespace HoloLight.STK.Core

{
    public class UWPDeviceScanner : IDeviceScanner
    {
        public List<IBLEDevice> DeviceList { get; set; }
        private List<DeviceInformation> devices;
        private DeviceFound OnDeviceFound;
        private ScanCompleted OnScanCompleted;
        private DeviceUpdated OnDeviceUpdatedCallback;
        private DeviceDiscovery DeviceDiscovery = new DeviceDiscovery();

        public UWPDeviceScanner()
        {
            DeviceList = new List<IBLEDevice>();
            devices = new List<DeviceInformation>();
        }
        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Id == deviceInfoUpdate.Id)
                {
                    devices[i].Update(deviceInfoUpdate);
                    OnDeviceUpdatedCallback?.Invoke(DeviceList[i]);
                }
            }
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Id == deviceInfoUpdate.Id)
                {
                    devices[i].Update(deviceInfoUpdate);
                }
            }
        }
        private void DeviceWatcherStopped(DeviceWatcher sender, object args)
        {
            DeviceDiscovery.DeviceWatcher.Added -= DeviceAdded;
            DeviceDiscovery.DeviceWatcher.EnumerationCompleted -= DeviceWatcherEnumerationCompleted;
            DeviceDiscovery.DeviceWatcher.Stopped -= DeviceWatcherStopped;
            DeviceDiscovery.DeviceWatcher.Removed -= OnDeviceRemoved;
            DeviceDiscovery.DeviceWatcher.Updated -= OnDeviceUpdated;
            OnScanCompleted?.Invoke(DeviceList);
        }
        private void DeviceWatcherEnumerationCompleted(DeviceWatcher sender, object args)
        {
            if (DeviceDiscovery.DeviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                DeviceDiscovery.DeviceWatcher.Stop();
            }
        }
        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            UWPBLEDevice device = new UWPBLEDevice();
            device.DeviceInformation = args;
            devices.Add(args);
            DeviceList.Add(device);
            OnDeviceFound?.Invoke(device);
        }

        public async void StartScanning()
        {
            if (DeviceDiscovery.DeviceWatcher == null)
            {
                Debug.LogError("DeviceDiscovery is null");
                return;
            }


            if (DeviceDiscovery.DeviceWatcher.Status != DeviceWatcherStatus.Started)
            {
                DeviceDiscovery.DeviceWatcher.Added += DeviceAdded;
                DeviceDiscovery.DeviceWatcher.EnumerationCompleted += DeviceWatcherEnumerationCompleted;
                DeviceDiscovery.DeviceWatcher.Stopped += DeviceWatcherStopped;
                DeviceDiscovery.DeviceWatcher.Removed += OnDeviceRemoved;
                DeviceDiscovery.DeviceWatcher.Updated += OnDeviceUpdated;
                DeviceDiscovery.DeviceWatcher.Start();
                devices.Clear();
                DeviceList.Clear();
            } else 
            {
                DeviceDiscovery.DeviceWatcher.Stop();
                await Task.Delay(200);
                StartScanning();
            }
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

        public void StopScanning()
        {
            if (DeviceDiscovery.DeviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                DeviceDiscovery.DeviceWatcher.Stop();
            }
        }
    }
}
#endif