
using System;
using UnityEngine;

namespace HoloLight.STK.Core
{
    public class CalibrationPreferences
    {
        public enum StylusHoldingHand { Right = 0, Left = 1, Auto = 2 }

        public Vector3 PositionOffset { get; private set; } = Vector3.zero;
        public Vector3 RotationOffset { get; private set; } = Vector3.zero;

        private HoloStylusManager _manager;
        private IConnection _connection;

        private int _noCalibrationDataRecieved = 0;

        public StylusHoldingHand StylusPreferredHand = StylusHoldingHand.Right;

        public void Init(HoloStylusManager manager)
        {
            _manager = manager;
            _connection = manager.Connector;
        }

        /// <summary>
        /// Changes the calibration offset and calls the function to save it on the HMU
        /// </summary>
        /// <param name="newPositionOffset"></param>
        public void SaveCalibration(Vector3 newPositionOffset, Vector3 newRotationOffset)
        {
            PositionOffset -= newPositionOffset;

            // Currently Rotation Calibration is disabled, because the rotation values are experimental
            // RotationOffset -= newRotationOffset;

            ChangeValuesOnHMU();
        }

        /// <summary>
        /// Changes the rotation offset
        /// </summary>
        /// <param name="newRotationOffset"></param>
        public void SaveOffset(Vector3 newRotationOffset)
        {
            RotationOffset -= newRotationOffset;
        }

        /// <summary>
        /// Before writing, we need to make sure, the storage is empty (FF:FF:FF ...)
        /// </summary>
        private void ChangeValuesOnHMU()
        {
            // DELETES  Range of 0x71110 - 0x72110 
            byte[] deleteBytes = { 0xFA, 0x5E, 0x00, 0x07, 0x11, 0x10, 0x01 };

            _connection.RegisterDataCallback(OnDeleteInfo);
            // or timeout?
            _connection.SendData(deleteBytes);
        }

        /// <summary>
        /// Sends the actual data to the HMU
        /// </summary>
        void SaveToHMU()
        {
            float x = PositionOffset.x;
            float y = PositionOffset.y;
            float z = PositionOffset.z;

            // 0x71110 CALIBRATION 
            byte[] sendBytes = { 0xFA, 0xAA, 0x00, 0x07, 0x11, 0x10, 0x00, 0x00, 0x00, 0x28 };

            _connection.SendData(sendBytes);

            byte[] positionBytes = new byte[20];

            // [0 - 3] Reserved to recognize Calibration Data
            positionBytes[0] = 0xCA; // CA
            positionBytes[1] = 0x71; // LI
            positionBytes[2] = 0xB8; // BR
            positionBytes[3] = 0x47; // AT

            // [4 - 15] The actual x,y,z Values 
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, positionBytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, positionBytes, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(z), 0, positionBytes, 12, 4);

            // [16] Prefered Stylus Hand Setting (can be changed with the Companion App)
            positionBytes[16] = (byte)StylusPreferredHand;     // 00 is right     01 is left    02 is auto

            // [17 - 19] Reserved to recognize the end
            positionBytes[17] = 0x99;
            positionBytes[18] = 0x99;
            positionBytes[19] = 0x99;

            _connection.SendData(positionBytes);



            byte[] rotationBytes = new byte[20];

            float xRot = RotationOffset.x;
            float yRot = RotationOffset.y;
            float zRot = RotationOffset.z;

            // [0 - 3] Reserved to recognize Calibration Data
            rotationBytes[0] = 0xCA; // CA
            rotationBytes[1] = 0x71; // LI
            rotationBytes[2] = 0xB8; // BR
            rotationBytes[3] = 0x47; // AT

            // [4 - 15] The actual x,y,z Values 
            Buffer.BlockCopy(BitConverter.GetBytes(xRot), 0, rotationBytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(yRot), 0, rotationBytes, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(zRot), 0, rotationBytes, 12, 4);

            // [16 - 19] Reserved to recognize the end
            rotationBytes[16] = 0x99;
            rotationBytes[17] = 0x99;
            rotationBytes[18] = 0x99;
            rotationBytes[19] = 0x99;

            _connection.SendData(rotationBytes);
        }

        // when the bug with BADOC0FFE response is fixed, this can be removed
        private int _packetsCounter = 0;

        /// <summary>
        /// When we can be sure, that the storage is empty, we can start writing
        /// </summary>
        /// <param name="data"></param>
        private void OnDeleteInfo(byte[] data)
        {
            _packetsCounter++;
            if (_packetsCounter > 50)
            {
                _connection.UnRegisterDataCallback(OnDeleteInfo);
                SaveToHMU();
                _packetsCounter = 0;
            }
            if (data.Length == 4)
            {
                if (data[0] == 0xBA && data[1] == 0xDC && data[2] == 0x0F && data[3] == 0xFE)
                {
                    _connection.UnRegisterDataCallback(OnDeleteInfo);
                    SaveToHMU();
                    _packetsCounter = 0;
                }
            }
        }

        /// <summary>
        /// Sending the read command to hmu, so it sends back the saved calibration data and assigns it
        /// </summary>
        public void ReadCalibration()
        {
            _connection.RegisterDataCallback(OnCalibrationData);

            _noCalibrationDataRecieved = 0;
            byte[] readBytes = { 0xFA, 0x99, 0x00, 0x07, 0x11, 0x10, 0x00, 0x00, 0x00, 0xF0 };

            _connection.SendData(readBytes);
        }

        /// <summary>
        /// When HMU sends my data back, I can parse and use it
        /// </summary>
        private void OnCalibrationData(byte[] calibrationData)
        {
            if (calibrationData.Length == 240)
            {
                if (calibrationData[0] == 0xFF && calibrationData[1] == 0xFF && calibrationData[2] == 0xFF && calibrationData[3] == 0xFF)
                {
                    _connection.UnRegisterDataCallback(OnCalibrationData);
                    // no calibration data saved
                    return;
                }
                // got a new calibration value
                if (calibrationData[0] == 0xCA && calibrationData[1] == 0x71 && calibrationData[2] == 0xB8 && calibrationData[3] == 0x47)
                {
                    byte[] positionBytes = calibrationData;

                    float x = BitConverter.ToSingle(positionBytes, 4);
                    float y = BitConverter.ToSingle(positionBytes, 8);
                    float z = BitConverter.ToSingle(positionBytes, 12);

                    Vector3 offsetValue = new Vector3(x, y, z);

                    if (positionBytes[16] == 0xFF || positionBytes[16] == 0x99)
                    {
                        positionBytes[16] = 0x00;
                    }

                    StylusPreferredHand = (StylusHoldingHand)positionBytes[16];

                    _manager.EventManager.TriggerNewPreferedHand();
                    _connection.UnRegisterDataCallback(OnCalibrationData);

                    PositionOffset = offsetValue;

                    if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                    {
                        Debug.Log("No Valid Calibration Data Read - x " + x + " y " + y + " z " + z);
                        return;
                    }

                    if (calibrationData[20] == 0xCA && calibrationData[21] == 0x71 && calibrationData[22] == 0xB8 && calibrationData[23] == 0x47)
                    {
                        byte[] rotationBytes = calibrationData;

                        float xRot = BitConverter.ToSingle(rotationBytes, 24);
                        float yRot = BitConverter.ToSingle(rotationBytes, 28);
                        float zRot = BitConverter.ToSingle(rotationBytes, 32);

                        Vector3 rotationOffsetValue = new Vector3(xRot, yRot, zRot);

                        RotationOffset = rotationOffsetValue;
                    }
                }
                _noCalibrationDataRecieved = 0;
            }
            else
            {
                _noCalibrationDataRecieved++;

                if (_noCalibrationDataRecieved > 40)
                {
                    _connection.UnRegisterDataCallback(OnCalibrationData);
                    ReadCalibration();
                }
            }
        }

        public void SetStylusHand(StylusHoldingHand stylusHoldingHand)
        {
            StylusPreferredHand = stylusHoldingHand;
            _manager.EventManager.TriggerNewPreferedHand();
            ChangeValuesOnHMU();
        }

        public static string ByteToString(byte[] data)
        {
            string testString = "";
            foreach (byte bbb in data)
            {
                testString += bbb.ToString("X2") + ":";
            }

            return testString;
        }
    }
}