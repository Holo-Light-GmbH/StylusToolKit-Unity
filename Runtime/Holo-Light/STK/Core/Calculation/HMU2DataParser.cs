
using UnityEngine;
using HoloLight.STK.Core.Tracker;
using System;
using System.Diagnostics;

namespace HoloLight.STK.Core
{
    public struct Euler
    {
        public float roll;
        public float pitch;
        public float yaw;
    };

    public class HMU2DataParser : IStylusDataParser
    {
        private Tracker.Tracker _tracker;

        private StylusData _stylusData = new StylusData();
        private float[] _positionFloat = new float[3];
        private float[] _cameraData = new float[4];

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        private bool _isVisible = false;
        public bool IsVisible => _isVisible;

        public HMU2DataParser()
        {
        }

        public bool Initialize(NeuralNetworkData nnfData)
        {
            _tracker = new Tracker.Tracker(nnfData);
            _isInitialized = true;
            return true;
        }

        public StylusData ParseFrame(byte[] data)
        {
            if (_tracker == null)
            {
                throw new System.Exception("The DataParser is not Initialized. Please call the Initialize Function first");
            }

            if (data.Length != 23)
            {
                throw new System.Exception("Data Length of Stylus Data should always be 20 bytes.");
            }

            for (int i = 0; i < 4; i++)
            {
                int tmp = data[2 + 2 * i];
                tmp = tmp << 8;
                _cameraData[i] = data[1 + 2 * i] + tmp;
            }

            bool visible = true;
            foreach (var element in _cameraData)
            {
                if (element == 4095)
                {
                    visible = false;
                }
            }
            _isVisible = visible;
            if (visible)
            {
                _tracker.CalculateCoordinates(_cameraData, ref _positionFloat);

                if (_positionFloat[2] > -0.05f)
                {
                    _stylusData.Position = new Vector3(_positionFloat[0], _positionFloat[1], 0.1f + _positionFloat[2]);
                }
            }

            _stylusData.Buttons[0] = data[9] == 1;
            _stylusData.Buttons[1] = data[10] == 1;

            /* ROTATION EXPERIMENTAL DEV STUFF
            float x = BitConverter.ToSingle(data, 11); // x 
            float y = BitConverter.ToSingle(data, 15); // y
            float z = BitConverter.ToSingle(data, 19); // z
            float w = (float)Math.Sqrt(1 - x * x - y * y - z * z);
            Quaternion quat = new Quaternion(z, y, x, w);
            
            Euler newEuler;

            newEuler.roll = BitConverter.ToSingle(data, 11); // x 
            newEuler.pitch = BitConverter.ToSingle(data, 15); // z
            newEuler.yaw = BitConverter.ToSingle(data, 19);  // y

            newEuler.roll = newEuler.roll % 360;    // x
            newEuler.yaw = -newEuler.yaw % 360;      // y
            newEuler.pitch = newEuler.pitch % 360;  // z
            */

            _stylusData.Rotation = Vector3.zero;// Rotation is currently disabled
            //_stylusData.Rotation = new Vector3(newEuler.roll, newEuler.yaw, newEuler.pitch);
            return _stylusData;
        }
    }
}