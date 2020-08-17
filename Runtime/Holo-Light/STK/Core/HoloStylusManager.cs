
#if UNITY_STANDALONE || UNITY_EDITOR
using HoloLight.UnityDriver;
#endif

using System.Collections;
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
        public bool IsPaired { get; set; }
        public bool IsConnecting { get; set; }

        private Stopwatch _timeOutWatch = new Stopwatch();

        private string _filePath;


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

            CalibrationPreferences.Init(this);
            StylusTransform.Init(this);
            InfoManager.Init(Connector);
            _nnReader.Init(Connector);

            EventManager.RegisterCallback(StylusEventType.OnStylusDisconnected, OnDisconnected);

            if (StylusConfiguration.UseBluetoothSettings)
            {
                Destroy(_nativePairingManager.gameObject);
                DeviceScanner.RegisterDeviceFoundCallback(OnDeviceFound);
                if (StylusConfiguration.StartupBehavior == StylusConfiguration.StartupBehaviorType.AutoStart)
                {
                    AutoConnect();
                }
            }
            else
            {
                _nativePairingManager.Init(this);
                DeviceScanner.RegisterDeviceFoundCallback(OnDeviceFoundList);
                DeviceScanner.RegisterDeviceUpdatedCallback(OnDeviceUpdated); 
                if (StylusConfiguration.StartupBehavior == StylusConfiguration.StartupBehaviorType.AutoStart)
                {
                    _nativePairingManager.SearchHMU();
                }
            }
        }

        /// <summary>
        /// Starts searching for the Stylus and tries to connect to it
        /// </summary>
        public void StartStylus()
        {
            if (!IsPaired && !IsConnecting)
            {
                if (StylusConfiguration.UseBluetoothSettings)
                {
                    AutoConnect();
                }
                else
                {
                    _nativePairingManager.SearchHMU();
                }
            } else
            {
                Debug.Log("Stylus is already connected or it is connecting now");
            }
        }

        /// <summary>
        /// Starts searching for BLE devices
        /// </summary>
        public void AutoConnect()
        {
            IsPaired = false;
            IsConnecting = false;
            DeviceScanner.StartScanning();
        }

        private void OnDeviceUpdated(IBLEDevice device)
        {
            OnDeviceFoundList(device);
        }

        public void OnDeviceFoundList(IBLEDevice device)
        {
            if (FilterHMUs(device)) return;

            if (device.IsConnectable())
            {
                if (_nativePairingManager.HasSavedDevice() && device.ID == _nativePairingManager.SavedDeviceID)
                {
                    _nativePairingManager.Connect(device);
                }
                else
                {
                    _nativePairingManager.AddDevice(device);
                }
            }
        }

        /// <summary>
        /// When devices are found, they we look for a device with a hmu_v_2 in the name and if the device is paired (HoloLens Bluetooth Settigns)
        /// And connects to the device if the paired HMU is found
        /// </summary>
        /// <param name="device"></param>
        public void OnDeviceFound(IBLEDevice device)
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

        public void OnStylusDataRecieved(byte[] data)
        {
            if (data.Length == 0)
            {
                Debug.Log("EMPTY STREAM. Please turn off/on the HMU and try again.");
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

            if (_timeOutWatch.ElapsedMilliseconds > 3000)
            {
                EventManager.TriggerDisconnected();
            }

            _debugInformationCount++;
            if (_debugInformationCount > 600)
            {
                _debugInformationCount = 0;
                LogImportantInfos();
            }
        }

        public async void OnStylusConnected(IBLEDevice connectedDevice)
        {
            Debug.Log("OnStylusConnected: " + connectedDevice.Name);

            IsConnecting = false;
            IsPaired = true;
            Device = connectedDevice;

#if UNITY_EDITOR
            if (StylusConfiguration.IsEmulator)
            {
                _mouseStylusControl.Activate(this);
                return; // Emulator does not need the NNF File 
            }
#endif
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

                if (!DataParser.IsInitialized)
                {
                    try
                    {
                        nnfData = await _nnReader.GetHMUData();
                        DataParser.Initialize(nnfData);
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

            if (InfoManager.FirmwareVersion == "1.0.0")
            {   
                // Read Firmware
                InfoManager.ReadFirmwareVersion();
            }

            InfoManager.Vibrate(VibrateTime.VeryShort);

            DeviceScanner.StopScanning();
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
            // fire disconnected event immediately
            EventManager.TriggerDisconnected();
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
            AutoConnect();
        }

        private void OnDisconnected(StylusData data)
        {
            Debug.Log("Disconnected");
            _timeOutWatch.Reset();
            _timeOutWatch.Stop();
            IsPaired = false;
            _stylusFrame = null;
            PointerSwitcher.DisablePointer(StylusPointerSwitcher.PointerType.All);
            if (StylusConfiguration.ReconnectAfterDisconnection)
            {
                StartStylus();
            }
        }

        private void LogImportantInfos()
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