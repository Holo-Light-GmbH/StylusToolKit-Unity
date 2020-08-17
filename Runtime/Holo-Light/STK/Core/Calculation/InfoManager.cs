using System;
using System.Net.Http;
using Debug = UnityEngine.Debug;

namespace HoloLight.STK.Core
{
    enum VibrateTime
    {
        VeryShort = 40,
        Short = 100,
        Long = 250
    }

    /// <summary>
    /// Handles reading additional Informations like FirmwareVersion, Stylus Battery or Vibrating
    /// </summary>
    public class InfoManager
    {
        private IConnection _connection;

        public string FirmwareVersion = "1.0.0";

        public int BatteryPercentage = 100;
        public bool BatteryValueChanged = false;
        public void Init(IConnection connection)
        {
            _connection = connection;
        }

        private int _noBatteryDataRecieved = 0;
        private int _noFirwareDataRecieved = 0;

        private int _callbackCounter = 0;

        /// <summary>
        /// Sends the command to read the get the Battery Value
        /// </summary>
        internal void ReadFirmwareVersion()
        {
            byte[] readFirmwareBytes = { 0xFA, 0x52 };
            _connection.RegisterDataCallback(OnFirmwareVersion);
            _noFirwareDataRecieved = 0;

            _connection.SendData(readFirmwareBytes);
        }

        private void OnFirmwareVersion(byte[] data)
        {
            if (data.Length == 4)
            {
                FirmwareVersion = data[1].ToString() + "." + data[2].ToString() + "." + data[3].ToString();
                _noFirwareDataRecieved = 0;
                _connection.UnRegisterDataCallback(OnFirmwareVersion);
            }
            else
            {
                _noFirwareDataRecieved++;

                if (_noFirwareDataRecieved > 60)
                {
                    _connection.UnRegisterDataCallback(OnFirmwareVersion);
                    ReadFirmwareVersion();
                }
            }
        }

        internal void Vibrate(VibrateTime time)
        {
            Vibrate((int)time);
        }

        /// <summary>
        /// Vibrates the Stylus for the amount of ms passed. The maximum amount of time is limited at 255ms
        /// </summary>
        /// <param name="ms"></param>
        internal void Vibrate(int ms = 100)
        {
            if (ms > 255)
            {
                Debug.Log("Notice: The maximum amount of time for Vibration is limited at 255ms");
            }
            if (_connection.IsConnected())
            {
                byte msByte = Convert.ToByte(ms);
                byte[] vibrateCommand = new byte[] { 0x02, 0x51, msByte };

                _connection.SendData(vibrateCommand);
                ReadBattery();
            }
        }

        /// <summary>
        /// Sends the command to read the get the Battery Value
        /// </summary>
        internal void ReadBattery()
        {
            byte[] readBatPercentageBytes = { 0xBA, 0x51 };
            _connection.RegisterDataCallback(OnStylusBatteryData);
            _callbackCounter++;
            _noBatteryDataRecieved = 0;

            _connection.SendData(readBatPercentageBytes);
        }

        /// <summary>
        /// Pushes the Event through the EventManager, so all Listeners recieve the new Battery value
        /// </summary>
        /// <param name="data"></param>
        private void OnStylusBatteryData(byte[] data)
        {
            if (data.Length != 23) return;

            if (data[13] == 0x00 && data[14] == 0x00 && data[15] == 0x00 && data[16] == 0x00 && data[17] == 0x00 && data[18] == 0x00 && data[19] == 0x00)
            {
                int batterymV = data[12] | data[11] << 8;

                // 4075mV -> 100%
                // 3400mV -> 2%
                float percentageValue = 0.145f * batterymV - 491.63f;

                if (percentageValue > 100)
                {
                    percentageValue = 100;
                }
                else if (percentageValue < 0)
                {
                    // percentageValue = 0; // this line has to be uncommented, once the firmware supports reading battery again
                    percentageValue = 100;
                }

                BatteryPercentage = Convert.ToInt32(percentageValue);
                BatteryValueChanged = true;
              
                /*
                
                Debug.Log("BATTERY STATE");
                Debug.Log(NNReader.ByteArrayHelper.ByteToString(data));

                Debug.Log(batterymV);
                Debug.Log(percentageValue);
                Debug.Log(BatteryPercentage);

                */

                _connection.UnRegisterDataCallback(OnStylusBatteryData);
                ReduceCallbackCounter();
            }
            else
            {
                _noBatteryDataRecieved++;

                if (_noBatteryDataRecieved > 47)
                {
                    _connection.UnRegisterDataCallback(OnStylusBatteryData);
                    ReduceCallbackCounter();
                    ReadBattery();
                }
            }
        }

        private void ReduceCallbackCounter()
        {
            _callbackCounter--;
            while (_callbackCounter > 0)
            {
                _connection.UnRegisterDataCallback(OnStylusBatteryData);
                _callbackCounter--;
            }
        }
    }
}