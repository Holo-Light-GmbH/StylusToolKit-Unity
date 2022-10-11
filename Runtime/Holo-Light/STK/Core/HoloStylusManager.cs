
#if UNITY_STANDALONE || UNITY_EDITOR
using HoloLight.UnityDriver;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLight.STK.Core.Tracker;
using HoloLight.STK.MRTK;
#if UNITY_EDITOR
using HoloLight.STK.Core.Emulator;
#endif
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// This is the main global class for the stylus which handles all the communication between the classes
    /// </summary>
    public class HoloStylusManager : MonoBehaviour
    {
        /// <summary>
        /// With the StylusConfiguration, you can adjust the speed and some other settings regarding connection.
        /// </summary>
        public StylusConfiguration StylusConfiguration;

        public IConnection Connector;

        public IDeviceScanner DeviceScanner;
        public IStylusDataParser DataParser = new HMU2DataParser();
        public IBLEDevice Device;

        private StylusData _stylusFrame;

        private const string HMU_SUFFIX = "hmu_v_2";

        public EventManager EventManager = new EventManager();
        public CalibrationPreferences CalibrationPreferences = new CalibrationPreferences();
        public StylusTransform StylusTransform = new StylusTransform();
        public InfoManager InfoManager = new InfoManager();
        private NNReader _nnReader = new NNReader();
         
        public VisualSettings VisualSettings = new VisualSettings();
        public IBLEDevice ConnectedHMU;

        /// <summary>
        /// Is Stylus visible in the current frame.
        /// </summary>
        public bool IsStylusVisible
        {
            get => DataParser.IsVisible;
        }
        /// <summary>
        /// Was Stylus visible in the previous frame.
        /// </summary>
        private bool wasStylusVisible = false;

        [SerializeField]
        private bool _logInfos = false;

        /// <summary>
        /// A Reference to the PointerSwitcher
        /// </summary>
        [SerializeField]
        public StylusPointerSwitcher PointerSwitcher;

        /// <summary>
        /// Responsible to handle the GUI for Pairing the HMU
        /// </summary>
        [SerializeField]
        private NativePairingManager _nativePairingManager;

#if UNITY_EDITOR
        /// <summary>
        /// The Emulator Script that Simulates the Stylus
        /// </summary>
        private MouseStylusControl _mouseStylusControl;
#endif
        public bool IsPaired { get; set; } = false;
        public bool IsConnecting { get; set; } = false;

        private Stopwatch _timeOutWatch = new Stopwatch();

        private string _filePath;
        private string _applicationPath;

        private bool _invokeStylusReadyEvent;
        private bool _invokeStylusDisConnectedEvent = false;

        public event EventHandler StartSearchEvent;
        public event EventHandler<IBLEDevice> DeviceFoundEvent;
        public event EventHandler<List<IBLEDevice>> DeviceSearchCompleteEvent;
        public event EventHandler<IBLEDevice> ConnectingStarted;
        /// <summary>
        /// OnConnected = Bluetooth Connection established
        /// OnStylusReady = HMU Initialized and ready to use
        /// </summary>
        public event EventHandler<IBLEDevice> OnStylusReady;
        public event EventHandler DisconnectedEvent;
        public event EventHandler ConnectionTimeoutEvent;

        public event EventHandler StylusFoundEvent;
        public event EventHandler StylusLostEvent;

        private ConcurrentQueue<IBLEDevice> _lastFoundHMUDevices = new ConcurrentQueue<IBLEDevice>();
        private List<IBLEDevice> _knownHMUs = new List<IBLEDevice>();

        internal void OnStartSearch()
        {
            _knownHMUs.Clear();
            Debug.Log("OnStartSearch");
            StartSearchEvent?.Invoke(this, EventArgs.Empty);
        }

        internal void OnHMUDeviceFound(IBLEDevice device)
        {
            Debug.Log("OnHMUDeviceFound");
            if(!_knownHMUs.Contains(device))
            {
                _knownHMUs.Add(device);
                _lastFoundHMUDevices.Enqueue(device);
            }
        }

        internal void OnDeviceSearchComplete(List<IBLEDevice> devices)
        {
            Debug.Log("OnDeviceSearchComplete" + devices.Count);
            DeviceSearchCompleteEvent?.Invoke(this, devices);
        }

        internal void OnReadyToUse(IBLEDevice device)
        {
            Debug.Log("OnReadyToUse"); 
            OnStylusReady?.Invoke(this, device);
        }

        internal void OnDisconnected()
        {
            Debug.Log("OnDisconnected");
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        internal void OnConnectionTimeoutEvent()
        {
            Debug.Log("OnConnectionTimeoutEvent");
            ConnectionTimeoutEvent?.Invoke(this, EventArgs.Empty);
        }

        void Awake()
        {
            Init();
        }

        /// <summary>
        /// Initializes the different classes so we can search and establish a connection to the Stylus and read the data
        /// </summary>
        private void Init()
        {
            _filePath = Application.persistentDataPath + "/stylus.nnf";
            _applicationPath = Application.persistentDataPath;
#if UNITY_EDITOR
            if (StylusConfiguration.IsEmulator)
            {
                _mouseStylusControl = transform.Find("MouseStylusControl").GetComponent<MouseStylusControl>();

                Connector = new EmulatorConnection(_mouseStylusControl);
                DeviceScanner = new EmulatorDeviceScanner();
            }
            else
            {
                Connector = new UnityConnection();
                DeviceScanner = new UnityDeviceScanner(Connector.GetStylusControl());
            }
#elif UNITY_STANDALONE

            Connector = new UnityConnection();
            DeviceScanner = new UnityDeviceScanner(Connector.GetStylusControl());
            
#elif WINDOWS_UWP
            Connector = new UWPConnection();
            DeviceScanner = new UWPDeviceScanner();
#else
            throw new System.Exception("Platform for Stylus Currently not Implemented. Please refer to the Documentation of the STK for supported Platforms");
#endif

            if (StylusConfiguration == null)
            {
                Debug.LogError("Please add a StylusConfiguration to the HoloStylusManager Component. To create one, go to Assets->Create->Holo-Light->StylusConfiguration");
            }

            InvokeRepeating("ReadBattery", 1.0f, 180f);

            Connector.RegisterDataCallback(OnStylusConnected);
            Connector.RegisterDataCallback(OnStylusDataRecieved);
            Connector.RegisterDisconnectCallback(OnStylusDisconnected);

            CalibrationPreferences.Init(this);
            StylusTransform.Init(this);
            InfoManager.Init(Connector);
            _nnReader.Init(Connector);

            EventManager.RegisterCallback(StylusEventType.OnStylusDisconnected, OnDisconnected);

            if (StylusConfiguration.UseBluetoothSettings)
            {
                Destroy(_nativePairingManager.gameObject);
                DeviceScanner.RegisterDeviceFoundCallback(OnDeviceFound);
            }
            else
            {
                _nativePairingManager.Init(this);
                DeviceScanner.RegisterDeviceFoundCallback(OnDeviceFoundList);
                DeviceScanner.RegisterDeviceUpdatedCallback(OnDeviceUpdated); 
            }

            if (StylusConfiguration.StartupBehavior == StylusConfiguration.StartupBehaviorType.AutoStart)
            {
                StartStylus();
            }
        }

        private void OnStylusDisconnected(IBLEDevice disconnectedDevice)
        {
            if (IsPaired)
            {
                _invokeStylusDisConnectedEvent = true;
            }
        }

        /// <summary>
        /// Starts searching for the Stylus and tries to connect to it
        /// </summary>
        public void StartStylus()
        {
            if (!IsPaired && !IsConnecting)
            {
                OnStartSearch();
                if (StylusConfiguration.UseBluetoothSettings)
                {
                    ScanForHMUs();
                }
                else
                {
                    _nativePairingManager.SearchAndListHMUs();
                }
            }
            else
            {
                Debug.Log("Stylus is already connected or it is connecting now");
            }
        }

        /// <summary>
        /// Starts searching for BLE devices and connects automatically
        /// </summary>
        internal void ScanForHMUs()
        {
            IsPaired = false;
            IsConnecting = false;
            DeviceScanner.StartScanning();
        }

        private void OnDeviceUpdated(IBLEDevice device)
        {
            OnDeviceFoundList(device);
        }

        private void OnDeviceFoundList(IBLEDevice device)
        {
            if (FilterHMUs(device)) return;

            if (device.IsConnectable())
            {
                // Invoke an event when stylus device is found.
                OnHMUDeviceFound(device);
                
                if (StylusConfiguration.ConnectToLastDevice && _nativePairingManager.HasSavedDevice() && device.ID == _nativePairingManager.SavedDeviceID)
                {
                    ConnectToHMU(device);
                }
                else
                {
                    _nativePairingManager.AddDevice(device);
                }
            }
        }

        /// <summary>
        /// The connect function
        /// </summary>
        /// <param name="device"></param>
        public void ConnectToHMU(IBLEDevice device)
        {
            ConnectingStarted?.Invoke(this, device);
            _nativePairingManager.Connect(device);
        }

        /// <summary>
        /// ! This device found is only triggered, when using the HoloLens Bluetooth Settings !
        /// When devices are found, they we look for a device with a hmu_v_2 in the name and if the device is paired 
        /// And connects to the device if the paired HMU is found
        /// </summary>
        /// <param name="device"></param>
        private void OnDeviceFound(IBLEDevice device)
        {
            if (FilterHMUs(device)) return;

            if (device.IsPaired)
            {
                Debug.Log("Connecting to... " + device.Name);
                IsConnecting = true;
                Connector.Connect(device);
            }
        }

        private bool FilterHMUs(IBLEDevice device)
        {
            return device.Name == "" || IsConnecting || IsPaired || !device.Name.ToLower().EndsWith(HMU_SUFFIX);
        }

        private void OnStylusDataRecieved(byte[] data)
        {            
            if (data.Length == 0)
            {
                Debug.Log("EMPTY STREAM. Please turn off/on the HMU and try again.");
                _nativePairingManager.EmptyStreamHandling();
            }

            if (data[0] == 0xFE && DataParser.IsInitialized && data.Length == 23)
            {
                _stylusFrame = DataParser.ParseFrame(data);
            }

            _timeOutWatch.Restart();
        }

        private int _debugInformationCount = 0;

        private void Update()
        {
            if (_stylusFrame != null && IsPaired)
            {
                StylusData filteredStylusData = StylusTransform.GetFilteredDataV2(_stylusFrame);
                EventManager.PushData(filteredStylusData);
            }

            if (InfoManager.BatteryValueChanged)
            {
                InfoManager.BatteryValueChanged = false;
                EventManager.PushNewBatteryData(InfoManager.BatteryPercentage);
            }

            _debugInformationCount++;
            if (_debugInformationCount > 600)
            {
                _debugInformationCount = 0;
                LogImportantInfos();
            }

            if (_lastFoundHMUDevices.Count > 0)
            {
                if (_lastFoundHMUDevices.TryDequeue(out var hmu))
                {
                    DeviceFoundEvent?.Invoke(this, hmu);
                }
            }

            if (_invokeStylusReadyEvent)
            {
                _invokeStylusReadyEvent = false;
                OnReadyToUse(ConnectedHMU);
            }

            if (_invokeStylusDisConnectedEvent)
            {
                _invokeStylusDisConnectedEvent = false;
                EventManager.TriggerDisconnected();
            }

            if (DataParser.IsVisible != wasStylusVisible)
            {
                OnStylusVisibilityChange();
            }

            wasStylusVisible = DataParser.IsVisible;
        }

        private void OnStylusVisibilityChange()
        {
            if (DataParser.IsVisible)
            {
                StylusFoundEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StylusLostEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void OnStylusConnected(IBLEDevice connectedDevice)
        {
            Debug.Log("OnStylusConnected: " + connectedDevice.Name);

            IsConnecting = false;
            IsPaired = true;

#if UNITY_EDITOR
            if (StylusConfiguration.IsEmulator)
            {
                ConnectedHMU = connectedDevice;
                _mouseStylusControl.Activate(this);
                _invokeStylusReadyEvent = true;
                return; // Emulator does not need the NNF File 
            }
#endif
            if (ConnectedHMU != null)
            {
                // If a new HMU connects, NNF needs to be read again
                if (ConnectedHMU.ID != connectedDevice.ID)
                {
                    DataParser = new HMU2DataParser();
                }
            }

            if (DataParser.IsInitialized == false)
            {
                NeuralNetworkData nnfData;

                if (File.Exists(_filePath))
                {
                    try
                    {
                        nnfData = NNFileReader.GetData(_filePath);
                        DataParser.Initialize(nnfData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("The stylus.nnf File is wrong or corrupted. DataParser could not parse that file.\n" + e.Message.ToString());
                    }
                }

                string clearedHMUID = GetCleanMACAddress(connectedDevice.ID).Replace(":", string.Empty);
                string NNFfilePathOfThisHMU = Path.Combine(_applicationPath, clearedHMUID + ".nnf");
                // try to read the "HMU:ID.nnf" file
                if (StylusConfiguration.AllowFastConnection)
                {
                    bool nnfFileLocalAvailable = File.Exists(NNFfilePathOfThisHMU);
                    if (nnfFileLocalAvailable)
                    {
                        try
                        {
                            using (FileStream nnfBinaryFile = new FileStream(NNFfilePathOfThisHMU, FileMode.Open, FileAccess.Read))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();

                                try
                                {
                                    nnfData = (NeuralNetworkData)formatter.Deserialize(nnfBinaryFile);
                                    DataParser.Initialize(nnfData);
                                }
                                catch (SerializationException excp)
                                {
                                    Debug.LogError("Failed to Deserialize. Reason: " + excp.Message);
                                    throw;
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("The " + connectedDevice.ID + ".nnf File is wrong or corrupted. DataParser could not parse that file.\n" + e.Message.ToString());
                        }
                    }
                }


                // try to read directly from the HMU
                if (!DataParser.IsInitialized)
                {
                    try
                    {
                        nnfData = await _nnReader.GetHMUData();
                        DataParser.Initialize(nnfData);

                        // and save the file locally so next time we can read it from the file
                        if (StylusConfiguration.AllowFastConnection)
                        {
                            using (FileStream fs = new FileStream(NNFfilePathOfThisHMU, FileMode.OpenOrCreate))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                try
                                {
                                    formatter.Serialize(fs, nnfData);
                                    fs.Close();
                                }
                                catch (SerializationException excp)
                                {
                                    Debug.LogError("Failed to serialize. Reason: " + excp.Message);
                                    throw;
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Reading the NNF File from the HMU Failed.\n" + e.Message.ToString());
                    }
                    finally
                    {
                        if (!DataParser.IsInitialized)
                        {
                            Debug.LogWarning("DataParser was not initialized. Stylus Position can not be interpreted.");
                        }
                    }
                }
            }

            // Read Calibration
            CalibrationPreferences.ReadCalibration();

            // Read Firmware
            InfoManager.ReadFirmwareVersion();
            
            // Vibrate the Stylus to give feedback to user
            InfoManager.Vibrate(VibrateTime.VeryShort);

            ConnectedHMU = connectedDevice;
            DeviceScanner.StopScanning();
            _invokeStylusReadyEvent = true;
        }
        
        public void StopDeviceSearch()
        {
            _nativePairingManager.StopDeviceSearch();
        }

        /// <summary>
        /// Requests the Battery Status from the Stylus
        /// </summary>
        public void ReadBattery()
        {
            if (IsPaired)
            {
                InfoManager.ReadBattery();
            }
        }

        /// <summary>
        /// Disconnects from the HMU and disables/removes the Stylus Pointers
        /// </summary>
        public void Disconnect()
        {
            Connector.Disconnect();
        }

        /// <summary>
        /// Vibrates the Stylus for the amount of the (max. value can be 255ms)
        /// </summary>
        /// <param name="ms"></param>
        public void Vibrate(int ms)
        {
            InfoManager.Vibrate(ms);
        }

        internal void ReConnectDelayed(float time)
        {
            StartCoroutine(ReconnectWithDelay(time));
        }

        private IEnumerator ReconnectWithDelay(float delayTime)
        {
            IsPaired = false;
            yield return new WaitForSeconds(delayTime);
            StartStylus();
        }

        private void OnDisconnected(StylusData data)
        {
            _timeOutWatch.Reset();
            _timeOutWatch.Stop();
            IsPaired = false;
            IsConnecting = false;
            _stylusFrame = null;
            OnDisconnected();
            //PointerSwitcher.(StylusPointerSwitcher.PointerType.All);
            if (StylusConfiguration.ReconnectAfterDisconnection)
            {
                ReConnectDelayed(1);
            }
        }

        private void LogImportantInfos()
        {
            if (_logInfos)
            {
                string logText = "";

                logText += $"HMU IsConnecting: {IsConnecting}\n";
                logText += $"HMU Connected (IsPaired): {IsPaired}\n";
                logText += $"NNF Read: {_nnReader.GetInformation()}\n";
                logText += $"DataParser Initialized: {DataParser.IsInitialized}\n";
                logText += $"Stylus Visible: {DataParser.IsVisible}\n";
                logText += $"Stylus Battery: {InfoManager.BatteryPercentage}\n";
                logText += $"Firmware Version: {InfoManager.FirmwareVersion}\n";
                logText += $"PositionOffset: {CalibrationPreferences.PositionOffset.ToString("F3")}\n";
                logText += $"RotationOffset: {CalibrationPreferences.RotationOffset.ToString("F3")}\n";
                logText += $"Prefered Hand: {CalibrationPreferences.StylusPreferredHand}\n";

                Debug.Log(logText);
            }
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
            }
            else if (ID.Length < 17)
            {
                macAddress = ID.ToUpper();
            }
            else
            {
                macAddress = ID.Substring(ID.Length - 17).ToUpper();
            }
            return macAddress;
        }
#if !WINDOWS_UWP
        private void OnDestroy()
        {
            if (!StylusConfiguration.IsEmulator && typeof(UnityConnection).IsInstanceOfType(Connector))
            {
                Connector.Disconnect();
            }
        }
#endif
    }
}